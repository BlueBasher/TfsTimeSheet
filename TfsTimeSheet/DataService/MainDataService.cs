namespace TfsTimeSheet.DataService
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Transactions;
	using TfsTimeSheet.Models;

	public class MainDataService
	{
		private readonly TfsDataService _tfsDataService;
		private string _url;
		private string _project;

		public string UserName
		{
			get { return _tfsDataService.UserName; }
		}

		public bool IsAuthenticated
		{
			get { return _tfsDataService.IsAuthenticated; }
		}

		public MainDataService()
		{
			_tfsDataService = new TfsDataService();
		}

		public void Connect(string url, string project, string workItemQuery, string ignoreRemainingArea, string activeState, string closedState)
		{
			_url = url;
			_project = project;
			_tfsDataService.Connect(_url, _project, workItemQuery, ignoreRemainingArea, activeState, closedState);
		}

		public void Disconnect()
		{
			_tfsDataService.Disconnect();
		}

		public IEnumerable<TimeSheetItem> GetWorkItems(DateTime firstDayOfWeek, string userName)
		{
			List<TimeSheetItem> dbEntries;
			using (var appDbContext = new AppDbContext())
			{
				dbEntries =
					appDbContext.TimeSheetItems
								.Where(t => t.FirstDayOfWeek.Equals(firstDayOfWeek) &&
											t.UserId.Equals(_tfsDataService.UserId) &&
											t.ServerUrl.Equals(_url) &&
											t.Project.Equals(_project))
								.OrderBy(t => t.Name)
								.ToList();
			}

			var tfsEntries = _tfsDataService.GetWorkItems(firstDayOfWeek, userName);

			// All dbEntries that aren't available in tfs anymore are removed or closed
			foreach (var entry in dbEntries.Where(entry => !tfsEntries.Any(t => t.WorkItemId.Equals(entry.WorkItemId))))
			{
				entry.IsClosed = true;
			}

			// Add all new tfsEntries
			foreach (var timeSheetItem in tfsEntries.Where(timeSheetItem => !dbEntries.Any(t => t.WorkItemId.Equals(timeSheetItem.WorkItemId))))
			{
				dbEntries.Add(new TimeSheetItem
				{
					WorkItemId = timeSheetItem.WorkItemId,
					Name = timeSheetItem.Name,
					FirstDayOfWeek = firstDayOfWeek
				});
			}
			return dbEntries;
		}

		public void SaveItem(TimeSheetItem timeSheetItem, string changedPropertyName)
		{
			if (timeSheetItem == null)
			{
				throw new ArgumentNullException("timeSheetItem");
			}
			if (string.IsNullOrWhiteSpace(changedPropertyName))
			{
				throw new ArgumentNullException("changedPropertyName");
			}

			using (var transactionScope = new TransactionScope())
			{
				using (var appDbContext = new AppDbContext())
				{
					var original =
						appDbContext.TimeSheetItems
									.SingleOrDefault(t => t.WorkItemId.Equals(timeSheetItem.WorkItemId) &&
														  t.UserId.Equals(_tfsDataService.UserId));

					// Create a new item when it doesn't exist in the Db
					if (original == null)
					{
						original = new TimeSheetItem
							{
								WorkItemId = timeSheetItem.WorkItemId,
								Project = _project,
								ServerUrl = _url,
								Name = timeSheetItem.Name,
								FirstDayOfWeek = timeSheetItem.FirstDayOfWeek,
								UserId = _tfsDataService.UserId
							};
						appDbContext.TimeSheetItems.Add(original);
					}

					var property = typeof(TimeSheetItem).GetProperty(changedPropertyName);
					// Check if we're updating the effort
					if (property.PropertyType == typeof(DateTime))
					{
						var delta = GetDateTimeDelta(timeSheetItem, original, property);

						_tfsDataService.UpdateWorkItemEffort(original.WorkItemId, delta);

						// Update the object with the delta and save it into the Db
						property.SetValue(original, Convert.ToDateTime(property.GetValue(original)).Add(delta));
						appDbContext.SaveChanges();
					}
					// Or if we're updating the state
					else if (property.Name.Equals("IsClosed"))
					{
						_tfsDataService.UpdateWorkItemClosedState(original.WorkItemId);

						appDbContext.SaveChanges();
					}

					// Commit the entire transaction
					transactionScope.Complete();
				}
			}
		}

		private static TimeSpan GetDateTimeDelta(TimeSheetItem timeSheetItem, TimeSheetItem original, PropertyInfo property)
		{
			// Get the original value or get the default value
			var defaultDateTime = new TimeSheetItem().Monday;

			var originalValue = original == null
									? defaultDateTime
									: Convert.ToDateTime(property.GetValue(original));

			var changedValue = Convert.ToDateTime(property.GetValue(timeSheetItem));

			// Sanity check to make sure we're not adding a massive amount of time
			// And to make sure we can save the datetime into sql Db
			changedValue = new DateTime(defaultDateTime.Year, defaultDateTime.Month, defaultDateTime.Day,
				changedValue.Hour, changedValue.Minute, changedValue.Second);

			// Determine the delta between the to datetimes
			return changedValue.Subtract(originalValue);
		}
	}
}

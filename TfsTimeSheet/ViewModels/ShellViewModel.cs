namespace TfsTimeSheet.ViewModels
{
    using System.Windows;
    using Caliburn.Micro;

    public class ShellViewModel : Conductor<object>
    {
	    public Visibility MenuVisibility { get { return Visibility.Collapsed; } }

		public override string DisplayName
		{
			get
			{
				var activeItem = ActiveItem as IHaveDisplayName;
				
				return
					activeItem != null ? 
						string.Format("{0} - {1}", base.DisplayName, activeItem.DisplayName) : 
						base.DisplayName;
			}
			set
			{
				base.DisplayName = value;
			}
		}

		protected override void OnInitialize()
        {
            base.OnInitialize();
            DisplayName = "Tfs Timesheet";

			ShowWeekOverview();
		}

        public void ShowWeekOverview()
        {
            ActivateItem(new WeekOverviewViewModel());
        }
    }
}

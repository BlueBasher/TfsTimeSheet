namespace TfsTimeSheet.ViewModels
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using Caliburn.Micro;
    using Itenso.TimePeriod;
    using TfsTimeSheet.DataService;
    using TfsTimeSheet.Models;

    public class WeekOverviewViewModel : Screen
    {
        #region Properties
        private readonly MainDataService _dataService;

        public string Title
        {
            get
            {
                var week = new Week(CurrentFirstDayOfWeek);

                return string.Format("Week {0} - {1}",
                    week.WeekOfYear.ToString(CultureInfo.InvariantCulture),
                    CurrentFirstDayOfWeek.Year.ToString(CultureInfo.InvariantCulture));
            }
        }

        private string _currentUserName;
        public string CurrentUserName
        {
            get { return _currentUserName; }
            set
            {
                _currentUserName = value;
                NotifyOfPropertyChange(() => CurrentUserName);
            }
        }

        private bool _isWeekReadOnly;
        public bool IsWeekReadOnly
        {
            get { return _isWeekReadOnly; }
            set
            {
                _isWeekReadOnly = value;
                NotifyOfPropertyChange(() => IsWeekReadOnly);
            }
        }

        private DateTime _currentFirstDayOfWeek;
        public DateTime CurrentFirstDayOfWeek
        {
            get { return _currentFirstDayOfWeek; }
            set
            {
                _currentFirstDayOfWeek = value;
                NotifyOfPropertyChange(() => CurrentFirstDayOfWeek);
                NotifyOfPropertyChange(() => Title);
            }
        }

        private IObservableCollection<TimeSheetItem> _week;
        public IObservableCollection<TimeSheetItem> Week
        {
            get { return _week; }
            set
            {
                if (_week == null)
                {
                    _week = new BindableCollection<TimeSheetItem>();
                    _week.CollectionChanged += Week_CollectionChanged;
                }
                _week.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        _week.Add(item);
                    }
                    UpdateTotalRecord();
                }

                NotifyOfPropertyChange(() => Week);
            }
        }

        private void UpdateTotalRecord()
        {
            _week.Remove(_week.SingleOrDefault(t => t.IsTotal));
            var total = new TimeSheetItem { IsTotal = true, Name = "TOTAL"};

            foreach (var item in _week)
            {
                total.Monday = total.Monday
                                    .AddHours(item.Monday.Hour)
                                    .AddMinutes(item.Monday.Minute)
                                    .AddSeconds(item.Monday.Second);
                total.Tuesday = total.Tuesday
                                     .AddHours(item.Tuesday.Hour)
                                     .AddMinutes(item.Tuesday.Minute)
                                     .AddSeconds(item.Tuesday.Second);
                total.Wednesday = total.Wednesday
                                       .AddHours(item.Wednesday.Hour)
                                       .AddMinutes(item.Wednesday.Minute)
                                       .AddSeconds(item.Wednesday.Second);
                total.Thursday = total.Thursday
                                      .AddHours(item.Thursday.Hour)
                                      .AddMinutes(item.Thursday.Minute)
                                      .AddSeconds(item.Thursday.Second);
                total.Friday = total.Friday
                                    .AddHours(item.Friday.Hour)
                                    .AddMinutes(item.Friday.Minute)
                                    .AddSeconds(item.Friday.Second);
            }
            _week.Add(total);
        }

        #endregion

        #region ctor
        public WeekOverviewViewModel()
        {
            _dataService = new MainDataService();

            CurrentFirstDayOfWeek = new Week(DateTime.Now).FirstDayOfWeek;
        }
        protected override void OnInitialize()
        {
            base.OnInitialize();

            _dataService.Connect(
                ConfigurationManager.AppSettings["TfsUrl"],
                ConfigurationManager.AppSettings["TfsProject"],
                ConfigurationManager.AppSettings["TfsWorkItemQuery"],
                ConfigurationManager.AppSettings["TfsActiveState"],
                ConfigurationManager.AppSettings["TfsClosedState"]);

            CurrentUserName = _dataService.UserName;

            GetWeekData();
        }
        #endregion

        #region Public Methods
        public void Previous()
        {
            CurrentFirstDayOfWeek = CurrentFirstDayOfWeek.AddDays(-7);
            GetWeekData();
        }
        public void Next()
        {
            CurrentFirstDayOfWeek = CurrentFirstDayOfWeek.AddDays(7);
            GetWeekData();
        }
        #endregion

        #region Private Methods
        private void Week_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (TimeSheetItem item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (TimeSheetItem item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
        }
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                _dataService.SaveItem(sender as TimeSheetItem, e.PropertyName);
                UpdateTotalRecord();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving item. " + ex.Message);

                GetWeekData(); // Refresh so we make sure we have the last data
            }
        }

        private void GetWeekData()
        {
            Week = new BindableCollection<TimeSheetItem>(_dataService.GetWorkItems(CurrentFirstDayOfWeek, _dataService.UserName));
        }
        #endregion
    }
}

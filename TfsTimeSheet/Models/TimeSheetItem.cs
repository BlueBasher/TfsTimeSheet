namespace TfsTimeSheet.Models
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using Caliburn.Micro;

    public class TimeSheetItem : PropertyChangedBase
    {
        public TimeSheetItem()
        {
            var defaultDateTime = new DateTime(2000, 1, 1);
            _monday = defaultDateTime;
            _tuesday = defaultDateTime;
            _wednesday = defaultDateTime;
            _thursday = defaultDateTime;
            _friday = defaultDateTime;
        }

        public int Id { get; set; }

        public int WorkItemId { get; set; }
		public string Project { get; set; }
		public string ServerUrl { get; set; }
		public string Name { get; set; }
        public DateTime FirstDayOfWeek { get; set; }
        public int UserId { get; set; }
        public bool IsTotal { get; set; }
        public string Url {
            get { return ConfigurationManager.AppSettings["WorkItemUrl"] + WorkItemId.ToString(CultureInfo.InvariantCulture); }
        }

        private bool _isClosed;
        public bool IsClosed
        {
            get
            {
                return _isClosed;
            }
            set
            {
                _isClosed = value;
                NotifyOfPropertyChange(() => IsClosed);
            }
        }

        private DateTime _monday;
        public DateTime Monday
        {
            get { return _monday; }
            set
            {
                _monday = value;
                NotifyOfPropertyChange(() => Monday);
            }
        }

        private DateTime _tuesday;
        public DateTime Tuesday
        {
            get { return _tuesday; }
            set
            {
                _tuesday = value;
                NotifyOfPropertyChange(() => Tuesday);
            }
        }

        private DateTime _wednesday;
        public DateTime Wednesday
        {
            get { return _wednesday; }
            set
            {
                _wednesday = value;
                NotifyOfPropertyChange(() => Wednesday);
            }
        }

        private DateTime _thursday;
        public DateTime Thursday
        {
            get { return _thursday; }
            set
            {
                _thursday = value;
                NotifyOfPropertyChange(() => Thursday);
            }
        }

        private DateTime _friday;
        public DateTime Friday
        {
            get { return _friday; }
            set
            {
                _friday = value;
                NotifyOfPropertyChange(() => Friday);
            }
        }

    }
}

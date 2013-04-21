namespace TfsTimeSheet.ViewModels
{
    using System.Windows;
    using Caliburn.Micro;

    public class ShellViewModel : Conductor<object>
    {
        public ShellViewModel()
        {
            ShowWeekOverview();
        }

        public Visibility MenuVisibility { get { return Visibility.Collapsed; } }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DisplayName = "Tfs Timesheet";
        }

        public void ShowWeekOverview()
        {
            ActivateItem(new WeekOverviewViewModel());
        }
    }
}

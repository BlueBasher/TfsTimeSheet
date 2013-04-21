namespace TfsTimeSheet
{
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using Caliburn.Micro;
    using TfsTimeSheet.ViewModels;

    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private Mutex _mutex;

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var mutexName = Assembly.GetExecutingAssembly().GetName().Name;
            bool createdNew;
            _mutex = new Mutex(true, mutexName, out createdNew);

            if (_mutex == null || createdNew)
            {
                base.OnStartup(sender, e);
                return;
            }
            MessageBox.Show("Instance already running.");
            Application.Current.Shutdown();
        }
    }
}

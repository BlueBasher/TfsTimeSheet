namespace TfsTimeSheet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using System.Windows;
	using Caliburn.Micro;
	using TfsTimeSheet.ViewModels;

	public class AppBootstrapper : Bootstrapper<ShellViewModel>
	{
		private Mutex _mutex;

		protected override void Configure()
		{
			base.Configure();

			ConventionManager.AddElementConvention<UIElement>(UIElement.VisibilityProperty, "Visibility", "VisibilityChanged");

			var baseBindProperties = ViewModelBinder.BindProperties;
			ViewModelBinder.BindProperties =
				(frameWorkElements, viewModel) =>
				{
					var frameWorkElementsList = frameWorkElements.ToList();
					BindVisiblityProperties(frameWorkElementsList, viewModel);
					return baseBindProperties(frameWorkElementsList, viewModel);
				};

			// Need to override BindActions as well, as it's called first and filters out anything it binds to before
			// BindProperties is called.
			var baseBindActions = ViewModelBinder.BindActions;
			ViewModelBinder.BindActions =
				(frameWorkElements, viewModel) =>
				{
					var frameWorkElementsList = frameWorkElements.ToList();
					BindVisiblityProperties(frameWorkElementsList, viewModel);
					return baseBindActions(frameWorkElementsList, viewModel);
				};
		}

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

		private static void BindVisiblityProperties(IEnumerable<FrameworkElement> frameWorkElements, Type viewModel)
		{
			foreach (var frameworkElement in frameWorkElements)
			{
				var propertyName = frameworkElement.Name + "IsVisible";
				var property = viewModel.GetPropertyCaseInsensitive(propertyName);
				if (property != null)
				{
					var convention = ConventionManager
						.GetElementConvention(typeof(FrameworkElement));
					ConventionManager.SetBindingWithoutBindingOverwrite(
						viewModel,
						propertyName,
						property,
						frameworkElement,
						convention,
						convention.GetBindableProperty(frameworkElement));
				}
			}
		}
	}
}

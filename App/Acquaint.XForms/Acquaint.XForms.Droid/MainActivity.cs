namespace Acquaint.XForms.Droid
{
	[Activity (Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	// inhertiting from FormsAppCompatActivity is imperative to taking advantage of Android AppCompat libraries
	{
		protected override void OnCreate (Bundle bundle)
		{
			// register HockeyApp as the crash reporter
			CrashManager.Register(this, Settings.HockeyAppId);

			RegisterDependencies();

			// Azure Mobile Services initilizatio
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			// this line is essential to wiring up the toolbar styles defined in ~/Resources/layout/toolbar.axml
			FormsAppCompatActivity.ToolbarResource = Resource.Layout.toolbar;

			base.OnCreate (bundle);

			Forms.Init (this, bundle);

			FormsMaps.Init (this, bundle);

			LoadApplication (new App ());
		}

		/// <summary>
		/// Registers dependencies with an IoC container.
		/// </summary>
		/// <remarks>
		/// Since some of our libraries are shared between the Forms and Native versions 
		/// of this app, we're using an IoC/DI framework to provide access across implementations.
		/// </remarks>
		static void RegisterDependencies()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(new EnvironmentService()).As<IEnvironmentService>();
			builder.RegisterInstance(new GuidUtility()).As<IGuidUtility>();
			builder.RegisterInstance(new HttpClientHandlerFactory()).As<IHttpClientHandlerFactory>();

			var container = builder.Build();

			var csl = new AutofacServiceLocator(container);
			ServiceLocator.SetLocatorProvider(() => csl);
		}
	}
}

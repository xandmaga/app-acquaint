using Android.App;
using Android.Content.PM;
using Android.OS;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Xamarin;
using HockeyApp;
using Microsoft.Practices.ServiceLocation;
using Autofac.Extras.CommonServiceLocator;
using Autofac;
using Acquaint.Data;

namespace Acquaint.XForms.Droid
{
	[Activity (Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	// inhertiting from FormsAppCompatActivity is imperative to taking advantage of Android AppCompat libraries
	{
		public static IContainer Container { get; set; }

		protected override void OnCreate (Bundle bundle)
		{
			// If you would like to collect crash reports with HockeyApp, do so here
			CrashManager.Register(this, "11111111222222223333333344444444"); // This is just a placeholder value. Replace with your real HockeyApp App ID

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

		static void RegisterDependencies()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(new NullHttpClientHandlerFactory()).As<IHttpClientHandlerFactory>();

			Container = builder.Build();

			var csl = new AutofacServiceLocator(Container);
			ServiceLocator.SetLocatorProvider(() => csl);
		}
	}
}

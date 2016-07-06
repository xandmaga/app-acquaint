using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin;
using ImageCircle.Forms.Plugin.iOS;
using HockeyApp;
using Autofac;
using Acquaint.Data;
using Microsoft.Practices.ServiceLocation;
using Autofac.Extras.CommonServiceLocator;

namespace Acquaint.XForms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
		public static IContainer Container { get; set; }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			var manager = BITHockeyManager.SharedHockeyManager;
			// Set the HockeyApp App Id here:
			manager.Configure("11111111222222223333333344444444"); // This is just a placeholder value. Replace with your real HockeyApp App ID
			manager.StartManager();

			RegisterDependencies();

			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

            Forms.Init();

			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			SQLitePCL.CurrentPlatform.Init();

            FormsMaps.Init();

            LoadApplication(new App());

            ConfigureTheming();

            ImageCircleRenderer.Init();

            return base.FinishedLaunching(app, options);
        }

        void ConfigureTheming()
        {
            UINavigationBar.Appearance.TintColor = UIColor.White;
            UINavigationBar.Appearance.BarTintColor = Color.FromHex("547799").ToUIColor();
            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            UIBarButtonItem.Appearance.SetTitleTextAttributes (new UITextAttributes { TextColor = UIColor.White }, UIControlState.Normal);
        }

		static void RegisterDependencies()
		{
			var builder = new ContainerBuilder();

			#if DEBUG
			builder.RegisterInstance(new HttpClientHandlerFactory()).As<IHttpClientHandlerFactory>();
			#endif

			Container = builder.Build();

			var csl = new AutofacServiceLocator(Container);
			ServiceLocator.SetLocatorProvider(() => csl);
		}
    }
}


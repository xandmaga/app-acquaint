using Acquaint.Abstractions;
using Acquaint.Common.iOS;
using Acquaint.Util;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using FFImageLoading.Forms.Touch;
using Foundation;
using HockeyApp.iOS;
using ImageCircle.Forms.Plugin.iOS;
using Microsoft.Practices.ServiceLocation;
using UIKit;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Acquaint.XForms.iOS
{
	[Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			var manager = BITHockeyManager.SharedHockeyManager;
			manager.Configure(Settings.HockeyAppId);
			manager.StartManager();

			RegisterDependencies();

			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

            Forms.Init();

			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			SQLitePCL.CurrentPlatform.Init();

            FormsMaps.Init();

			CachedImageRenderer.Init();

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

		/// <summary>
		/// Registers dependencies with an IoC container.
		/// </summary>
		/// <remarks>
		/// Since some of our libraries are shared between the Forms and Native versions 
		/// of this app, we're using an IoC/DI framework to provide access across implementations.
		/// </remarks>
		void RegisterDependencies()
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


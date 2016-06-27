using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin;
using ImageCircle.Forms.Plugin.iOS;
using HockeyApp;

namespace Acquaint.XForms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			//var manager = BITHockeyManager.SharedHockeyManager;
			//// Set the HockeyApp App Id here:
			//manager.Configure("11111111222222223333333344444444"); // This is just a dummy value. Replace with your real HockeyApp App ID
			//manager.StartManager();

			// If you would like to collect crash reports with Xamarin Insights, see Main.cs

			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

            Forms.Init();

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
    }
}


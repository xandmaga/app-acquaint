using Android.App;
using Android.Content.PM;
using Android.OS;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Xamarin;
using HockeyApp;

namespace Acquaint.XForms.Droid
{
	[Activity (Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	// inhertiting from FormsAppCompatActivity is imperative to taking advantage of Android AppCompat libraries
	{
		protected override void OnCreate (Bundle bundle)
		{
			// If you would like to collect crash reports with HockeyApp, do so here
			// CrashManager.Register(this, "11111111222222223333333344444444"); // This is just a dummy value. Replace with your real HockeyApp App ID

			// If you would like to collect crash reports with Xamarin Insights, do so here
			// Replace Insights.DebugModeKey with "[your Insights API key]"
			Insights.Initialize(Insights.DebugModeKey, this);

			// this line is essential to wiring up the toolbar styles defined in ~/Resources/layout/toolbar.axml
			FormsAppCompatActivity.ToolbarResource = Resource.Layout.toolbar;
			base.OnCreate (bundle);
			Forms.Init (this, bundle);
			FormsMaps.Init (this, bundle);
			LoadApplication (new App ());
		}
	}
}

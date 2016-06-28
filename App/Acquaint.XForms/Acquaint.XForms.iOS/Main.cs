using UIKit;
using Xamarin;

namespace Acquaint.XForms.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// If you would like to collect crash reports with Xamarin Insights, do so here
			// Replace Insights.DebugModeKey with "[your Insights API key]"
			Insights.Initialize(Insights.DebugModeKey);

			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}

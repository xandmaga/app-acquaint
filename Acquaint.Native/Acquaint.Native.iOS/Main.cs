using UIKit;
using Xamarin;

namespace Acquaint.Native.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
			// Initialize Insights
			// Replace Insights.DebugModeKey with "[your Insights API key]"
			Insights.Initialize(Insights.DebugModeKey);

            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}

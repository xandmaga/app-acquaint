using System;
using Acquaint.Data;
using Android.App;
using Android.OS;
using Android.Runtime;
using HockeyApp;
using Plugin.CurrentActivity;
using Xamarin;

namespace Acquaint.Native.Droid
{
	//You can specify additional application information in this attribute
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
		public static IDataSource<Acquaintance> AcquaintanceDataSource { get; private set; }

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
        }

        public override void OnCreate()
        {
			// Set the HockeyApp App Id here:
			CrashManager.Register(this, "11111111222222223333333344444444");

			// Initialize Insights
			// Replace Insights.DebugModeKey with "[your Insights API key]
			Insights.Initialize(Insights.DebugModeKey, this);

            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
			//A great place to initialize Xamarin.Insights and Dependency Services!

			AcquaintanceDataSource = new AcquaintanceDataSource();
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}
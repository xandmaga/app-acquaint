using System;
using Acquaint.Data;
using Acquaint.Models;
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
		public static IDataSource<IAcquaintance> DataSource { get; private set; }

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
        }

        public override void OnCreate()
        {
			// If you would like to collect crash reports with HockeyApp, do so here
			CrashManager.Register(this, "11111111222222223333333344444444"); // This is just a placeholder value. Replace with your real HockeyApp App ID

            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
			//A great place to initialize Xamarin.Insights and Dependency Services!

			DataSource = new AcquaintanceDataSource();
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
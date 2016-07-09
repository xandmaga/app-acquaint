using System;
using Acquaint.Data;
using Acquaint.Models;
using Android.App;
using Android.OS;
using Android.Runtime;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using HockeyApp;
using Microsoft.Practices.ServiceLocation;
using Plugin.CurrentActivity;
using Xamarin;

namespace Acquaint.Native.Droid
{
	//You can specify additional application information in this attribute
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
		public static IContainer Container { get; set; }

		public static IDataSource<Acquaintance> DataSource { get; private set; }

        public MainApplication(IntPtr handle, JniHandleOwnership transer) :base(handle, transer) { }

        public override void OnCreate()
        {
			// If you would like to collect crash reports with HockeyApp, do so here
			CrashManager.Register(this, "11111111222222223333333344444444"); // This is just a placeholder value. Replace with your real HockeyApp App ID

			RegisterDependencies();

			// Azure Mobile Services initilization
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            base.OnCreate();

            RegisterActivityLifecycleCallbacks(this);

			DataSource = new AzureAcquaintanceSource();
        }

		static void RegisterDependencies()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(new NullHttpClientHandlerFactory()).As<IHttpClientHandlerFactory>();

			Container = builder.Build();

			var csl = new AutofacServiceLocator(Container);
			ServiceLocator.SetLocatorProvider(() => csl);
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
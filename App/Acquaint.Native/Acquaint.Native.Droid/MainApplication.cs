using System;
using Acquaint.Abstractions;
using Acquaint.Common.Droid;
using Acquaint.Data;
using Acquaint.Util;
using Android.App;
using Android.OS;
using Android.Runtime;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using HockeyApp.Android;
using Microsoft.Practices.ServiceLocation;
using Plugin.CurrentActivity;

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
			CrashManager.Register(this, Settings.HockeyAppId);

			RegisterDependencies();

			// Azure Mobile Services initilization
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            base.OnCreate();

            RegisterActivityLifecycleCallbacks(this);

			DataSource = new AzureAcquaintanceSource();
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
			builder.RegisterInstance(new HttpClientHandlerFactory()).As<IHttpClientHandlerFactory>();

			var container = builder.Build();

			var csl = new AutofacServiceLocator(container);
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
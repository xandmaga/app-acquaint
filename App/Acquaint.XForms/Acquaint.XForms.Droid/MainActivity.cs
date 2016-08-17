using Acquaint.Abstractions;
using Acquaint.Common.Droid;
using Acquaint.Data;
using Acquaint.Models;
using Acquaint.Util;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using FFImageLoading.Forms.Droid;
using HockeyApp.Android;
using Microsoft.Practices.ServiceLocation;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Acquaint.XForms.Droid
{
	[Activity (Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	// inhertiting from FormsAppCompatActivity is imperative to taking advantage of Android AppCompat libraries
	{
		// an IoC Container
		IContainer _IoCContainer;

		protected override void OnCreate (Bundle bundle)
		{
			// register HockeyApp as the crash reporter
			CrashManager.Register(this, Settings.HockeyAppId);

			RegisterDependencies();

			Settings.OnDataPartitionPhraseChanged += (sender, e) => {
				UpdateDataSourceIfNecessary();
			};

			// Azure Mobile Services initilizatio
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			CachedImageRenderer.Init();

			// this line is essential to wiring up the toolbar styles defined in ~/Resources/layout/toolbar.axml
			FormsAppCompatActivity.ToolbarResource = Resource.Layout.toolbar;

			base.OnCreate (bundle);

			Forms.Init (this, bundle);

			FormsMaps.Init (this, bundle);

			LoadApplication (new App ());
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

			builder.RegisterInstance(new DatastoreFolderPathProvider()).As<IDatastoreFolderPathProvider>();

			builder.RegisterInstance(new DataSyncConflictMessagePresenter()).As<IDataSyncConflictMessagePresenter>();

			if (Settings.IsUsingLocalDataSource)
				builder.RegisterInstance(new FilesystemOnlyAcquaintanceDataSource()).As<IDataSource<Acquaintance>>();
			else
				builder.RegisterInstance(new AzureAcquaintanceSource()).As<IDataSource<Acquaintance>>();

			_IoCContainer = builder.Build();

			var csl = new AutofacServiceLocator(_IoCContainer);
			ServiceLocator.SetLocatorProvider(() => csl);
		}

		/// <summary>
		/// Updates the data source if necessary.
		/// </summary>
		void UpdateDataSourceIfNecessary()
		{
			var dataSource = ServiceLocator.Current.GetInstance<IDataSource<Acquaintance>>();

			// if the settings dictate that a local data source should be used, then register the local data provider and update the IoC container
			if (Settings.IsUsingLocalDataSource && !(dataSource is FilesystemOnlyAcquaintanceDataSource))
			{
				var builder = new ContainerBuilder();
				builder.RegisterInstance(new FilesystemOnlyAcquaintanceDataSource()).As<IDataSource<Acquaintance>>();
				builder.Update(_IoCContainer);
				return;
			}

			// if the settings dictate that a local data souce should not be used, then register the remote data source and update the IoC container
			if (!Settings.IsUsingLocalDataSource && !(dataSource is AzureAcquaintanceSource))
			{
				var builder = new ContainerBuilder();
				builder.RegisterInstance(new AzureAcquaintanceSource()).As<IDataSource<Acquaintance>>();
				builder.Update(_IoCContainer);
			}
		}
	}
}

using System;
using System.Threading.Tasks;
using Acquaint.Util;
using FFImageLoading;
using FFImageLoading.Cache;
using Xamarin.Forms;

namespace Acquaint.XForms
{
	public class SettingsViewModel : BaseNavigationViewModel
	{
		public string AzureAppServiceUrl { get; set; }
		public string DataPartitionPhrase { get; set; }
		public int ImageCacheDurationHours { get; set; }
		public bool ClearImageCache { get; set; }
		public bool ResetToDefaults { get; set; }

		public SettingsViewModel()
		{
			AzureAppServiceUrl = Settings.AzureAppServiceUrl;
			DataPartitionPhrase = Settings.DataSeedPhrase;
			ImageCacheDurationHours = Settings.ImageCacheDurationHours;
		}

		Command _CancelCommand;

		public Command CancelCommand => _CancelCommand ?? (_CancelCommand = new Command(async () => await ExecuteCancelCommand()));

		async Task ExecuteCancelCommand()
		{
			await PopModalAsync();
		}

		Command _SaveCommand;

		public Command SaveCommand => _SaveCommand ?? (_SaveCommand = new Command(async () => await ExecuteSaveCommand()));

		async Task ExecuteSaveCommand()
		{
			if (ResetToDefaults)
			{
				Settings.ResetUserConfigurableSettingsToDefaults();
				Settings.ClearImageCacheIsRequested = true;
			}
			else if (ClearImageCache)
			{
				Settings.ClearImageCacheIsRequested = true;
			}
			else
			{
				int localStoreResetConditions = 0;

				if (Settings.AzureAppServiceUrl.ToLower() != AzureAppServiceUrl.ToLower())
					localStoreResetConditions++;

				Settings.AzureAppServiceUrl = AzureAppServiceUrl;

				if (Settings.DataSeedPhrase.ToLower() != DataPartitionPhrase.ToLower())
					localStoreResetConditions++;

				Settings.DataSeedPhrase = DataPartitionPhrase;

				if (localStoreResetConditions > 0)
					Settings.LocalDataResetIsRequested = true;

				if (Settings.ImageCacheDurationHours != ImageCacheDurationHours || localStoreResetConditions > 0)
					Settings.ClearImageCacheIsRequested = true;
				
				Settings.ImageCacheDurationHours = ImageCacheDurationHours;
			}

			if (Settings.ClearImageCacheIsRequested)
			{
				await ImageService.Instance.InvalidateCacheAsync(CacheType.All);
			}

			await PopModalAsync();
		}
	}
}


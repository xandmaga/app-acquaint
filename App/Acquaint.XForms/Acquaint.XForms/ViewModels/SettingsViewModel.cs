using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Acquaint.Util;
using FFImageLoading;
using FFImageLoading.Cache;
using FormsToolkit;
using Xamarin.Forms;

namespace Acquaint.XForms
{
	public class SettingsViewModel : BaseNavigationViewModel
	{
		public string AzureAppServiceUrl { get; set; }
		public string DataPartitionPhrase { get; set; }
		public int ImageCacheDurationHours { get; set; }
		public bool ClearImageCache { get; set; }

		bool _ResetToDefaults;
		public bool ResetToDefaults
		{
			get { return _ResetToDefaults; }
			set
			{
				SetProperty(ref _ResetToDefaults, value);
				if (value)
				{
					ClearImageCache = value; // if the data is being refreshed, we should clear the image cache as well
					OnPropertyChanged(nameof(ClearImageCache)); // notify that ClearImageCache has been updated
				}
			}
		}

		public SettingsViewModel()
		{
			AzureAppServiceUrl = Settings.AzureAppServiceUrl;
			DataPartitionPhrase = Settings.DataPartitionPhrase;
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
			if (string.IsNullOrWhiteSpace(DataPartitionPhrase))
			{
				MessagingService.Current.SendMessage(MessageKeys.DataPartitionPhraseValidation);
				return;
			}

			Uri testUri;

			if (!Uri.TryCreate(AzureAppServiceUrl, UriKind.Absolute, out testUri))
			{
				MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.DisplayAlert, new MessagingServiceAlert()
				{ 
					Title = "Invalid URL", 
					Message = "Please enter a valid URL", 
					Cancel = "OK" });
				return;
			}

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

				if (Settings.DataPartitionPhrase.ToLower() != DataPartitionPhrase.ToLower())
					localStoreResetConditions++;

				Settings.DataPartitionPhrase = DataPartitionPhrase;

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


using Xamarin.Forms;
using System.Threading.Tasks;
using Acquaint.Util;

namespace Acquaint.XForms
{
	/// <summary>
	/// Splash Page that is used on Androd only. iOS splash characteristics are NOT defined here, ub tn the iOS prject settings.
	/// </summary>
	public partial class SplashPage : ContentPage
	{
		bool _ShouldDelayForSplash = true;

		public SplashPage()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_ShouldDelayForSplash)
				// delay for a few seconds on the splash screen
				await Task.Delay(3000);

			if (string.IsNullOrWhiteSpace(Settings.DataPartitionPhrase))
			{
				await Navigation.PushModalAsync(new NavigationPage(new SetupPage()));
				_ShouldDelayForSplash = false;
			}
			else
			{
				var navPage = new NavigationPage(new AcquaintanceListPage() { Title = "Acquaintances", BindingContext = new AcquaintanceListViewModel() });

				// on the main UI thread, set the MainPage to the navPage
				Device.BeginInvokeOnMainThread(() => {
					Application.Current.MainPage = navPage;
				});
			}
		}
	}
}


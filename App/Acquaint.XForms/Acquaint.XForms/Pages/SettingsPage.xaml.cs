using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Acquaint.XForms
{
	public partial class SettingsPage : ContentPage
	{
		protected SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

		public SettingsPage()
		{
			InitializeComponent();
		}
	}
}


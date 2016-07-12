using System;
using Acquaint.XForms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SettingsTableView), typeof(Acquaint.XForms.Droid.SettingsTableViewRenderer))]

namespace Acquaint.XForms.Droid
{
	public class SettingsTableViewRenderer : TableViewRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
				return;

			var listView = Control as global::Android.Widget.ListView;
			listView.DividerHeight = 0;
		}
	}
}


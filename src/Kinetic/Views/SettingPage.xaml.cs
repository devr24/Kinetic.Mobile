using MauiDemo.Presentation.Services;

namespace Kinetic.Presentation.Views;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
        SetDeviceInfo();

    }

    public void SetDeviceInfo()
    {
        var di = new DeviceInfoService();

        label_Email.Text = Preferences.Get("UserEmail","Unknown");
        label_name.Text = Preferences.Get("UserName", "Unknown");
        label_phone_name.Text = di.DeviceName;
        label_OS.Text = di.DevicePlatform.ToString();
        label_idom.Text = di.DeviceIdiom.ToString();
        label_manufacturer.Text = di.Manfacturer;
        label_model.Text = di.Model;
        label_version.Text = di.AppVersion;
    }
}
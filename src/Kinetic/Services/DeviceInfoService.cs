using System.Reflection;

namespace MauiDemo.Presentation.Services
{
    public class DeviceInfoService
    {
        public string DeviceName => DeviceInfo.Current.Name;

        public string Model => DeviceInfo.Current.Model;

        public string Manfacturer => DeviceInfo.Current.Manufacturer;

        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string OsVersion => DeviceInfo.Current.VersionString;

        public bool IsAndroid() => DeviceInfo.Current.Platform == Microsoft.Maui.Devices.DevicePlatform.Android;

        public DevicePlatform DevicePlatform => DeviceInfo.Current.Platform;

        public DeviceIdiom DeviceIdiom => DeviceInfo.Current.Idiom;

        public DisplayRotation DeviceRotation => Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Rotation;

        public DisplayOrientation DeviceOrientation => Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Orientation;

        public bool IsVirtualDevice = DeviceInfo.Current.DeviceType switch
        {
            DeviceType.Physical => false,
            DeviceType.Virtual => true,
            _ => false
        };

        public string ReadDeviceInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"Model: {DeviceInfo.Current.Model}");
            sb.AppendLine($"Manufacturer: {DeviceInfo.Current.Manufacturer}");
            sb.AppendLine($"Name: {DeviceInfo.Current.Name}");
            sb.AppendLine($"OS Version: {DeviceInfo.Current.VersionString}");
            sb.AppendLine($"Idiom: {DeviceInfo.Current.Idiom}");
            sb.AppendLine($"Platform: {DeviceInfo.Current.Platform}");

            bool isVirtual = DeviceInfo.Current.DeviceType switch
            {
                DeviceType.Physical => false,
                DeviceType.Virtual => true,
                _ => false
            };

            sb.AppendLine($"Virtual device: {isVirtual}");

            return sb.ToString();
        }

        private void PrintIdiom()
        {
            if (DeviceInfo.Current.Idiom == DeviceIdiom.Desktop)
                Console.WriteLine("The current device is a desktop");
            else if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
                Console.WriteLine("The current device is a phone");
            else if (DeviceInfo.Current.Idiom == DeviceIdiom.Watch)
                Console.WriteLine("The current device is a watch");
            else if (DeviceInfo.Current.Idiom == DeviceIdiom.TV)
                Console.WriteLine("The current device is a tv");
            else if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
                Console.WriteLine("The current device is a Tablet");
            else
                Console.WriteLine("Unknown device");
        }
    }
}

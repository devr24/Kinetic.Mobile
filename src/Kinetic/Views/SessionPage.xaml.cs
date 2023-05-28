using Kinetic.Presentation.Data.Entities;
using Kinetic.Presentation.Models;
using System.Globalization;

namespace Kinetic.Presentation.Views;

public partial class SessionPage : ContentPage
{

    public SessionPage()
	{
		InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        var sessions = await Database.Instance.GetAsync<SessionEntity>();
        List<SessionDisplayModel> list = sessions.OrderByDescending(t=> t.SessionStarted).Select(t => new SessionDisplayModel
        {
            Session = t.SessionStarted.ToString("dd/MM/yy HH:mm"),
            TotalTime = (int)t.TotalTime.TotalHours + t.TotalTime.ToString(@"\:mm\:ss"),
            Distance = $"{t.DistanceTravelledKm:0.0} km" ,
            Complete = t.Completed,
            TextColor = !t.Completed ? "Red": "Black"
        }).ToList();

        BindingContext = list;
        
        base.OnAppearing();
    }
}

public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = value.ToString();
        switch (color)
        {
            case "Red":
                return Colors.Red;
            default:
                return Colors.Black;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
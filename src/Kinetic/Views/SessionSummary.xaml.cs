using CommunityToolkit.Maui.Views;
using Kinetic.Presentation.Models;

namespace Kinetic.Presentation.Views;

public partial class SessionSummary : Popup
{
    private readonly SessionDataModel _sessionData;

    public SessionSummary(SessionDataModel sessionData)
    {
        _sessionData = sessionData;
        InitializeComponent();
        lbl_SessionId.Text = sessionData.SessionId.ToString();
        lbl_SessionStarted.Text = sessionData.SessionStarted.ToString("HH:mm:ss dd/MM/yy");
        lbl_SessionDuration.Text = $"{sessionData.TotalTime.Hours:00}:{sessionData.TotalTime.Minutes:00}:{sessionData.TotalTime.Seconds:00}";
        lbl_SessionDistance.Text = $"{sessionData.DistanceTravelledKm:0.0} km";
    }

    private void OkButton_OnTapped(object sender, TappedEventArgs e)
    {
         Close();//.PopModalAsync();
    }
}
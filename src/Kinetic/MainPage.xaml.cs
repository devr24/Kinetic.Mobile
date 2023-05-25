using Kinetic.Presentation.Services;

namespace Kinetic;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked1(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

    private DistanceTracker t = new();
    private int counter = 0;

    private void OnCounterClicked(object sender, EventArgs e)
    {
        t.StartTracking();

        t.TrackingCaptured += T_TrackingCaptured;



        //// Validate that UserNameEntry is not empty
        //if (string.IsNullOrEmpty(edi_name.Text) || !EmailValidationService.IsValidEmail(edi_email.Text))
        //{
        //    lbl_error.IsVisible = true;
        //    lbl_spacer.IsVisible = false;
        //    return;
        //}

        //lbl_error.IsVisible = false;
        //lbl_spacer.IsVisible = true;

        //// Save username to Preferences
        //Preferences.Set("UserName", edi_name.Text);
        //Preferences.Set("UserEmail", edi_email.Text);

        //// hide page
    }

    private void T_TrackingCaptured(object sender, DistanceTrackingEventArgs e)
    {
        Console.WriteLine($"Distance: {e.TrackingData.Distance} km, Duration: {e.TrackingData.Time}, Location: {e.TrackingData.Location}, Accelerometer: {e.TrackingData.AccelerometerReading}");
        if (counter == 100)
        {

            t.StopTracking();

        }

        counter++;
    }
}


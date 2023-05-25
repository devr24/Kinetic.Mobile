using System.Drawing;

namespace Kinetic.Presentation.View;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void SaveButtonClick(object sender, TappedEventArgs e)
    {
		//navigate to main page
		await Shell.Current.GoToAsync("//" + "Home/" + nameof(MainPage));
    }

    private void edi_email_Focused(object sender, FocusEventArgs e)
    {
        line_email.BackgroundColor = Colors.Turquoise;
    }

    private void edi_email_Unfocused(object sender, FocusEventArgs e)
    {
        line_email.BackgroundColor = Colors.Gray;
    }

    private void edi_name_Focused(object sender, FocusEventArgs e)
    {
        line_name.BackgroundColor = Colors.Turquoise;
    }

    private void edi_name_Unfocused(object sender, FocusEventArgs e)
    {
        line_name.BackgroundColor = Colors.Gray;
    }
}
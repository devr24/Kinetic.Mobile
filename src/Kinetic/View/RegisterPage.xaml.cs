using Kinetic.Presentation.Services;

namespace Kinetic.Presentation.View;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private void SaveButtonClick(object sender, TappedEventArgs e)
    {
        // Validate that UserNameEntry is not empty
        if (string.IsNullOrEmpty(edi_name.Text) || !EmailValidationService.IsValidEmail(edi_email.Text))
        {
            lbl_error.IsVisible = true;
            return;
        }

        lbl_error.IsVisible = false;

        // Save username to Preferences
        Preferences.Set("UserName", edi_name.Text);
        Preferences.Set("UserEmail", edi_email.Text);

        // hide page
    }
}
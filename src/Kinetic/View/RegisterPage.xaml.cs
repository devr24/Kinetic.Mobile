using Kinetic.Presentation.Services;

namespace Kinetic.Presentation.View;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}

    private void SaveButtonClick(object sender, TappedEventArgs e)
    {
        // Validate that UserNameEntry is not empty
        if (string.IsNullOrEmpty(edi_name.Text) || !EmailValidationService.IsValidEmail(edi_email.Text))
        {
            lbl_error.IsVisible = true;
            lbl_spacer.IsVisible = false;
            return;
        }

        lbl_error.IsVisible = false;
        lbl_spacer.IsVisible = true;

        // Save username to Preferences
        Preferences.Set("UserName", edi_name.Text);
        Preferences.Set("UserEmail", edi_email.Text);

        // hide page
    }
}
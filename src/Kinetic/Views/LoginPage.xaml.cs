using Kinetic.Presentation.Services;

namespace Kinetic.Presentation.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();

        //var username = Preferences.Get("UserName", null);
        //var email = Preferences.Get("UserEmail", null);

        //if (username != null && email != null)
        //{
        //    Shell.Current.GoToAsync("//Home/" + nameof(MainPage)).GetAwaiter().GetResult();
        //}
    }

    private async void SaveButtonClick(object sender, TappedEventArgs e)
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

        //navigate to main page
        await Shell.Current.GoToAsync("//Home/" + nameof(MainPage));
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
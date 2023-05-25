using Kinetic.Presentation.Models;

namespace Kinetic.Presentation.View;

public partial class SessionPage : ContentPage
{
	public SessionPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        List<TestModels> list = new List<TestModels>()
        {
            new TestModels(),
            new TestModels(),
            new TestModels(),
            new TestModels(),
            new TestModels()
        };

        BindingContext = list;
        base.OnAppearing();
    }
}
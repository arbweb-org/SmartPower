namespace SmartPower.Client;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new MainPage()) { Title = "المصحف المرتل" };
		window.Width = 400;
		window.Height = 800;
		window.MaximumWidth = 400;
		window.MinimumWidth = 400;
		window.MinimumHeight = 400;

		// get screen size
		var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
		var screenWidth = displayInfo.Width / displayInfo.Density;
		var screenHeight = displayInfo.Height / displayInfo.Density;

		// move window to center of screen
		window.X = (screenWidth - window.Width) / 2;
		window.Y = (screenHeight - window.Height) / 2;

		return window;
	}
}

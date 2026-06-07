



namespace gadifff.Mobile;

// SEARCH INDEX
// MAUI, MOBILE, APP, WINDOW, MAINPAGE, STARTUP
//
// Topic: MAUI APPLICATION ROOT
// Purpose: Loads shared resources from App.xaml and opens MainPage as the first screen.
// Search keywords: MAUI MOBILE APP WINDOW MAINPAGE STARTUP
// When to use it: Show this after MauiProgram when explaining how the mobile UI appears.
// Important notes: This project uses MainPage directly, not AppShell navigation, for the current Android app.
public partial class App : Application
{
	// Topic: App resource initialization
	// Purpose: Loads App.xaml resources, including merged style dictionaries.
	// Search keywords: MAUI APP STYLE RESOURCE
	// When to use it: Use when explaining why MainPage can use StaticResource styles.
	// Important notes: No API call happens here.
	public App()
	{
		InitializeComponent();
	}

	// Topic: First mobile window
	// Purpose: Creates the first app window and places MainPage inside it.
	// Search keywords: MAUI WINDOW MAINPAGE STARTUP
	// When to use it: Use when explaining where the visible MAUI screen starts.
	// Important notes: MainPage then chooses the server URL and handles all user actions/API calls.
	// FLOW_MAUI_STARTUP_03: App.CreateWindow opens MainPage as the first visible MAUI screen.
	// This file is involved because it connects startup to the UI; next step is MainPage constructor and server URL setup.
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage());
	}
}

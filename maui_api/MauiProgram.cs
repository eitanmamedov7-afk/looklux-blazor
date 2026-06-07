



using Microsoft.Extensions.Logging;

namespace gadifff.Mobile;

// SEARCH INDEX
// MAUI, MOBILE, STARTUP, ANDROID, APP, CONFIG
//
// Topic: MAUI APP STARTUP
// Purpose: Creates the MAUI application object, registers the root App class, fonts, and debug logging.
// Search keywords: MAUI MOBILE STARTUP ANDROID APP CONFIG
// When to use it: Show this first when explaining how the MAUI app starts before any API call happens.
// Important notes: Android MainApplication calls CreateMauiApp(), then App creates MainPage.
public static class MauiProgram
{
	// Topic: MAUI app builder
	// Purpose: Builds the native MAUI app and connects it to App.xaml/App.xaml.cs.
	// Search keywords: MAUI STARTUP APP ANDROID FONT DEBUG
	// When to use it: Use when explaining the first startup step of the mobile project.
	// Important notes: This does not call gadifff API directly; it only prepares the mobile app runtime.
	// FLOW_MAUI_STARTUP_02: MauiProgram.CreateMauiApp registers App as the root MAUI application.
	// This file is involved because Android startup must create a MauiApp; next step is App.CreateWindow opening MainPage.
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

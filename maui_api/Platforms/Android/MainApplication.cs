



using Android.App;
using Android.Runtime;

namespace gadifff.Mobile;

// SEARCH INDEX
// ANDROID, MAUI, MOBILE, STARTUP, APP
//
// Topic: ANDROID APPLICATION ENTRY
// Purpose: Android creates this application object, then asks MauiProgram to build the MAUI app.
// Search keywords: ANDROID MAUI MOBILE STARTUP APP
// When to use it: Show this as the first Android-specific startup file.
// Important notes: This is used because the project targets net9.0-android.
[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	// Topic: Android to MAUI bridge
	// Purpose: Hands Android startup over to the shared MAUI startup builder.
	// Search keywords: ANDROID MAUI STARTUP APP
	// When to use it: Use when tracing app launch from Android into shared MAUI code.
	// Important notes: Next file in the startup flow is MauiProgram.cs.
	// FLOW_MAUI_STARTUP_01: Android MainApplication calls MauiProgram.CreateMauiApp.
	// This file is involved because Android starts here; next step is MauiProgram building the MAUI app.
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}





using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace gadifff.Mobile;

// SEARCH INDEX
// ANDROID, MAUI, MOBILE, ACTIVITY, KEYBOARD, STARTUP
//
// Topic: ANDROID MAIN ACTIVITY
// Purpose: Native Android activity that hosts the MAUI UI and controls keyboard resizing behavior.
// Search keywords: ANDROID MAUI MOBILE ACTIVITY KEYBOARD STARTUP
// When to use it: Show this when explaining emulator launch or why the keyboard should not cover inputs.
// Important notes: WindowSoftInputMode.AdjustResize helps text fields stay usable with the emulator keyboard.
// FLOW_MAUI_STARTUP_04: Android MainActivity hosts the MainPage UI after the MAUI app is built.
// This file is involved because Android needs an Activity to display MAUI pages; next step is user interaction in MainPage.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, WindowSoftInputMode = SoftInput.AdjustResize, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}

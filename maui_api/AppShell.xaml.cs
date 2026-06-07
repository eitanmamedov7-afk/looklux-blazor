



namespace gadifff.Mobile;

// SEARCH INDEX
// MAUI, MOBILE, SHELL, TEMPLATE, UNUSED
//
// Topic: MAUI SHELL TEMPLATE
// Purpose: Template Shell navigation class that is compiled but not used as the current first screen.
// Search keywords: MAUI SHELL TEMPLATE UNUSED
// When to use it: Mention only if asked why AppShell exists.
// Important notes: App.CreateWindow currently opens MainPage directly, so user/admin/API flows do not pass through AppShell.
public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}
}

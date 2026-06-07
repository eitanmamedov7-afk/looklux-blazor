// SEARCH INDEX
// MAUI, MOBILE, API, LOGIN, REGISTER, PASSWORD, CLOSET, GARMENT, UPLOAD, MATCH, OUTFIT, ADMIN, DELETE, LIST
//
// Topic: MAUI MAIN PAGE CODE-BEHIND
// Purpose: Handles mobile UI events and calls LookLuxApiClient for all backend work.
// Search keywords: MAUI MOBILE API LOGIN REGISTER PASSWORD CLOSET GARMENT UPLOAD MATCH OUTFIT ADMIN DELETE LIST
// When to use it: Show this when explaining how the MAUI app uses the Blazor backend through API calls.
// Important notes: This file should not access SQL directly; it uses LookLuxApiClient.

using Models;

namespace gadifff.Mobile;

public partial class MainPage : ContentPage
{
    // SECTION: MAUI STATE API UI
    // Topic: Mobile page state
    // Purpose: Tracks signed-in user, active viewed customer, loaded garments/outfits, and current tab.
    // Search keywords: MAUI STATE USER ADMIN LIST
    // When to use it: Show this when explaining how mobile screens switch without separate pages.
    // Important notes: _activeDataUserId is used when admin views a customer.
    private LookLuxApiClient? _api;
    private MobileUserDto? _currentUser;
    private string? _activeDataUserId;
    private string? _activeDataUserLabel;
    private List<Garment> _garments = new();
    private List<Outfit> _outfits = new();
    private string _activeTab = "home";

    // Topic: MainPage startup
    // Purpose: Initializes UI defaults and chooses the first backend API URL candidate.
    // Search keywords: MAUI STARTUP SERVER API MAINPAGE
    // When to use it: Show this when explaining what happens as soon as the MAUI screen opens.
    // Important notes: The chosen URL is passed to LookLuxApiClient; no login/API request has happened yet.
    // FLOW_MAUI_STARTUP_05: MainPage constructor initializes the first visible MAUI screen.
    // This file is involved because all user actions start from MainPage; next step is user tapping a button.
    // FLOW_MAUI_SERVER_CONNECT_03: MainPage chooses the first server candidate and calls SetApiServer.
    // This file is involved because the app must know the gadifff API base URL before login/register/forgot-password calls.
    public MainPage()
    {
        InitializeComponent();

        AdminRolePicker.ItemsSource = new List<string> { "customer", "admin" };
        AdminRolePicker.SelectedIndex = 0;

        var firstServer = ServerEndpointProvider.GetStartupCandidates().FirstOrDefault()
            ?? new Uri(ServerEndpointProvider.GetDefaultUrl());
        SetApiServer(firstServer);
    }

    // Topic: API server selection
    // Purpose: Stores the backend base URL in LookLuxApiClient and displays it in the mobile header.
    // Search keywords: MAUI SERVER API URL CLIENT
    // When to use it: Show this when explaining how MAUI knows where gadifff API is running.
    // Important notes: Android emulator usually uses 10.0.2.2:7166 for the PC-hosted web/API server.
    // FLOW_MAUI_SERVER_CONNECT_04: SetApiServer creates/updates LookLuxApiClient with the selected backend URL.
    // This file is involved because all later mobile flows call gadifff through this API client.
    private void SetApiServer(Uri serverUri)
    {
        _api ??= new LookLuxApiClient(serverUri);
        _api.SetBaseUri(serverUri);
        ServerUrlEntry.Text = serverUri.ToString();
        ServerLabel.Text = serverUri.ToString();
    }

    // Topic: Mobile login button
    // Purpose: Sends email/password to the mobile login API and enters signed-in state.
    // Search keywords: MAUI LOGIN API VALIDATE USER
    // When to use it: Show this when explaining mobile login.
    // Important notes: Login result comes from Program.cs /api/mobile/auth/login.
    // FLOW_LOGIN_MOBILE_02: OnLoginClicked reads email/password from MAUI fields and calls LookLuxApiClient.LoginAsync.
    // This file is involved because it owns the mobile button event; next step is LookLuxApiClient sending HTTP.
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            var result = await RequireApi().LoginAsync(EmailEntry.Text ?? "", PasswordEntry.Text ?? "");
            if (result?.Success != true || result.User == null)
            {
                ShowMessage(result?.Message ?? "Invalid email or password.", true);
                return;
            }

            await SignInAsync(result.User);
        });
    }

    // Topic: Mobile register button
    // Purpose: Validates register form values, checks password confirmation, and calls the register API.
    // Search keywords: MAUI REGISTER API VALIDATE PASSWORD USER
    // When to use it: Show this when explaining mobile account creation.
    // Important notes: The server still owns duplicate-email checks and password hashing.
    // FLOW_REGISTER_MOBILE_02: OnRegisterClicked validates the two MAUI password fields before any API call.
    // This file is involved because the double-password check is a UI responsibility; next step is LookLuxApiClient.RegisterAsync.
    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            var password = RegisterPasswordEntry.Text ?? "";
            var confirmPassword = RegisterConfirmPasswordEntry.Text ?? "";
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ShowMessage("Passwords do not match.", true);
                return;
            }

            var result = await RequireApi().RegisterAsync(
                RegisterNameEntry.Text ?? "",
                RegisterEmailEntry.Text ?? "",
                password);

            if (result?.Success != true || result.User == null)
            {
                ShowMessage(result?.Message ?? "Registration failed.", true);
                return;
            }

            await SignInAsync(result.User);
        });
    }

    // Topic: Mobile forgot-password button
    // Purpose: Validates the login email field and asks the backend to send a reset email.
    // Search keywords: MAUI PASSWORD RESET EMAIL API
    // When to use it: Show this when explaining password recovery from the MAUI app.
    // Important notes: MAUI does not send email; it only calls the backend API.
    // FLOW_PASSWORD_RESET_MOBILE_02: OnForgotPasswordClicked validates the email and calls LookLuxApiClient.ForgotPasswordAsync.
    // This file is involved because the reset process starts from the MAUI login screen; next step is LookLuxApiClient.ForgotPasswordAsync.
    private async void OnForgotPasswordClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            var email = (EmailEntry.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowMessage("Enter your email first.", true);
                return;
            }

            var result = await RequireApi().ForgotPasswordAsync(email);
            ShowMessage(result?.Message ?? "Could not send reset email.", result?.Success != true);
        });
    }

    private async Task SignInAsync(MobileUserDto user)
    {
        _currentUser = user;
        _activeDataUserId = null;
        _activeDataUserLabel = null;
        AuthPanel.IsVisible = false;
        AppPanel.IsVisible = true;
        AdminTabButton.IsVisible = IsAdmin;
        CurrentUserLabel.Text = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
        CurrentRoleLabel.Text = $"{user.Email} - {user.Role}";
        ShowMessage("Signed in.", false);
        await LoadAllAsync();
        ShowTab("home");
    }

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        ResetToSignedOutHome("Signed out.");
    }

    // Topic: Mobile delete account
    // Purpose: Confirms account deletion and calls the mobile delete-account API.
    // Search keywords: MAUI DELETE ACCOUNT API REMOVE USER
    // When to use it: Show this when explaining account deletion from the app.
    // Important notes: Server handles cascade deletion; this method only confirms and calls API.
    // FLOW_DELETE_ACCOUNT_MOBILE_02: OnDeleteAccountClicked shows the warning confirmation before deleting anything.
    // This file is involved because destructive actions need user confirmation; next step is LookLuxApiClient.DeleteAccountAsync.
    private async void OnDeleteAccountClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            if (_currentUser == null)
                return;

            var confirm = await DisplayAlert(
                "Delete account",
                "This will permanently delete your account, garments, and saved outfits. This cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            var ok = await RequireApi().DeleteAccountAsync(_currentUser.UserId);
            if (!ok)
            {
                ShowMessage("Could not delete account.", true);
                return;
            }

            ResetToSignedOutHome("Account deleted.");
        });
    }

    private void ResetToSignedOutHome(string message)
    {
        _currentUser = null;
        _activeDataUserId = null;
        _activeDataUserLabel = null;
        _garments.Clear();
        _outfits.Clear();
        GarmentList.Clear();
        OutfitList.Clear();
        MatchResultsList.Clear();
        AdminUserList.Clear();
        AdminTabButton.IsVisible = false;
        CurrentUserLabel.Text = string.Empty;
        CurrentRoleLabel.Text = string.Empty;
        MatchRequestEditor.Text = string.Empty;
        ShowTab("home");
        AppPanel.IsVisible = false;
        AuthPanel.IsVisible = true;
        PasswordEntry.Text = "";
        ShowMessage(message, false);
    }

    private void OnServerClicked(object? sender, EventArgs e)
    {
        ServerPanel.IsVisible = !ServerPanel.IsVisible;
    }

    private void OnSaveServerClicked(object? sender, EventArgs e)
    {
        if (!ServerEndpointProvider.SaveCustomUrl(ServerUrlEntry.Text, out var saved) || saved == null)
        {
            ShowMessage("Invalid server URL.", true);
            return;
        }

        SetApiServer(saved);
        ServerPanel.IsVisible = false;
        ShowMessage("Server saved.", false);
    }

    private async void OnRetryServerClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            if (_currentUser != null)
                await LoadAllAsync();
            ShowMessage("Server reachable.", false);
        });
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(LoadAllAsync);
    }

    private async void OnHomeTabClicked(object? sender, EventArgs e) => await ShowOwnDataTabAsync("home");

    private async void OnClosetTabClicked(object? sender, EventArgs e) => await ShowOwnDataTabAsync("closet");

    private async void OnMatchTabClicked(object? sender, EventArgs e) => await ShowOwnDataTabAsync("match");

    private async void OnOutfitsTabClicked(object? sender, EventArgs e) => await ShowOwnDataTabAsync("outfits");

    private async void OnAdminTabClicked(object? sender, EventArgs e)
    {
        ShowTab("admin");
        await RunSafeAsync(LoadAdminUsersAsync);
    }

    private void ShowTab(string tab)
    {
        _activeTab = tab;
        HomePanel.IsVisible = tab == "home";
        ClosetPanel.IsVisible = tab == "closet";
        MatchPanel.IsVisible = tab == "match";
        OutfitsPanel.IsVisible = tab == "outfits";
        AdminPanel.IsVisible = tab == "admin" && IsAdmin;
    }

    private async Task ShowOwnDataTabAsync(string tab)
    {
        if (_currentUser != null && !string.IsNullOrWhiteSpace(_activeDataUserId))
        {
            _activeDataUserId = null;
            _activeDataUserLabel = null;
            ShowAdminCustomerList();
            await RunSafeAsync(LoadAllAsync);
        }

        ShowTab(tab);
    }

    // Topic: Mobile load closet/outfits
    // Purpose: Loads current user's or admin-selected customer's garments and saved outfits through API.
    // Search keywords: MAUI API CLOSET OUTFIT LIST ADMIN
    // When to use it: Show this when explaining how mobile screens get data.
    // Important notes: ActiveUserId controls own data vs admin-viewed customer data.
    // FLOW_CLOSET_VIEW_MOBILE_02: LoadAllAsync asks the API for the active user's or viewed customer's garments.
    // This file is involved because it controls which user data the mobile closet displays; next step is LookLuxApiClient.GetClosetAsync.
    // FLOW_OUTFIT_VIEW_MOBILE_02: LoadAllAsync asks the API for the active user's or viewed customer's saved outfits.
    // This file is involved because it controls which user data the mobile outfit screen displays; next step is LookLuxApiClient.GetOutfitsAsync.
    private async Task LoadAllAsync()
    {
        if (_currentUser == null)
            return;

        _garments = await RequireApi().GetClosetAsync(_currentUser.UserId, ActiveUserId);
        _outfits = await RequireApi().GetOutfitsAsync(_currentUser.UserId, ActiveUserId);
        CurrentRoleLabel.Text = ActiveUserId == _currentUser.UserId
            ? $"{_currentUser.Email} - {_currentUser.Role}"
            : $"Viewing {_activeDataUserLabel ?? "customer"}";
        await LoadDashboardAsync();
        RenderGarments();
        RenderOutfits();
        RenderAdminCustomerData();

        if (_activeTab == "admin" && IsAdmin && string.IsNullOrWhiteSpace(_activeDataUserId))
            await LoadAdminUsersAsync();
    }

    private async Task LoadDashboardAsync()
    {
        // Topic: Mobile dashboard load
        // Purpose: Calls the dashboard API and writes returned counts into the MAUI home labels.
        // Search keywords: MAUI DASHBOARD COUNT API ADMIN
        // When to use it: Show this when explaining home/admin dashboard totals in MAUI.
        // Important notes: Admins receive global counts; customers receive their own garment/outfit totals.
        // FLOW_DASHBOARD_COUNTS_MOBILE_02: LoadDashboardAsync asks LookLuxApiClient for dashboard counts.
        // This file is involved because the Home screen displays the returned totals; next step is LookLuxApiClient.GetDashboardAsync.
        if (_currentUser == null)
            return;

        var dashboard = await RequireApi().GetDashboardAsync(_currentUser.UserId);
        if (dashboard == null)
            return;

        DashboardGarmentsLabel.Text = dashboard.GarmentCount.ToString();
        DashboardOutfitsLabel.Text = dashboard.OutfitCount.ToString();
        DashboardCustomersLabel.Text = dashboard.CustomerCount.ToString();
        DashboardAdminsLabel.Text = dashboard.AdminCount.ToString();
        DashboardCustomersGroup.IsVisible = dashboard.IsAdmin;
        DashboardAdminsGroup.IsVisible = dashboard.IsAdmin;

        HomeSummaryLabel.Text = dashboard.IsAdmin
            ? "Admin dashboard data is loaded from the same database counts used by the web home page."
            : "Your closet and saved outfit totals are loaded through the API.";
    }

    // Topic: Mobile garment upload
    // Purpose: Picks an image, converts it to base64, and sends it to the backend upload API.
    // Search keywords: MAUI UPLOAD GARMENT IMAGE API ADD
    // When to use it: Show this when explaining mobile garment creation.
    // Important notes: AI analysis and DB insert happen on the backend, not inside MAUI.
    // FLOW_GARMENT_UPLOAD_MOBILE_02: OnUploadClicked opens the phone file picker, reads image bytes, and prepares base64.
    // This file is involved because MAUI owns device image selection; next step is LookLuxApiClient.CreateGarmentAsync.
    private async void OnUploadClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            if (_currentUser == null)
                return;

            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose garment image",
                FileTypes = FilePickerFileType.Images
            });

            if (file == null)
                return;

            var bytes = await ReadFileBytesAsync(file);
            if (bytes.Length > 6 * 1024 * 1024)
            {
                ShowMessage("Image too large (max 6MB).", true);
                return;
            }

            var mimeType = ResolveMimeType(file.FileName);

            var response = await RequireApi().CreateGarmentAsync(new MobileGarmentCreateRequest(
                ActiveUserId,
                Convert.ToBase64String(bytes),
                mimeType,
                file.FileName,
                Type: null,
                AnalyzeWithAi: true));

            if (response?.Success == true)
            {
                ShowMessage("Garment added.", false);
                await LoadAllAsync();
                return;
            }

            if (response?.RequiresManualType == true)
            {
                ShowMessage("AI could not detect whether this is a shirt, pants, or shoes. Try a clearer photo.", true);
                return;
            }

            ShowMessage(response?.Message ?? "Upload failed.", true);
        });
    }

    private async void OnRunMatchClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            if (_currentUser == null)
                return;

            await RunRecommendationAsync(Array.Empty<string>(), allowNoSeed: true);
        });
    }

    // Topic: Mobile recommendation run
    // Purpose: Calls the backend matcher API and renders returned outfit suggestions.
    // Search keywords: MAUI MATCH RECOMMENDATION API OUTFIT LIST
    // When to use it: Show this when explaining mobile recommendations.
    // Important notes: MatchingService on the server owns scoring and minimum garment validation.
    // FLOW_MATCH_RUN_MOBILE_02: RunRecommendationAsync sends selected seed ids/request text to the match API.
    // This file is involved because it starts recommendations from the mobile Match tab or garment card; next step is LookLuxApiClient.RunMatchAsync.
    private async Task RunRecommendationAsync(string[] seedGarmentIds, bool allowNoSeed)
    {
        MatchResultsList.Clear();
        ShowTab("match");

        var result = await RequireApi().RunMatchAsync(new MobileMatchRequest(
            ActiveUserId,
            SeedGarmentIds: seedGarmentIds,
            AllowNoSeed: allowNoSeed,
            MatchRequestEditor.Text));

        if (result == null)
        {
            ShowMessage("Matching failed.", true);
            return;
        }

        if (result.Blocked)
        {
            var shirts = result.Counts.GetValueOrDefault("shirt");
            var pants = result.Counts.GetValueOrDefault("pants");
            var shoes = result.Counts.GetValueOrDefault("shoes");
            ShowMessage($"Matching needs at least {result.MinPerTypeRequired} shirts, pants, and shoes. Current: {shirts}/{pants}/{shoes}.", true);
            return;
        }

        if (!result.Success || result.Results.Count == 0)
        {
            ShowMessage(result.NoEligibleMessage ?? result.ErrorMessage ?? "No matching outfits found.", true);
            return;
        }

        foreach (var suggestion in result.Results)
            MatchResultsList.Add(BuildMatchCard(suggestion));

        ShowMessage(seedGarmentIds.Length > 0 ? "Recommendations ready for this garment." : "Matches ready.", false);
    }

    private async void OnCreateUserClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            if (_currentUser == null || !IsAdmin)
                return;

            var created = await RequireApi().CreateUserAsync(new MobileUserCreateRequest(
                _currentUser.UserId,
                AdminNameEntry.Text ?? "",
                AdminEmailEntry.Text ?? "",
                AdminPasswordEntry.Text ?? "",
                AdminRolePicker.SelectedItem?.ToString() ?? "customer"));

            if (created == null)
            {
                ShowMessage("Could not create user.", true);
                return;
            }

            AdminNameEntry.Text = "";
            AdminEmailEntry.Text = "";
            AdminPasswordEntry.Text = "";
            AdminRolePicker.SelectedIndex = 0;
            ShowMessage("User created.", false);
            await LoadAdminUsersAsync();
        });
    }

    private View BuildMatchCard(ScoredCombination suggestion)
    {
        var card = CreateCard();
        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(new Label { Text = $"{suggestion.StyleLabel} - {suggestion.Score}%", Style = GetStyle("SectionTitle") });
        stack.Add(new Label { Text = suggestion.Explanation, Style = GetStyle("MutedText") });
        stack.Add(new Label { Text = suggestion.RecommendedPlaces, Style = GetStyle("MutedText") });
        stack.Add(BuildImageStrip(new[] { suggestion.ShirtId, suggestion.PantsId, suggestion.ShoesId }));

        var save = new Button { Text = "Save outfit" };
        save.Clicked += async (_, _) => await RunSafeAsync(async () =>
        {
            if (_currentUser == null)
                return;

            // FLOW_MATCH_SAVE_MOBILE_01: User taps Save outfit on a MAUI recommendation card.
            // This file packages the selected recommendation; next step is LookLuxApiClient.SaveMatchAsync.
            var result = await RequireApi().SaveMatchAsync(new MobileSaveMatchRequest(
                ActiveUserId,
                suggestion,
                SeedGarmentIds: Array.Empty<string>()));

            ShowMessage(result?.Message ?? "Save finished.", result?.Success != true && result?.Duplicate != true);
            await LoadAllAsync();
        });
        stack.Add(save);
        card.Content = stack;
        return card;
    }

    private void RenderGarments()
    {
        GarmentList.Clear();
        if (_garments.Count == 0)
        {
            GarmentList.Add(EmptyText("No garments yet."));
            return;
        }

        foreach (var garment in _garments.OrderByDescending(x => x.CreatedAt))
            GarmentList.Add(BuildGarmentCard(garment, includeActions: true));
    }

    private View BuildGarmentCard(Garment garment, bool includeActions)
    {
        var card = CreateCard();
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 92 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 12
        };

        grid.Add(new Image
        {
            Source = ImageSource.FromUri(RequireApi().BuildAbsoluteUri($"/media/garments/by-garment/{garment.GarmentId}")),
            Aspect = Aspect.AspectFill,
            HeightRequest = 92,
            WidthRequest = 92
        });

        var info = new VerticalStackLayout { Spacing = 4 };
        info.Add(new Label { Text = DisplayType(garment.Type), Style = GetStyle("SectionTitle") });
        info.Add(new Label { Text = GarmentMeta(garment), Style = GetStyle("MutedText") });
        if (includeActions)
        {
            // FLOW_MATCH_RUN_MOBILE_01: User taps Get recommendations on one garment card; next step is RecommendFromGarmentAsync.
            var recommend = new Button { Text = "Get recommendations" };
            recommend.Clicked += async (_, _) => await RecommendFromGarmentAsync(garment);
            info.Add(recommend);
            // FLOW_DELETE_GARMENT_MOBILE_01: User taps Delete on a garment card; next step is DeleteGarmentAsync confirmation.
            var delete = new Button { Text = "Delete", Style = GetStyle("SecondaryButton") };
            delete.Clicked += async (_, _) => await DeleteGarmentAsync(garment);
            info.Add(delete);
        }
        grid.Add(info, 1);

        card.Content = grid;
        return card;
    }

    private async Task RecommendFromGarmentAsync(Garment garment)
    {
        if (_currentUser == null)
            return;

        await RunSafeAsync(async () =>
        {
            await RunRecommendationAsync(new[] { garment.GarmentId }, allowNoSeed: false);
        });
    }

    private async Task DeleteGarmentAsync(Garment garment)
    {
        if (_currentUser == null)
            return;

        var confirm = await DisplayAlert("Delete garment", "Delete this garment?", "Delete", "Cancel");
        if (!confirm)
            return;

        await RunSafeAsync(async () =>
        {
            var ok = await RequireApi().DeleteGarmentAsync(ActiveUserId, garment.GarmentId);
            ShowMessage(ok ? "Garment deleted." : "Could not delete garment.", !ok);
            await LoadAllAsync();
        });
    }

    private void RenderOutfits()
    {
        OutfitList.Clear();
        if (_outfits.Count == 0)
        {
            OutfitList.Add(EmptyText("No saved outfits yet."));
            return;
        }

        foreach (var outfit in _outfits.OrderByDescending(x => x.CreatedAt))
            OutfitList.Add(BuildOutfitCard(outfit, includeActions: true));
    }

    private View BuildOutfitCard(Outfit outfit, bool includeActions)
    {
        var card = CreateCard();
        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(new Label { Text = $"{outfit.StyleLabel ?? "Outfit"} - {outfit.Score}%", Style = GetStyle("SectionTitle") });
        stack.Add(new Label { Text = outfit.Explanation ?? "", Style = GetStyle("MutedText") });
        stack.Add(BuildImageStrip(new[] { outfit.ShirtGarmentId, outfit.PantsGarmentId, outfit.ShoesGarmentId }));

        if (includeActions)
        {
            // FLOW_DELETE_OUTFIT_MOBILE_01: User taps Delete outfit; next step is DeleteOutfitAsync confirmation.
            var delete = new Button { Text = "Delete outfit", Style = GetStyle("SecondaryButton") };
            delete.Clicked += async (_, _) => await DeleteOutfitAsync(outfit);
            stack.Add(delete);
        }
        card.Content = stack;
        return card;
    }

    private async Task DeleteOutfitAsync(Outfit outfit)
    {
        if (_currentUser == null)
            return;

        var confirm = await DisplayAlert("Delete outfit", "Delete this saved outfit?", "Delete", "Cancel");
        if (!confirm)
            return;

        await RunSafeAsync(async () =>
        {
            var ok = await RequireApi().DeleteOutfitAsync(ActiveUserId, outfit.OutfitId);
            ShowMessage(ok ? "Outfit deleted." : "Could not delete outfit.", !ok);
            await LoadAllAsync();
        });
    }

    // Topic: Mobile admin user list
    // Purpose: Loads all users for admin management/viewing through the API.
    // Search keywords: MAUI ADMIN USER LIST API
    // When to use it: Show this when explaining admin features in the mobile app.
    // Important notes: Only admin users should reach this screen.
    // FLOW_ADMIN_USER_MANAGE_MOBILE_02: LoadAdminUsersAsync requests all users for the Admin screen.
    // This file is involved because it renders admin user cards; next step is LookLuxApiClient.GetUsersAsync.
    private async Task LoadAdminUsersAsync()
    {
        if (_currentUser == null || !IsAdmin)
            return;

        AdminUserList.Clear();
        if (string.IsNullOrWhiteSpace(_activeDataUserId))
            ShowAdminCustomerList();
        var users = await RequireApi().GetUsersAsync(_currentUser.UserId);
        foreach (var user in users)
            AdminUserList.Add(BuildAdminUserCard(user));
    }

    private View BuildAdminUserCard(MobileUserDto user)
    {
        var card = CreateCard();
        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(new Label { Text = user.Email, Style = GetStyle("SectionTitle") });
        stack.Add(new Label { Text = $"{user.FullName} - {user.Role}", Style = GetStyle("MutedText") });

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 8
        };

        // FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_01: Admin taps View data on a customer card; next step is ViewUserDataAsync.
        var viewData = new Button { Text = "View data", Style = GetStyle("SecondaryButton") };
        viewData.Clicked += async (_, _) => await ViewUserDataAsync(user);
        row.Add(viewData);

        var edit = new Button { Text = "Edit", Style = GetStyle("SecondaryButton") };
        edit.Clicked += async (_, _) => await EditUserAsync(user);
        row.Add(edit, 1);

        var delete = new Button { Text = "Delete", Style = GetStyle("SecondaryButton"), IsEnabled = !IsCurrentUser(user.UserId) };
        delete.Clicked += async (_, _) => await DeleteUserAsync(user);
        row.Add(delete, 2);
        stack.Add(row);
        card.Content = stack;
        return card;
    }

    private async Task EditUserAsync(MobileUserDto user)
    {
        if (_currentUser == null)
            return;

        var name = await DisplayPromptAsync("Edit user", "Full name", initialValue: user.FullName);
        if (name == null) return;
        var email = await DisplayPromptAsync("Edit user", "Email", initialValue: user.Email, keyboard: Keyboard.Email);
        if (email == null) return;
        var role = await DisplayActionSheet("Role", "Cancel", null, "customer", "admin");
        if (role == null || role == "Cancel") return;

        await RunSafeAsync(async () =>
        {
            await RequireApi().UpdateUserAsync(user.UserId, new MobileUserUpdateRequest(_currentUser.UserId, name, email, role));
            ShowMessage("User updated.", false);
            await LoadAdminUsersAsync();
        });
    }

    // Topic: Mobile admin view customer data
    // Purpose: Stores the selected customer and switches the Admin screen into read-only garment/outfit detail mode.
    // Search keywords: MAUI ADMIN CUSTOMER CLOSET OUTFIT LIST
    // When to use it: Show this when explaining how admins view customer data without leaving the Admin screen.
    // Important notes: This does not switch to the admin's own Closet/Outfits tabs.
    // FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_02: ViewUserDataAsync stores the selected customer id and opens the Admin detail panel.
    // This file is involved because the app must stay in the customer window; next step is LoadAllAsync using the customer id.
    private async Task ViewUserDataAsync(MobileUserDto user)
    {
        if (_currentUser == null || !IsAdmin)
            return;

        _activeDataUserId = user.UserId;
        _activeDataUserLabel = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
        ShowTab("admin");
        ShowAdminCustomerDetail();
        ShowMessage($"Viewing data for {_activeDataUserLabel}.", false);
        await RunSafeAsync(LoadAllAsync);
    }

    private async void OnBackToAdminCustomersClicked(object? sender, EventArgs e)
    {
        await RunSafeAsync(async () =>
        {
            _activeDataUserId = null;
            _activeDataUserLabel = null;
            AdminCustomerGarmentList.Clear();
            AdminCustomerOutfitList.Clear();
            ShowAdminCustomerList();
            await LoadAllAsync();
            await LoadAdminUsersAsync();
            ShowMessage("Back to customers.", false);
        });
    }

    private void ShowAdminCustomerList()
    {
        AdminCustomerListPanel.IsVisible = true;
        AdminCustomerDetailPanel.IsVisible = false;
    }

    private void ShowAdminCustomerDetail()
    {
        AdminCustomerListPanel.IsVisible = false;
        AdminCustomerDetailPanel.IsVisible = true;
        AdminCustomerDetailTitle.Text = string.IsNullOrWhiteSpace(_activeDataUserLabel)
            ? "Customer data"
            : _activeDataUserLabel;
    }

    private void RenderAdminCustomerData()
    {
        // Topic: Mobile admin read-only customer rendering
        // Purpose: Draws the selected customer's garments and outfits inside the Admin detail panel.
        // Search keywords: MAUI ADMIN CUSTOMER CLOSET OUTFIT LIST
        // When to use it: Show this when explaining the final admin customer-view screen.
        // Important notes: includeActions is false, so admins view customer garments/outfits but do not delete them here.
        // FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_04: RenderAdminCustomerData draws read-only customer garments and outfits inside Admin.
        // This file is involved because it owns MAUI UI rendering; final step is the admin sees data or presses Back.
        if (!IsAdmin || string.IsNullOrWhiteSpace(_activeDataUserId) || !AdminCustomerDetailPanel.IsVisible)
            return;

        AdminCustomerGarmentList.Clear();
        if (_garments.Count == 0)
        {
            AdminCustomerGarmentList.Add(EmptyText("No garments yet."));
        }
        else
        {
            foreach (var garment in _garments.OrderByDescending(x => x.CreatedAt))
                AdminCustomerGarmentList.Add(BuildGarmentCard(garment, includeActions: false));
        }

        AdminCustomerOutfitList.Clear();
        if (_outfits.Count == 0)
        {
            AdminCustomerOutfitList.Add(EmptyText("No saved outfits yet."));
        }
        else
        {
            foreach (var outfit in _outfits.OrderByDescending(x => x.CreatedAt))
                AdminCustomerOutfitList.Add(BuildOutfitCard(outfit, includeActions: false));
        }
    }

    private async Task DeleteUserAsync(MobileUserDto user)
    {
        if (_currentUser == null)
            return;

        var confirm = await DisplayAlert("Delete user", $"Delete {user.Email} and related data?", "Delete", "Cancel");
        if (!confirm)
            return;

        await RunSafeAsync(async () =>
        {
            var ok = await RequireApi().DeleteUserAsync(_currentUser.UserId, user.UserId);
            ShowMessage(ok ? "User deleted." : "Could not delete user.", !ok);
            await LoadAdminUsersAsync();
        });
    }

    private Grid BuildImageStrip(IEnumerable<string?> garmentIds)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 8,
            HeightRequest = 110
        };

        var ids = garmentIds.ToList();
        for (var i = 0; i < 3; i++)
        {
            var id = ids.ElementAtOrDefault(i);
            var image = new Image
            {
                Aspect = Aspect.AspectFill,
                BackgroundColor = Color.FromArgb("#262626")
            };

            if (!string.IsNullOrWhiteSpace(id))
                image.Source = ImageSource.FromUri(RequireApi().BuildAbsoluteUri($"/media/garments/by-garment/{id}"));

            grid.Add(image, i);
        }

        return grid;
    }

    private Border CreateCard() => new()
    {
        Style = GetStyle("AppCard")
    };

    private Label EmptyText(string text) => new()
    {
        Text = text,
        Style = GetStyle("MutedText"),
        HorizontalTextAlignment = TextAlignment.Center,
        Margin = new Thickness(0, 12)
    };

    private Style GetStyle(string key) => (Style)Application.Current!.Resources[key];

    private LookLuxApiClient RequireApi() =>
        _api ?? throw new InvalidOperationException("Server is not configured.");

    private bool IsAdmin =>
        string.Equals(_currentUser?.Role, "admin", StringComparison.OrdinalIgnoreCase);

    private string ActiveUserId =>
        !string.IsNullOrWhiteSpace(_activeDataUserId)
            ? _activeDataUserId
            : _currentUser?.UserId ?? string.Empty;

    private bool IsCurrentUser(string userId) =>
        string.Equals(_currentUser?.UserId, userId, StringComparison.OrdinalIgnoreCase);

    private async Task RunSafeAsync(Func<Task> action)
    {
        BusyIndicator.IsVisible = true;
        BusyIndicator.IsRunning = true;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, true);
        }
        finally
        {
            BusyIndicator.IsRunning = false;
            BusyIndicator.IsVisible = false;
        }
    }

    private void ShowMessage(string message, bool isError)
    {
        MessageLabel.Text = message;
        MessageLabel.TextColor = isError ? Color.FromArgb("#F87171") : Color.FromArgb("#86EFAC");
        MessageLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
    }

    private static async Task<byte[]> ReadFileBytesAsync(FileResult file)
    {
        await using var stream = await file.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private static string ResolveMimeType(string? fileName)
    {
        var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }

    private static string DisplayType(string? value)
    {
        var type = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(type) ? "Garment" : char.ToUpperInvariant(type[0]) + type[1..];
    }

    private static string GarmentMeta(Garment garment)
    {
        var parts = new[]
        {
            garment.Color,
            garment.StyleCategory,
            garment.Season,
            garment.Occasion,
            garment.Brand
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var text = string.Join(" / ", parts);
        return string.IsNullOrWhiteSpace(text) ? "No extra details" : text;
    }
}

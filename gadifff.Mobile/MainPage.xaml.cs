namespace gadifff.Mobile;

public partial class MainPage : ContentPage
{
    private Uri? _currentUri;
    private readonly Queue<Uri> _startupCandidates = new();
    private bool _hasLoadedSuccessfully;

    public MainPage()
    {
        InitializeComponent();

        foreach (var candidate in ServerEndpointProvider.GetStartupCandidates())
        {
            _startupCandidates.Enqueue(candidate);
        }

        if (!NavigateToNextCandidate())
        {
            ShowConnectionError("No valid startup URL found. Enter a server URL.");
        }
    }

    private void OnRetryClicked(object? sender, EventArgs e)
    {
        ShowLoadingState();
        AppWebView.Reload();
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        ShowLoadingState();
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success)
        {
            _hasLoadedSuccessfully = true;
            HideOverlay();
            return;
        }

        if (!_hasLoadedSuccessfully && NavigateToNextCandidate())
        {
            return;
        }

        ShowConnectionError("Cannot connect to the server. Check the URL and confirm backend is running.");
    }

    private void OnSaveServerUrlClicked(object? sender, EventArgs e)
    {
        if (!ServerEndpointProvider.SaveCustomUrl(ServerUrlEntry.Text, out var savedUri) || savedUri is null)
        {
            ShowConnectionError("Invalid URL. Use format like http://192.168.1.33:7164.");
            return;
        }

        _currentUri = savedUri;
        ServerUrlEntry.Text = savedUri.ToString();
        NavigateTo(savedUri);
    }

    private void NavigateTo(Uri? uri)
    {
        if (uri == null)
        {
            ShowConnectionError("Invalid startup URL configuration. Enter a working server URL.");
            return;
        }

        _currentUri = uri;
        ServerUrlEntry.Text = uri.ToString();
        ShowLoadingState();
        AppWebView.Source = uri.ToString();
    }

    private bool NavigateToNextCandidate()
    {
        while (_startupCandidates.Count > 0)
        {
            var candidate = _startupCandidates.Dequeue();
            NavigateTo(candidate);
            return true;
        }

        return false;
    }

    private void ShowLoadingState()
    {
        Overlay.IsVisible = true;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        RetryButton.IsVisible = false;
        SaveServerUrlButton.IsVisible = false;
        ServerUrlEntry.IsVisible = false;
        OverlayMessage.Text = "Loading...";
    }

    private void ShowConnectionError(string message)
    {
        Overlay.IsVisible = true;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        RetryButton.IsVisible = true;
        SaveServerUrlButton.IsVisible = true;
        ServerUrlEntry.IsVisible = true;

        var current = _currentUri?.ToString() ?? "(not set)";
        OverlayMessage.Text = $"{message}\nCurrent URL: {current}\n\nAndroid emulator: http://10.0.2.2:7164\nReal phone: use your PC LAN IP (http://192.168.x.x:7164) or adb reverse + http://127.0.0.1:7164.";
    }

    private void HideOverlay()
    {
        Overlay.IsVisible = false;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        RetryButton.IsVisible = false;
        SaveServerUrlButton.IsVisible = false;
        ServerUrlEntry.IsVisible = false;
        OverlayMessage.Text = string.Empty;
    }
}

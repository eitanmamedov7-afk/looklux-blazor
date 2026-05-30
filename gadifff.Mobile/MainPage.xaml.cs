// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// מה הקובץ עושה: הקובץ שיך לאפליקציית המובייל ומגדיר מסך, אתחול או התאמה לפלטפורמה.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר לפרויקט האטר, להגדרי המובייל, לדף הראוי ולכטובת השרת.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים בדף הראוי של המובייל ובקוד שמוחליט לאיזה כטובת שרת להתחבר.



// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace gadifff.Mobile;

// הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
public partial class MainPage : ContentPage
{
    private Uri? _currentUri;
    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private readonly Queue<Uri> _startupCandidates = new();
    private bool _hasLoadedSuccessfully;

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    public MainPage()
    {
        InitializeComponent();

        // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
        foreach (var candidate in ServerEndpointProvider.GetStartupCandidates())
        {
            _startupCandidates.Enqueue(candidate);
        }

        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!NavigateToNextCandidate())
        {
            ShowConnectionError("No valid startup URL found. Enter a server URL.");
        }
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private void OnRetryClicked(object? sender, EventArgs e)
    {
        ShowLoadingState();
        AppWebView.Reload();
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        ShowLoadingState();
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (e.Result == WebNavigationResult.Success)
        {
            _hasLoadedSuccessfully = true;
            HideOverlay();
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return;
        }

        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!_hasLoadedSuccessfully && NavigateToNextCandidate())
        {
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return;
        }

        ShowConnectionError("Cannot connect to the server. Check the URL and confirm backend is running.");
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private void OnSaveServerUrlClicked(object? sender, EventArgs e)
    {
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!ServerEndpointProvider.SaveCustomUrl(ServerUrlEntry.Text, out var savedUri) || savedUri is null)
        {
            ShowConnectionError("Invalid URL. Use format like http://192.168.1.33:7166.");
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return;
        }

        _currentUri = savedUri;
        ServerUrlEntry.Text = savedUri.ToString();
        NavigateTo(savedUri);
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private void NavigateTo(Uri? uri)
    {
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (uri == null)
        {
            ShowConnectionError("Invalid startup URL configuration. Enter a working server URL.");
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return;
        }

        _currentUri = uri;
        ServerUrlEntry.Text = uri.ToString();
        ShowLoadingState();
        AppWebView.Source = uri.ToString();
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private bool NavigateToNextCandidate()
    {
        // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
        while (_startupCandidates.Count > 0)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var candidate = _startupCandidates.Dequeue();
            NavigateTo(candidate);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return true;
        }

        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return false;
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
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

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private void ShowConnectionError(string message)
    {
        Overlay.IsVisible = true;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        RetryButton.IsVisible = true;
        SaveServerUrlButton.IsVisible = true;
        ServerUrlEntry.IsVisible = true;

        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var current = _currentUri?.ToString() ?? "(not set)";
        OverlayMessage.Text = $"{message}\nCurrent URL: {current}\n\nAndroid emulator: http://10.0.2.2:7166\nReal phone: use your PC LAN IP (http://192.168.x.x:7166).\nWith adb reverse, http://127.0.0.1:7166 also works.";
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
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

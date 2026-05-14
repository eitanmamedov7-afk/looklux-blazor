# מדריך בעברית לפרויקט (פשוט וברור ממבט ראשון)

## 1) מה הפרויקט עושה
מערכת לניהול ארון בגדים והתאמת אאוטפיטים:
- משתמש נרשם/מתחבר.
- מעלה פריטי לבוש.
- מתקבלת חלוקה לתכונות (AI).
- המערכת מציעה התאמות אאוטפיט.
- אפשר לשמור התאמות.
- יש פאנל אדמין לניהול משתמשים.
- יש תהליך איפוס סיסמה דרך אימייל.

---

## 2) זרימה מלאה (End-to-End)
1. הדפדפן מגיע ל־`App.razor` ואז `Routes.razor`.
2. `MainLayout.razor` טוען את `NavMenu.razor`.
3. דפי התחברות/הרשמה משתמשים ב־`IAuthService` (`AuthService` בפועל).
4. דף `Closet` טוען פריטים מה־DB דרך `GarmentDB`, שומר תמונות דרך `GarmentImageDB`, ומנתח תכונות דרך `GarmentFeatureService` → `GeminiClient`.
5. רכיב `SlotMachineMatch` מפעיל `MatchingService` שמייצר/מדרג התאמות ושומר אותן (`OutfitDB` + `OutfitGarmentDB`).
6. דף `Outfits` מציג, מסנן ומוחק אאוטפיטים.
7. איפוס סיסמה: `ForgotPassword` → `AuthService.SendPasswordResetEmailAsync` → `SmtpEmailSender`; אח"כ `ResetPassword` מעדכן hash ב־`UserDB`.

---

## 3) מפת קבצים – כל קובץ ומה הוא עושה

## `gadifff` (אפליקציית Web Blazor)
- `gadifff/Program.cs`  
  אתחול האפליקציה: DI, Auth, API endpoints, SMTP options, קריאת קונפיג.
- `gadifff/Components/App.razor`  
  מעטפת HTML ראשית, טעינת CSS ו־`Routes`.
- `gadifff/Components/Routes.razor`  
  Router מרכזי לכל ה־pages.
- `gadifff/Components/_Imports.razor`  
  ייבוא namespaces גלובליים לרכיבים.

### Layout
- `gadifff/Components/Layout/MainLayout.razor`  
  מבנה עמוד: סיידבר + תוכן.
- `gadifff/Components/Layout/MainLayout.razor.css`  
  עיצוב למבנה הכללי.
- `gadifff/Components/Layout/NavMenu.razor`  
  ניווט דינמי לפי סטטוס משתמש/אדמין.
- `gadifff/Components/Layout/NavMenu.razor.css`  
  עיצוב התפריט.

### Shared Components
- `gadifff/Components/Shared/MultiSelectFilter.razor`  
  קומפוננטה רב־בחירה עם חיפוש וצ'יפים.
- `gadifff/Components/Shared/SlotMachineMatch.razor`  
  רכיב התאמות אינטראקטיבי: seed, הרצה, תוצאות, שמירה.

### Pages
- `gadifff/Components/Pages/Home.razor`  
  דף בית (משתמש רגיל/אדמין).
- `gadifff/Components/Pages/Login.razor`  
  התחברות.
- `gadifff/Components/Pages/Register.razor`  
  הרשמה.
- `gadifff/Components/Pages/Logout.razor`  
  יציאה מהמערכת.
- `gadifff/Components/Pages/ForgotPassword.razor`  
  בקשת מייל איפוס סיסמה.
- `gadifff/Components/Pages/ResetPassword.razor`  
  קביעת סיסמה חדשה מטוקן.
- `gadifff/Components/Pages/Closet.razor`  
  ניהול ארון, העלאות, סינונים, פתיחת מנוע התאמות.
- `gadifff/Components/Pages/Outfits.razor`  
  תצוגת אאוטפיטים שמורים, סינונים, מחיקה, התאמה מחדש.
- `gadifff/Components/Pages/AdminClosets.razor`  
  ניהול משתמשים (אדמין): יצירה, עריכה, מחיקה.
- `gadifff/Components/Pages/AdminClosetUser.razor`  
  צפייה/ניהול ארון ואאוטפיטים של משתמש ספציפי.
- `gadifff/Components/Pages/AdminUsers.razor`  
  דף redirect היסטורי ל־AdminClosets.
- `gadifff/Components/Pages/AdminCloset.razor`  
  דף redirect `/admin/closet-guid/{id}` אל `/admin/closet/{id}`.
- `gadifff/Components/Pages/AccessDenied.razor`  
  הודעת חוסר הרשאה.
- `gadifff/Components/Pages/Error.razor`  
  עמוד שגיאה כללי.

### Services
- `gadifff/Services/IAuthService.cs`  
  חוזה פעולות auth.
- `gadifff/Services/AuthService.cs`  
  מימוש auth: login/register/logout + reset password.
- `gadifff/Services/PasswordResetEmailStatus.cs`  
  סטטוסים לתוצאת שליחת איפוס סיסמה.
- `gadifff/Services/IEmailSender.cs`  
  חוזה לשליחת אימייל.
- `gadifff/Services/SmtpEmailSender.cs`  
  מימוש SMTP אמיתי (host/port/user/pass).
- `gadifff/Services/SmtpOptions.cs`  
  מודל קונפיג SMTP.
- `gadifff/Services/GeminiClient.cs`  
  לקוח HTTP ל־Gemini.
- `gadifff/Services/GarmentFeatureService.cs`  
  הפקת JSON תכונות מפריט לבוש מתוך תמונה.
- `gadifff/Services/MatchingService.cs`  
  לוגיקת התאמות אאוטפיט, ניקוד, שמירה, fallback.

### Static
- `gadifff/wwwroot/app.css`  
  עיצוב מותאם אפליקציה.
- `gadifff/wwwroot/bootstrap/bootstrap.min.css`  
  קובץ vendor (Bootstrap) – לא עורכים ידנית.

## `ConsoleApp2` (Models)
- `ConsoleApp2/User.cs` – מודל משתמש.
- `ConsoleApp2/Garment.cs` – מודל פריט לבוש.
- `ConsoleApp2/Outfit.cs` – מודל אאוטפיט.
- `ConsoleApp2/OutfitGarment.cs` – קשר פריט↔אאוטפיט.
- `ConsoleApp2/Style.cs` – מודל סגנון.
- `ConsoleApp2/Recommendation.cs` – מודל המלצה.
- `ConsoleApp2/UserMatchingState.cs` – מצב lockout/דרישות התאמה למשתמש.
- `ConsoleApp2/FilterModels.cs` – בקשות/אפשרויות סינון + normalize.
- `ConsoleApp2/Program.cs` – placeholder בלבד.

## `ConsoleApp1` (Data Access / DBL)
- `ConsoleApp1/DB.cs` – בסיס חיבור MySQL ו־command/reader.
- `ConsoleApp1/BaseDB.cs` – CRUD generic + בניית SQL פרמטרי.
- `ConsoleApp1/UserDB.cs` – גישת נתוני משתמשים.
- `ConsoleApp1/StyleDB.cs` – גישת נתוני סגנונות.
- `ConsoleApp1/GarmentDB.cs` – גישת נתוני ארון (פריטים + פילטרים).
- `ConsoleApp1/GarmentImageDB.cs` – שמירה/שליפה של תמונות בינאריות.
- `ConsoleApp1/OutfitDB.cs` – גישת אאוטפיטים + פילטרים + תאימות סכמות.
- `ConsoleApp1/OutfitGarmentDB.cs` – גישת טבלת קשר אאוטפיט־פריטים.
- `ConsoleApp1/UserMatchingStateDB.cs` – מצב התאמות פר משתמש (upsert).
- `ConsoleApp1/Pogram.cs` – placeholder בלבד.

---

## 4) הסבר פונקציות ובלוקים (בעברית, לפי קובץ)

## `gadifff/Program.cs`
### בלוקי קוד
- **Configuration**: טוען `appsettings`, `secrets`, משתני סביבה.
- **DI Registration**: רושם DB classes, Services, Auth, SMTP.
- **Middleware**: `UseStaticFiles`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `UseAntiforgery`.
- **API Endpoints**: media image endpoint + closet/outfits filter endpoints.
- **Razor Components**: מיפוי UI.

### פונקציות
- `LogGarmentPromptConfiguration`  
  מסביר בלוגים מאיפה נטען פרומפט ניתוח תמונה.
- `LogGeminiConfiguration`  
  מסביר בלוגים אם API key ל־Gemini קיים.
- `ResolvePromptPath`  
  ממיר path יחסי למלא.
- `ParseGarmentFilterRequest` / `ParseOutfitFilterRequest`  
  בונה מודלי פילטר מה־query string.
- `ReadList`  
  קורא רשימות מה־query, מנקה duplicates ורווחים.
- `ApplyLegacySmtpOverrides`  
  ממפה גם פורמט env ישן (`SMTP_*`) אל `SmtpOptions`.
- `FirstNonEmpty`  
  מחזיר הערך הלא ריק הראשון.
- `TryParseBool`  
  parse לבוליאני כולל `yes/no/on/off/1/0`.

## `gadifff/Services/AuthService.cs`
### למה הקובץ קיים
מרכז את כל הלוגיקה של משתמשים והתחברות.

### פונקציות
- `NotifyAuthChanged`  
  שולח event ל־UI שהמשתמש השתנה.
- `LoginAsync`  
  מאמת אימייל+סיסמה מול hash.
- `LogoutAsync`  
  מנקה user נוכחי.
- `RegisterAsync`  
  יוצר משתמש חדש עם hash.
- `SendPasswordResetEmailAsync`  
  בודק אם אימייל קיים, מייצר טוקן מוגן לזמן מוגבל, ושולח לינק.
- `ResetPasswordAsync`  
  מאמת טוקן + תוקף, ומעדכן hash חדש.
- `CurrentUserAsync`  
  מחזיר את המשתמש הנוכחי בזיכרון.

## `gadifff/Services/SmtpEmailSender.cs`
- `SendAsync`  
  בונה `MailMessage`, פותח `SmtpClient`, שולח אימייל, ומחזיר הצלחה/כישלון.

## `gadifff/Services/GeminiClient.cs`
### זרימה
1. בודק שיש API Key.  
2. שולח בקשת `generateContent`.  
3. אם model לא קיים – fallback ל־`gemini-2.5-flash`.  
4. מחלץ text מה־JSON response.

### פונקציות
- `GenerateAsync` – orchestration מלא של קריאה ל־Gemini.
- `SendRequestAsync` – POST HTTP עם payload.
- `BuildParts` – בונה חלקי prompt + image base64.
- `NormalizeModel` – מנקה/מנרמל שם מודל.
- `ResolveApiKey` – מחפש key במשתני סביבה נתמכים.
- `Shorten` – קיצור טקסט ללוגים.

## `gadifff/Services/GarmentFeatureService.cs`
### זרימה
1. טוען prompt (Config/קובץ/Built-in).  
2. שולח ל־Gemini.  
3. מנקה תשובה ל־JSON נקי.  
4. מאמת JSON; אם לא תקין זורק שגיאה.

### פונקציות
- `ExtractFromImageAsync` – ניתוח תמונה לתכונות JSON.
- `LoadPromptAsync` – בחירת מקור פרומפט.
- `NormalizeJson` – ניקוי markdown/code fences.
- `ResolvePromptPath` – path לקובץ פרומפט.

## `gadifff/Services/MatchingService.cs` (קובץ גדול)
### בלוק 1: State/Gate
- `GetOrInitStateAsync`, `CanMatchAsync`, `RecordFailureAsync`, `RecordSuccessAsync`  
  מנהל תנאי סף למשתמש לפני התאמה.

### בלוק 2: התאמה ראשית
- `FindMatchesAsync`  
  בונה seed + pools, מייצר קומבינציות, מדרג, מחזיר Top תוצאות.

### בלוק 3: שמירה
- `SaveOutfitSuggestionAsync` + `BuildOutfitGarments`  
  שומר תוצאת התאמה לטבלאות outfits/outfit_garments.

### בלוק 4: יצירת קומבינציות וניקוד
- `BuildCombinations`, `ScoreCombinationsAsync`, `LoadScoringPromptAsync`, `BuildMatchPrompt`  
  מכין מועמדים ושולח ל־Gemini לציון.

### בלוק 5: Parsing/Normalization
- `ParseScoringOutcome`, `ParseRecommendationArray`, `TryParseRecommendationItem` ועוד  
  מפענח JSON מה־AI ומגן מפני פורמטים שונים.

### בלוק 6: Heuristics/Fallback
- `ComputePromptPriority`, `ComputePromptFitScore`, `BuildFallbackRecommendation` ועוד  
  חישוב fallback כש־AI לא מחזיר תוצאה טובה.

### בלוק 7: Utilities
- `NormalizeTypeName`, `IsType`, `MergeGarments`, `SplitToTokens` וכו'.  
  פונקציות עזר ללוגיקה ולניקוי נתונים.

## `gadifff/Components/Shared/MultiSelectFilter.razor`
- `IsSelected` – האם ערך נבחר.
- `ToggleOpen` – פותח/סוגר panel.
- `ClearSelection` – מאפס בחירה.
- `RemoveOption` – מסיר צ'יפ מסוים.
- `ToggleOption` – מסמן/מוריד option מרשימת הנבחרים.

## `gadifff/Components/Shared/SlotMachineMatch.razor` (קובץ גדול)
### בלוק UI וסנכרון
- `OnParametersSetAsync`, `BuildSyncSignature` – מסנכרן מצב לפי פריטים/seed.
- `RenderSlot` – בניית UI slot (shirt/pants/shoes).

### בלוק בחירת seed ותנועה
- `SetSeed`, `SetExclusiveSeed`, `IsSeedSelected`, `SelectedSeedCount`.
- `Cycle` + `Prev/Next` לכל סוג.

### בלוק הרצה
- `RunMatch` – מריץ התאמה דרך `MatchingService`.
- `RunProgressAsync` – אנימציית התקדמות.

### בלוק תוצאות ושמירה
- `ToggleSuggestion`, `IsSuggestionSaveDisabled`, `GetSuggestionSaveButtonText`, `SaveSuggestionAsync`.

### עזר
- `FindGarment`, `NormalizeTypeName`, `GetImageUrl`, `Mod`.

## דפי Auth קצרים
- `Login.razor`  
  `HandleLoginAsync`, `GoToRegister`.
- `Register.razor`  
  `HandleRegisterAsync`, `GoToLogin`.
- `Logout.razor`  
  `OnInitializedAsync` (logout אוטומטי + redirect).
- `ForgotPassword.razor`  
  `HandleSendAsync`, `GoToLogin`.
- `ResetPassword.razor`  
  `OnParametersSet`, `HandleResetAsync`, `GoToLogin`.

## דפי Admin
- `AdminClosets.razor`  
  בלוקים: טעינה, יצירה, עריכה, מחיקה, dirty-check, הודעות UI.  
  פונקציות: `ReloadUsersAsync`, `CreateUserAsync`, `SaveUserAsync`, `DeleteUserAsync` ועוד.
- `AdminClosetUser.razor`  
  בלוקים: טעינת משתמש יעד, מחיקת פריט/אאוטפיט, דיאלוג אישור, ניווט.  
  פונקציות: `LoadTargetDataAsync`, `DeleteGarmentAsync`, `DeleteOutfitAsync`, `ConfirmDeleteDialogAsync` ועוד.
- `AdminCloset.razor`, `AdminUsers.razor`  
  Redirect בלבד.

## דפי עבודה גדולים
- `Closet.razor`  
  בלוקים:
  - טעינת משתמש וקונטקסט אדמין (`ResolveActiveUserContextAsync`)
  - העלאת קובץ וניתוח (`OnFileChange`, `UploadFromFileAsync`, `PersistGarmentAsync`)
  - fallback ידני (`PrepareManualFallback`, `SaveManualAsync`)
  - ניהול סינונים (`BuildGarmentFilterRequest`, `RefreshClosetFiltersAsync`, `On*Changed`, drawer/group methods)
  - התאמות וחלונות (`OpenMatcher`, `ReloadAfterMatch`, preview methods)
  - מחיקה (`ConfirmDeleteGarmentDialog`, `DeleteGarmentConfirmedAsync`)
- `Outfits.razor`  
  בלוקים:
  - טעינה + קונטקסט (`LoadData`, `ResolveActiveUserContextAsync`)
  - סינונים (`BuildRequest`, `RefreshOutfitFiltersAsync`, `On*Changed`, drawer/group methods)
  - פעולות אאוטפיט (`FindFromOutfit`, `DeleteOutfitConfirmedAsync`, `Reload`)
  - חלונות עזר (`OpenBestRecommendationWindow`, matcher/preview methods)
- `Home.razor`  
  בלוקים:
  - טעינת dashboard לאדמין (`LoadAdminDashboardAsync`)
  - בניית נקודות גרף/שימוש (`BuildUsagePoints`)
  - אינטראקציית hover/select (`SelectUsagePoint`, `HoverUsagePoint`, `ClearUsageHover`)

## `ConsoleApp1` (DB Access)
### `DB.cs`
- constructor: יוצר connection/command reader בסיסיים.

### `BaseDB.cs`
#### בלוק CRUD
- `SelectAllAsync` (3 overloads), `InsertAsync`, `InsertGetObjAsync`, `UpdateAsync`, `DeleteAsync`.
#### בלוק בניית SQL
- `PrepareWhereQueryWithParameters`, `PrepareUpdateQueryWithParameters`, `PrepareInsertQueryWithParameters`.
#### בלוק הרצה
- `ExecNonQueryAsync`, `ExecScalarAsync`, `StringListSelectAllAsync`, `StingListSelectAllAsync` (תאימות לאחור).
#### בלוק תשתית
- `AddParameterToCommand`, `PreQueryAsync`, `PostQueryAsync`.

### `UserDB.cs`
- get/create/update/delete/count + `UpdatePasswordHashAsync` לאיפוס סיסמה.

### `StyleDB.cs`
- קריאה/יצירה של style.

### `UserMatchingStateDB.cs`
- `GetByUserAsync`, `UpsertAsync` למצב התאמה.

### `GarmentImageDB.cs`
- `CreateAsync` לשמירת image bytes.
- `GetLatestByGarmentIdAsync` לשליפת תמונה להצגה.

### `GarmentDB.cs` (גדול)
- בלוק mapping סכמות שונות (`CreateModelAsync` עם fallback לפי מספר עמודות).
- CRUD בסיסי (`GetByUserAsync`, `GetByIdAsync`, `CreateAsync`, `DeleteGarmentAsync`).
- בלוק סטטיסטיקות (`CountByUserRoleAsync` וכו').
- בלוק פילטרים (`GetFilteredByUserAsync`, `GetFilterOptionsAsync` + query helpers).
- בלוק עזרי parsing/normalization.

### `OutfitDB.cs` (גדול)
- בלוק mapping סכמות legacy/modern (`CreateModelAsync`).
- CRUD (`GetAllAsync`, `GetByUserAsync`, `GetByIdAsync`, `DeleteOutfitAsync`, `CreateAsync`).
- בדיקת כפילויות (`ExistsDuplicateAsync`).
- פילטרים + אפשרויות (`GetFilteredByUserAsync`, `GetFilterOptionsAsync`).
- תאימות סכמות דינמית (בדיקת columns בזמן ריצה).

### `OutfitGarmentDB.cs`
- כתיבה/קריאה/מחיקה של רשומות קישור פריטים־לאאוטפיט.
- fallback לסכמות legacy.

## `ConsoleApp2` (Models)
### מודלים (ללא לוגיקה כבדה)
- `User`, `Garment`, `Outfit`, `OutfitGarment`, `Style`, `Recommendation`, `UserMatchingState`.
### `FilterModels.cs`
- `Normalize()` לכל request פילטר לניקוי/איחוד ערכים.

---

## 5) מה לפשט הלאה בקוד (תכנית קצרה וברורה)
1. לפרק קבצים גדולים (`Closet`, `Outfits`, `MatchingService`) לתת־שירותים/קומפוננטות.
2. לבטל `async void` ב־`NavMenu.HandleAuthChanged` ולעבור ל־`Task` בטוח.
3. להזיז מחרוזות הודעות לקובץ קבועים אחד.
4. לאחד naming (`role`/`garment_type`/legacy columns) לשכבת map אחת.
5. להוסיף בדיקות יחידה ל־`AuthService`, `MatchingService`, ו־DB query builders.

---

## 6) הערות חשובות להבנה מיידית
- קבצי `Program.cs` בתוך `ConsoleApp1/2` הם placeholder ולא משפיעים על זרימת ה־Web.
- `bootstrap.min.css` הוא vendor: לא מפשטים ידנית.
- DBL כולל הרבה fallback לסכמות ישנות; זה נראה מורכב אבל נועד לשמור תאימות לנתונים קיימים.


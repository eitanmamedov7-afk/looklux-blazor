<!--




# ×ž×“×¨×™×š ×‘×¢×‘×¨×™×ª ×œ×¤×¨×•×™×§×˜ (×¤×©×•×˜ ×•×‘×¨×•×¨ ×ž×ž×‘×˜ ×¨××©×•×Ÿ)

## 1) ×ž×” ×”×¤×¨×•×™×§×˜ ×¢×•×©×”
×ž×¢×¨×›×ª ×œ× ×™×”×•×œ ××¨×•×Ÿ ×‘×’×“×™× ×•×”×ª××ž×ª ×××•×˜×¤×™×˜×™×:
- ×ž×©×ª×ž×© × ×¨×©×/×ž×ª×—×‘×¨.
- ×ž×¢×œ×” ×¤×¨×™×˜×™ ×œ×‘×•×©.
- ×ž×ª×§×‘×œ×ª ×—×œ×•×§×” ×œ×ª×›×•× ×•×ª (AI).
- ×”×ž×¢×¨×›×ª ×ž×¦×™×¢×” ×”×ª××ž×•×ª ×××•×˜×¤×™×˜.
- ××¤×©×¨ ×œ×©×ž×•×¨ ×”×ª××ž×•×ª.
- ×™×© ×¤×× ×œ ××“×ž×™×Ÿ ×œ× ×™×”×•×œ ×ž×©×ª×ž×©×™×.
- ×™×© ×ª×”×œ×™×š ××™×¤×•×¡ ×¡×™×¡×ž×” ×“×¨×š ××™×ž×™×™×œ.

---

## 2) ×–×¨×™×ž×” ×ž×œ××” (End-to-End)
1. ×”×“×¤×“×¤×Ÿ ×ž×’×™×¢ ×œÖ¾`App.razor` ×•××– `Routes.razor`.
2. `MainLayout.razor` ×˜×•×¢×Ÿ ××ª `NavMenu.razor`.
3. ×“×¤×™ ×”×ª×—×‘×¨×•×ª/×”×¨×©×ž×” ×ž×©×ª×ž×©×™× ×‘Ö¾`IAuthService` (`AuthService` ×‘×¤×•×¢×œ).
4. ×“×£ `Closet` ×˜×•×¢×Ÿ ×¤×¨×™×˜×™× ×ž×”Ö¾DB ×“×¨×š `GarmentDB`, ×©×•×ž×¨ ×ª×ž×•× ×•×ª ×“×¨×š `GarmentImageDB`, ×•×ž× ×ª×— ×ª×›×•× ×•×ª ×“×¨×š `GarmentFeatureService` â†’ `GeminiClient`.
5. ×¨×›×™×‘ `SlotMachineMatch` ×ž×¤×¢×™×œ `MatchingService` ×©×ž×™×™×¦×¨/×ž×“×¨×’ ×”×ª××ž×•×ª ×•×©×•×ž×¨ ××•×ª×Ÿ (`OutfitDB` + `OutfitGarmentDB`).
6. ×“×£ `Outfits` ×ž×¦×™×’, ×ž×¡× ×Ÿ ×•×ž×•×—×§ ×××•×˜×¤×™×˜×™×.
7. ××™×¤×•×¡ ×¡×™×¡×ž×”: `ForgotPassword` â†’ `AuthService.SendPasswordResetEmailAsync` â†’ `SmtpEmailSender`; ××—"×› `ResetPassword` ×ž×¢×“×›×Ÿ hash ×‘Ö¾`UserDB`.

---

## 3) ×ž×¤×ª ×§×‘×¦×™× â€“ ×›×œ ×§×•×‘×¥ ×•×ž×” ×”×•× ×¢×•×©×”

## `gadifff` (××¤×œ×™×§×¦×™×™×ª Web Blazor)
- `gadifff/Program.cs`
  ××ª×—×•×œ ×”××¤×œ×™×§×¦×™×”: DI, Auth, API endpoints, SMTP options, ×§×¨×™××ª ×§×•× ×¤×™×’.
- `gadifff/Components/App.razor`
  ×ž×¢×˜×¤×ª HTML ×¨××©×™×ª, ×˜×¢×™× ×ª CSS ×•Ö¾`Routes`.
- `gadifff/Components/Routes.razor`
  Router ×ž×¨×›×–×™ ×œ×›×œ ×”Ö¾pages.
- `gadifff/Components/_Imports.razor`
  ×™×™×‘×•× namespaces ×’×œ×•×‘×œ×™×™× ×œ×¨×›×™×‘×™×.

### Layout
- `gadifff/Components/Layout/MainLayout.razor`
  ×ž×‘× ×” ×¢×ž×•×“: ×¡×™×™×“×‘×¨ + ×ª×•×›×Ÿ.
- `gadifff/Components/Layout/MainLayout.razor.css`
  ×¢×™×¦×•×‘ ×œ×ž×‘× ×” ×”×›×œ×œ×™.
- `gadifff/Components/Layout/NavMenu.razor`
  × ×™×•×•×˜ ×“×™× ×ž×™ ×œ×¤×™ ×¡×˜×˜×•×¡ ×ž×©×ª×ž×©/××“×ž×™×Ÿ.
- `gadifff/Components/Layout/NavMenu.razor.css`
  ×¢×™×¦×•×‘ ×”×ª×¤×¨×™×˜.

### Shared Components
- `gadifff/Components/Shared/MultiSelectFilter.razor`
  ×§×•×ž×¤×•× × ×˜×” ×¨×‘Ö¾×‘×—×™×¨×” ×¢× ×—×™×¤×•×© ×•×¦'×™×¤×™×.
- `gadifff/Components/Shared/SlotMachineMatch.razor`
  ×¨×›×™×‘ ×”×ª××ž×•×ª ××™× ×˜×¨××§×˜×™×‘×™: seed, ×”×¨×¦×”, ×ª×•×¦××•×ª, ×©×ž×™×¨×”.

### Pages
- `gadifff/Components/Pages/Home.razor`
  ×“×£ ×‘×™×ª (×ž×©×ª×ž×© ×¨×’×™×œ/××“×ž×™×Ÿ).
- `gadifff/Components/Pages/Login.razor`
  ×”×ª×—×‘×¨×•×ª.
- `gadifff/Components/Pages/Register.razor`
  ×”×¨×©×ž×”.
- `gadifff/Components/Pages/Logout.razor`
  ×™×¦×™××” ×ž×”×ž×¢×¨×›×ª.
- `gadifff/Components/Pages/ForgotPassword.razor`
  ×‘×§×©×ª ×ž×™×™×œ ××™×¤×•×¡ ×¡×™×¡×ž×”.
- `gadifff/Components/Pages/ResetPassword.razor`
  ×§×‘×™×¢×ª ×¡×™×¡×ž×” ×—×“×©×” ×ž×˜×•×§×Ÿ.
- `gadifff/Components/Pages/Closet.razor`
  × ×™×”×•×œ ××¨×•×Ÿ, ×”×¢×œ××•×ª, ×¡×™× ×•× ×™×, ×¤×ª×™×—×ª ×ž× ×•×¢ ×”×ª××ž×•×ª.
- `gadifff/Components/Pages/Outfits.razor`
  ×ª×¦×•×’×ª ×××•×˜×¤×™×˜×™× ×©×ž×•×¨×™×, ×¡×™× ×•× ×™×, ×ž×—×™×§×”, ×”×ª××ž×” ×ž×—×“×©.
- `gadifff/Components/Pages/AdminClosets.razor`
  × ×™×”×•×œ ×ž×©×ª×ž×©×™× (××“×ž×™×Ÿ): ×™×¦×™×¨×”, ×¢×¨×™×›×”, ×ž×—×™×§×”.
- `gadifff/Components/Pages/AdminClosetUser.razor`
  ×¦×¤×™×™×”/× ×™×”×•×œ ××¨×•×Ÿ ×•×××•×˜×¤×™×˜×™× ×©×œ ×ž×©×ª×ž×© ×¡×¤×¦×™×¤×™.
- `gadifff/Components/Pages/AdminUsers.razor`
  ×“×£ redirect ×”×™×¡×˜×•×¨×™ ×œÖ¾AdminClosets.
- `gadifff/Components/Pages/AdminCloset.razor`
  ×“×£ redirect `/admin/closet-guid/{id}` ××œ `/admin/closet/{id}`.
- `gadifff/Components/Pages/AccessDenied.razor`
  ×”×•×“×¢×ª ×—×•×¡×¨ ×”×¨×©××”.
- `gadifff/Components/Pages/Error.razor`
  ×¢×ž×•×“ ×©×’×™××” ×›×œ×œ×™.

### Services
- `gadifff/Services/IAuthService.cs`
  ×—×•×–×” ×¤×¢×•×œ×•×ª auth.
- `gadifff/Services/AuthService.cs`
  ×ž×™×ž×•×© auth: login/register/logout + reset password.
- `gadifff/Services/PasswordResetEmailStatus.cs`
  ×¡×˜×˜×•×¡×™× ×œ×ª×•×¦××ª ×©×œ×™×—×ª ××™×¤×•×¡ ×¡×™×¡×ž×”.
- `gadifff/Services/IEmailSender.cs`
  ×—×•×–×” ×œ×©×œ×™×—×ª ××™×ž×™×™×œ.
- `gadifff/Services/SmtpEmailSender.cs`
  ×ž×™×ž×•×© SMTP ××ž×™×ª×™ (host/port/user/pass).
- `gadifff/Services/SmtpOptions.cs`
  ×ž×•×“×œ ×§×•× ×¤×™×’ SMTP.
- `gadifff/Services/GeminiClient.cs`
  ×œ×§×•×— HTTP ×œÖ¾Gemini.
- `gadifff/Services/GarmentFeatureService.cs`
  ×”×¤×§×ª JSON ×ª×›×•× ×•×ª ×ž×¤×¨×™×˜ ×œ×‘×•×© ×ž×ª×•×š ×ª×ž×•× ×”.
- `gadifff/Services/MatchingService.cs`
  ×œ×•×’×™×§×ª ×”×ª××ž×•×ª ×××•×˜×¤×™×˜, × ×™×§×•×“, ×©×ž×™×¨×”, fallback.

### Static
- `gadifff/wwwroot/app.css`
  ×¢×™×¦×•×‘ ×ž×•×ª×× ××¤×œ×™×§×¦×™×”.
- `gadifff/wwwroot/bootstrap/bootstrap.min.css`
  ×§×•×‘×¥ vendor (Bootstrap) â€“ ×œ× ×¢×•×¨×›×™× ×™×“× ×™×ª.

## `ConsoleApp2` (Models)
- `ConsoleApp2/User.cs` â€“ ×ž×•×“×œ ×ž×©×ª×ž×©.
- `ConsoleApp2/Garment.cs` â€“ ×ž×•×“×œ ×¤×¨×™×˜ ×œ×‘×•×©.
- `ConsoleApp2/Outfit.cs` â€“ ×ž×•×“×œ ×××•×˜×¤×™×˜.
- `ConsoleApp2/OutfitGarment.cs` â€“ ×§×©×¨ ×¤×¨×™×˜â†”×××•×˜×¤×™×˜.
- `ConsoleApp2/Style.cs` â€“ ×ž×•×“×œ ×¡×’× ×•×Ÿ.
- `ConsoleApp2/Recommendation.cs` â€“ ×ž×•×“×œ ×”×ž×œ×¦×”.
- `ConsoleApp2/UserMatchingState.cs` â€“ ×ž×¦×‘ lockout/×“×¨×™×©×•×ª ×”×ª××ž×” ×œ×ž×©×ª×ž×©.
- `ConsoleApp2/FilterModels.cs` â€“ ×‘×§×©×•×ª/××¤×©×¨×•×™×•×ª ×¡×™× ×•×Ÿ + normalize.
- `ConsoleApp2/Program.cs` â€“ placeholder ×‘×œ×‘×“.

## `ConsoleApp1` (Data Access / DBL)
- `ConsoleApp1/DB.cs` â€“ ×‘×¡×™×¡ ×—×™×‘×•×¨ MySQL ×•Ö¾command/reader.
- `ConsoleApp1/BaseDB.cs` â€“ CRUD generic + ×‘× ×™×™×ª SQL ×¤×¨×ž×˜×¨×™.
- `ConsoleApp1/UserDB.cs` â€“ ×’×™×©×ª × ×ª×•× ×™ ×ž×©×ª×ž×©×™×.
- `ConsoleApp1/StyleDB.cs` â€“ ×’×™×©×ª × ×ª×•× ×™ ×¡×’× ×•× ×•×ª.
- `ConsoleApp1/GarmentDB.cs` â€“ ×’×™×©×ª × ×ª×•× ×™ ××¨×•×Ÿ (×¤×¨×™×˜×™× + ×¤×™×œ×˜×¨×™×).
- `ConsoleApp1/GarmentImageDB.cs` â€“ ×©×ž×™×¨×”/×©×œ×™×¤×” ×©×œ ×ª×ž×•× ×•×ª ×‘×™× ××¨×™×•×ª.
- `ConsoleApp1/OutfitDB.cs` â€“ ×’×™×©×ª ×××•×˜×¤×™×˜×™× + ×¤×™×œ×˜×¨×™× + ×ª××™×ž×•×ª ×¡×›×ž×•×ª.
- `ConsoleApp1/OutfitGarmentDB.cs` â€“ ×’×™×©×ª ×˜×‘×œ×ª ×§×©×¨ ×××•×˜×¤×™×˜Ö¾×¤×¨×™×˜×™×.
- `ConsoleApp1/UserMatchingStateDB.cs` â€“ ×ž×¦×‘ ×”×ª××ž×•×ª ×¤×¨ ×ž×©×ª×ž×© (upsert).
- `ConsoleApp1/Pogram.cs` â€“ placeholder ×‘×œ×‘×“.

---

## 4) ×”×¡×‘×¨ ×¤×•× ×§×¦×™×•×ª ×•×‘×œ×•×§×™× (×‘×¢×‘×¨×™×ª, ×œ×¤×™ ×§×•×‘×¥)

## `gadifff/Program.cs`
### ×‘×œ×•×§×™ ×§×•×“
- **Configuration**: ×˜×•×¢×Ÿ `appsettings`, `secrets`, ×ž×©×ª× ×™ ×¡×‘×™×‘×”.
- **DI Registration**: ×¨×•×©× DB classes, Services, Auth, SMTP.
- **Middleware**: `UseStaticFiles`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `UseAntiforgery`.
- **API Endpoints**: media image endpoint + closet/outfits filter endpoints.
- **Razor Components**: ×ž×™×¤×•×™ UI.

### ×¤×•× ×§×¦×™×•×ª
- `LogGarmentPromptConfiguration`
  ×ž×¡×‘×™×¨ ×‘×œ×•×’×™× ×ž××™×¤×” × ×˜×¢×Ÿ ×¤×¨×•×ž×¤×˜ × ×™×ª×•×— ×ª×ž×•× ×”.
- `LogGeminiConfiguration`
  ×ž×¡×‘×™×¨ ×‘×œ×•×’×™× ×× API key ×œÖ¾Gemini ×§×™×™×.
- `ResolvePromptPath`
  ×ž×ž×™×¨ path ×™×—×¡×™ ×œ×ž×œ×.
- `ParseGarmentFilterRequest` / `ParseOutfitFilterRequest`
  ×‘×•× ×” ×ž×•×“×œ×™ ×¤×™×œ×˜×¨ ×ž×”Ö¾query string.
- `ReadList`
  ×§×•×¨× ×¨×©×™×ž×•×ª ×ž×”Ö¾query, ×ž× ×§×” duplicates ×•×¨×•×•×—×™×.
- `ApplyLegacySmtpOverrides`
  ×ž×ž×¤×” ×’× ×¤×•×¨×ž×˜ env ×™×©×Ÿ (`SMTP_*`) ××œ `SmtpOptions`.
- `FirstNonEmpty`
  ×ž×—×–×™×¨ ×”×¢×¨×š ×”×œ× ×¨×™×§ ×”×¨××©×•×Ÿ.
- `TryParseBool`
  parse ×œ×‘×•×œ×™×× ×™ ×›×•×œ×œ `yes/no/on/off/1/0`.

## `gadifff/Services/AuthService.cs`
### ×œ×ž×” ×”×§×•×‘×¥ ×§×™×™×
×ž×¨×›×– ××ª ×›×œ ×”×œ×•×’×™×§×” ×©×œ ×ž×©×ª×ž×©×™× ×•×”×ª×—×‘×¨×•×ª.

### ×¤×•× ×§×¦×™×•×ª
- `NotifyAuthChanged`
  ×©×•×œ×— event ×œÖ¾UI ×©×”×ž×©×ª×ž×© ×”×©×ª× ×”.
- `LoginAsync`
  ×ž××ž×ª ××™×ž×™×™×œ+×¡×™×¡×ž×” ×ž×•×œ hash.
- `LogoutAsync`
  ×ž× ×§×” user × ×•×›×—×™.
- `RegisterAsync`
  ×™×•×¦×¨ ×ž×©×ª×ž×© ×—×“×© ×¢× hash.
- `SendPasswordResetEmailAsync`
  ×‘×•×“×§ ×× ××™×ž×™×™×œ ×§×™×™×, ×ž×™×™×¦×¨ ×˜×•×§×Ÿ ×ž×•×’×Ÿ ×œ×–×ž×Ÿ ×ž×•×’×‘×œ, ×•×©×•×œ×— ×œ×™× ×§.
- `ResetPasswordAsync`
  ×ž××ž×ª ×˜×•×§×Ÿ + ×ª×•×§×£, ×•×ž×¢×“×›×Ÿ hash ×—×“×©.
- `CurrentUserAsync`
  ×ž×—×–×™×¨ ××ª ×”×ž×©×ª×ž×© ×”× ×•×›×—×™ ×‘×–×™×›×¨×•×Ÿ.

## `gadifff/Services/SmtpEmailSender.cs`
- `SendAsync`
  ×‘×•× ×” `MailMessage`, ×¤×•×ª×— `SmtpClient`, ×©×•×œ×— ××™×ž×™×™×œ, ×•×ž×—×–×™×¨ ×”×¦×œ×—×”/×›×™×©×œ×•×Ÿ.

## `gadifff/Services/GeminiClient.cs`
### ×–×¨×™×ž×”
1. ×‘×•×“×§ ×©×™×© API Key.
2. ×©×•×œ×— ×‘×§×©×ª `generateContent`.
3. ×× model ×œ× ×§×™×™× â€“ fallback ×œÖ¾`gemini-2.5-flash`.
4. ×ž×—×œ×¥ text ×ž×”Ö¾JSON response.

### ×¤×•× ×§×¦×™×•×ª
- `GenerateAsync` â€“ orchestration ×ž×œ× ×©×œ ×§×¨×™××” ×œÖ¾Gemini.
- `SendRequestAsync` â€“ POST HTTP ×¢× payload.
- `BuildParts` â€“ ×‘×•× ×” ×—×œ×§×™ prompt + image base64.
- `NormalizeModel` â€“ ×ž× ×§×”/×ž× ×¨×ž×œ ×©× ×ž×•×“×œ.
- `ResolveApiKey` â€“ ×ž×—×¤×© key ×‘×ž×©×ª× ×™ ×¡×‘×™×‘×” × ×ª×ž×›×™×.
- `Shorten` â€“ ×§×™×¦×•×¨ ×˜×§×¡×˜ ×œ×œ×•×’×™×.

## `gadifff/Services/GarmentFeatureService.cs`
### ×–×¨×™×ž×”
1. ×˜×•×¢×Ÿ prompt (Config/×§×•×‘×¥/Built-in).
2. ×©×•×œ×— ×œÖ¾Gemini.
3. ×ž× ×§×” ×ª×©×•×‘×” ×œÖ¾JSON × ×§×™.
4. ×ž××ž×ª JSON; ×× ×œ× ×ª×§×™×Ÿ ×–×•×¨×§ ×©×’×™××”.

### ×¤×•× ×§×¦×™×•×ª
- `ExtractFromImageAsync` â€“ × ×™×ª×•×— ×ª×ž×•× ×” ×œ×ª×›×•× ×•×ª JSON.
- `LoadPromptAsync` â€“ ×‘×—×™×¨×ª ×ž×§×•×¨ ×¤×¨×•×ž×¤×˜.
- `NormalizeJson` â€“ × ×™×§×•×™ markdown/code fences.
- `ResolvePromptPath` â€“ path ×œ×§×•×‘×¥ ×¤×¨×•×ž×¤×˜.

## `gadifff/Services/MatchingService.cs` (×§×•×‘×¥ ×’×“×•×œ)
### ×‘×œ×•×§ 1: State/Gate
- `GetOrInitStateAsync`, `CanMatchAsync`, `RecordFailureAsync`, `RecordSuccessAsync`
  ×ž× ×”×œ ×ª× ××™ ×¡×£ ×œ×ž×©×ª×ž×© ×œ×¤× ×™ ×”×ª××ž×”.

### ×‘×œ×•×§ 2: ×”×ª××ž×” ×¨××©×™×ª
- `FindMatchesAsync`
  ×‘×•× ×” seed + pools, ×ž×™×™×¦×¨ ×§×•×ž×‘×™× ×¦×™×•×ª, ×ž×“×¨×’, ×ž×—×–×™×¨ Top ×ª×•×¦××•×ª.

### ×‘×œ×•×§ 3: ×©×ž×™×¨×”
- `SaveOutfitSuggestionAsync` + `BuildOutfitGarments`
  ×©×•×ž×¨ ×ª×•×¦××ª ×”×ª××ž×” ×œ×˜×‘×œ××•×ª outfits/outfit_garments.

### ×‘×œ×•×§ 4: ×™×¦×™×¨×ª ×§×•×ž×‘×™× ×¦×™×•×ª ×•× ×™×§×•×“
- `BuildCombinations`, `ScoreCombinationsAsync`, `LoadScoringPromptAsync`, `BuildMatchPrompt`
  ×ž×›×™×Ÿ ×ž×•×¢×ž×“×™× ×•×©×•×œ×— ×œÖ¾Gemini ×œ×¦×™×•×Ÿ.

### ×‘×œ×•×§ 5: Parsing/Normalization
- `ParseScoringOutcome`, `ParseRecommendationArray`, `TryParseRecommendationItem` ×•×¢×•×“
  ×ž×¤×¢× ×— JSON ×ž×”Ö¾AI ×•×ž×’×Ÿ ×ž×¤× ×™ ×¤×•×¨×ž×˜×™× ×©×•× ×™×.

### ×‘×œ×•×§ 6: Heuristics/Fallback
- `ComputePromptPriority`, `ComputePromptFitScore`, `BuildFallbackRecommendation` ×•×¢×•×“
  ×—×™×©×•×‘ fallback ×›×©Ö¾AI ×œ× ×ž×—×–×™×¨ ×ª×•×¦××” ×˜×•×‘×”.

### ×‘×œ×•×§ 7: Utilities
- `NormalizeTypeName`, `IsType`, `MergeGarments`, `SplitToTokens` ×•×›×•'.
  ×¤×•× ×§×¦×™×•×ª ×¢×–×¨ ×œ×œ×•×’×™×§×” ×•×œ× ×™×§×•×™ × ×ª×•× ×™×.

## `gadifff/Components/Shared/MultiSelectFilter.razor`
- `IsSelected` â€“ ×”×× ×¢×¨×š × ×‘×—×¨.
- `ToggleOpen` â€“ ×¤×•×ª×—/×¡×•×’×¨ panel.
- `ClearSelection` â€“ ×ž××¤×¡ ×‘×—×™×¨×”.
- `RemoveOption` â€“ ×ž×¡×™×¨ ×¦'×™×¤ ×ž×¡×•×™×.
- `ToggleOption` â€“ ×ž×¡×ž×Ÿ/×ž×•×¨×™×“ option ×ž×¨×©×™×ž×ª ×”× ×‘×—×¨×™×.

## `gadifff/Components/Shared/SlotMachineMatch.razor` (×§×•×‘×¥ ×’×“×•×œ)
### ×‘×œ×•×§ UI ×•×¡× ×›×¨×•×Ÿ
- `OnParametersSetAsync`, `BuildSyncSignature` â€“ ×ž×¡× ×›×¨×Ÿ ×ž×¦×‘ ×œ×¤×™ ×¤×¨×™×˜×™×/seed.
- `RenderSlot` â€“ ×‘× ×™×™×ª UI slot (shirt/pants/shoes).

### ×‘×œ×•×§ ×‘×—×™×¨×ª seed ×•×ª× ×•×¢×”
- `SetSeed`, `SetExclusiveSeed`, `IsSeedSelected`, `SelectedSeedCount`.
- `Cycle` + `Prev/Next` ×œ×›×œ ×¡×•×’.

### ×‘×œ×•×§ ×”×¨×¦×”
- `RunMatch` â€“ ×ž×¨×™×¥ ×”×ª××ž×” ×“×¨×š `MatchingService`.
- `RunProgressAsync` â€“ ×× ×™×ž×¦×™×™×ª ×”×ª×§×“×ž×•×ª.

### ×‘×œ×•×§ ×ª×•×¦××•×ª ×•×©×ž×™×¨×”
- `ToggleSuggestion`, `IsSuggestionSaveDisabled`, `GetSuggestionSaveButtonText`, `SaveSuggestionAsync`.

### ×¢×–×¨
- `FindGarment`, `NormalizeTypeName`, `GetImageUrl`, `Mod`.

## ×“×¤×™ Auth ×§×¦×¨×™×
- `Login.razor`
  `HandleLoginAsync`, `GoToRegister`.
- `Register.razor`
  `HandleRegisterAsync`, `GoToLogin`.
- `Logout.razor`
  `OnInitializedAsync` (logout ××•×˜×•×ž×˜×™ + redirect).
- `ForgotPassword.razor`
  `HandleSendAsync`, `GoToLogin`.
- `ResetPassword.razor`
  `OnParametersSet`, `HandleResetAsync`, `GoToLogin`.

## ×“×¤×™ Admin
- `AdminClosets.razor`
  ×‘×œ×•×§×™×: ×˜×¢×™× ×”, ×™×¦×™×¨×”, ×¢×¨×™×›×”, ×ž×—×™×§×”, dirty-check, ×”×•×“×¢×•×ª UI.
  ×¤×•× ×§×¦×™×•×ª: `ReloadUsersAsync`, `CreateUserAsync`, `SaveUserAsync`, `DeleteUserAsync` ×•×¢×•×“.
- `AdminClosetUser.razor`
  ×‘×œ×•×§×™×: ×˜×¢×™× ×ª ×ž×©×ª×ž×© ×™×¢×“, ×ž×—×™×§×ª ×¤×¨×™×˜/×××•×˜×¤×™×˜, ×“×™××œ×•×’ ××™×©×•×¨, × ×™×•×•×˜.
  ×¤×•× ×§×¦×™×•×ª: `LoadTargetDataAsync`, `DeleteGarmentAsync`, `DeleteOutfitAsync`, `ConfirmDeleteDialogAsync` ×•×¢×•×“.
- `AdminCloset.razor`, `AdminUsers.razor`
  Redirect ×‘×œ×‘×“.

## ×“×¤×™ ×¢×‘×•×“×” ×’×“×•×œ×™×
- `Closet.razor`
  ×‘×œ×•×§×™×:
  - ×˜×¢×™× ×ª ×ž×©×ª×ž×© ×•×§×•× ×˜×§×¡×˜ ××“×ž×™×Ÿ (`ResolveActiveUserContextAsync`)
  - ×”×¢×œ××ª ×§×•×‘×¥ ×•× ×™×ª×•×— (`OnFileChange`, `UploadFromFileAsync`, `PersistGarmentAsync`)
  - fallback ×™×“× ×™ (`PrepareManualFallback`, `SaveManualAsync`)
  - × ×™×”×•×œ ×¡×™× ×•× ×™× (`BuildGarmentFilterRequest`, `RefreshClosetFiltersAsync`, `On*Changed`, drawer/group methods)
  - ×”×ª××ž×•×ª ×•×—×œ×•× ×•×ª (`OpenMatcher`, `ReloadAfterMatch`, preview methods)
  - ×ž×—×™×§×” (`ConfirmDeleteGarmentDialog`, `DeleteGarmentConfirmedAsync`)
- `Outfits.razor`
  ×‘×œ×•×§×™×:
  - ×˜×¢×™× ×” + ×§×•× ×˜×§×¡×˜ (`LoadData`, `ResolveActiveUserContextAsync`)
  - ×¡×™× ×•× ×™× (`BuildRequest`, `RefreshOutfitFiltersAsync`, `On*Changed`, drawer/group methods)
  - ×¤×¢×•×œ×•×ª ×××•×˜×¤×™×˜ (`FindFromOutfit`, `DeleteOutfitConfirmedAsync`, `Reload`)
  - ×—×œ×•× ×•×ª ×¢×–×¨ (`OpenBestRecommendationWindow`, matcher/preview methods)
- `Home.razor`
  ×‘×œ×•×§×™×:
  - ×˜×¢×™× ×ª dashboard ×œ××“×ž×™×Ÿ (`LoadAdminDashboardAsync`)
  - ×‘× ×™×™×ª × ×§×•×“×•×ª ×’×¨×£/×©×™×ž×•×© (`BuildUsagePoints`)
  - ××™× ×˜×¨××§×¦×™×™×ª hover/select (`SelectUsagePoint`, `HoverUsagePoint`, `ClearUsageHover`)

## `ConsoleApp1` (DB Access)
### `DB.cs`
- constructor: ×™×•×¦×¨ connection/command reader ×‘×¡×™×¡×™×™×.

### `BaseDB.cs`
#### ×‘×œ×•×§ CRUD
- `SelectAllAsync` (3 overloads), `InsertAsync`, `InsertGetObjAsync`, `UpdateAsync`, `DeleteAsync`.
#### ×‘×œ×•×§ ×‘× ×™×™×ª SQL
- `PrepareWhereQueryWithParameters`, `PrepareUpdateQueryWithParameters`, `PrepareInsertQueryWithParameters`.
#### ×‘×œ×•×§ ×”×¨×¦×”
- `ExecNonQueryAsync`, `ExecScalarAsync`, `StringListSelectAllAsync`, `StingListSelectAllAsync` (×ª××™×ž×•×ª ×œ××—×•×¨).
#### ×‘×œ×•×§ ×ª×©×ª×™×ª
- `AddParameterToCommand`, `PreQueryAsync`, `PostQueryAsync`.

### `UserDB.cs`
- get/create/update/delete/count + `UpdatePasswordHashAsync` ×œ××™×¤×•×¡ ×¡×™×¡×ž×”.

### `StyleDB.cs`
- ×§×¨×™××”/×™×¦×™×¨×” ×©×œ style.

### `UserMatchingStateDB.cs`
- `GetByUserAsync`, `UpsertAsync` ×œ×ž×¦×‘ ×”×ª××ž×”.

### `GarmentImageDB.cs`
- `CreateAsync` ×œ×©×ž×™×¨×ª image bytes.
- `GetLatestByGarmentIdAsync` ×œ×©×œ×™×¤×ª ×ª×ž×•× ×” ×œ×”×¦×’×”.

### `GarmentDB.cs` (×’×“×•×œ)
- ×‘×œ×•×§ mapping ×¡×›×ž×•×ª ×©×•× ×•×ª (`CreateModelAsync` ×¢× fallback ×œ×¤×™ ×ž×¡×¤×¨ ×¢×ž×•×“×•×ª).
- CRUD ×‘×¡×™×¡×™ (`GetByUserAsync`, `GetByIdAsync`, `CreateAsync`, `DeleteGarmentAsync`).
- ×‘×œ×•×§ ×¡×˜×˜×™×¡×˜×™×§×•×ª (`CountByUserRoleAsync` ×•×›×•').
- ×‘×œ×•×§ ×¤×™×œ×˜×¨×™× (`GetFilteredByUserAsync`, `GetFilterOptionsAsync` + query helpers).
- ×‘×œ×•×§ ×¢×–×¨×™ parsing/normalization.

### `OutfitDB.cs` (×’×“×•×œ)
- ×‘×œ×•×§ mapping ×¡×›×ž×•×ª legacy/modern (`CreateModelAsync`).
- CRUD (`GetAllAsync`, `GetByUserAsync`, `GetByIdAsync`, `DeleteOutfitAsync`, `CreateAsync`).
- ×‘×“×™×§×ª ×›×¤×™×œ×•×™×•×ª (`ExistsDuplicateAsync`).
- ×¤×™×œ×˜×¨×™× + ××¤×©×¨×•×™×•×ª (`GetFilteredByUserAsync`, `GetFilterOptionsAsync`).
- ×ª××™×ž×•×ª ×¡×›×ž×•×ª ×“×™× ×ž×™×ª (×‘×“×™×§×ª columns ×‘×–×ž×Ÿ ×¨×™×¦×”).

### `OutfitGarmentDB.cs`
- ×›×ª×™×‘×”/×§×¨×™××”/×ž×—×™×§×” ×©×œ ×¨×©×•×ž×•×ª ×§×™×©×•×¨ ×¤×¨×™×˜×™×Ö¾×œ×××•×˜×¤×™×˜.
- fallback ×œ×¡×›×ž×•×ª legacy.

## `ConsoleApp2` (Models)
### ×ž×•×“×œ×™× (×œ×œ× ×œ×•×’×™×§×” ×›×‘×“×”)
- `User`, `Garment`, `Outfit`, `OutfitGarment`, `Style`, `Recommendation`, `UserMatchingState`.
### `FilterModels.cs`
- `Normalize()` ×œ×›×œ request ×¤×™×œ×˜×¨ ×œ× ×™×§×•×™/××™×—×•×“ ×¢×¨×›×™×.

---

## 5) ×ž×” ×œ×¤×©×˜ ×”×œ××” ×‘×§×•×“ (×ª×›× ×™×ª ×§×¦×¨×” ×•×‘×¨×•×¨×”)
1. ×œ×¤×¨×§ ×§×‘×¦×™× ×’×“×•×œ×™× (`Closet`, `Outfits`, `MatchingService`) ×œ×ª×ªÖ¾×©×™×¨×•×ª×™×/×§×•×ž×¤×•× × ×˜×•×ª.
2. ×œ×‘×˜×œ `async void` ×‘Ö¾`NavMenu.HandleAuthChanged` ×•×œ×¢×‘×•×¨ ×œÖ¾`Task` ×‘×˜×•×—.
3. ×œ×”×–×™×– ×ž×—×¨×•×–×•×ª ×”×•×“×¢×•×ª ×œ×§×•×‘×¥ ×§×‘×•×¢×™× ××—×“.
4. ×œ××—×“ naming (`role`/`garment_type`/legacy columns) ×œ×©×›×‘×ª map ××—×ª.
5. ×œ×”×•×¡×™×£ ×‘×“×™×§×•×ª ×™×—×™×“×” ×œÖ¾`AuthService`, `MatchingService`, ×•Ö¾DB query builders.

---

## 6) ×”×¢×¨×•×ª ×—×©×•×‘×•×ª ×œ×”×‘× ×” ×ž×™×™×“×™×ª
- ×§×‘×¦×™ `Program.cs` ×‘×ª×•×š `ConsoleApp1/2` ×”× placeholder ×•×œ× ×ž×©×¤×™×¢×™× ×¢×œ ×–×¨×™×ž×ª ×”Ö¾Web.
- `bootstrap.min.css` ×”×•× vendor: ×œ× ×ž×¤×©×˜×™× ×™×“× ×™×ª.
- DBL ×›×•×œ×œ ×”×¨×‘×” fallback ×œ×¡×›×ž×•×ª ×™×©× ×•×ª; ×–×” × ×¨××” ×ž×•×¨×›×‘ ××‘×œ × ×•×¢×“ ×œ×©×ž×•×¨ ×ª××™×ž×•×ª ×œ× ×ª×•× ×™× ×§×™×™×ž×™×.

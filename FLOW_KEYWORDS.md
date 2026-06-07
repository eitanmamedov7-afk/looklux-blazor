# Numbered Flow Keywords

Use Ctrl+F on the shared prefix, for example `FLOW_LOGIN_MOBILE`, to see the whole process in order.

## FLOW_MAUI_STARTUP
Android MAUI app startup.

- `FLOW_MAUI_STARTUP_01` - Android MainApplication calls MauiProgram.CreateMauiApp. File: `maui_api/Platforms/Android/MainApplication.cs`
- `FLOW_MAUI_STARTUP_02` - MauiProgram registers App as the root MAUI application. File: `maui_api/MauiProgram.cs`
- `FLOW_MAUI_STARTUP_03` - App.CreateWindow opens MainPage. File: `maui_api/App.xaml.cs`
- `FLOW_MAUI_STARTUP_04` - Android MainActivity hosts the MAUI UI and keyboard resize behavior. File: `maui_api/Platforms/Android/MainActivity.cs`
- `FLOW_MAUI_STARTUP_05` - MainPage constructor initializes the visible mobile screen. File: `maui_api/MainPage.xaml.cs`

## FLOW_MAUI_SERVER_CONNECT
MAUI backend server URL selection.

- `FLOW_MAUI_SERVER_CONNECT_01` - ServerEndpointProvider chooses the Android emulator default URL. File: `maui_api/ServerEndpointProvider.cs`
- `FLOW_MAUI_SERVER_CONNECT_02` - ServerEndpointProvider builds saved/default/fallback backend URLs. File: `maui_api/ServerEndpointProvider.cs`
- `FLOW_MAUI_SERVER_CONNECT_03` - MainPage chooses the first candidate at startup. File: `maui_api/MainPage.xaml.cs`
- `FLOW_MAUI_SERVER_CONNECT_04` - MainPage stores the selected URL in LookLuxApiClient. File: `maui_api/MainPage.xaml.cs`

## FLOW_LOGIN_WEB
Browser customer/admin login.

- `FLOW_LOGIN_WEB_01` - User submits the web sign-in form. File: `gadifff/Components/Pages/Login.razor`
- `FLOW_LOGIN_WEB_02` - Login page sends credentials to `AuthService.LoginAsync`. File: `gadifff/Components/Pages/Login.razor`
- `FLOW_LOGIN_WEB_03` - AuthService owns web auth state and validates credentials. File: `gadifff/Services/AuthService.cs`
- `FLOW_LOGIN_WEB_04` - UserDB loads the account by email from MySQL. File: `ConsoleApp1/UserDB.cs`

## FLOW_LOGIN_MOBILE
MAUI login through the API.

- `FLOW_LOGIN_MOBILE_01` - User taps Sign in. File: `maui_api/MainPage.xaml`
- `FLOW_LOGIN_MOBILE_02` - MAUI reads email/password and calls the API client. File: `maui_api/MainPage.xaml.cs`
- `FLOW_LOGIN_MOBILE_03` - API client posts to `/api/mobile/auth/login`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_LOGIN_MOBILE_04` - Program.cs endpoint validates credentials with BCrypt. File: `gadifff/Program.cs`
- `FLOW_LOGIN_MOBILE_05` - UserDB loads the account by email. File: `ConsoleApp1/UserDB.cs`

## FLOW_AUTH_STATE_WEB
Where the web project keeps the user logged in.

- `FLOW_AUTH_STATE_WEB_00` - Program.cs registers cookie auth middleware in the ASP.NET pipeline. File: `gadifff/Program.cs`
- `FLOW_AUTH_STATE_WEB_01` - AuthService declares `_currentUser` and `_lastSignedInUser` as the project's remembered web user state. File: `gadifff/Services/AuthService.cs`
- `FLOW_AUTH_STATE_WEB_02` - LoginAsync stores the validated user in `_currentUser` and `_lastSignedInUser`. File: `gadifff/Services/AuthService.cs`
- `FLOW_AUTH_STATE_WEB_04` - CurrentUserAsync returns the remembered user to pages/layouts. File: `gadifff/Services/AuthService.cs`
- `FLOW_AUTH_STATE_WEB_05` - LogoutAsync clears the remembered web user. File: `gadifff/Services/AuthService.cs`

## FLOW_AUTH_STATE_MOBILE
Where the MAUI app keeps the user logged in.

- `FLOW_AUTH_STATE_MOBILE_00` - Program.cs notes that MAUI does not use the browser cookie middleware. File: `gadifff/Program.cs`
- `FLOW_AUTH_STATE_MOBILE_01` - MainPage declares `_currentUser` as the remembered mobile user. File: `maui_api/MainPage.xaml.cs`
- `FLOW_AUTH_STATE_MOBILE_02` - SignInAsync stores the API-returned user DTO and shows the app UI. File: `maui_api/MainPage.xaml.cs`
- `FLOW_AUTH_STATE_MOBILE_03` - Program.cs returns the safe user DTO from `/api/mobile/auth/login`. File: `gadifff/Program.cs`
- `FLOW_AUTH_STATE_MOBILE_04` - ResetToSignedOutHome clears `_currentUser` on logout/account deletion. File: `maui_api/MainPage.xaml.cs`

## FLOW_REGISTER_WEB
Browser account registration.

- `FLOW_REGISTER_WEB_01` - User submits the register form. File: `gadifff/Components/Pages/Register.razor`
- `FLOW_REGISTER_WEB_02` - Page validates confirm password and calls AuthService. File: `gadifff/Components/Pages/Register.razor`
- `FLOW_REGISTER_WEB_03` - AuthService checks duplicate email, hashes password, creates customer. File: `gadifff/Services/AuthService.cs`
- `FLOW_REGISTER_WEB_04` - UserDB inserts the user row. File: `ConsoleApp1/UserDB.cs`

## FLOW_REGISTER_MOBILE
MAUI account registration through API.

- `FLOW_REGISTER_MOBILE_01` - User taps Register. File: `maui_api/MainPage.xaml`
- `FLOW_REGISTER_MOBILE_02` - MAUI checks password and confirm password match. File: `maui_api/MainPage.xaml.cs`
- `FLOW_REGISTER_MOBILE_03` - API client posts to `/api/mobile/auth/register`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_REGISTER_MOBILE_04` - Program.cs validates fields and hashes password. File: `gadifff/Program.cs`
- `FLOW_REGISTER_MOBILE_05` - UserDB inserts the user row. File: `ConsoleApp1/UserDB.cs`

## FLOW_PASSWORD_RESET_WEB
Browser forgot/reset password.

- `FLOW_PASSWORD_RESET_WEB_01` - User clicks Send reset email on the web forgot-password form. File: `gadifff/Components/Pages/ForgotPassword.razor`
- `FLOW_PASSWORD_RESET_WEB_02` - ForgotPassword page sends email/base URL to AuthService. File: `gadifff/Components/Pages/ForgotPassword.razor`
- `FLOW_PASSWORD_RESET_WEB_03` - UserDB loads account by email before a reset email is created. File: `ConsoleApp1/UserDB.cs`
- `FLOW_PASSWORD_RESET_WEB_04` - AuthService creates protected reset token and browser reset link. File: `gadifff/Services/AuthService.cs`
- `FLOW_PASSWORD_RESET_WEB_05` - SmtpEmailSender sends the reset email through SMTP. File: `gadifff/Services/SmtpEmailSender.cs`, `gadifff/Services/IEmailSender.cs`
- `FLOW_PASSWORD_RESET_WEB_06` - ResetPassword page opens from the email link and reads the token query string. File: `gadifff/Components/Pages/ResetPassword.razor`
- `FLOW_PASSWORD_RESET_WEB_07` - ResetPassword page submits token and new password to AuthService. File: `gadifff/Components/Pages/ResetPassword.razor`
- `FLOW_PASSWORD_RESET_WEB_08` - AuthService validates token, checks expiry, and hashes new password. File: `gadifff/Services/AuthService.cs`
- `FLOW_PASSWORD_RESET_WEB_09` - UserDB writes the new password hash. File: `ConsoleApp1/UserDB.cs`

## FLOW_PASSWORD_RESET_MOBILE
MAUI forgot/reset password through API.

- `FLOW_PASSWORD_RESET_MOBILE_01` - User taps Forgot password. File: `maui_api/MainPage.xaml`
- `FLOW_PASSWORD_RESET_MOBILE_02` - MAUI validates email and calls API client. File: `maui_api/MainPage.xaml.cs`
- `FLOW_PASSWORD_RESET_MOBILE_03` - API client posts to `/api/mobile/auth/forgot-password`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_PASSWORD_RESET_MOBILE_04` - Program.cs receives the API request and converts emulator URL to browser-openable reset origin. File: `gadifff/Program.cs`
- `FLOW_PASSWORD_RESET_MOBILE_05` - UserDB loads account by email before a reset email is created. File: `ConsoleApp1/UserDB.cs`
- `FLOW_PASSWORD_RESET_MOBILE_06` - AuthService creates protected reset token and reset link. File: `gadifff/Services/AuthService.cs`
- `FLOW_PASSWORD_RESET_MOBILE_07` - SmtpEmailSender sends the reset email through SMTP. File: `gadifff/Services/SmtpEmailSender.cs`, `gadifff/Services/IEmailSender.cs`
- `FLOW_PASSWORD_RESET_MOBILE_08` - User opens the reset email link in the browser and ResetPassword reads the token. File: `gadifff/Components/Pages/ResetPassword.razor`
- `FLOW_PASSWORD_RESET_MOBILE_09` - ResetPassword submits token/new password and AuthService validates/hashes it. File: `gadifff/Components/Pages/ResetPassword.razor`, `gadifff/Services/AuthService.cs`
- `FLOW_PASSWORD_RESET_MOBILE_10` - UserDB writes the new password hash. File: `ConsoleApp1/UserDB.cs`

## FLOW_CLOSET_VIEW_WEB
Browser closet display.

- `FLOW_CLOSET_VIEW_WEB_03` - GarmentDB loads garments for the active user. File: `ConsoleApp1/GarmentDB.cs`

## FLOW_CLOSET_VIEW_MOBILE
MAUI closet display.

- `FLOW_CLOSET_VIEW_MOBILE_01` - User changes to closet/home tabs. File: `maui_api/MainPage.xaml`
- `FLOW_CLOSET_VIEW_MOBILE_02` - MAUI `LoadAllAsync` asks API for garments. File: `maui_api/MainPage.xaml.cs`
- `FLOW_CLOSET_VIEW_MOBILE_03` - API client calls `/api/mobile/closet/items`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_CLOSET_VIEW_MOBILE_04` - Program.cs checks actor/target access. File: `gadifff/Program.cs`
- `FLOW_CLOSET_VIEW_MOBILE_05` - GarmentDB loads the target user's garments. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_CLOSET_VIEW_MOBILE_06` - MAUI deserializes garment JSON into the mobile Garment model and renders cards. File: `maui_api/Models/Garment.cs`

## FLOW_GARMENT_UPLOAD_WEB
Browser garment upload.

- `FLOW_GARMENT_UPLOAD_WEB_01` - User selects image in closet upload. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_GARMENT_UPLOAD_WEB_02` - Closet page reads bytes and calls AI feature extraction. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_GARMENT_UPLOAD_WEB_04` - Closet page maps parsed fields to Garment model. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_GARMENT_UPLOAD_WEB_05` - GarmentDB inserts garment/image/sha256 row. File: `ConsoleApp1/GarmentDB.cs`

## FLOW_GARMENT_UPLOAD_MOBILE
MAUI garment upload through API.

- `FLOW_GARMENT_UPLOAD_MOBILE_01` - User taps Upload garment photo. File: `maui_api/MainPage.xaml`
- `FLOW_GARMENT_UPLOAD_MOBILE_02` - MAUI picks image and prepares base64. File: `maui_api/MainPage.xaml.cs`
- `FLOW_GARMENT_UPLOAD_MOBILE_03` - API client posts to `/api/mobile/garments`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_GARMENT_UPLOAD_MOBILE_04` - Program.cs validates image and starts AI analysis. File: `gadifff/Program.cs`
- `FLOW_GARMENT_UPLOAD_MOBILE_06` - GarmentDB inserts garment/image/sha256 row. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_GARMENT_UPLOAD_MOBILE_07` - MAUI can receive/render the saved Garment shape returned by the API. File: `maui_api/Models/Garment.cs`

## FLOW_GARMENT_IMAGE_SERVE
Image display for web and MAUI.

- `FLOW_GARMENT_IMAGE_SERVE_02` - Program.cs receives image URL request. File: `gadifff/Program.cs`
- `FLOW_GARMENT_IMAGE_SERVE_03` - GarmentDB loads image bytes and mime type. File: `ConsoleApp1/GarmentDB.cs`

## FLOW_CLOSET_FILTER_WEB
Browser closet filters.

- `FLOW_CLOSET_FILTER_WEB_01` - User opens closet filter drawer. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_CLOSET_FILTER_WEB_02` - Closet page sends selected filters to GarmentDB. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_CLOSET_FILTER_WEB_03` - GarmentDB applies SQL filters. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_CLOSET_FILTER_WEB_04` - GarmentDB builds filter options. File: `ConsoleApp1/GarmentDB.cs`

## FLOW_CLOSET_FILTER_API
Closet filters through API endpoints.

- `FLOW_CLOSET_FILTER_API_02` - Program.cs parses query-string filters. File: `gadifff/Program.cs`
- `FLOW_CLOSET_FILTER_API_03` - GarmentDB applies SQL filters. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_CLOSET_FILTER_API_04` - GarmentDB builds filter options. File: `ConsoleApp1/GarmentDB.cs`

## FLOW_OUTFIT_FILTER_API
Saved outfit filters through API endpoints.

- `FLOW_OUTFIT_FILTER_API_03` - OutfitDB applies score/style/place/season filters for API requests. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_OUTFIT_FILTER_API_04` - OutfitDB builds selectable saved-outfit filter options for API requests. File: `ConsoleApp1/OutfitDB.cs`

## FLOW_MATCH_RUN_WEB
Browser recommendation generation.

- `FLOW_MATCH_RUN_WEB_01` - User starts match from Closet. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_MATCH_RUN_WEB_01A` - User starts match from Outfits. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_MATCH_RUN_WEB_02` - SlotMachineMatch builds seeds and calls MatchingService. File: `gadifff/Components/Shared/SlotMachineMatch.razor`
- `FLOW_MATCH_RUN_WEB_03` - MatchingService starts the recommendation engine and asks for closet rows. File: `gadifff/Services/MatchingService.cs`
- `FLOW_MATCH_RUN_WEB_04` - GarmentDB loads the user's closet rows used as matcher inventory. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_MATCH_RUN_WEB_05` - MatchingService validates minimum 2 shirts/pants/shoes from loaded garments. File: `gadifff/Services/MatchingService.cs`
- `FLOW_MATCH_RUN_WEB_06` - GarmentDB loads candidate garment pools when MatchingService needs DB-backed pools. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_MATCH_RUN_WEB_07` - MatchingService scores valid combinations and returns suggestions. File: `gadifff/Services/MatchingService.cs`

## FLOW_MATCH_RUN_MOBILE
MAUI recommendation generation.

- `FLOW_MATCH_RUN_MOBILE_01` - User taps Find matches or garment recommendation. File: `maui_api/MainPage.xaml`, `maui_api/MainPage.xaml.cs`
- `FLOW_MATCH_RUN_MOBILE_02` - MAUI sends seed/request data to API client. File: `maui_api/MainPage.xaml.cs`
- `FLOW_MATCH_RUN_MOBILE_03` - API client posts to `/api/mobile/matches/run`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_MATCH_RUN_MOBILE_04` - Program.cs calls MatchingService. File: `gadifff/Program.cs`
- `FLOW_MATCH_RUN_MOBILE_05` - MatchingService starts the recommendation engine and asks for closet rows. File: `gadifff/Services/MatchingService.cs`
- `FLOW_MATCH_RUN_MOBILE_06` - GarmentDB loads the user's closet rows used as matcher inventory. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_MATCH_RUN_MOBILE_07` - MatchingService validates minimum inventory from loaded garments. File: `gadifff/Services/MatchingService.cs`
- `FLOW_MATCH_RUN_MOBILE_08` - GarmentDB loads candidate garment pools. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_MATCH_RUN_MOBILE_09` - MatchingService scores valid combinations and returns suggestions. File: `gadifff/Services/MatchingService.cs`

## FLOW_MATCH_SAVE_WEB
Browser save recommendation.

- `FLOW_MATCH_SAVE_WEB_01` - User clicks Save in SlotMachineMatch. File: `gadifff/Components/Shared/SlotMachineMatch.razor`
- `FLOW_MATCH_SAVE_WEB_02` - MatchingService validates and prepares save. File: `gadifff/Services/MatchingService.cs`
- `FLOW_MATCH_SAVE_WEB_03` - OutfitDB blocks duplicates. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_MATCH_SAVE_WEB_04` - OutfitDB inserts outfit metadata. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_MATCH_SAVE_WEB_05` - OutfitGarmentDB inserts garment links. File: `ConsoleApp1/OutfitGarmentDB.cs`
- `FLOW_MATCH_SAVE_WEB_05A` - OutfitGarmentDB batch helper loops through the shirt/pants/shoes link rows. File: `ConsoleApp1/OutfitGarmentDB.cs`

## FLOW_MATCH_SAVE_MOBILE
MAUI save recommendation through API.

- `FLOW_MATCH_SAVE_MOBILE_01` - User taps Save outfit in MAUI. File: `maui_api/MainPage.xaml.cs`
- `FLOW_MATCH_SAVE_MOBILE_02` - API client posts selected recommendation. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_MATCH_SAVE_MOBILE_03` - Program.cs calls MatchingService save. File: `gadifff/Program.cs`, `gadifff/Services/MatchingService.cs`
- `FLOW_MATCH_SAVE_MOBILE_04` - OutfitDB blocks duplicates. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_MATCH_SAVE_MOBILE_05` - OutfitDB inserts outfit metadata. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_MATCH_SAVE_MOBILE_06` - OutfitGarmentDB inserts garment links. File: `ConsoleApp1/OutfitGarmentDB.cs`
- `FLOW_MATCH_SAVE_MOBILE_06A` - OutfitGarmentDB batch helper loops through the shirt/pants/shoes link rows. File: `ConsoleApp1/OutfitGarmentDB.cs`

## FLOW_OUTFIT_VIEW_WEB
Browser saved outfits display.

- `FLOW_OUTFIT_VIEW_WEB_02` - Outfits page reloads saved outfit data. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_VIEW_WEB_03` - OutfitDB loads saved outfit rows. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_OUTFIT_VIEW_WEB_04` - OutfitGarmentDB loads garment links. File: `ConsoleApp1/OutfitGarmentDB.cs`

## FLOW_OUTFIT_VIEW_MOBILE
MAUI saved outfits display.

- `FLOW_OUTFIT_VIEW_MOBILE_01` - User changes to outfits/home tabs. File: `maui_api/MainPage.xaml`
- `FLOW_OUTFIT_VIEW_MOBILE_02` - MAUI `LoadAllAsync` asks API for outfits. File: `maui_api/MainPage.xaml.cs`
- `FLOW_OUTFIT_VIEW_MOBILE_03` - API client calls `/api/mobile/outfits/items`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_OUTFIT_VIEW_MOBILE_04` - Program.cs checks actor/target access. File: `gadifff/Program.cs`
- `FLOW_OUTFIT_VIEW_MOBILE_05` - OutfitDB loads saved outfits. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_OUTFIT_VIEW_MOBILE_06` - MAUI deserializes saved outfit JSON into the mobile Outfit model and renders cards. File: `maui_api/Models/Outfit.cs`

## FLOW_OUTFIT_WEAR
Web and MAUI outfit-only "Mark worn" history.

- `FLOW_OUTFIT_WEAR_WEB_01` - User/admin clicks Mark worn on a saved outfit card. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_WEAR_WEB_02` - Outfits page validates the visible outfit and active target user. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_WEAR_WEB_03` - OutfitWearLogDB inserts the current timestamp into `outfit_wear_logs`. File: `ConsoleApp1/OutfitWearLogDB.cs`
- `FLOW_OUTFIT_WEAR_MOBILE_01` - User/admin taps Mark worn on a MAUI outfit card. File: `maui_api/MainPage.xaml.cs`
- `FLOW_OUTFIT_WEAR_MOBILE_02` - MAUI sends actor user id, target user id, and outfit id to the API client. File: `maui_api/MainPage.xaml.cs`
- `FLOW_OUTFIT_WEAR_MOBILE_03` - LookLuxApiClient posts to `/api/mobile/outfits/{outfitId}/wear`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_OUTFIT_WEAR_MOBILE_03A` - MobileOutfitWearRequest carries actor/target ids into the backend endpoint. File: `gadifff/Services/MobileApiContracts.cs`
- `FLOW_OUTFIT_WEAR_MOBILE_04` - Program.cs checks actor access and outfit ownership. File: `gadifff/Program.cs`
- `FLOW_OUTFIT_WEAR_MOBILE_05` - OutfitWearLogDB inserts the current timestamp into `outfit_wear_logs`. File: `ConsoleApp1/OutfitWearLogDB.cs`
- `FLOW_OUTFIT_WEAR_DATA_01` - OutfitWearLog defines the data shape for one wear-history row. File: `ConsoleApp2/OutfitWearLog.cs`

## FLOW_OUTFIT_WEAR_STATS
Web and MAUI display of how many times an outfit was worn, plus first and last worn times.

- `FLOW_OUTFIT_WEAR_STATS_01` - Web Outfits page asks for summaries for currently loaded outfits. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_WEAR_STATS_02` - OutfitWearLogDB groups `outfit_wear_logs` by outfit id using count/min/max. File: `ConsoleApp1/OutfitWearLogDB.cs`
- `FLOW_OUTFIT_WEAR_STATS_03` - OutfitWearLog holds calculated summary values as the single outfit-wear model. File: `ConsoleApp2/OutfitWearLog.cs`
- `FLOW_OUTFIT_WEAR_STATS_04` - Summary values are copied onto Outfit models. File: `gadifff/Components/Pages/Outfits.razor`, `ConsoleApp2/Outfit.cs`, `maui_api/Models/Outfit.cs`
- `FLOW_OUTFIT_WEAR_STATS_05` - Web outfit card displays count, first worn, and last worn. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_WEAR_STATS_MOBILE_01` - Mobile outfit endpoint starts loading summary values for MAUI. File: `gadifff/Program.cs`
- `FLOW_OUTFIT_WEAR_STATS_MOBILE_04` - Program.cs copies summary values onto API Outfit objects. File: `gadifff/Program.cs`
- `FLOW_OUTFIT_WEAR_STATS_MOBILE_05` - MAUI outfit card displays count, first worn, and last worn. File: `maui_api/MainPage.xaml.cs`

## FLOW_OUTFIT_FILTER_WEB
Browser saved outfit filters.

- `FLOW_OUTFIT_FILTER_WEB_01` - User opens saved-outfit filter drawer. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_FILTER_WEB_02` - Outfits page sends selected filters to OutfitDB. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_OUTFIT_FILTER_WEB_03` - OutfitDB applies filters. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_OUTFIT_FILTER_WEB_04` - OutfitDB builds filter options. File: `ConsoleApp1/OutfitDB.cs`

## FLOW_DELETE_GARMENT_WEB / FLOW_DELETE_GARMENT_MOBILE
Delete one garment.

- `FLOW_DELETE_GARMENT_WEB_02` - Web confirmed delete. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_DELETE_GARMENT_WEB_04` - GarmentDB deletes garment. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_DELETE_GARMENT_MOBILE_01` - User taps Delete in MAUI. File: `maui_api/MainPage.xaml.cs`
- `FLOW_DELETE_GARMENT_MOBILE_03` - API client calls delete endpoint. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_DELETE_GARMENT_MOBILE_04` - Program.cs checks ownership. File: `gadifff/Program.cs`
- `FLOW_DELETE_GARMENT_MOBILE_05` - GarmentDB deletes garment. File: `ConsoleApp1/GarmentDB.cs`

## FLOW_DELETE_OUTFIT_WEB / FLOW_DELETE_OUTFIT_MOBILE
Delete one saved outfit.

- `FLOW_DELETE_OUTFIT_WEB_02` - Web confirmed delete. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_DELETE_OUTFIT_WEB_03` - OutfitGarmentDB deletes links. File: `ConsoleApp1/OutfitGarmentDB.cs`
- `FLOW_DELETE_OUTFIT_WEB_04` - OutfitDB deletes outfit. File: `ConsoleApp1/OutfitDB.cs`
- `FLOW_DELETE_OUTFIT_MOBILE_01` - User taps Delete outfit in MAUI. File: `maui_api/MainPage.xaml.cs`
- `FLOW_DELETE_OUTFIT_MOBILE_03` - API client calls delete endpoint. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_DELETE_OUTFIT_MOBILE_04` - Program.cs checks ownership and deletes links. File: `gadifff/Program.cs`, `ConsoleApp1/OutfitGarmentDB.cs`
- `FLOW_DELETE_OUTFIT_MOBILE_05` - OutfitDB deletes outfit. File: `ConsoleApp1/OutfitDB.cs`

## FLOW_DELETE_ACCOUNT_WEB / FLOW_DELETE_ACCOUNT_MOBILE
Delete signed-in account and all wardrobe data.

- `FLOW_DELETE_ACCOUNT_WEB_02` - Web user confirms delete account. File: `gadifff/Components/Layout/NavMenu.razor`
- `FLOW_DELETE_ACCOUNT_WEB_03` - Web cascade deletes links/outfits/garments. File: `gadifff/Components/Layout/NavMenu.razor`, `ConsoleApp1/OutfitGarmentDB.cs`, `ConsoleApp1/OutfitDB.cs`
- `FLOW_DELETE_ACCOUNT_WEB_04` - GarmentDB deletes garments. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_DELETE_ACCOUNT_WEB_05` - GarmentDB deletes one garment row during cascade cleanup. File: `ConsoleApp1/GarmentDB.cs`
- `FLOW_DELETE_ACCOUNT_WEB_06` - UserDB deletes user. File: `ConsoleApp1/UserDB.cs`
- `FLOW_DELETE_ACCOUNT_MOBILE_01` - User taps Delete account in MAUI. File: `maui_api/MainPage.xaml`
- `FLOW_DELETE_ACCOUNT_MOBILE_02` - MAUI shows warning confirmation. File: `maui_api/MainPage.xaml.cs`
- `FLOW_DELETE_ACCOUNT_MOBILE_03` - API client calls `/api/mobile/account`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_DELETE_ACCOUNT_MOBILE_04` - Program.cs validates and starts cascade. File: `gadifff/Program.cs`
- `FLOW_DELETE_ACCOUNT_MOBILE_05` - DB classes delete links/outfits/garments. Files: `ConsoleApp1/OutfitGarmentDB.cs`, `ConsoleApp1/OutfitDB.cs`, `ConsoleApp1/GarmentDB.cs`
- `FLOW_DELETE_ACCOUNT_MOBILE_06` - UserDB deletes user. File: `ConsoleApp1/UserDB.cs`

## FLOW_ADMIN_USER_MANAGE_WEB / FLOW_ADMIN_USER_MANAGE_MOBILE
Admin list/create/update/delete users.

- `FLOW_ADMIN_USER_MANAGE_WEB_02` - Web admin loads all accounts. File: `gadifff/Components/Pages/AdminClosets.razor`
- `FLOW_ADMIN_USER_MANAGE_WEB_03` - Web admin saves edited user. File: `gadifff/Components/Pages/AdminClosets.razor`
- `FLOW_ADMIN_USER_MANAGE_WEB_03A` - Web admin creates user. File: `gadifff/Components/Pages/AdminClosets.razor`
- `FLOW_ADMIN_USER_MANAGE_WEB_04` - UserDB inserts user. File: `ConsoleApp1/UserDB.cs`
- `FLOW_ADMIN_USER_MANAGE_WEB_05` - Web admin confirms delete. File: `gadifff/Components/Pages/AdminClosets.razor`
- `FLOW_ADMIN_USER_MANAGE_WEB_07` - UserDB deletes user. File: `ConsoleApp1/UserDB.cs`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_01` - Admin taps Create account. File: `maui_api/MainPage.xaml`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_02` - MAUI loads admin user list. File: `maui_api/MainPage.xaml.cs`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_03` - API client calls admin user endpoints. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_03A` - API client posts admin create-user data. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_04` - Program.cs checks admin permission. File: `gadifff/Program.cs`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_05` - UserDB inserts user. File: `ConsoleApp1/UserDB.cs`
- `FLOW_ADMIN_USER_MANAGE_MOBILE_07` - UserDB deletes user. File: `ConsoleApp1/UserDB.cs`

## FLOW_ADMIN_VIEW_CUSTOMER_WEB / FLOW_ADMIN_VIEW_CUSTOMER_MOBILE
Admin read-only viewing of a customer's garments/outfits.

- `FLOW_ADMIN_VIEW_CUSTOMER_WEB_02` - Web admin detail loads customer garments/outfits. File: `gadifff/Components/Pages/AdminClosetUser.razor`
- `FLOW_ADMIN_VIEW_CUSTOMER_WEB_03` - Closet.razor resolves asUserId for read-only closet. File: `gadifff/Components/Pages/Closet.razor`
- `FLOW_ADMIN_VIEW_CUSTOMER_WEB_03A` - Admin opens full closet page. File: `gadifff/Components/Pages/AdminClosetUser.razor`
- `FLOW_ADMIN_VIEW_CUSTOMER_WEB_04` - Outfits.razor resolves asUserId for read-only outfits. File: `gadifff/Components/Pages/Outfits.razor`
- `FLOW_ADMIN_VIEW_CUSTOMER_WEB_04A` - Admin opens full outfits page. File: `gadifff/Components/Pages/AdminClosetUser.razor`
- `FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_01` - Admin taps View data. File: `maui_api/MainPage.xaml.cs`
- `FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_02` - MAUI stores selected customer id and opens admin detail panel. File: `maui_api/MainPage.xaml.cs`
- `FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_04` - MAUI renders read-only customer garments/outfits inside Admin. File: `maui_api/MainPage.xaml.cs`
- `FLOW_ADMIN_VIEW_CUSTOMER_MOBILE_05` - Admin taps Back to return to customer list. File: `maui_api/MainPage.xaml`

## FLOW_DASHBOARD_COUNTS_MOBILE
MAUI dashboard totals and admin counts.

- `FLOW_DASHBOARD_COUNTS_MOBILE_01` - User lands on/taps Home in MAUI. File: `maui_api/MainPage.xaml`
- `FLOW_DASHBOARD_COUNTS_MOBILE_02` - MAUI home screen asks the API client for dashboard counts. File: `maui_api/MainPage.xaml.cs`
- `FLOW_DASHBOARD_COUNTS_MOBILE_03` - API client calls `/api/mobile/dashboard`. File: `maui_api/LookLuxApiClient.cs`
- `FLOW_DASHBOARD_COUNTS_MOBILE_04` - Program.cs builds customer/admin/garment/outfit counts. File: `gadifff/Program.cs`

## FLOW_LOGOUT_WEB / FLOW_LOGOUT_MOBILE
Logout.

- `FLOW_LOGOUT_WEB_01` - User clicks web logout. File: `gadifff/Components/Layout/NavMenu.razor`
- `FLOW_LOGOUT_MOBILE_01` - User taps MAUI logout. File: `maui_api/MainPage.xaml`

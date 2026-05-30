# LookLux Web Flow Map (Login -> Filters -> Recommendations)

## 1) Login Flow

### File
- `gadifff/Components/Pages/Login.razor`

### What this file does
- Renders the sign-in form and routes user into authenticated app features.

### Key functions
- `HandleLoginAsync()`
  - Calls `AuthService.LoginAsync(email, password)`.
  - On success, navigates to `/` (home/closet access).
  - On failure, shows `Invalid email or password`.
- `GoToRegister()`
  - Sends user to `/register`.

### Service behind login
- `gadifff/Services/AuthService.cs`
- `LoginAsync(...)`: validates credentials against `UserDB` and sets current session user.
- `LogoutAsync()`: clears current session user.
- `CurrentUserAsync()`: used by pages to gate access.

## 2) Closet Filters Flow

### File
- `gadifff/Components/Shared/MultiSelectFilter.razor`

### What this file does
- Generic reusable multi-select UI control for filters (color/brand/season/etc).
- It only edits selection state and notifies parent page; it does not query DB by itself.

### Key functions
- `ToggleOption(...)`
  - Adds/removes one selected value and emits `SelectedValuesChanged`.
- `ClearSelection()`
  - Clears this filter group and emits change event.
- `RemoveOption(...)`
  - Removes one selected chip and emits change event.

### Parent page using this behavior
- `gadifff/Components/Pages/Closet.razor`
- Parent receives change events and rebuilds filter request for garment refresh.

## 3) Recommendation (Matcher) Flow

### UI file
- `gadifff/Components/Shared/SlotMachineMatch.razor`

### What this file does
- Modal/panel for recommendation generation.
- Accepts current filtered garments and optional seed selections.
- Presents top suggestions and allows saving one as an outfit.

### Key functions
- `OnParametersSetAsync()`
  - Syncs incoming garments/seeds from Closet into matcher internal slot state.
- `RunMatch()`
  - Builds seed list from locked slots / selected items.
  - Calls `MatchingService.FindMatchesAsync(...)`.
  - Maps result into UI states: blocked, no-match, error, or suggestion list.
- `RunProgressAsync(...)`
  - UI progress animation while matching runs.
- `SaveSuggestionAsync(...)`
  - Calls `MatchingService.SaveOutfitSuggestionAsync(...)`.
  - Prevents duplicate saves and updates feedback per suggestion.
- `NormalizeTypeName(...)`
  - Normalizes garment type text into `shirt/pants/shoes` buckets.

### Service file
- `gadifff/Services/MatchingService.cs`

### What this file does
- Core recommendation engine coordinator.
- Validates closet readiness, builds combinations, scores combinations, and persists selected outfit suggestions.

### Key functions
- `CanMatchAsync(...)`
  - Verifies user has enough garment types to attempt matching.
- `FindMatchesAsync(...)`
  - Main matching pipeline (seed resolution, combination generation, scoring, fallback ranking, top results).
- `SaveOutfitSuggestionAsync(...)`
  - Persists selected recommendation into outfit tables and handles duplicate detection.
- `BuildCombinations(...)`
  - Produces valid shirt/pants/shoes combinations from pools + seeds.
- `ScoreCombinationsAsync(...)`
  - Sends candidate combinations to Gemini and parses scoring response.
- `ParseRequestIntent(...)`
  - Extracts prompt intent tokens (style/color/occasion) for request-aware fallback ranking.

# Podprojekt 0 – Fundamenty: plan implementacji

> **Dla agentów:** wymagany sub-skill: superpowers:executing-plans (lub superpowers:subagent-driven-development).
> Kroki mają checkboxy (`- [ ]`) do śledzenia postępu.

**Cel:** nowy szkielet BM2 na .NET 10 (Blazor WebAssembly + API w jednym hoście) z logowaniem Identity (cookie,
passkeys), bazą w trybie zgodności SQL Server 2012, testami i CI.

**Architektura:** szablon „Blazor Web App” (.NET 10, interaktywność WebAssembly, konta indywidualne) rozdzielony na
`BM2.Server` (host, Minimal API, EF Core, strony konta SSR) i `BM2.Client` (WASM). Obok czyste `BM2.Domain`
i `BM2.Contracts`. Stary kod trafia do `legacy/`.

**Stack:** .NET 10, ASP.NET Core Identity, EF Core 10 (SQL Server), Scalar, xUnit v3, Shouldly, Respawn, GitHub Actions.

**Spec:** [docs/specs/2026-10-24-podprojekt-0-fundamenty.md](../specs/2026-10-24-podprojekt-0-fundamenty.md)

## Ograniczenia globalne

- SDK `10.0.301` (`global.json`, `rollForward: latestFeature`), `net10.0`.
- Paczki tylko stabilne i tylko na licencjach MIT / Apache-2.0 / BSD. Zakaz: MediatR, AutoMapper, FluentAssertions.
- EF Core zawsze z `UseCompatibilityLevel(110)`; bez funkcji SQL Server 2016+.
- Kod, identyfikatory, komentarze i commity po angielsku; komentarze krótkie, tylko przy nieoczywistej logice.
- Teksty UI po angielsku (i18n w podprojekcie 1).
- Sekrety nigdy w repo.
- Commity na `dev` dozwolone; push tylko na prośbę użytkownika. Stopka commita:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Oszczędność: w zadaniach uruchamiaj tylko testy danego zadania; pełna suita na końcu (Task 8).

## Na co uważać w review

1. **Restart aplikacji (recykling puli IIS)** – sesje i tokeny muszą pozostać ważne → test w Task 3.
2. **Cookie usuniętego użytkownika** – `/api/me` ma zwrócić 401, nie 500 → test w Task 5.
3. **Nieznany endpoint `/api/...`** – ma zwrócić 404 jako `application/problem+json`, nie stronę HTML → test w Task 5.
4. **Strona logowania poza Development** – bez linku do rejestracji → test w Task 4.
5. **Brute force przez prawdziwy formularz logowania** – 5 złych haseł blokuje konto → test w Task 4.

## Mapa plików

```
global.json, Directory.Build.props, Directory.Packages.props, .editorconfig, .config/dotnet-tools.json, BM2.slnx
legacy/Directory.Build.props, legacy/Directory.Packages.props        odcięcie starego kodu od nowej konfiguracji
src/BM2.Domain/BM2.Domain.csproj
src/BM2.Contracts/BM2.Contracts.csproj, Me/MeResponse.cs
src/BM2.Server/
  Program.cs
  Data/ApplicationUser.cs, AppDbContext.cs, DatabaseSetup.cs, Migrations/ (+ .editorconfig)
  Infrastructure/ApiPaths.cs, AuthenticationSetup.cs, CurrentUser.cs, ErrorHandling.cs
  Features/ApiEndpoints.cs, Features/Me/GetMe.cs
  Components/** (z szablonu), wwwroot/app.css
src/BM2.Client/ (z szablonu) + Components/MeGreeting.razor
tests/BM2.UnitTests/ArchitectureTests.cs
tests/BM2.IntegrationTests/Infrastructure/{IntegrationFixture,BM2AppFactory,TestAuthHandler,IntegrationTest}.cs
tests/BM2.IntegrationTests/{DatabaseTests,Features/AuthenticationTests,Features/MeTests,Features/ApiErrorTests}.cs
.github/workflows/ci.yml, .github/dependabot.yml
```

---

### Task 1: legacy, konfiguracja budowania, Domain/Contracts, testy architektury

**Pliki:**
- Przenieś: `src/` → `legacy/src/`, `BM2.sln` → `legacy/BM2.sln` (`git mv`)
- Utwórz: `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`,
  `.config/dotnet-tools.json`, `BM2.slnx`, `legacy/Directory.Build.props`, `legacy/Directory.Packages.props`,
  `src/BM2.Domain/BM2.Domain.csproj`, `src/BM2.Contracts/BM2.Contracts.csproj`,
  `tests/BM2.UnitTests/BM2.UnitTests.csproj`, `tests/BM2.UnitTests/ArchitectureTests.cs`

**Interfejsy:**
- Tworzy: nazwy assembly `BM2.Domain`, `BM2.Contracts`; centralne wersje paczek dla kolejnych zadań.

- [ ] **Krok 1: przenieś stary kod.** `git mv src legacy/src` i `git mv BM2.sln legacy/BM2.sln`.
- [ ] **Krok 2: odetnij legacy od nowej konfiguracji.** `legacy/Directory.Build.props` = `<Project />`;
  `legacy/Directory.Packages.props` = `<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>`.
- [ ] **Krok 3: konfiguracja w katalogu głównym.**
  - `global.json`: `{"sdk": {"version": "10.0.301", "rollForward": "latestFeature"}}`.
  - `Directory.Build.props`: `TargetFramework=net10.0`, `Nullable=enable`, `ImplicitUsings=enable`,
    `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild=true`, `TreatWarningsAsErrors=true`
    z warunkiem `'$(CI)' == 'true'`.
  - `Directory.Packages.props`: `ManagePackageVersionsCentrally=true` i `PackageVersion`:
    `Microsoft.AspNetCore.Components.WebAssembly`, `...WebAssembly.Authentication`, `...WebAssembly.Server`,
    `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`,
    `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`, `Microsoft.AspNetCore.OpenApi`,
    `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
    – wszystkie `10.0.13`; `Scalar.AspNetCore 2.18.3`, `Microsoft.NET.Test.Sdk 18.11.0`, `xunit.v3 4.0.2`,
    `xunit.runner.visualstudio 4.0.1`, `Shouldly 4.3.0`, `Respawn 7.0.0`. Jeśli któraś wersja 10.0.13 nie istnieje
    przy restore, użyj najnowszej stabilnej 10.0.x.
  - `.editorconfig`: `root = true`; 4 spacje dla `*.cs`/`*.razor`, 2 dla `*.json`/`*.yml`/`*.props`/`*.csproj`;
    `csharp_style_namespace_declarations = file_scoped:warning`; `csharp_style_var_when_type_is_apparent = true:suggestion`.
  - `dotnet new tool-manifest` + `dotnet tool install dotnet-ef --version 10.0.13`.
- [ ] **Krok 4: projekty.** `BM2.Domain` i `BM2.Contracts` jako classlib bez `Class1.cs`, csproj bez właściwości
  dziedziczonych z `Directory.Build.props`; `BM2.Domain` → `ProjectReference` do `BM2.Contracts`.
  `BM2.UnitTests`: `OutputType=Exe`, `IsPackable=false`, paczki `xunit.v3`, `xunit.runner.visualstudio`,
  `Microsoft.NET.Test.Sdk`, `Shouldly`; referencje do Domain i Contracts. `dotnet new sln -n BM2 --format slnx`
  i `dotnet sln BM2.slnx add` dla trzech projektów.
- [ ] **Krok 5: testy architektury** (`BM2.UnitTests/ArchitectureTests.cs`). To strażnicy reguł, więc przechodzą od razu.

```csharp
public class ArchitectureTests
{
    [Fact]
    public void Domain_does_not_reference_ef_core_or_aspnetcore() =>
        References("BM2.Domain").ShouldNotContain(n =>
            n.StartsWith("Microsoft.EntityFrameworkCore") || n.StartsWith("Microsoft.AspNetCore"));

    [Fact]
    public void Domain_references_only_contracts_among_bm2_projects() =>
        References("BM2.Domain").Where(n => n.StartsWith("BM2.")).ShouldAllBe(n => n == "BM2.Contracts");

    [Fact]
    public void Contracts_does_not_reference_other_bm2_projects() =>
        References("BM2.Contracts").ShouldNotContain(n => n.StartsWith("BM2."));

    private static string[] References(string assembly) =>
        Assembly.Load(assembly).GetReferencedAssemblies().Select(a => a.Name!).ToArray();
}
```

- [ ] **Krok 6: uruchom.** `dotnet test tests/BM2.UnitTests` → 3 testy PASS, build bez ostrzeżeń.
- [ ] **Krok 7: commit.** `Move legacy code and add new solution skeleton`

### Task 2: serwer i klient z szablonu .NET 10

**Pliki:**
- Utwórz: `src/BM2.Server/**`, `src/BM2.Client/**` (z szablonu), `src/BM2.Server/Data/{ApplicationUser,AppDbContext,DatabaseSetup}.cs`,
  `src/BM2.Server/Data/Migrations/.editorconfig`, migracja `Initial`
- Usuń z szablonu: `Data/ApplicationDbContext.cs`, `Data/Migrations/00000000000000_*`, snapshot

**Interfejsy (tworzy):**
- `BM2.Server.Data.ApplicationUser : IdentityUser<Guid>`
- `BM2.Server.Data.AppDbContext(DbContextOptions<AppDbContext>) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IDataProtectionKeyContext`
  z `DbSet<DataProtectionKey> DataProtectionKeys`; `ConfigureConventions`: `decimal` → precyzja `(19, 4)`.
- `BM2.Server.Data.DatabaseSetup`: `public const int CompatibilityLevel = 110;`
  `public static Task PrepareAsync(AppDbContext db, CancellationToken ct = default)` – tworzy bazę, jeśli nie istnieje
  (`IRelationalDatabaseCreator`), ustawia `ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 110`, gdy poziom jest
  inny, potem `MigrateAsync`; `public static Task<int> GetCompatibilityLevelAsync(AppDbContext db, CancellationToken ct = default)`
  – `SELECT compatibility_level AS [Value] FROM sys.databases WHERE name = DB_NAME()`.
- Klucz konfiguracji: `ConnectionStrings:BM2`.

- [ ] **Krok 1: wygeneruj szablon** do katalogu tymczasowego (poza repo):
  `dotnet new blazor -n BM2 -o <tmp> --interactivity WebAssembly --all-interactive --auth Individual --use-local-db --empty --no-restore`.
  Skopiuj `<tmp>/BM2` → `src/BM2.Server` (csproj → `BM2.Server.csproj`) i `<tmp>/BM2.Client` → `src/BM2.Client`.
- [ ] **Krok 2: przestrzenie nazw serwera.** W `src/BM2.Server` (`*.cs`, `*.razor`) zamień `BM2.Components` →
  `BM2.Server.Components`, `BM2.Data` → `BM2.Server.Data`, `BM2.styles.css` → `BM2.Server.styles.css`.
  `BM2.Client` zostaje bez zmian.
- [ ] **Krok 3: csproj.** Usuń właściwości dziedziczone i atrybuty `Version`. Zostaw `UserSecretsId`
  i `BlazorDisableThrowNavigationException`.
  - Serwer: paczki `WebAssembly.Server`, `Diagnostics.EntityFrameworkCore`, `Identity.EntityFrameworkCore`,
    `DataProtection.EntityFrameworkCore`, `EntityFrameworkCore.SqlServer`, `EntityFrameworkCore.Design`
    (`PrivateAssets=all`), `OpenApi`, `Scalar.AspNetCore`; referencje do Client, Domain i Contracts.
  - Klient: referencja do Contracts.
- [ ] **Krok 4: dane.** `ApplicationUser`, `AppDbContext` i `DatabaseSetup` wg interfejsów.
- [ ] **Krok 5: `Program.cs`.**
  - `AddDbContext<AppDbContext>((sp, o) => o.UseSqlServer(cs, x => x.UseCompatibilityLevel(DatabaseSetup.CompatibilityLevel)))`,
    gdzie `cs` jest odczytywany z `sp.GetRequiredService<IConfiguration>().GetConnectionString("BM2")` (leniwie,
    żeby testy mogły go nadpisać); brak wartości → `InvalidOperationException`.
  - Identity: `AddEntityFrameworkStores<AppDbContext>()`. Data Protection:
    `AddDataProtection().PersistKeysToDbContext<AppDbContext>().SetApplicationName("BM2")`.
  - Po `Build()`, tylko w Development: utwórz scope i wywołaj `DatabaseSetup.PrepareAsync`.
  - Nie dodawaj `public partial class Program` – w .NET 10 generuje ją SDK.
- [ ] **Krok 6: konfiguracja.**
  - `appsettings.json` bez connection stringa.
  - `appsettings.Development.json`: `ConnectionStrings:BM2` =
    `Server=localhost\SQLEXPRESS;Database=BM2Dev;Integrated Security=True;TrustServerCertificate=True`.
  - `launchSettings.json`: tylko profil `https` (cookie `Secure` i passkeys wymagają HTTPS).
- [ ] **Krok 7: migracja.** `dotnet tool restore`, potem
  `dotnet ef migrations add Initial --project src/BM2.Server --output-dir Data/Migrations`.
  `Data/Migrations/.editorconfig`: `[*.cs]` + `generated_code = true`. Sprawdź w migracji: tabele `AspNetUserPasskeys`
  i `DataProtectionKeys`, kolumny `Id` typu `uniqueidentifier`.
- [ ] **Krok 8: build.** `dotnet sln BM2.slnx add src/BM2.Server src/BM2.Client`, potem `dotnet build BM2.slnx`.
  Ostrzeżenia w naszym kodzie poprawiamy. Ostrzeżenia analizatorów w kodzie szablonu (`Components/Account`)
  wyłączamy per reguła w `src/BM2.Server/Components/Account/.editorconfig` (`dotnet_diagnostic.<ID>.severity = none`,
  z komentarzem, że to kod szablonu).
- [ ] **Krok 9: uruchom.** `dotnet run --project src/BM2.Server --launch-profile https` → aplikacja startuje, baza
  `BM2Dev` powstaje. Sprawdź `sqlcmd -S localhost\SQLEXPRESS -E -C -Q "SELECT compatibility_level FROM sys.databases WHERE name='BM2Dev'"`
  → `110`. Zatrzymaj aplikację.
- [ ] **Krok 10: commit.** `Add server and client from .NET 10 Blazor template`

### Task 3: infrastruktura testów integracyjnych

**Pliki:**
- Utwórz: `tests/BM2.IntegrationTests/BM2.IntegrationTests.csproj`,
  `Infrastructure/{IntegrationFixture,BM2AppFactory,TestAuthHandler,IntegrationTest}.cs`, `DatabaseTests.cs`

**Interfejsy:**
- Korzysta z: `AppDbContext`, `DatabaseSetup` (Task 2).
- Tworzy:
  - `IntegrationCollection` (`[CollectionDefinition(Name)]`, `ICollectionFixture<IntegrationFixture>`,
    `public const string Name = "Integration"`). Wszystkie testy integracyjne są w tej kolekcji, więc wykonują się
    sekwencyjnie.
  - `IntegrationFixture : IAsyncLifetime` – `string ConnectionString`, `BM2AppFactory Factory`,
    `BM2AppFactory CreateFactory()`, `Task ResetDatabaseAsync()`. Bazowy connection string z env `BM2_TEST_SQL`
    (domyślnie `Server=localhost\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True`), baza
    `BM2_Test_<guid:N>`, `DatabaseSetup.PrepareAsync`. Respawn (`DbAdapter.SqlServer`) z `TablesToIgnore`:
    `__EFMigrationsHistory`, `DataProtectionKeys`. Na koniec `SqlConnection.ClearAllPools()` + `EnsureDeletedAsync()`.
  - `BM2AppFactory(string connectionString) : WebApplicationFactory<Program>` – środowisko `Testing`,
    `UseSetting("ConnectionStrings:BM2", ...)`. W `ConfigureTestServices` dodaje schemat `TestAuthHandler` i policy
    scheme `TestOrIdentity` (gdy jest nagłówek `X-Test-UserId`, przekazuje do `Test`, w przeciwnym razie do
    `IdentityConstants.ApplicationScheme`) jako `DefaultScheme`/`DefaultAuthenticateScheme`/`DefaultChallengeScheme`.
    Metody `HttpClient CreateAnonymousClient()` i `HttpClient CreateClientAs(Guid userId)` – `BaseAddress = https://localhost`,
    `AllowAutoRedirect = false`.
  - `TestAuthHandler` – `SchemeName = "Test"`, `UserIdHeader = "X-Test-UserId"`; claim `NameIdentifier` z nagłówka.
  - `abstract class IntegrationTest(IntegrationFixture fixture) : IAsyncLifetime` – `[Collection(IntegrationCollection.Name)]`,
    `InitializeAsync` → `ResetDatabaseAsync`; `protected const string DefaultPassword = "correct-horse-battery-staple"`;
    `protected Task<ApplicationUser> CreateUserAsync(string email, string password = DefaultPassword)` (przez
    `UserManager`, `EmailConfirmed = true`, `UserName = email`).

- [ ] **Krok 1: csproj.** `OutputType=Exe`, `IsPackable=false`, paczki `xunit.v3`, runner, `Test.Sdk`, `Shouldly`,
  `Mvc.Testing`, `Respawn`; referencja do `BM2.Server`; dodaj do `BM2.slnx`.
- [ ] **Krok 2: infrastruktura** wg interfejsów. Jeśli start fabryki rzuca wyjątek o brakującym manifeście
  statycznych zasobów, dodaj w fabryce `builder.UseStaticWebAssets()`.
- [ ] **Krok 3: testy** (`DatabaseTests.cs`):
  - `Test_database_runs_in_sql_server_2012_compatibility_level` – `GetCompatibilityLevelAsync` → `110`.
  - `Data_protection_keys_survive_app_restart` – fabryka A (`CreateFactory()`) chroni `"hello"` protektorem
    `"restart-test"`; dispose A; fabryka B odszyfrowuje → `"hello"`; liczba wierszy `DataProtectionKeys` > 0.
- [ ] **Krok 4: uruchom.** `dotnet test tests/BM2.IntegrationTests` → 2 PASS (oba weryfikują zadanie 2).
- [ ] **Krok 5: commit.** `Add integration test infrastructure`

### Task 4: utwardzenie logowania

**Pliki:**
- Utwórz: `src/BM2.Server/Infrastructure/ApiPaths.cs`, `src/BM2.Server/Infrastructure/AuthenticationSetup.cs`,
  `tests/BM2.IntegrationTests/Features/AuthenticationTests.cs`
- Zmień: `Program.cs` (konfiguracja auth z szablonu → `AddBm2Authentication()`), `Components/Account/Pages/Login.razor`,
  `Register.razor`, `RegisterConfirmation.razor`

**Interfejsy (tworzy):**
- `ApiPaths`: `public const string Prefix = "/api";` oraz `public static bool IsApiRequest(this HttpRequest request)`.
- `AuthenticationSetup.AddBm2Authentication(this IServiceCollection services) : IServiceCollection` – przenosi
  z `Program.cs` uwierzytelnianie, Identity i Data Protection oraz ustawia:
  - hasło: `RequiredLength = 12`, `RequireDigit`, `RequireLowercase`, `RequireUppercase` i `RequireNonAlphanumeric` = `false`;
  - blokada: `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min`, `AllowedForNewUsers = true`;
  - `RequireConfirmedAccount = true`, `Stores.SchemaVersion = IdentitySchemaVersions.Version3`;
  - cookie aplikacji: `HttpOnly`, `SecurePolicy = Always`, `SameSite = Lax`, `ExpireTimeSpan = 30 dni`,
    `SlidingExpiration = true`; `OnRedirectToLogin` → 401 i `OnRedirectToAccessDenied` → 403, gdy
    `IsApiRequest()`, w pozostałych przypadkach domyślne przekierowanie.

- [ ] **Krok 1: testy** (`AuthenticationTests : IntegrationTest`), wszystkie w środowisku `Testing`:
  - `Identity_options_match_security_policy` – `IOptions<IdentityOptions>`: `RequiredLength == 12`,
    `MaxFailedAccessAttempts == 5`, `DefaultLockoutTimeSpan == 15 min`, wymogi złożoności = `false`.
  - `Application_cookie_is_long_lived_and_secure` – `IOptionsMonitor<CookieAuthenticationOptions>.Get(IdentityConstants.ApplicationScheme)`:
    30 dni, sliding, `HttpOnly`, `SecurePolicy.Always`, `SameSiteMode.Lax`.
  - `Registration_page_is_not_available_outside_development` – GET `/Account/Register` → 404.
  - `Login_page_has_no_register_link_outside_development` – GET `/Account/Login` → body nie zawiera `Account/Register`.
  - `Five_failed_logins_lock_the_account`:
    - `CreateUserAsync("lock@example.com")`, potem 5 razy: GET `/Account/Login`, wyciągnij
      `__RequestVerificationToken` (regex `__RequestVerificationToken[^>]*value="([^"]+)"`) i wyślij POST
      `/Account/Login` (form: `_handler=login`, token, `Input.Email`, `Input.Password=wrong-password-123`,
      `Input.RememberMe=false`) klientem anonimowym (cookies włączone);
    - na koniec `UserManager.IsLockedOutAsync(user)` → `true`.
- [ ] **Krok 2: uruchom.** `dotnet test tests/BM2.IntegrationTests --filter "FullyQualifiedName~AuthenticationTests"`
  → FAIL (opcje domyślne, rejestracja dostępna, `lockoutOnFailure: false`).
- [ ] **Krok 3: implementacja.** `ApiPaths` i `AuthenticationSetup` wg interfejsów.
  - `Login.razor`: `lockoutOnFailure: true`; link do rejestracji tylko, gdy `IWebHostEnvironment.IsDevelopment()`.
  - `Register.razor` i `RegisterConfirmation.razor`: na początku inicjalizacji, poza Development →
    `NavigationManager.NotFound()` i return.
- [ ] **Krok 4: uruchom ponownie** → 5 PASS.
- [ ] **Krok 5: commit.** `Harden Identity options and limit registration to development`

### Task 5: API – `/api/me`, `ICurrentUser`, ProblemDetails

**Pliki:**
- Utwórz: `src/BM2.Contracts/Me/MeResponse.cs`, `src/BM2.Server/Infrastructure/CurrentUser.cs`,
  `src/BM2.Server/Infrastructure/ErrorHandling.cs`, `src/BM2.Server/Features/ApiEndpoints.cs`,
  `src/BM2.Server/Features/Me/GetMe.cs`, `tests/BM2.IntegrationTests/Features/{MeTests,ApiErrorTests}.cs`
- Zmień: `Program.cs`

**Interfejsy:**
- Korzysta z: `ApiPaths` (Task 4), `AppDbContext` (Task 2).
- Tworzy:
  - `BM2.Contracts.Me.MeResponse` – `public sealed record MeResponse(Guid Id, string Email);`
  - `ICurrentUser { Guid UserId { get; } }` + `HttpContextCurrentUser(IHttpContextAccessor)` (claim
    `NameIdentifier`; brak claimu lub zły format → `InvalidOperationException`); rejestracja scoped
    + `AddHttpContextAccessor()`.
  - `ErrorHandling.UseBm2ErrorHandling(this WebApplication app) : WebApplication` – zastępuje linie
    `UseExceptionHandler` / `UseStatusCodePagesWithReExecute` z szablonu (HSTS zostaje w `Program.cs`):
    - gałąź `IsApiRequest()`: `UseExceptionHandler()` + `UseStatusCodePages()` (ProblemDetails dzięki `AddProblemDetails()`);
    - pozostałe ścieżki: poza Development `UseExceptionHandler("/Error", createScopeForErrors: true)`, zawsze
      `UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true)`.
  - `ApiEndpoints.MapBm2Api(this WebApplication app) : RouteGroupBuilder` – `MapGroup(ApiPaths.Prefix).RequireAuthorization()`
    + `.MapMeEndpoints()`, oraz `app.MapFallback($"{ApiPaths.Prefix}/{{**path}}", () => TypedResults.NotFound())`.
  - `GetMe.MapMeEndpoints(this RouteGroupBuilder api) : RouteGroupBuilder` – `GET /me` →
    `Results<Ok<MeResponse>, UnauthorizedHttpResult>`; użytkownik szukany `AsNoTracking` po `ICurrentUser.UserId`;
    brak użytkownika → `Unauthorized`.
  - OpenAPI: `AddOpenApi()`, w Development `MapOpenApi()` i `MapScalarApiReference()`.

- [ ] **Krok 1: testy.**
  - `MeTests`:
    - `Anonymous_request_gets_401_without_redirect` – anonimowy GET `/api/me` → 401, `Headers.Location` = null;
    - `Authenticated_user_gets_own_email` – `CreateUserAsync("ala@example.com")`, `CreateClientAs(user.Id)` → 200
      i `MeResponse(user.Id, "ala@example.com")`;
    - `Cookie_for_deleted_user_gets_401` – `CreateClientAs(Guid.NewGuid())` → 401.
  - `ApiErrorTests`:
    - `Unknown_api_route_returns_problem_details` – anonimowy GET `/api/does-not-exist` → 404,
      `Content-Type` = `application/problem+json`.
- [ ] **Krok 2: uruchom.** `--filter "FullyQualifiedName~MeTests|FullyQualifiedName~ApiErrorTests"` → FAIL.
- [ ] **Krok 3: implementacja** wg interfejsów; w `Program.cs` `AddProblemDetails()`, `UseBm2ErrorHandling()`,
  `MapBm2Api()` (przed `MapRazorComponents`).
- [ ] **Krok 4: uruchom ponownie** → 4 PASS; dla pewności też `AuthenticationTests` → PASS.
- [ ] **Krok 5: commit.** `Add /api/me endpoint with ProblemDetails error handling`

### Task 6: klient – strona startowa i wylogowanie

**Pliki:**
- Utwórz: `src/BM2.Client/Components/MeGreeting.razor`
- Zmień: `src/BM2.Client/Program.cs`, `Pages/Home.razor`, `Layout/MainLayout.razor`, `_Imports.razor`,
  `src/BM2.Server/wwwroot/app.css`

**Interfejsy:**
- Korzysta z: `MeResponse`, `GET /api/me` (Task 5).

- [ ] **Krok 1: klient HTTP.** `Program.cs` klienta:
  `AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })`.
  W `_Imports.razor` dodaj `@using BM2.Contracts.Me` i `@using BM2.Client.Components`.
- [ ] **Krok 2: komponenty.**
  - `MeGreeting.razor`: pobiera `api/me` (`GetFromJsonAsync<MeResponse>`) i pokazuje `Welcome, {Email}`.
  - `Home.razor`: w `<AuthorizeView>` zalogowany widzi `MeGreeting`, gdy `RendererInfo.IsInteractive`, inaczej
    `Loading…`; niezalogowany widzi link `Log in` → `Account/Login`.
  - `MainLayout.razor`: dla zalogowanego link `Account/Manage` („Account”) i formularz wylogowania –
    `<form action="Account/Logout" method="post">` z `<AntiforgeryToken />`, ukrytym `ReturnUrl=""` i przyciskiem `Log out`.
- [ ] **Krok 3: `app.css`.** Zaczątek tokenów: `--color-bg`, `--color-fg`, `--color-accent`, `--space-1..4`,
  `--radius`, systemowy stos fontów; wariant `prefers-color-scheme: dark`; widoczny `:focus-visible`; podstawowe
  style dla `form-control`/`btn` używanych przez strony konta.
- [ ] **Krok 4: weryfikacja ręczna.** Uruchom (`--launch-profile https`). Przejdź: rejestracja → potwierdzenie
  linkiem z `RegisterConfirmation` → logowanie → „Welcome, …” → `Account/Manage/Passkeys` (dodaj passkey) →
  wylogowanie → logowanie passkeyem → wylogowanie. Podaj link użytkownikowi do samodzielnego testu.
- [ ] **Krok 5: commit.** `Add client home page with current user greeting`

### Task 7: CI i Dependabot

**Pliki:**
- Utwórz: `.github/workflows/ci.yml`, `.github/dependabot.yml`

- [ ] **Krok 1: `ci.yml`.**
  - Wyzwalacze: `push` na `dev` i `master`, `pull_request`. Job `build` na `ubuntu-latest`.
  - Usługa `mssql`: `mcr.microsoft.com/mssql/server:2022-latest`, `ACCEPT_EULA=Y`, `MSSQL_SA_PASSWORD=Ci_Only_Pa55w0rd!`
    (komentarz: kontener jednorazowy), port `1433:1433`, health check
    `/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Ci_Only_Pa55w0rd!' -C -Q 'SELECT 1'` (10 prób co 10 s).
  - Env: `BM2_TEST_SQL=Server=localhost,1433;User Id=sa;Password=Ci_Only_Pa55w0rd!;TrustServerCertificate=True`.
  - Kroki:
    - `actions/checkout@v4`, `actions/setup-dotnet@v4` (`global-json-file: global.json`);
    - `dotnet restore BM2.slnx`, `dotnet build BM2.slnx -c Release --no-restore`, `dotnet test BM2.slnx -c Release --no-build`;
    - `dotnet list BM2.slnx package --vulnerable --include-transitive` z zapisem do pliku; obecność
      `has the following vulnerable packages` = błąd.
- [ ] **Krok 2: `dependabot.yml`.** Ekosystemy `nuget` (katalog `/`) i `github-actions`, `interval: weekly`.
- [ ] **Krok 3: commit.** `Add CI workflow and Dependabot`
- [ ] **Krok 4: weryfikacja.** Poproś użytkownika o zgodę na push `dev`. Po pushu `gh run watch` → job zielony.

### Task 8: AGENTS.md i odbiór

**Pliki:**
- Zmień: `AGENTS.md`, `docs/roadmap.md`

- [ ] **Krok 1: AGENTS.md.** Odnośnik do speca w sekcji „Stack i architektura” zastąp skrótem:
  - projekty i dozwolone referencje;
  - foldery per funkcja, bez MediatR i repozytoriów, `TypedResults` + ProblemDetails, `ICurrentUser`, grupa `/api`;
  - testy integracyjne: kolekcja `Integration`, `BM2_TEST_SQL`, SQLEXPRESS lokalnie;
  - narzędzia lokalne przez `dotnet tool restore`.
- [ ] **Krok 2: odbiór.**
  - `dotnet build BM2.slnx -c Release` → 0 ostrzeżeń;
  - `dotnet test BM2.slnx` → wszystkie PASS;
  - przegląd `Directory.Packages.props` pod kątem licencji i wersji stabilnych.
- [ ] **Krok 3: roadmapa.** Podprojekt 0 oznacz jako ukończony z datą.
- [ ] **Krok 4: commit.** `Document skeleton conventions and close sub-project 0`

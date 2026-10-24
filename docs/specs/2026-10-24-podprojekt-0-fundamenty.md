# Podprojekt 0 – Fundamenty: nowy szkielet

Data: 2026-10-24 · Status: do przeglądu · Roadmapa: [docs/roadmap.md](../roadmap.md)

## Cel

Postawić nową, czystą strukturę BM2 na .NET 10 (Blazor WebAssembly + API w jednym hoście) z działającym logowaniem,
bazą w trybie zgodności SQL Server 2012, testami i CI. Na tym szkielecie w podprojekcie 1 budujemy design system
i przenosimy funkcje ze starego kodu.

## Zakres

- Przeniesienie starego kodu do `legacy/`.
- Nowa solucja `BM2.slnx` z projektami `BM2.Domain`, `BM2.Contracts`, `BM2.Server`, `BM2.Client` i dwoma projektami testów.
- Wspólna konfiguracja budowania: `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`.
- ASP.NET Core Identity z szablonu .NET 10: strony konta (SSR), passkeys, cookie.
- EF Core 10 na SQL Server w trybie zgodności 110, migracja `Initial`.
- Przykładowy chroniony endpoint `GET /api/me` i strona WASM, która go woła.
- Testy jednostkowe i integracyjne, GitHub Actions, Dependabot.
- Aktualizacja AGENTS.md o strukturę i konwencje.

## Poza zakresem

- Design system, shell aplikacji, i18n, encje domenowe, przenoszenie funkcji, model członkostwa i zaproszeń,
  antiforgery dla API, rate limiting, CSP – podprojekt 1.
- Prawdziwa wysyłka e-maili (SMTP) – później.
- Wdrożenie na Webio – podprojekt 5.

## Struktura repo

```
BM2.slnx
global.json
Directory.Build.props
Directory.Packages.props
.editorconfig
.github/
  workflows/ci.yml
  dependabot.yml
src/
  BM2.Domain/
  BM2.Contracts/
  BM2.Server/
  BM2.Client/
tests/
  BM2.UnitTests/
  BM2.IntegrationTests/
legacy/
  BM2.sln
  src/            stary kod (git mv – historia zostaje)
docs/
AGENTS.md
CLAUDE.md
```

`legacy/` nie należy do `BM2.slnx` i nie jest budowany w CI. Służy wyłącznie jako źródło do przenoszenia (tylko odczyt);
usuwamy go na końcu podprojektu 1.

## Projekty i zależności

| Projekt | Referencje | Zawartość |
|---------|------------|-----------|
| `BM2.Domain` | `BM2.Contracts` (tylko wspólne enumy i stałe) | Encje i logika domenowa, czysty C#, bez EF i ASP.NET. |
| `BM2.Contracts` | – | DTO, requesty, walidatory FluentValidation, wspólne enumy. Używane przez klienta i serwer. |
| `BM2.Server` | `BM2.Domain`, `BM2.Contracts`, `BM2.Client` | Host: Minimal API, EF Core, Identity, strony konta (SSR), serwowanie WASM. |
| `BM2.Client` | `BM2.Contracts` | Blazor WebAssembly. Bez biblioteki komponentów (własny design system powstanie w podprojekcie 1). |
| `BM2.UnitTests` | `BM2.Domain`, `BM2.Contracts` | Testy domeny i walidatorów. |
| `BM2.IntegrationTests` | `BM2.Server` | Testy API na prawdziwym SQL Server. |

Klient nigdy nie referencjonuje `BM2.Domain` ani `BM2.Server` – nie zna encji.

## Konwencje kodu serwera

- **Foldery per funkcja:** `Features/<Obszar>/<Akcja>.cs` – endpoint i jego logika w jednym pliku
  (np. `Features/Me/GetMe.cs`). Każdy obszar ma metodę rozszerzającą `Map<Obszar>Endpoints(this RouteGroupBuilder)`.
- **Bez MediatR i bez warstwy repozytoriów.** Endpoint wstrzykuje `AppDbContext` i potrzebne serwisy. Złożona logika
  trafia do domeny (testy jednostkowe) albo do serwisu w folderze funkcji.
- **Wyniki:** endpointy zwracają `TypedResults` (`Results<Ok<T>, NotFound, ValidationProblem>` itp.). Błędy biznesowe
  z domeny jako lekki typ `Result`, mapowany w endpoincie.
- **Walidacja:** walidatory FluentValidation w `BM2.Contracts`; na serwerze uruchamia je filtr endpointu
  (`ValidationFilter<T>`) i zwraca `ValidationProblem`. Klient używa tych samych walidatorów w formularzach.
- **Błędy:** `AddProblemDetails()`, `UseExceptionHandler()`, `UseStatusCodePages()`. Poza Development bez szczegółów
  wyjątków. Wyjątki logowane, nigdy połykane.
- **Bieżący użytkownik:** serwis `ICurrentUser` (Id i e-mail z claimów).
- **Grupa API:** `app.MapGroup("/api").RequireAuthorization()`; publiczne endpointy jawnie przez `AllowAnonymous()`.
- **OpenAPI:** wbudowane `Microsoft.AspNetCore.OpenApi` + Scalar UI, tylko w Development.

## Uwierzytelnianie

- Szablon .NET 10 „Blazor Web App” z kontami indywidualnymi: strony konta w `BM2.Server/Components/Account`
  renderowane statycznie (SSR), z wbudowanym antiforgery. Na razie ostylowane minimalnie, docelowy wygląd w podprojekcie 1.
- `ApplicationUser : IdentityUser<Guid>`, role `IdentityRole<Guid>`.
- **Passkeys** włączone (logowanie odciskiem palca / Face ID). Hasło zostaje jako zapasowa metoda.
- **Cookie:** `HttpOnly`, `SecurePolicy = Always`, `SameSite = Lax`, `ExpireTimeSpan = 30 dni`, `SlidingExpiration = true`.
  „Zapamiętaj mnie” daje trwałe cookie.
- **Hasło:** minimum 12 znaków, bez wymogów złożoności (zgodnie z zaleceniami NIST/ASVS). **Blokada:** 5 nieudanych
  prób → 15 minut.
- **Rejestracja** dostępna tylko w Development; poza nim strona i endpoint rejestracji zwracają 404.
  Zaproszenia – podprojekt 1.
- `RequireConfirmedAccount = true`; e-maile przez no-op sender z szablonu (link potwierdzający widoczny lokalnie).
- **Stan logowania w WASM:** `AddAuthenticationStateSerialization()` na serwerze,
  `AddAuthenticationStateDeserialization()` w kliencie.
- **API a przekierowania:** w .NET 10 endpointy API dostają 401/403 zamiast przekierowania na stronę logowania.
  Potwierdzamy to testem integracyjnym.
- Klient woła API przez `HttpClient` z `BaseAddress` = adres hosta; cookie idzie automatycznie (same-origin).

## Dane

- `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`.
- `UseSqlServer(connectionString, o => o.UseCompatibilityLevel(110))`.
- Konwencja: `decimal` domyślnie `decimal(19,4)` (`ConfigureConventions`).
- Klucze `Guid` generowane przez EF (sekwencyjne GUID-y przyjazne dla SQL Server).
- Migracja `Initial`: tabele Identity (schemat v3, z passkeys) i tabela kluczy Data Protection. Encje domenowe
  dochodzą w kolejnych podprojektach.
- **Klucze Data Protection** (szyfrują cookie) zapisywane w bazie: `PersistKeysToDbContext<AppDbContext>()`,
  `SetApplicationName("BM2")`. Bez tego restart puli IIS unieważniałby sesje. Szyfrowanie kluczy w spoczynku –
  do rozważenia przy wdrożeniu (podprojekt 5).
- **Development:** baza `BM2Dev` na `localhost\SQLEXPRESS` (connection string w `appsettings.Development.json`,
  Integrated Security – nie jest sekretem). Przy starcie: `MigrateAsync()` oraz `DevDatabaseInitializer`, który
  ustawia `COMPATIBILITY_LEVEL = 110`, jeśli jest inny. Stara baza `BM2` zostaje nietknięta (możesz ją usunąć ręcznie).
- **Produkcja:** brak automatycznych migracji przy starcie; idempotentny skrypt SQL przy wdrożeniu (podprojekt 5).
- Sekrety (jeśli się pojawią) w user-secrets projektu `BM2.Server`.

## Klient (szkielet)

- Minimalny layout i strona startowa `/`: dla zalogowanego „Witaj, {e-mail}” na podstawie `GET /api/me`,
  dla niezalogowanego link do logowania.
- Strony wymagające logowania przez `[Authorize]`; niezalogowany trafia na `/Account/Login`.
- Plik `wwwroot/css/app.css` jako zaczątek tokenów CSS (pełny design system w podprojekcie 1).

## Testy

- **Wspólne:** xUnit v3 + Shouldly.
- **`BM2.UnitTests`:** na start testy architektury (na podstawie referencji assembly):
  `BM2.Domain` nie referencjonuje EF Core ani ASP.NET Core i z projektów BM2 zna tylko `BM2.Contracts`;
  `BM2.Contracts` nie referencjonuje żadnego innego projektu BM2.
- **`BM2.IntegrationTests`:** `WebApplicationFactory<Program>` + Respawn.
  - Fixture tworzy bazę `BM2_Test_<guid>` na serwerze z konfiguracji (`BM2_TEST_SQL`; domyślnie
    `localhost\SQLEXPRESS`), stosuje migracje, ustawia `COMPATIBILITY_LEVEL = 110`, a po przebiegu usuwa bazę.
  - Respawn czyści dane między testami (z pominięciem `__EFMigrationsHistory`).
  - Uwierzytelnianie w testach API: testowy schemat (`TestAuthHandler`) z claimami użytkownika utworzonego w bazie.
  - Testy na start:
    1. `GET /api/me` bez uwierzytelnienia → 401 (nie przekierowanie).
    2. `GET /api/me` jako użytkownik testowy → 200 z jego e-mailem.
    3. Baza testowa ma poziom zgodności 110.
- Testy integracyjne wymagają SQL Server: lokalnie SQLEXPRESS, w CI kontener.

## Narzędzia

- **`global.json`:** SDK `10.0.301`, `rollForward: latestFeature`.
- **`Directory.Build.props`:** `net10.0`, `Nullable`, `ImplicitUsings`, `AnalysisLevel = latest-recommended`,
  `EnforceCodeStyleInBuild = true`; `TreatWarningsAsErrors = true`, gdy `CI == true` (GitHub Actions ustawia `CI`).
- **`Directory.Packages.props`:** centralne wersje paczek; tylko wersje stabilne.
- **`.editorconfig`:** styl C# (file-scoped namespaces, `var` gdy typ oczywisty, nazewnictwo).
- **Licencje paczek** (wszystkie darmowe także komercyjnie):
  - MIT: EF Core (z `Microsoft.Data.SqlClient`), ASP.NET Core Identity, OpenApi, Data Protection EF Core,
    `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.NET.Test.Sdk`, Scalar.AspNetCore;
  - Apache-2.0: FluentValidation (+ `DependencyInjectionExtensions`), xUnit v3 (+ runner), Respawn;
  - BSD: Shouldly.

## CI

- **`.github/workflows/ci.yml`:** push na `dev` i `master`, pull requesty.
  - `ubuntu-latest`, kontener usługowy `mcr.microsoft.com/mssql/server:2022-latest` (jednorazowe hasło SA w pliku
    workflow – kontener istnieje tylko w czasie joba).
  - Kroki: checkout → setup-dotnet (z `global.json`) → restore → build `Release` → test (z `BM2_TEST_SQL` wskazującym
    na kontener) → kontrola podatnych paczek (`dotnet list package --vulnerable --include-transitive`; znalezisko
    = błąd).
- **`.github/dependabot.yml`:** NuGet i GitHub Actions, co tydzień.

## AGENTS.md

Uzupełnić sekcję „Stack i architektura” o strukturę projektów, konwencje serwera (wyżej) i zasadę, że `legacy/`
jest tylko do odczytu.

## Kryteria akceptacji

1. `dotnet build BM2.slnx -c Release` przechodzi bez ostrzeżeń.
2. `dotnet test BM2.slnx` przechodzi lokalnie (SQLEXPRESS) i w CI.
3. Lokalnie: rejestracja (Development), logowanie hasłem i passkeyem, strona WASM pokazuje e-mail z `/api/me`,
   wylogowanie działa.
4. `GET /api/me` bez cookie zwraca 401.
5. Baza `BM2Dev` ma poziom zgodności 110.
6. Brak paczek preview/beta i paczek o licencji niepozwalającej na darmowe użycie komercyjne.

# AGENTS.md – wytyczne dla agentów (BM2)

Kontekst, decyzje i kolejność prac: [docs/roadmap.md](docs/roadmap.md). Specy podprojektów: `docs/specs/`.
Aktualizuj ten plik, gdy zapadnie nowa decyzja dotycząca konwencji.

## Kontekst

- BM2 to v2 aplikacji MoneyManager (repo `C:\Users\<user>\source\repos\MoneyManager`, wdrożona na Webio).
- Aplikacja do finansów osobistych: prywatna + współdzielone portfele/konta (zaproszenia). Architektura ma nie blokować
  wersji publicznej w przyszłości.
- Główne urządzenie: telefon (PWA na ekranie głównym). Priorytet: szybkość i szybkie dodawanie rekordów.

## Zasady pracy

- **Oszczędzaj tokeny:** bez zbędnych buildów, uruchomień testów i szerokiego czytania plików. Droższy krok (pełny build,
  cała suita testów, duże przeszukiwanie, subagent) – zaproponuj i poczekaj na zgodę.
- **Decyzje:** proste decyzje i wątpliwości rozstrzygaj sam, wybierając najbardziej optymalny darmowy wariant
  (i krótko je odnotuj). Dopytuj tylko przy istotnych decyzjach produktowych/architektonicznych.
- Gdy powstaje działające demo – uruchom je lokalnie i podaj link do przetestowania.
- Każdy podprojekt: spec → plan → implementacja. Bez kodu produkcyjnego przed zatwierdzeniem speca.
- Commity na `dev` są dozwolone bez pytania (gałąź robocza `dev`, główna `master`). Push, merge do `master`
  i operacje nieodwracalne – tylko na wyraźną prośbę.
- **Język:** rozmowa i dokumentacja (`docs/`) po polsku; kod, identyfikatory, komentarze i commity po angielsku
  (commity w trybie rozkazującym, np. „Add …”). Komentarze krótkie i tylko tam, gdzie logika nie jest oczywista.

## Stack i architektura

- .NET: najnowsza wersja wspierana przez Webio (obecnie .NET 10).
- Front: Blazor WebAssembly (PWA) na własnym design systemie – bez biblioteki komponentów. Do list/tabel `QuickGrid`
  i `Virtualize`, wykresy Apache ECharts. Tylko online.
- Backend: ASP.NET Core API w tym samym hoście (same-origin), uwierzytelnianie cookie (ASP.NET Core Identity).
- Baza: SQL Server przez EF Core z `UseCompatibilityLevel(110)` – produkcja to **MSSQL 2012**. Nie używaj funkcji
  SQL Server 2016+ (OPENJSON / kolumny JSON, `STRING_AGG`, temporal tables itp.).
- Brak zależności od WebSocketów i od ciągłego działania procesu (pula IIS może usypiać). Zadania cykliczne jako
  idempotentny catch-up.
- Paczki NuGet tylko w wersjach stabilnych (bez preview/beta).
- **Licencje:** tylko biblioteki darmowe także do użytku komercyjnego (MIT, Apache-2.0, BSD itp.) – możliwy jest
  przyszły model subskrypcyjny. Przed dodaniem paczki sprawdź licencję. Nie używamy m.in. MediatR, AutoMapper,
  FluentAssertions (nowe wersje są komercyjne).
- **Projekty** (`BM2.slnx`): `BM2.Domain` (czysty C#, z BM2 zna tylko Contracts), `BM2.Contracts` (DTO/requesty,
  bez referencji do BM2), `BM2.Server` (host: Minimal API, EF Core, Identity, strony konta SSR), `BM2.Client`
  (WASM, zna tylko Contracts). Reguły pilnują testy architektury w `BM2.UnitTests`.
- **Serwer:** foldery per funkcja `Features/<Obszar>/<Akcja>.cs` z `Map<Obszar>Endpoints(this RouteGroupBuilder)`;
  bez MediatR i repozytoriów (endpoint używa `AppDbContext`); `TypedResults`; błędy API jako ProblemDetails;
  bieżący użytkownik przez `ICurrentUser`; wszystko pod `/api` wymaga logowania (wyjątki jawnie `AllowAnonymous()`).
- **Konfiguracja:** `ConnectionStrings:BM2`; `Database:PrepareOnStartup` (tylko dev/testy) tworzy i migruje bazę.
  Paczki w `Directory.Packages.props` (centralnie, z pinowaniem zależności przechodnich), źródło tylko nuget.org.
- **Testy:** xUnit v3 na Microsoft.Testing.Platform (`dotnet test --project …`, filtry `-- --filter-class …`).
  Integracyjne w kolekcji `Integration` (sekwencyjnie), baza tymczasowa na serwerze z `BM2_TEST_SQL`
  (domyślnie `localhost\SQLEXPRESS`), Respawn czyści dane między testami.
- Narzędzia lokalne: `dotnet tool restore`. Uruchomienie lokalne: [docs/dev-setup.md](docs/dev-setup.md).
- `legacy/` – stary kod, tylko do odczytu jako źródło do przenoszenia.

## Bezpieczeństwo (punkt odniesienia: OWASP ASVS poziom 2)

- Autoryzacja zawsze po stronie serwera, w każdym zapytaniu: dostęp wynika z członkostwa użytkownika w portfelu/koncie
  (rola), nigdy z samego ID przysłanego przez klienta.
- Każdy endpoint ma test integracyjny sprawdzający brak dostępu do cudzych danych.
- Sekrety nigdy w repo (lokalnie user-secrets, na produkcji konfiguracja hosta).
- Cookie: HttpOnly, Secure, SameSite; antiforgery dla żądań modyfikujących; rate limiting logowania; nagłówki CSP/HSTS.
- Błędy do klienta jako ProblemDetails, bez szczegółów wewnętrznych. Wyjątków nie połykamy bez logowania.
- Walidacja zwraca wyniki – wyjątki nie służą do sterowania przepływem.

## Pieniądze i daty

- Kwoty zawsze `decimal` z jawną walutą. Nigdy nie sumuj kwot w różnych walutach bez przeliczenia.
- Momenty w czasie zapisuj w UTC; wyświetlaj w strefie użytkownika (konwersja po stronie klienta).
- Zakresy dat w zapytaniach jako `>= start AND < end` (indeksowalne), nie `.Year` / `.Month`.

## UI, dostępność, i18n

- Mobile-first. Dodanie rekordu ma zajmować < 5 s.
- Wygląd ma być unikalny i dopracowany – nie generyczny „kolejny panel”. Spójność z design systemem (tokeny CSS,
  komponenty bazowe); bez doklejania gotowych bibliotek UI.
- WCAG 2.2 AA: etykiety pól, widoczny focus, kontrast, cele dotyku ≥ 24×24 px (zalecane 44×44), informacja nie tylko
  kolorem, pełna obsługa klawiatury, respektowanie `prefers-reduced-motion`.
- Wszystkie teksty UI w zasobach (EN domyślnie, PL). Daty i kwoty formatowane wg kultury użytkownika.

## Wydajność

- Odczyty: projekcja do DTO (`Select`), `AsNoTracking`, paginacja, indeksy pod używane filtry, bez N+1.
- Minimalne payloady API.

## Testy

- TDD dla logiki domenowej i aplikacyjnej.
- Testy integracyjne API, w tym autoryzacji. Narzędzia: do ustalenia w specu podprojektu 0.

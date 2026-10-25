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
- **Tryb obecny (od 2026-10-24): najpierw funkcje, najniższym kosztem.** Funkcje robimy tak, jak opisuje je
  użytkownik: krótkie potwierdzenie w czacie zamiast speca i planu, bez nowych testów, bez pełnego WCAG i bez i18n
  (UI tylko po angielsku, teksty wprost w komponentach). Zostaje to, co nic nie kosztuje: autoryzacja przez
  `WalletAccessFilter`, zgodność z MSSQL 2012, istniejące komponenty i tokeny, kod się buduje. Zasady o testach,
  WCAG i i18n poniżej obowiązują ponownie przy uzupełnianiu braków przed wdrożeniem (razem z utwardzeniem 1C).
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
  SQL Server 2016+ (funkcje JSON / `OPENJSON`, `STRING_AGG`, `STRING_SPLIT`, temporal tables itp.). Tryb 110 na
  nowszym silniku ich nie blokuje – w testach pilnuje tego `SqlServer2012Guard` (interceptor EF). Kolumna JSON
  Identity dla passkeys jest dozwolona (zapisywana w całości jako tekst, sprawdzone testem).
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
- **Wynik i błędy:** reguły domenowe zwracają `Result`/`Result<T>` z `Error` (Validation, NotFound, Forbidden, Conflict,
  Gone); endpoint mapuje je przez `error.ToProblem()` (400 z błędem pola, 404, 403, 409, 410). Walidacja żądań:
  walidator FluentValidation w `BM2.Contracts` obok requestu + `.Validate<TRequest>()` na endpoincie (wspólne reguły
  w `RuleExtensions`: `Name(max)`, `Currency()`, `Balance()`).
- **Dostęp do portfela:** endpointy portfela są w grupie `/api/wallets/{walletId}` z `WalletAccessFilter`, który ładuje
  `WalletAccess` (rola w portfelu + role na udostępnionych kontach) do scoped `CurrentWallet`. Brak dostępu → 404,
  za niska rola → 403 (`Access.Can(...)`, `CanWriteRecord(...)`). Identyfikatory z żądania zawsze sprawdzaj względem
  portfela z trasy. Każdy nowy endpoint portfela dopisuje przypadek do `WalletEndpointAccessTests`.
- **Rekordy:** kwota nieujemna (0 dozwolone), kierunek wynika z typu (wydatek/przychód/transfer jako jeden wiersz); data rekordu
  `DateOnly` (`date`), momenty `DateTimeOffset` UTC z wstrzykiwanego `TimeProvider`; saldo = saldo otwarcia + rekordy
  `Completed` i `Pending` (`Planned` tylko w prognozie „with planned” – saldo i budżet); sumy zawsze per waluta konta.
- **Szablony i podpowiedzi:** `Template` (portfel, pola opcjonalne, bez FK do kont/kategorii – zarchiwizowane
  lub usunięte `ListTemplates` zwraca jako puste; etykieta = nazwa albo kategoria). Ikona kategorii to klucz z
  `CategoryIcons` (SVG w `Icon`), kolor tagu to klucz z `TagColors` (tokeny `--tag-*`, klasa `tag-color--*`); ikonę
  kategorii pokazuje `RecordAvatar`. Podpowiedzi formularza z `GET records/suggestions`, cache w `WalletState`.
- **Cykliczne:** `RecurringRule` pamięta `NextIndex`/`NextDate`; `RecurringCatchUp` (wołany w `GetWallet`) dodaje
  wystąpienia do dzisiaj, przyszłych rekordów nie tworzymy. Idempotencja przez unikalny indeks
  (`RecurringRuleId`, `OccurrenceDate`) – konflikt oznacza „już zrobione”.
- **Budżety i dashboard:** `Budget` = miesięczny limit kategorii wydatków w walucie bazowej portfela (unikalne
  portfel+kategoria, obowiązuje co miesiąc, bez przenoszenia); wydane liczymy tylko z kont w walucie bazowej, inne
  waluty osobno jako „nieliczone”. Zapis wymaga `ManageCategories`. Home ładuje wszystko jednym `GET dashboard`.
- **Wykresy:** komponent `Chart` (opcje ECharts, kolory jako `"var(--token)"`, przerysowanie po zmianie motywu, bez
  animacji przy `prefers-reduced-motion`); ECharts 5.6.0 lokalnie w `BM2.Server/wwwroot/lib/echarts`, ładowany leniwie
  przez `Chart.razor.js`. Przy wykresie zawsze te same liczby tekstem (lista/tabela). Statystyki: `stats/categories`,
  `stats/monthly`, `stats/category-months` (rekordy `Completed` i `Pending`, bez transferów, jedna waluta kont, `tagIds` = dowolny
  z tagów); filtry ekranów Stats współdzieli scoped `StatsState` + komponent `StatsFilters`; kliknięcie słupka (id `Typ:yyyy-MM-dd`) lub kategorii otwiera `RecordsPreview` z rekordami.
- **Klient:** strony korzystające z portfela dziedziczą `WalletComponentBase` (`WalletState` z migawką portfela,
  ostatni wybór w `localStorage`); API przez `ApiClient` (`ApiResult`/`ApiProblem`, 401 → logowanie); formularze to
  `EditForm` na modyfikowalnym modelu z nazwami pól jak w requeście + `FluentValidator` (ten sam walidator co serwer,
  `ShowServerErrors` dla 400/409). Komponenty bazowe: `Sheet` (natywny `<dialog>`), `ConfirmDialog`, `TextField`,
  `SelectField`, `MoneyInput`, `DateField`, `SegmentedControl`, `ChipGroup`, `ListRow`, `Amount`, `ToastHost`, `BackButton` (wstecz w historii aplikacji, bez niej `Fallback`).
  Usługi klienta są rejestrowane także na serwerze (prerendering); API wołamy tylko w trybie interaktywnym.
- **Konfiguracja:** `ConnectionStrings:BM2`; `Database:PrepareOnStartup` (tylko dev/testy) tworzy i migruje bazę.
  Paczki w `Directory.Packages.props` (centralnie, z pinowaniem zależności przechodnich), źródło tylko nuget.org.
- **Testy:** xUnit v3 na Microsoft.Testing.Platform (`dotnet test --project …`, filtry `-- --filter-class …`).
  Integracyjne w kolekcji `Integration` (sekwencyjnie), baza tymczasowa na serwerze z `BM2_TEST_SQL`
  (domyślnie `localhost\SQLEXPRESS`), Respawn czyści dane między testami. Komponenty klienta testujemy bUnit
  w `BM2.ClientTests` (HTTP przez `StubApi`, `UseBm2Client()`, bez bibliotek do mocków). Dane testów integracyjnych
  przez `TestData` (encje tworzone przez domenę), czas przez `TestTimeProvider` (`Factory.Time`, reset przed testem).
- Narzędzia lokalne: `dotnet tool restore`. Uruchomienie lokalne: [docs/dev-setup.md](docs/dev-setup.md).
- Stary kod (`legacy/`) usunięty w podprojekcie 1B; w razie potrzeby: `git show d4e818c:legacy/...`.

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
- **Zrozumiałość:** użytkownik ma od razu wiedzieć, po co jest dana opcja, co robi i jaki będzie efekt. Każde pole
  i przełącznik ma widoczną etykietę (w razie potrzeby krótki opis). Gdy zapis ma skutki poza bieżącym rekordem
  (powtarzanie, zmiany wielu rekordów, usuwanie powiązanych danych), UI przed zapisem mówi to wprost: podsumowanie
  skutku albo pytanie z nazwanymi wariantami zamiast domyślnego „OK”.
- Wygląd ma być unikalny i dopracowany – nie generyczny „kolejny panel”. Spójność z design systemem (tokeny CSS,
  komponenty bazowe); bez doklejania gotowych bibliotek UI.
- **Design system „Paper and ink”** ([spec](docs/specs/2026-10-24-podprojekt-1a-design-system.md)): style globalne
  w `BM2.Server/wwwroot/css` (`tokens.css` → `base.css` → `components.css`, `account.css` tylko dla stron Identity),
  kolory i odstępy wyłącznie przez tokeny (`--paper`, `--sheet`, `--ink`, `--ink-muted`, `--rule`, `--wash`,
  `--vivid`, `--link`, `--positive`, `--negative`, `--space-*`, `--radius*`). Fonty: Inter (UI), Fraunces (nagłówki,
  kwoty – klasa `.num`). Ikony przez komponent `Icon`: Lucide (ISC, `lucide-static` 1.61.0) wstawione lokalnie jako
  markup SVG, licencja w `BM2.Client/wwwroot/icons/LICENSE-lucide.txt`; nowe ikony dopisuj z tej samej wersji.
  Kwoty zawsze ze znakiem `+`/`−`.
- Motyw jasny/ciemny/systemowy obsługuje `wwwroot/js/theme.js` (przycisk z atrybutem `data-theme-toggle`). Bez skryptów
  inline (przyszły CSP).
- WCAG 2.2 AA: etykiety pól, widoczny focus, kontrast, cele dotyku ≥ 24×24 px (zalecane 44×44), informacja nie tylko
  kolorem, pełna obsługa klawiatury, respektowanie `prefers-reduced-motion`.
- Wszystkie teksty UI w zasobach (EN domyślnie, PL). Daty i kwoty formatowane wg kultury użytkownika.

## Wydajność

- Odczyty: projekcja do DTO (`Select`), `AsNoTracking`, paginacja, indeksy pod używane filtry, bez N+1.
- Minimalne payloady API.

## Testy

- TDD dla logiki domenowej i aplikacyjnej.
- Testy integracyjne API, w tym autoryzacji. Narzędzia: do ustalenia w specu podprojektu 0.

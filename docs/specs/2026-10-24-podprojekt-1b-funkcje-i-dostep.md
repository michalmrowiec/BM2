# Podprojekt 1B – Funkcje i model dostępu

Data: 2026-10-24 · Roadmapa: [docs/roadmap.md](../roadmap.md) · Poprzednie: [podprojekt 0](2026-10-24-podprojekt-0-fundamenty.md),
[podprojekt 1A](2026-10-24-podprojekt-1a-design-system.md)

## Cel

Portfele, konta, kategorie, tagi i rekordy działają w nowej architekturze (Blazor WASM + Minimal API) od razu na
**rdzeniu modelu v2** (typ rekordu, waluta transakcji i konta, statusy) – razem z modelem dostępu: członkostwo w portfelu
z rolami, udostępnianie pojedynczego konta i zaproszenia linkiem. Ekrany w stylu „Paper and ink”, mobile-first.

## Decyzje

| Decyzja | Uzasadnienie |
|---------|--------------|
| Rdzeń v2 od razu (zamiast przenoszenia starego modelu 1:1). | W BM2 nie ma danych do zachowania; stary model (znak kwoty, tabela statusów, Hangfire) oznaczałby pisanie endpointów, UI i testów dwa razy. |
| Szablony i rekordy cykliczne – podprojekt 2. Kopiowanie rekordu – 1B. | Szablony i cykliczne i tak są przebudowywane (catch-up zamiast Hangfire, przypinanie); kopiowanie jest tanie i było w BM2. |
| Kategorie i tagi należą do portfela (wspólne dla członków). | Przy współdzieleniu to jedyny prosty wariant; w legacy należały do użytkownika i były „włączane” per portfel. |
| Kategoria ma typ (wydatek / przychód). | Szybkie dodawanie pokazuje tylko pasujące kategorie. |
| „Projekty” / grupy (budowa domu, konkretne auto) realizujemy tagami. | Tag to grupa w poprzek kategorii. 1B: filtr po tagu z zakresem „cały okres”, podsumowanie listy, archiwizacja tagu. Raport tagu (kategorie, miesiące) – podprojekt 4. |
| Transfer jako jeden rekord (`AccountId` → `TransferAccountId`). | Brak osieroconych połówek, atomowa edycja. |
| Data rekordu jako `date` (`DateOnly`), nie moment w UTC. | To data kalendarzowa widziana przez użytkownika; wydatek z 23:30 nie przeskakuje do innego miesiąca. „Dzisiaj” liczy klient w swojej strefie. Momenty (`CreatedAt`, `UpdatedAt`) w UTC. |
| Przechowujemy `Amount` (waluta transakcji) i `AccountAmount` (waluta konta); kurs nie jest zapisywany. | Obie kwoty są dokładne (jak na wyciągu), kurs wynika z nich. UI przyjmuje kurs albo kwotę w walucie konta. Podpowiedź kursu z NBP – podprojekt 2. |
| Waluty jako stała lista ISO 4217 w `BM2.Contracts`, bez tabeli. | Lista się nie zmienia w praktyce; brak joinów. |
| Autoryzacja: jawny serwis dostępu + filtr grupy tras (wariant A). | Widoczna w każdym endpoincie, testowalna; listy filtrowane w SQL. Odrzucone: globalne filtry EF (ukryte, słabo pasują do ról) i autoryzacja zasobowa (nie filtruje list). |
| Brak dostępu → 404, za niska rola → 403. | Nie zdradzamy istnienia cudzych portfeli. |
| Zaproszenie to jednorazowy link (7 dni), który Owner sam wysyła; pozwala założyć konto. | Brak SMTP i zamknięta rejestracja; przypisanie do e-maila bez weryfikacji niewiele daje. |
| i18n i utwardzenie (antiforgery, rate limiting, CSP/HSTS) – podprojekt 1C. | W 1B teksty po angielsku wprost w komponentach, bez sklejania stringów (ekstrakcja do zasobów będzie mechaniczna). Do czasu antiforgery żądania cross-site POST blokuje `SameSite=Lax`. |
| Autor rekordu: zapisujemy `CreatedByUserId`, wyświetlanie później. | YAGNI w UI, dane gotowe. |
| Nowy portfel dostaje domyślny zestaw kategorii (EN, do edycji). | Szybszy start. |
| `legacy/` usuwamy na końcu 1B (zostaje w historii git). | Szablony i cykliczne powstają od nowa, a źródłem importu jest MM. |

## Zakres

- Encje, migracja i API: portfele, członkowie, konta (z saldem), udostępnienia kont, kategorie, tagi, rekordy
  (wydatek, przychód, transfer, waluty), zaproszenia.
- Ekrany: przełącznik portfela, Home, Records (lista, filtry, podsumowanie), szybkie dodawanie, edycja i kopiowanie
  rekordu, zarządzanie portfelami, kontami, kategoriami, tagami, członkami i zaproszeniami, strona zaproszenia.
- Rejestracja z ważnym zaproszeniem także poza Development.
- Nowe komponenty design systemu (sekcja „Klient”).
- `ValidationFilter<T>`, FluentValidation i lekki typ `Result` (zapowiedziane w podprojekcie 0).
- Usunięcie `legacy/`, aktualizacja roadmapy i AGENTS.md.

## Poza zakresem

Szablony, przypinanie, rekordy cykliczne, budżety, kurs z NBP, import z MM (podprojekt 2); zaawansowane szybkie
dodawanie i PWA (podprojekt 3); dashboard, raporty i raport tagu (podprojekt 4); i18n i utwardzenie (1C);
przenoszenie konta między portfelami; kolory/ikony kategorii; podkategorie; wyświetlanie autora rekordu.

## Model danych

Encje w `BM2.Domain` (czysty C#, logika i reguły), konfiguracja EF (Fluent API) w `BM2.Server/Data`. Klucze `Guid`.
Enumy współdzielone z klientem w `BM2.Contracts`. Teksty: nazwy przycinane (`Trim`), puste niedozwolone.

### Wallet

| Pole | Typ | Uwagi |
|------|-----|-------|
| `Id` | Guid | |
| `Name` | nvarchar(50) | |
| `BaseCurrency` | varchar(3) | ISO 4217, do raportów (podprojekt 2/4) |
| `CreatedAt` | datetime2 (UTC) | |

### WalletMember

`WalletId`, `UserId` (PK złożony), `Role` (`Owner` / `Editor` / `Viewer`), `JoinedAt`. Portfel ma zawsze co najmniej
jednego Ownera.

### Account

| Pole | Typ | Uwagi |
|------|-----|-------|
| `Id`, `WalletId` | Guid | |
| `Name` | nvarchar(50) | unikalna w portfelu |
| `Currency` | varchar(3) | ISO 4217, niezmienna, gdy konto ma rekordy |
| `OpeningBalance` | decimal(19,4) | domyślnie 0 |
| `IsArchived` | bit | ukryte w wyborach, historia zostaje |
| `CreatedAt` | datetime2 (UTC) | |

**Saldo** = `OpeningBalance` + Σ przychodów − Σ wydatków − Σ transferów wychodzących (`AccountAmount`)
+ Σ transferów przychodzących (`TransferAmount`), tylko rekordy `Completed`. Liczone zapytaniem z grupowaniem.

### AccountMember

`AccountId`, `UserId` (PK złożony), `Role` (`Editor` / `Viewer`), `JoinedAt`. Udostępnienie jednego konta osobie
spoza portfela.

### Category

`Id`, `WalletId`, `Name` nvarchar(50) (unikalna w portfelu w obrębie typu), `Type` (`Expense` / `Income`),
`IsArchived`. Usunięcie tylko, gdy kategoria nie ma rekordów; w przeciwnym razie archiwizacja.

Domyślny zestaw: wydatki – Groceries, Eating out, Transport, Home, Bills, Health, Shopping, Entertainment, Other;
przychody – Salary, Other income.

### Tag

`Id`, `WalletId`, `Name` nvarchar(30) (unikalna w portfelu), `IsArchived`. Relacja wiele-do-wielu z rekordami
(tabela `RecordTag`). Usunięcie tagu zdejmuje go z rekordów.

### Record

| Pole | Typ | Uwagi |
|------|-----|-------|
| `Id`, `AccountId` | Guid | `AccountId` – konto źródłowe przy transferze |
| `Type` | int | `Expense` / `Income` / `Transfer` |
| `Status` | int | `Planned` / `Pending` / `Completed` (domyślnie `Completed`) |
| `OccurredOn` | date | |
| `Amount` | decimal(19,4) | > 0, ≤ 999 999 999 999,99, maks. 2 miejsca po przecinku (dotyczy wszystkich kwot rekordu), w `Currency` |
| `Currency` | varchar(3) | waluta transakcji |
| `AccountAmount` | decimal(19,4) | > 0, w walucie konta; równe `Amount`, gdy waluty są te same |
| `CategoryId` | Guid? | wymagana dla wydatku/przychodu (typ kategorii = typ rekordu), pusta dla transferu |
| `TransferAccountId` | Guid? | tylko transfer; inne konto tego samego portfela |
| `TransferAmount` | decimal(19,4)? | tylko transfer; w walucie konta docelowego, równe `Amount` przy tej samej walucie |
| `Name` | nvarchar(100)? | opcjonalna; w UI zastępowana nazwą kategorii / „Transfer” |
| `CreatedByUserId` | Guid | |
| `CreatedAt`, `UpdatedAt` | datetime2 (UTC) | `UpdatedAt` nullable |

Reguły (domena, testy jednostkowe):
- Transfer: `Currency` = waluta konta źródłowego (`AccountAmount` = `Amount`), konta różne, oba w tym samym portfelu,
  bez kategorii.
- Wydatek/przychód: kategoria z tego samego portfela i o zgodnym typie, niezarchiwizowana przy tworzeniu.
- Konto zarchiwizowane nie przyjmuje nowych rekordów (edycja istniejących dozwolona).
- Tagi z tego samego portfela.

Indeksy: `Record (AccountId, OccurredOn)`, `Record (TransferAccountId, OccurredOn)` (filtrowany `IS NOT NULL`),
`Record (CategoryId)`, unikalne nazwy z sekcji wyżej, `Invitation (TokenHash)` unikalny.

### Invitation

| Pole | Typ | Uwagi |
|------|-----|-------|
| `Id`, `WalletId` | Guid | |
| `AccountId` | Guid? | ustawione = udostępnienie konta |
| `Role` | int | dla portfela: Owner/Editor/Viewer; dla konta: Editor/Viewer |
| `TokenHash` | binary(32) | SHA-256 tokenu; sam token (32 losowe bajty, base64url) widać tylko raz |
| `CreatedByUserId`, `CreatedAt`, `ExpiresAt` | | `ExpiresAt` = `CreatedAt` + 7 dni |
| `AcceptedByUserId`, `AcceptedAt` | nullable | jednorazowe |
| `RevokedAt` | nullable | |

### Usuwanie

- Rekord: twarde usunięcie po potwierdzeniu.
- Konto: archiwizacja albo usunięcie razem z rekordami (również transferami po obu stronach) – tylko Owner, po potwierdzeniu.
- Portfel: usunięcie wszystkiego w jednej transakcji (rekordy → tagi → kategorie → udostępnienia → konta → zaproszenia
  → członkowie → portfel), w kodzie przez `ExecuteDelete` – SQL Server nie pozwala na wiele ścieżek kaskady.
  W UI potwierdzenie wpisaniem nazwy portfela.

## Dostęp

### Macierz uprawnień (`BM2.Domain`, testy jednostkowe)

| Uprawnienie | Owner | Editor | Viewer | Editor konta | Viewer konta |
|-------------|:-----:|:------:|:------:|:------------:|:------------:|
| Podgląd portfela, kategorii, tagów | ✓ | ✓ | ✓ | ✓ | ✓ |
| Podgląd rekordów | wszystkie | wszystkie | wszystkie | tego konta | tego konta |
| Dodawanie / edycja / usuwanie rekordów | ✓ | ✓ | – | na tym koncie | – |
| Zarządzanie kategoriami i tagami | ✓ | ✓ | – | – | – |
| Zarządzanie kontami | ✓ | – | – | – | – |
| Ustawienia i usunięcie portfela | ✓ | – | – | – | – |
| Lista członków | ✓ | ✓ | ✓ | – | – |
| Członkowie, role, udostępnienia, zaproszenia | ✓ | – | – | – | – |

- Rekord dotyczący dwóch kont (transfer): odczyt przy dostępie do któregokolwiek, zapis tylko przy prawie edycji obu.
  Osoba z dostępem do jednego konta widzi nazwę konta po drugiej stronie transferu – nic więcej.
- Rola w portfelu i udostępnione konto naraz → obowiązuje wyższe uprawnienie.
- Ostatni Owner nie może odejść ani zostać zdegradowany. Każdy może sam opuścić portfel lub udostępnione konto.

### Mechanizm

- `WalletAccess` (domena): rola w portfelu (lub brak) + role per konto; metody `Can(WalletPermission)`,
  `CanReadAccount(id)`, `CanWriteAccount(id)`, `ReadableAccountIds`.
- Grupa `/api/wallets/{walletId:guid}` ma filtr endpointów, który jednym zapytaniem ładuje `WalletAccess` bieżącego
  użytkownika do serwisu o zasięgu żądania. Brak członkostwa i udostępnień → 404 (ProblemDetails).
- Endpoint sprawdza uprawnienie przed działaniem (403 przy braku). Zapytania o rekordy zawsze zawierają warunek
  `AccountId IN (dostępne) OR TransferAccountId IN (dostępne)`.
- Identyfikatory zasobów z żądania (konto, kategoria, tag) są zawsze sprawdzane względem portfela z trasy.

### Zaproszenia

1. Owner tworzy zaproszenie (rola, opcjonalnie konto) → API zwraca link `https://<host>/invite/{token}` i datę ważności.
   W bazie tylko skrót tokenu.
2. `/invite/{token}` (strona WASM):
   - niezalogowany: informacja o zaproszeniu bez szczegółów, „Log in” (`/Account/Login?ReturnUrl=/invite/{token}`)
     i „Create account” (`/Account/Register?invite={token}`);
   - zalogowany: `GET /api/invitations/{token}` → nazwa portfela, e-mail zapraszającego, rola, nazwa konta;
     przycisk „Join” → `POST /api/invitations/{token}/accept` → przejście do portfela.
3. Rejestracja poza Development działa tylko z ważnym tokenem (`RegistrationGate` sprawdza `invite`). Konto założone
   z zaproszenia ma potwierdzony e-mail (posiadanie linku od Ownera; brak SMTP), po rejestracji użytkownik jest
   zalogowany i wraca na stronę zaproszenia.
4. Przyjęcie: tworzy `WalletMember` / `AccountMember` albo podnosi niższą rolę; istniejąca wyższa rola zostaje.
   Zaproszenie nieznane → 404; wygasłe, odwołane lub użyte → 410 z ProblemDetails.
5. Odwołanie: `DELETE` przez Ownera (ustawia `RevokedAt`).

## API

Konwencje z podprojektu 0: `Features/<Obszar>/<Akcja>.cs`, `Map<Obszar>Endpoints`, `TypedResults`, ProblemDetails,
`ICurrentUser`. Walidatory FluentValidation w `BM2.Contracts` (współdzielone z formularzami klienta), na serwerze
`ValidationFilter<T>` → `ValidationProblem`. Reguły wymagające bazy (unikalność, przynależność do portfela) w endpointach
przez `Result` → `ValidationProblem` / 404 / 403 / 409.

| Metoda i trasa | Uprawnienie | Opis |
|----------------|-------------|------|
| `GET /api/wallets` | zalogowany | Moje portfele: id, nazwa, moja rola albo „konto udostępnione”. |
| `POST /api/wallets` | zalogowany | Nowy portfel; twórca = Owner; domyślne kategorie. |
| `GET /api/wallets/{w}` | podgląd | **Migawka portfela:** dane portfela, moje uprawnienia (flagi dla UI), dostępne konta z saldami i flagą edycji, kategorie, tagi. Jedno wywołanie wystarcza do szybkiego dodawania. |
| `PUT /api/wallets/{w}` | ustawienia | Nazwa, waluta bazowa. |
| `DELETE /api/wallets/{w}` | ustawienia | Usunięcie portfela. |
| `POST /api/wallets/{w}/accounts`, `PUT/DELETE …/{id}` | konta | CRUD, archiwizacja w `PUT`. |
| `POST /api/wallets/{w}/categories`, `PUT/DELETE …/{id}` | kategorie i tagi | CRUD, archiwizacja; `DELETE` → 409, gdy są rekordy. |
| `POST /api/wallets/{w}/tags`, `PUT/DELETE …/{id}` | kategorie i tagi | CRUD, archiwizacja. |
| `GET /api/wallets/{w}/records` | podgląd rekordów | Filtry: `from`, `to` (brak = cały okres), `accountId`, `categoryId`, `tagId`, `type`, `status`, `search` (nazwa). Stronicowanie keyset po (`OccurredOn` desc, `CreatedAt` desc, `Id` desc), 50 na stronę, `cursor` nieprzezroczysty. |
| `GET /api/wallets/{w}/records/summary` | podgląd rekordów | Te same filtry; sumy wydatków i przychodów per waluta konta (`AccountAmount`), bez transferów. |
| `GET /api/wallets/{w}/records/{id}` | podgląd rekordu | Szczegóły (edycja, kopiowanie). |
| `POST /api/wallets/{w}/records`, `PUT/DELETE …/{id}` | zapis na koncie | CRUD; przy transferze zapis na obu kontach. |
| `GET /api/wallets/{w}/members` | lista członków | Członkowie (e-mail, rola), udostępnienia kont, oczekujące zaproszenia (tylko dla Ownera). |
| `PUT /api/wallets/{w}/members/{userId}` | członkowie | Zmiana roli. |
| `DELETE /api/wallets/{w}/members/{userId}` | członkowie albo ja | Usunięcie / opuszczenie. |
| `PUT/DELETE /api/wallets/{w}/accounts/{a}/members/{userId}` | członkowie (`DELETE` także ja) | Rola / usunięcie udostępnienia konta. |
| `POST /api/wallets/{w}/invitations`, `DELETE …/{id}` | członkowie | Utworzenie (zwraca link) / odwołanie. |
| `GET /api/invitations/{token}`, `POST …/{token}/accept` | zalogowany | Podgląd / przyjęcie zaproszenia. |

DTO list są minimalne: rekord zawiera identyfikatory konta, kategorii i tagów (klient ma je z migawki portfela)
oraz nazwę konta docelowego transferu (może być niedostępne w migawce).

## Klient

- **Stan portfela:** serwis z bieżącym portfelem i jego migawką; ostatni wybór w `localStorage` (`bm2-wallet`),
  ostatnio używane konto (`bm2-last-account:{walletId}`). Migawka odświeżana po zmianach.
- **Pierwsze uruchomienie:** brak portfeli → pusty stan z utworzeniem portfela (nazwa, waluta), potem konta.
- **Przełącznik portfela** w nagłówku (nazwa portfela) → arkusz z listą i „New wallet”.
- **Home:** saldo per waluta, karty kont z saldami, podsumowanie bieżącego miesiąca (tylko `Completed`),
  5 ostatnich rekordów.
- **Records:** nawigacja miesiącami i zakres „All time”; grupy dzienne ze sticky nagłówkami i sumą dnia; kwota ze znakiem
  `+`/`−` i kolorem, transfer z ikoną strzałki; status `Planned`/`Pending` oznaczony etykietą (nie tylko kolorem);
  filtry w arkuszu (konto, kategoria, tag, typ, status, szukaj) z aktywnymi filtrami jako chipy; pasek podsumowania
  filtrowanej listy; doładowanie kolejnych stron przy przewijaniu. Tap → arkusz edycji (zapis, usuń, „Copy”).
- **Szybkie dodawanie (FAB, `/add`):** przełącznik Expense / Income / Transfer; duże pole kwoty (Fraunces, klawiatura
  numeryczna, autofocus) jako pierwsze; kategorie danego typu jako chipy; konto (domyślnie ostatnio używane); data
  (domyślnie dzisiaj, skróty Today/Yesterday); „More” rozwija nazwę, tagi, status i walutę. Waluta inna niż konta
  odsłania kurs ↔ kwotę w walucie konta (przeliczane w obie strony). Transfer: konto docelowe i – przy różnych walutach –
  kwota docelowa. Zapis → toast i powrót. „Copy” otwiera ten sam formularz z wypełnionymi polami, dzisiejszą datą
  i zaznaczoną kwotą.
- **More:** Wallets, Accounts, Categories, Tags, Members & invitations, Account (strony Identity), motyw.
- **Członkowie:** lista członków i udostępnień z rolami (zmiana, usunięcie, „Leave”), oczekujące zaproszenia
  (odwołanie), „Invite” → arkusz (rola, cały portfel albo konto) → link z przyciskiem kopiowania i udostępniania
  (`navigator.share`, gdy dostępne).
- **Nowe komponenty:** `Sheet` (natywny `<dialog>`, wjazd od dołu, na desktopie wyśrodkowany), `SegmentedControl`,
  `Chip`/`ChipGroup`, `MoneyInput`, pola formularza (`TextField`, `SelectField`, `DateField`) z etykietami i błędami
  walidacji, `ListRow`, `Toast`, `ConfirmDialog`, `Skeleton`, `Amount` (formatowanie ze znakiem i walutą).
- **Animacje:** wjazd arkusza, kaskada wierszy listy, płynna zmiana kwot salda; wszystko wyłączane przez
  `prefers-reduced-motion`. Kolory i odstępy wyłącznie z tokenów.
- **Dostępność:** etykiety pól, focus w arkuszu (pułapka focusu natywnego `<dialog>`, Escape zamyka), cele dotyku
  ≥ 44 px, komunikaty walidacji powiązane z polami (`aria-describedby`), toasty w `aria-live`.
- Błędy API: 401 → przekierowanie do logowania, 404/403 → pusty stan z komunikatem, 400 (`ValidationProblem`)
  i 409 → błędy przy polach, inne → toast „Something went wrong”.

## Plasterki

Każdy plasterek: testy (TDD) → implementacja → UI → demo lokalne → commit na `dev`.

1. **Fundamenty dostępu i portfele:** `Result`, `ValidationFilter<T>`, FluentValidation, `WalletAccess`, filtr grupy,
   encje `Wallet`/`WalletMember`/`AccountMember` (bez UI udostępnień), API portfeli, przełącznik portfela, pierwsze
   uruchomienie, ekran Wallets, komponenty `Sheet`, pola formularza, `Toast`, `ConfirmDialog`.
2. **Konta:** API, salda (na razie z saldem otwarcia), ekran Accounts, karty kont na Home.
3. **Kategorie i tagi:** API, domyślny zestaw, ekrany Categories i Tags (archiwizacja).
4. **Rekordy:** API (CRUD, transfery, waluty, filtry, keyset, podsumowanie), salda z rekordów, szybkie dodawanie,
   Records, edycja i kopiowanie, Home (miesiąc, ostatnie).
5. **Współdzielenie:** członkowie, role, udostępnienia kont, zaproszenia, rejestracja z zaproszeniem, ekran Members,
   strona `/invite/{token}`.
6. **Sprzątanie:** usunięcie `legacy/`, aktualizacja roadmapy i AGENTS.md.

## Testy

- **Jednostkowe (`BM2.UnitTests`):** macierz uprawnień `WalletAccess` (w tym transfer i łączenie ról), reguły rekordu,
  cykl życia zaproszenia (wygaśnięcie, jednorazowość, odwołanie, podnoszenie roli), przeliczenie kursu, walidatory
  z `BM2.Contracts`, saldo.
- **Integracyjne (`BM2.IntegrationTests`), dla każdego endpointu:** ścieżka poprawna; cudzy portfel → 404; za niska rola
  → 403; dostęp do jednego konta widzi tylko jego rekordy, sumy i saldo; identyfikatory z innego portfela (konto,
  kategoria, tag) odrzucone. Dodatkowo: przepływ zaproszenia (utworzenie → przyjęcie → dostęp), rejestracja
  z zaproszeniem poza Development i bez niego (404), usunięcie portfela usuwa wszystko. `SqlServer2012Guard` pilnuje
  składni SQL Server 2012.
- **bUnit (`BM2.ClientTests`):** szybkie dodawanie (przełączanie typu zmienia kategorie, inna waluta odsłania kurs,
  walidacja), grupowanie listy rekordów po dniach z sumą dnia, przełącznik portfela, `Amount` (znak i waluta).
- **Ręcznie w przeglądarce:** telefon (375 px) i desktop, jasny i ciemny motyw, przepływ zaproszenia w dwóch sesjach.

## Kryteria akceptacji

1. W demo (https://localhost:7237): utworzenie portfela, kont, kategorii i tagów; dodanie wydatku, przychodu i transferu
   (także w innej walucie); lista z filtrami i podsumowaniem (w tym tag z zakresem „All time”); edycja, kopiowanie
   i usunięcie rekordu; salda zgodne z rekordami.
2. Zaproszenie linkiem w drugiej sesji: założenie konta z zaproszenia i dostęp do portfela z nadaną rolą; udostępnienie
   jednego konta pokazuje tylko jego rekordy.
3. Każdy endpoint ma test braku dostępu do cudzych danych; wszystkie testy zielone.
4. `dotnet build BM2.slnx -c Release` z `CI=true` bez ostrzeżeń.
5. Katalog `legacy/` usunięty; roadmapa i AGENTS.md zaktualizowane.

# Uruchomienie lokalne

## Wymagania

- .NET SDK 10.0.301+ (`global.json`), SQL Server Express jako `localhost\SQLEXPRESS` (Integrated Security).
- Zaufany certyfikat deweloperski: `dotnet dev-certs https --trust`.
- Narzędzia lokalne: `dotnet tool restore` (m.in. `dotnet-ef`).

## Aplikacja

```bash
dotnet run --project src/BM2.Server --launch-profile https
```

Adres: https://localhost:7237. Przy starcie w Development baza `BM2Dev` jest tworzona, ustawiana na poziom
zgodności 110 (SQL Server 2012) i migrowana (`Database:PrepareOnStartup`).

Rejestracja działa tylko w Development. E-maile nie są wysyłane – link potwierdzający pokazuje się na stronie po
rejestracji.

## Konto demo (tylko lokalna baza `BM2Dev`)

- E-mail: `demo@bm2.test`
- Hasło: `bm2-local-demo-2026`

Wspólne konto demo tylko do odczytu („Try the demo” na logowaniu, użytkownik `demo@bm2.invalid`) jest lokalnie
włączone w `appsettings.Development.json` (`Demo:Enabled`). Opis: [deploy-webio](deploy-webio.md#konto-demo-opcjonalnie).

## Współdzielenie lokalnie

Members → Invite tworzy jednorazowy link (`/invite/{token}`, ważny 7 dni). Otwórz go w oknie prywatnym: „Create account”
zakłada nowe konto z potwierdzonym e-mailem i wraca na stronę zaproszenia, „Join” dołącza do portfela. Poza
Development rejestracja działa tylko z ważnym zaproszeniem.

## Testy

```bash
dotnet test
```

Testy integracyjne tworzą tymczasową bazę `BM2_Test_<guid>` na serwerze z `BM2_TEST_SQL`
(domyślnie `localhost\SQLEXPRESS`) i usuwają ją po przebiegu.

## Migracje

```bash
dotnet ef migrations add <Nazwa> --project src/BM2.Server --output-dir Data/Migrations
```

# Wdrożenie na Webio

Witryna: `<host-produkcji>` (domena tymczasowa Webio, SSL z panelu). Baza: MSSQL 2012 `<serwer-mssql>`.
Sekrety (hasło WebDeploy, connection string) są tylko w panelu Webio i w pliku na serwerze – nigdy w repo.

## Jednorazowa konfiguracja (zrobione 2026-10-25)

1. Panel → Dodaj domenę: pełna nazwa (np. `bm2-app.pl`), „Utwórz witrynę” + „Domena tymczasowa”, bez strefy DNS.
2. Witryna → SSL: włączony certyfikat. Aplikacja poza Development robi `UseHttpsRedirection` + HSTS.
3. Witryna → Publikowanie: włączony WebDeploy, pobrany `*.publishsettings` (zawiera hasło – trzymaj poza repo).
   Z niego profil `src/BM2.Server/Properties/PublishProfiles/Webio.pubxml` (bez hasła, `*.pubxml` w `.gitignore`):
   Release, framework-dependent, `EnableMSDeployAppOffline`, `SkipExtraFilesOnServer=true`.
4. Panel → baza MSSQL dla BM2.
5. W folderze domowym witryny (obok `BM2.Server.dll`) plik `appsettings.Production.json`, tylko na serwerze:

   ```json
   {
     "ConnectionStrings": {
       "BM2": "Server=<serwer-mssql>;Database=<baza>;User Id=<login>;Password=<hasło>;Encrypt=True;TrustServerCertificate=True"
     },
     "Database": { "PrepareOnStartup": true }
   }
   ```

   Hasło z `;`, `=` lub cudzysłowem bierz w apostrofy (`Password='…'`). Publikacja go nie usuwa (`SkipExtraFilesOnServer`).

## Publikacja

W katalogu repo (hasło WebDeploy tylko interaktywnie):

```powershell
$p = Read-Host "Haslo WebDeploy"
dotnet publish src/BM2.Server -p:PublishProfile=Webio -p:Password=$p
```

Alternatywa: import `*.publishsettings` w Riderze/VS i Publish. Ostrzeżenie NETSDK1198 dla `BM2.Client` jest nieszkodliwe.

## Aktualizacja (kolejne wdrożenia)

1. Sprawdź nowe migracje od poprzedniego wdrożenia (`git diff --name-status <poprzedni> HEAD -- src/BM2.Server/Data/Migrations`)
   pod kątem MSSQL 2012: bez `Sql(...)` ze składnią 2016+, JSON, `ALTER DATABASE`. Indeksy filtrowane są OK.
2. Backup bazy w panelu Webio – migracja wykona się sama przy pierwszym starcie po publikacji.
3. Publikacja z logiem pokazującym, czy pliki poszły (bez tego trudno odróżnić sam build od wdrożenia):

   ```powershell
   dotnet publish src/BM2.Server -p:PublishProfile=Webio -p:Password=$p -tl:off -v:n | Select-String "Adding|Updating|Deleting|Publish Succeeded|error"
   ```

4. Smoke test: `/Account/Login` → 200, `/api/wallets` bez logowania → 401 (500.30 = nieudana migracja lub konfiguracja),
   potem ręcznie: logowanie, dodanie rekordu, Stats, nowe funkcje. Na telefonie odśwież PWA (service worker).

## Schemat bazy

Decyzja: na produkcji auto-migracja przy starcie (`Database:PrepareOnStartup=true` w pliku na serwerze) – baza
już istnieje, poziom zgodności to 110, więc wykonuje się tylko `MigrateAsync`. Pierwsze żądanie po publikacji
z nową migracją trwa dłużej. Plan B (np. brak uprawnień): wyłącz przełącznik i uruchom ręcznie skrypt

```powershell
dotnet ef migrations script --idempotent --configuration Release --project src/BM2.Server --output bm2-schema.sql
```

(sprawdzony: bez składni 2016+ i bez `ALTER DATABASE`).

## Pierwsze konto

Na produkcji rejestracja działa tylko z zaproszeniem. Pierwsze konto: jednorazowo lokalnie w Development na bazie
produkcyjnej, w osobnym oknie PowerShell (zmienne znikają z oknem):

```powershell
$env:ConnectionStrings__BM2 = Read-Host "Connection string produkcyjny"
$env:Database__PrepareOnStartup = "false"
dotnet run --project src/BM2.Server --launch-profile https -c Release
```

Rejestracja na `https://localhost:7237/Account/Register`, link potwierdzający na stronie, potem logowanie na
produkcji. Passkey dodawaj dopiero na produkcji (passkey jest związany z domeną). Kolejne osoby – przez zaproszenie.

## Konto demo (opcjonalnie)

Wspólne konto tylko do odczytu z przykładowymi danymi, wejście przyciskiem „Try the demo” na stronie logowania.
Domyślnie wyłączone. Włączenie: w `appsettings.Production.json` na serwerze dopisz `"Demo": { "Enabled": true }`
i zrestartuj pulę (albo opublikuj ponownie). Migracja nie jest potrzebna.

- Przy pierwszym wejściu powstaje użytkownik `demo@bm2.invalid` (bez hasła i passkeya, claim roli `Demo`) z portfelem
  „Demo household” (ok. 9 miesięcy rekordów, konto EUR, fundusz inwestycyjny, budżety, tagi, reguły cykliczne).
  Reguły cykliczne same dopisują kolejne miesiące przy otwarciu portfela.
- Serwer odrzuca każdy zapis demo (`/api` i strony konta) kodem 403; działają odczyty i eksport.
- Wyłączenie: `"Enabled": false` – wejście zwraca 404, a istniejące sesje demo nadal tylko czytają. Żeby odświeżyć
  dane, usuń użytkownika demo i jego portfel (np. narzędziem, nie ręcznym SQL) – następne wejście utworzy je od nowa.

## Diagnoza błędów

- 500.30/500.31: w `web.config` na serwerze tymczasowo `stdoutLogEnabled="true"` i `stdoutLogFile=".\logs\stdout"`
  (folder `logs` musi istnieć), po diagnozie wyłącz.
- 404 na wszystkim: pliki nie trafiły na serwer – sprawdź FTP i log publikacji (`-tl:off -v:n`).
- „Format of the initialization string…”: błędna składnia connection stringa (cudzysłowy z JSON, znaki w haśle).
- Po zmianie `appsettings.Production.json`: restart puli w panelu.

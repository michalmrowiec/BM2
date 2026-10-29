# BM2 – roadmapa

Stan na: 2026-10-29. BM2 (v2 MoneyManager) działa produkcyjnie na `<host-produkcji>` z danymi z MM.
Konwencje i opis funkcji: [AGENTS.md](../AGENTS.md). Wdrożenie: [deploy-webio.md](deploy-webio.md).

## Decyzje, których nie ma w AGENTS.md

- Zakres: aplikacja prywatna + współdzielone portfele/konta przez zaproszenia. Bez publicznej rejestracji, płatności
  i integracji giełdowych – architektura nie może ich blokować.
- Tylko online; PWA służy instalacji i szybkiemu startowi.
- Logowanie passkey + hasło jako zapas, sesja 30 dni z przedłużaniem.
- Webio plan Podstawowy: 2 bazy MSSQL łącznie 250 MB, log transakcji maks. 500 MB, zadania harmonogramu co ≥ 30 min.
  Bez logów i kolejek w bazie.
- MM działa równolegle, wyłączamy po okresie równoległym.

## Do zrobienia

1. **Poczta (SMTP Webio):** potwierdzanie e-maila, reset hasła, zmiana e-maila, wysyłka zaproszeń. Sekrety
   w konfiguracji hosta.
2. **Wydajność na telefonie:** zimny start (usypianie puli IIS + pobranie WASM) i kilkusekundowy zapis rekordu.
   Najpierw pomiar (API vs klient), potem ping z harmonogramu Webio, AOT/trimming, kompresja, optymistyczne
   zamykanie formularza.
3. **Podpowiedź przy budżecie:** po wyborze kategorii wydatki z ostatnich 6–12 mies. (lista/mini wykres), średnia
   i proponowany limit (średnia zaokrąglona w górę). Dane z `stats/category-months`.
4. **Usuwanie konta użytkownika:** portfele, gdzie jest jedynym członkiem, usuwane z danymi; we wspólnych najpierw
   przekazanie własności.
5. **Utwardzenie (1C):** CSP i przegląd nagłówków, przegląd antiforgery dla `/api`, health check, backupy bazy.
6. **Testy (dług z trybu „najpierw funkcje”):** integracyjne dla nowych endpointów + przypadki
   w `WalletEndpointAccessTests`; `GET /culture/pl` (cookie, odrzuca nielokalny `returnUrl`);
   axe (Playwright) dla głównych ekranów w obu motywach. **Najpierw:** 35 z 95 testów `BM2.ClientTests` nie
   przechodzi (stan z 2026-10-29, jeszcze przed audytem) – brak rejestracji `WalletState` w `UseBm2Client()`,
   symbole walut („€” zamiast „EUR”), zmieniony markup formularza rekordu i `WalletSwitcher`.
7. **Wydajność – punkty z audytu (2026-10-29):**
   - indeks pod `Status` rekordu (dashboard filtruje Pending/Planned; obecny `(AccountId, OccurredOn)` go nie
     pokrywa) – addytywna migracja `(AccountId, Status)` albo indeks filtrowany `Status <> Completed`;
   - `WalletLimits.RecordCountAsync` liczy wszystkie rekordy portfela przy każdym zapisie – sprawdzać rzadziej albo
     trzymać licznik;
   - `GetWallet`: `AccountBalances.GetAsync` drugi raz czyta konta po saldo otwarcia;
   - `BaseCurrency` czytane kilka razy na żądanie (budżety, `stats/*`) – dołożyć do `WalletAccess`;
   - dashboard: `RecurringEndpoints.LoadAsync` liczy wystąpienia wszystkich reguł, Home potrzebuje tylko najbliższych
     (sprawdzić licznik rat w `RecurringText.Upcoming`);
   - `records/months` to pełny skan rekordów (klient cache'uje per portfel – na razie wystarczy).
   Po audycie zapis rekordu odświeża tylko bieżący portfel; sumy na liście portfeli odświeżają się na `/wallets`.
8. **WCAG – ręcznie:** czytnik ekranu (TalkBack/VoiceOver), powiększenie 200/400 %, klawiatura na wszystkich
   ekranach, powrót focusu po zamknięciu `Sheet`/`ConfirmDialog` na iOS Safari.
9. **Wyłączenie MM** po okresie równoległym.

## Backlog (później)

- Kurs waluty: podpowiedź z API NBP (kwota w walucie transakcji + kurs już są).
- Inwestycje: pozycje (ticker, ilość), ceny z API.
- Budżet tagu (raport tagu już jest).
- Cykliczne: „dziś” liczone wg strefy użytkownika zamiast UTC; po pauzie brak dorabiania wystąpień (świadome).
- Import wyciągów bankowych (CSV), powiadomienia o nadchodzących płatnościach.
- Publiczna rejestracja (RODO, plany, płatności).

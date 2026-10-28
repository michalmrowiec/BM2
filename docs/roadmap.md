# BM2 – roadmapa

Stan na: 2026-10-28. BM2 (v2 MoneyManager) działa produkcyjnie na `<host-produkcji>` z danymi z MM.
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
   w `WalletEndpointAccessTests`; `Strings` (`Plural` dla 1/2/5/12/22/112, `TrMessage` z `{0}`, brak wpisu → EN),
   test, że każdy literał `Tr`/`Plural` ma wpis PL; `GET /culture/pl` (cookie, odrzuca nielokalny `returnUrl`);
   axe (Playwright) dla głównych ekranów w obu motywach.
7. **WCAG – ręcznie:** czytnik ekranu (TalkBack/VoiceOver), powiększenie 200/400 %, klawiatura na wszystkich
   ekranach, powrót focusu po zamknięciu `Sheet`/`ConfirmDialog` na iOS Safari.
8. **Wyłączenie MM** po okresie równoległym.

## Backlog (później)

- Kurs waluty: podpowiedź z API NBP (kwota w walucie transakcji + kurs już są).
- Inwestycje: pozycje (ticker, ilość), ceny z API.
- Budżet tagu (raport tagu już jest).
- Cykliczne: „dziś” liczone wg strefy użytkownika zamiast UTC; po pauzie brak dorabiania wystąpień (świadome).
- Import wyciągów bankowych (CSV), powiadomienia o nadchodzących płatnościach.
- Publiczna rejestracja (RODO, plany, płatności).

# Przegląd WCAG 2.2 AA i i18n – 2026-10-27

Zakres: klient Blazor (`BM2.Client`), strony konta SSR (`BM2.Server/Components/Account`), CSS w
`BM2.Server/wwwroot/css`. Sprawdzone na telefonie 375 px, motyw jasny i ciemny, EN i PL. Bez testów
automatycznych (axe), tylko przegląd kodu, kontrast liczony skryptem i skan DOM w przeglądarce (nazwy
i rozmiary elementów interaktywnych na Start, Operacje, formularzu operacji, Statystykach i Więcej).

## Co sprawdzone

| Kryterium | Wynik |
|-----------|-------|
| 1.3.1 / 3.3.2 Etykiety pól | Każde pole ma widoczną etykietę (`FieldBase` łączy `label`/`id`). Strony konta: etykiety i komunikaty po polsku. |
| 4.1.2 Nazwy przycisków-ikon | Skan DOM: brak elementów interaktywnych bez nazwy. Przyciski-ikony mają `aria-label` (tłumaczone). |
| 2.4.7 / 2.4.11 Widoczny focus | Globalny `:focus-visible` (2 px, `--focus`, odstęp 3 px). Kontrast `--focus` ≥ 4,5:1 we wszystkich akcentach i motywach. |
| 1.4.3 Kontrast tekstu | Policzone dla tokenów na `--paper`, `--sheet`, `--sheet-raised`, `--wash`, oba motywy, 5 akcentów. Poprawki niżej. |
| 1.4.11 Kontrast elementów | Obramowanie pól formularza miało 1,2:1 – poprawione do ≥ 3:1. |
| 2.5.8 Cel dotyku ≥ 24×24 | Skan: wszystkie cele ≥ 24 px (przyciski i ikony 44 px przez `--tap`). Jedyny mniejszy element (checkbox 20 px) leży w etykiecie `.check` o wysokości 44 px. |
| 1.4.1 Nie tylko kolor | Kwoty zawsze ze znakiem +/−, statusy i typy jako tekst, budżet „Przekroczono o …”, wybrany typ w `SegmentedControl` także pogrubiony i z `aria-checked`. |
| 2.1.1 / 2.4.3 Klawiatura, focus w dialogach | `Sheet` i `ConfirmDialog` to natywne `<dialog>` z `showModal()`: pułapka focusu, Escape, inertne tło i powrót focusu robi przeglądarka. Skip link „Przejdź do treści”. |
| 2.3.3 / `prefers-reduced-motion` | Globalne wyłączenie animacji w `base.css`; przejścia stron (View Transitions), motyw, `Sheet` i wykresy sprawdzają preferencję w JS. |
| 4.1.3 Komunikaty statusu | `ToastHost` ma `role="status"` + `aria-live="polite"`. |
| 3.1.1 Język strony | `<html lang>` z kultury żądania (`en`/`pl`), zgodny z wybranym językiem. |

## Co poprawione

- **Kontrast tokenów (jasny motyw):** `--vivid` terakota `#d9481f` → `#c9411a` (biały tekst na FAB i przyciskach
  4,3 → 4,9:1), `--link`/`--focus` `#b9411f` → `#b13f1d` (na `--wash` 4,3 → 4,6:1), `--ink-muted` `#6e675b` →
  `#675f53` (na `--wash` 4,4 → 5,0:1), `--positive` `#4f7a28` → `#476f24` (4,4 → 5,1:1), `--tag-amber`/`--attention`
  `#8a6100` → `#805c00` (na `--wash` 4,4 → 4,8:1). Ciemny motyw i pozostałe akcenty (ochra, ink, zieleń, śliwka)
  spełniały 4,5:1 bez zmian.
- **Nowy token `--control-border`** (jasny `#8f8574`, ciemny `#756d60`) dla obramowania `.field__control` – ≥ 3:1
  względem `--paper` i `--sheet`.
- **Reduced motion:** dodane `animation-iteration-count: 1`, żeby animacje zapętlone (`float`, `shimmer`) nie migały
  przy skróconym czasie.
- **i18n:** cały UI w EN/PL (słownik `Strings`), kultura dat i kwot = wybrany język, komunikaty walidacji
  (FluentValidation, DataAnnotations na stronach konta, błędy Identity) i ProblemDetails tłumaczone,
  `lang` na `<html>`.

## Co zostaje

- Osie wykresów ECharts formatują liczby po angielsku (`-20,000`) niezależnie od języka – do ustawienia
  `axisLabel.formatter` z kulturą.
- Domyślne komunikaty FluentValidation bez `WithMessage` (np. `IsInEnum`) zawierają angielskie nazwy właściwości.
- Tytuły przypiętych wykresów są zapisywane w języku, w którym je przypięto.
- Teksty z danych użytkownika (nazwy kont, kategorii z importu) zostają w oryginale – to nie jest tłumaczenie UI.
- Nie sprawdzone narzędziami: czytnik ekranu (TalkBack/VoiceOver), powiększenie 200%/400% (1.4.4, 1.4.10),
  pełna nawigacja klawiaturą po wszystkich ekranach (sprawdzone na kodzie, nie ręcznie).
- `ConfirmDialog` i `Sheet` przy zamknięciu z animacją: focus wraca dzięki przeglądarce – warto potwierdzić
  na iOS Safari.

## Proponowane testy (przy uzupełnianiu braków przed wdrożeniem)

- Jednostkowe `Strings`: `Plural` (1, 2, 5, 12, 22, 112), `TrMessage` z wzorcami `{0}`/`{2}`, brak wpisu → EN.
- Test, że każdy literał przekazany do `Tr`/`Plural` ma wpis PL (skrypt jak w tym przeglądzie, jako test xUnit
  czytający pliki źródłowe).
- Integracyjny: `GET /culture/pl` ustawia cookie i odrzuca nielokalny `returnUrl`; `lang="pl"` w odpowiedzi.
- bUnit: komponent renderuje polski tekst przy `CultureInfo.CurrentUICulture = pl-PL`.
- Automatyczny axe (np. Playwright + axe-core) dla głównych ekranów w obu motywach.

# Podprojekt 1A – Design system „Paper and ink” + shell aplikacji

Data: 2026-10-24 · Roadmapa: [docs/roadmap.md](../roadmap.md) · Poprzedni: [podprojekt 0](2026-10-24-podprojekt-0-fundamenty.md)

## Cel

Nowy, unikalny wygląd BM2 (kierunek **B · Paper and ink**, wybrany na makietach) z trybem jasnym, ciemnym
i systemowym oraz shell aplikacji (nagłówek, dolna nawigacja z FAB). Ekrany przenoszone w kolejnych etapach powstają
od razu w tym stylu.

## Zakres

- Tokeny CSS (kolory, typografia, odstępy, promienie, ruch) w trzech plikach: `tokens.css`, `base.css`, `components.css`
  (+ `account.css` dla stron konta z szablonu). Stary `app.css` znika.
- Fonty hostowane w aplikacji: Inter (UI) i Fraunces (nagłówki, kwoty) – zmienne woff2, latin + latin-ext, SIL OFL.
- Motyw: `light` / `dark` / `system`, zapamiętany w `localStorage` (`bm2-theme`), nakładany przed pierwszym
  malowaniem (bez mrugnięcia), przełącznik działający także na statycznych stronach konta.
- Komponenty: `Icon` (własne proste SVG), `ThemeToggle`, `BottomNav` (z FAB), `EmptyState`.
- Shell: nagłówek (logo, przełącznik motywu), dolna nawigacja dla zalogowanych: Home, Records, + (szybkie dodawanie),
  Stats, More. Strony Records / Stats / More / szybkie dodawanie jako puste stany do czasu przeniesienia funkcji.
- Nowy wygląd stron konta (logowanie, rejestracja, zarządzanie kontem).

## Poza zakresem

Przenoszenie funkcji, i18n, wykresy, PWA (manifest/service worker) – kolejne etapy.

## Paleta

| Rola | Jasny | Ciemny | Uwagi (kontrast WCAG) |
|------|-------|--------|-----------------------|
| `--paper` (tło) | `#F3EFE6` | `#161412` | |
| `--sheet` (karta) | `#FBF8F2` | `#201D19` | |
| `--ink` (tekst) | `#1B1A17` | `#EFE9DD` | ≫ 7:1 |
| `--ink-muted` | `#6E675B` | `#A79F90` | 5,3:1 / 6,4:1 |
| `--rule` (linie) | `#E2D9C8` | `#3A342D` | |
| `--wash` (chip) | `#ECE4D4` | `#2C2722` | |
| `--vivid` (FAB, akcent) | `#D9481F` | `#FF7A50` | ikona na FAB ≥ 3:1 |
| `--link` | `#B9411F` | `#FF8A66` | 5,2:1 / ≥ 6:1 |
| `--negative` (wydatek) | `#B8361A` | `#FF8A66` | 5,5:1 / ≥ 6:1 |
| `--positive` (przychód) | `#4F7A28` | `#A6CC73` | 4,8:1 / ≥ 7:1 |

Przycisk główny: tło `--ink`, tekst `--paper` („atrament na papierze”). Kwoty zawsze ze znakiem `+`/`−`
(informacja nie tylko kolorem).

## Typografia

- UI: Inter 400/500/600; nagłówki i kwoty: Fraunces 600/700 (`font-optical-sizing: auto`), kwoty w listach
  z `tabular-nums`.
- Skala: 12 / 14 / 16 / 20 / 28 / 40 px. Pola formularzy min. 16 px (brak zoomu na iOS).

## Motyw – działanie

- `wwwroot/js/theme.js` ładowany synchronicznie w `<head>` (bez skryptów inline – pod przyszły CSP):
  ustawia `data-theme-mode` (`system|light|dark`) i `data-theme` (`light|dark`, tylko gdy wybrany jawnie) na `<html>`,
  aktualizuje `<meta name="theme-color">`, reaguje na zmianę motywu systemu, obsługuje kliknięcia
  w `[data-theme-toggle]` (cykl system → light → dark) i ponawia ustawienia po nawigacji Blazora (`enhancedload`).
- CSS: ciemna paleta pod `@media (prefers-color-scheme: dark)` dla `:root:not([data-theme="light"])`
  i pod `:root[data-theme="dark"]`.

## Dostępność

Kontrasty jak w tabeli, widoczny `:focus-visible`, cele dotyku ≥ 44 px, `aria-current="page"` w nawigacji,
`aria-label` dla przycisków-ikon, `prefers-reduced-motion`, safe-area dla iPhone (`viewport-fit=cover`).

## Testy

- bUnit: `ThemeToggle` (przycisk z `data-theme-toggle` i etykietą), `BottomNav` (5 pozycji, etykieta FAB,
  `aria-current` dla bieżącej strony).
- Integracyjny: strona logowania ładuje `theme.js` w `<head>`, ma `theme-color` i przełącznik motywu.
- Ręcznie w przeglądarce: jasny / ciemny / systemowy, zapamiętanie wyboru, brak mrugnięcia, strony konta.

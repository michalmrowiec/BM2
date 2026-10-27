// Blazor JS initializer. Registers the service worker (fast start, see wwwroot/service-worker.js) on every page.
export function afterWebStarted() {
    navigator.serviceWorker?.register("/service-worker.js", { scope: "/" })
        .catch(error => console.warn("Service worker registration failed", error));
}

// WebAssembly replaces the prerendered markup when it starts; that swap must not animate.
// Afterwards new page content inside <main> gets .page-enter (entrance animation in css/motion.css).
export function afterWebAssemblyStarted() {
    document.addEventListener("beforeinput", blockInvalidMoney);
    document.addEventListener("input", cleanMoney);
    document.addEventListener("keydown", keepComboboxEnter);
    document.addEventListener("click", slideBetweenTabs, true);

    setTimeout(() => {
        document.documentElement.classList.add("motion-ready");

        const main = document.getElementById("main");
        if (!main) return;

        new MutationObserver(records => {
            let index = 0;
            for (const record of records) {
                for (const node of record.addedNodes) {
                    if (!(node instanceof HTMLElement) || document.documentElement.dataset.slide) continue;
                    node.style.setProperty("--enter-delay", `${Math.min(index++, 3) * 50}ms`);
                    node.classList.add("page-enter");
                }
            }
        }).observe(main, { childList: true });
    }, 400);
}

// Switching main tabs slides the page toward the tapped tab (View Transitions; see css/motion.css).
const tabs = ["", "records", "stats", "more"];
const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
const tabIndex = href => tabs.indexOf(new URL(href, document.baseURI).pathname.replace(/^\/|\/$/g, ""));

function slideBetweenTabs(event) {
    const link = event.target instanceof Element ? event.target.closest("a.bottom-nav__item") : null;
    if (!link || !document.startViewTransition || !window.Blazor?.navigateTo || reducedMotion.matches
        || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        return;
    }

    const from = tabIndex(location.href);
    const to = tabIndex(link.href);
    if (from < 0 || to < 0 || from === to) return;

    // Navigate ourselves (before Blazor sees the click) so the old page is captured first.
    event.preventDefault();
    event.stopPropagation();
    const root = document.documentElement;
    root.dataset.slide = to > from ? "forward" : "back";

    const main = document.getElementById("main");
    const transition = document.startViewTransition(() => new Promise(resolve => {
        const done = () => {
            observer.disconnect();
            resolve();
        };
        const observer = new MutationObserver(done);
        observer.observe(main, { childList: true });
        setTimeout(done, 300);
        window.Blazor.navigateTo(link.href);
    }));
    transition.finished.finally(() => delete root.dataset.slide);
}

// Enter on a highlighted combobox suggestion, or in a field marked data-enter-picks, picks (in Blazor) instead of
// submitting the form; cancelling keydown also stops the keypress that would reach a refocused form field.
function keepComboboxEnter(event) {
    const input = event.target;
    if (event.key === "Enter" && input instanceof HTMLInputElement
        && (input.hasAttribute("data-enter-picks")
            || (input.getAttribute("role") === "combobox" && input.getAttribute("aria-activedescendant")))) {
        event.preventDefault();
    }
}

// Amount fields (data-money="<decimals>"): digits, one "," or "." separator, at most <decimals> places.
function moneyRules(input) {
    const decimals = Number(input.dataset.money) || 0;
    const sign = input.hasAttribute("data-money-negative") ? "-?" : "";
    return { decimals, pattern: new RegExp(`^${sign}\\d*([.,]\\d{0,${decimals}})?$`) };
}

// Typed characters that would break the format never land, so the caret does not jump.
function blockInvalidMoney(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.dataset.money || event.data == null || !event.inputType.startsWith("insert")) {
        return;
    }

    // Longer insertions (paste, keyboard suggestions, dictation) go through and get cleaned on input.
    const next = input.value.slice(0, input.selectionStart) + event.data + input.value.slice(input.selectionEnd);
    if (event.data.length === 1 && !moneyRules(input).pattern.test(next)) {
        event.preventDefault();
    }
}

// Fallback for what beforeinput cannot stop (pastes, Android composition): "1 234,567" becomes "1234,56".
// A paste keeps its last separator as the decimal one; typing keeps the first, so "12,3" + "." stays "12,3".
function cleanMoney(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.dataset.money) {
        return;
    }

    const { decimals, pattern } = moneyRules(input);
    const value = input.value;
    if (pattern.test(value)) {
        return;
    }

    const negative = input.hasAttribute("data-money-negative") && value.trim().startsWith("-");
    let text = value.replace(/[^\d.,]/g, "");
    const pasted = event.inputType === "insertFromPaste" || event.inputType === "insertFromDrop";
    const at = pasted ? Math.max(text.lastIndexOf("."), text.lastIndexOf(",")) : text.search(/[.,]/);
    if (at >= 0) {
        const whole = text.slice(0, at).replace(/[.,]/g, "");
        const fraction = text.slice(at + 1).replace(/[.,]/g, "").slice(0, decimals);
        text = whole + text[at] + fraction;
    }

    const cleaned = (negative ? "-" : "") + text;
    const caret = Math.min(cleaned.length, Math.max(0, (input.selectionStart ?? value.length) - (value.length - cleaned.length)));
    input.value = cleaned;
    input.setSelectionRange(caret, caret);
}

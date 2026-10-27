// Applies the saved theme and preferences before first paint. Handles [data-theme-toggle] (light ↔ dark; follows the
// system until the first choice) and [data-balances-toggle] (hide totals and balances).
(() => {
    const storageKey = "bm2-theme";
    const modes = ["system", "light", "dark"]; // "system" = nothing saved yet
    const themeColors = { light: "#f3efe6", dark: "#161412" };
    const root = document.documentElement;
    const systemDark = window.matchMedia("(prefers-color-scheme: dark)");

    const read = () => {
        try { return localStorage.getItem(storageKey); } catch { return null; }
    };

    const write = mode => {
        try {
            if (mode === "system") localStorage.removeItem(storageKey);
            else localStorage.setItem(storageKey, mode);
        } catch { /* storage unavailable: the choice lasts for this page only */ }
    };

    const apply = mode => {
        if (!modes.includes(mode)) mode = "system";
        root.dataset.themeMode = mode;
        if (mode === "system") delete root.dataset.theme;
        else root.dataset.theme = mode;

        const dark = mode === "dark" || (mode === "system" && systemDark.matches);
        document.querySelector('meta[name="theme-color"]')?.setAttribute("content", dark ? themeColors.dark : themeColors.light);
    };

    apply(read());

    // Preferences as data-* on <html>: amounts ("plain" = UI font), accent (none = terracotta), balances ("hidden").
    const prefs = ["amounts", "accent", "balances"];
    const readPref = name => {
        try { return localStorage.getItem(`bm2-${name}`); } catch { return null; }
    };
    const applyPref = (name, value) => {
        if (value) root.dataset[name] = value;
        else delete root.dataset[name];
    };
    const syncBalancesToggles = () => {
        const pressed = root.dataset.balances === "hidden" ? "true" : "false";
        document.querySelectorAll("[data-balances-toggle]").forEach(button => button.setAttribute("aria-pressed", pressed));
    };
    const applyPrefs = () => {
        prefs.forEach(name => applyPref(name, readPref(name)));
        syncBalancesToggles();
    };
    applyPrefs();

    window.bm2Prefs = {
        get: name => (prefs.includes(name) ? root.dataset[name] ?? null : null),
        set: (name, value) => {
            if (!prefs.includes(name)) return;
            try {
                if (value) localStorage.setItem(`bm2-${name}`, value);
                else localStorage.removeItem(`bm2-${name}`);
            } catch { /* storage unavailable: the choice lasts for this page only */ }
            applyPref(name, value);
            if (name === "balances") syncBalancesToggles();
        },
    };

    systemDark.addEventListener("change", () => apply(root.dataset.themeMode));

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

    document.addEventListener("click", event => {
        const balances = event.target instanceof Element ? event.target.closest("[data-balances-toggle]") : null;
        if (balances) window.bm2Prefs.set("balances", root.dataset.balances === "hidden" ? null : "hidden");
    });

    document.addEventListener("click", event => {
        const toggle = event.target instanceof Element ? event.target.closest("[data-theme-toggle]") : null;
        if (!toggle) return;
        const dark = root.dataset.theme === "dark" || (!root.dataset.theme && systemDark.matches);
        const next = dark ? "light" : "dark";
        write(next);

        if (!document.startViewTransition || reducedMotion.matches) {
            apply(next);
            return;
        }

        // Reveal the new theme from the button (see motion.css).
        const box = toggle.getBoundingClientRect();
        root.style.setProperty("--reveal-x", `${box.left + box.width / 2}px`);
        root.style.setProperty("--reveal-y", `${box.top + box.height / 2}px`);
        root.classList.add("theme-switching");
        document.startViewTransition(() => apply(next)).finished
            .finally(() => root.classList.remove("theme-switching"));
    });

    // Enhanced navigation can patch <html> attributes; restore the theme afterwards.
    document.addEventListener("DOMContentLoaded", () => {
        syncBalancesToggles();
        window.Blazor?.addEventListener?.("enhancedload", () => {
            apply(read());
            applyPrefs();
        });
    });
})();

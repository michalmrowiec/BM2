// Applies the saved theme and amount font before first paint and handles [data-theme-toggle] buttons (system → light → dark).
(() => {
    const storageKey = "bm2-theme";
    const modes = ["system", "light", "dark"];
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

    // Amount font: the display serif by default, or the plain UI font (switched in More).
    const amountsKey = "bm2-amounts";
    const readAmounts = () => {
        try { return localStorage.getItem(amountsKey); } catch { return null; }
    };
    const applyAmounts = value => {
        if (value === "plain") root.dataset.amounts = "plain";
        else delete root.dataset.amounts;
    };
    applyAmounts(readAmounts());

    window.bm2Amounts = {
        isPlain: () => root.dataset.amounts === "plain",
        setPlain: plain => {
            try {
                if (plain) localStorage.setItem(amountsKey, "plain");
                else localStorage.removeItem(amountsKey);
            } catch { /* storage unavailable: the choice lasts for this page only */ }
            applyAmounts(plain ? "plain" : null);
        },
    };

    systemDark.addEventListener("change", () => apply(root.dataset.themeMode));

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

    document.addEventListener("click", event => {
        const toggle = event.target instanceof Element ? event.target.closest("[data-theme-toggle]") : null;
        if (!toggle) return;
        const next = modes[(modes.indexOf(root.dataset.themeMode) + 1) % modes.length];
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
        window.Blazor?.addEventListener?.("enhancedload", () => {
            apply(read());
            applyAmounts(readAmounts());
        });
    });
})();

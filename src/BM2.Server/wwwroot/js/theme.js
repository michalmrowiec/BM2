// Applies the saved theme before first paint and handles [data-theme-toggle] buttons (system → light → dark).
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

    systemDark.addEventListener("change", () => apply(root.dataset.themeMode));

    document.addEventListener("click", event => {
        if (!(event.target instanceof Element) || !event.target.closest("[data-theme-toggle]")) return;
        const next = modes[(modes.indexOf(root.dataset.themeMode) + 1) % modes.length];
        write(next);
        apply(next);
    });

    // Enhanced navigation can patch <html> attributes; restore the theme afterwards.
    document.addEventListener("DOMContentLoaded", () => {
        window.Blazor?.addEventListener?.("enhancedload", () => apply(read()));
    });
})();

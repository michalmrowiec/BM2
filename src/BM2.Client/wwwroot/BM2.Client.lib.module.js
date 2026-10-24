// Blazor JS initializer. WebAssembly replaces the prerendered markup when it starts; that swap must not animate.
// Afterwards new page content inside <main> gets .page-enter (entrance animation in css/motion.css).
export function afterWebAssemblyStarted() {
    setTimeout(() => {
        document.documentElement.classList.add("motion-ready");

        const main = document.getElementById("main");
        if (!main) return;

        new MutationObserver(records => {
            let index = 0;
            for (const record of records) {
                for (const node of record.addedNodes) {
                    if (!(node instanceof HTMLElement)) continue;
                    node.style.setProperty("--enter-delay", `${Math.min(index++, 3) * 50}ms`);
                    node.classList.add("page-enter");
                }
            }
        }).observe(main, { childList: true });
    }, 400);
}

const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)");

export function open(dialog) {
    if (!dialog.open) {
        dialog.classList.remove("is-closing");
        dialog.showModal();
    }

    // Escape closes with the same animation as the close button.
    if (!dialog.dataset.animatedCancel) {
        dialog.dataset.animatedCancel = "true";
        dialog.addEventListener("cancel", event => {
            event.preventDefault();
            close(dialog);
        });
    }
}

// Plays the exit animation (css/motion.css, .sheet.is-closing), then closes.
export function close(dialog) {
    if (!dialog.open || dialog.classList.contains("is-closing")) {
        return Promise.resolve();
    }

    if (reducedMotion.matches) {
        dialog.close();
        return Promise.resolve();
    }

    dialog.classList.add("is-closing");
    return new Promise(resolve => {
        const done = () => {
            clearTimeout(timer);
            dialog.removeEventListener("animationend", onEnd);
            dialog.classList.remove("is-closing");
            dialog.close();
            resolve();
        };
        const onEnd = event => {
            if (event.target === dialog) done();
        };
        // Fallback in case the animation never runs (e.g. the tab is hidden).
        const timer = setTimeout(done, 400);
        dialog.addEventListener("animationend", onEnd);
    });
}

// Asks .NET for the next page when the end of the list comes near.
export function observe(sentinel, page) {
    const observer = new IntersectionObserver(entries => {
        if (entries.some(entry => entry.isIntersecting)) {
            page.invokeMethodAsync("LoadMoreAsync");
        }
    }, { rootMargin: "600px 0px" });
    observer.observe(sentinel);
    return observer;
}

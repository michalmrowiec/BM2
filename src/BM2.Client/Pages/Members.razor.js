export const canShare = () => typeof navigator.share === "function";

export function share(url) {
    return navigator.share({ title: "BM2 invitation", url });
}

export function copy(text) {
    return navigator.clipboard.writeText(text);
}

export function focus(input, select) {
    input.focus({ preventScroll: true });
    if (select) {
        input.select();
    }
}

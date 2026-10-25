// Re-opening a popover moves it to the top of the top layer, above any modal dialog opened since.
export function raise(host) {
    if (host.matches(":popover-open")) {
        host.hidePopover();
    }
    host.showPopover();
}

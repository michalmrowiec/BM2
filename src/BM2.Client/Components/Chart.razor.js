// Thin ECharts wrapper: loads the library on first use, resolves "var(--token)" colors and redraws on theme changes.
const scriptUrl = "/lib/echarts/echarts.min.js";
const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)");
const systemDark = matchMedia("(prefers-color-scheme: dark)");
const charts = new Set();
let loading;

function loadLibrary() {
    if (window.echarts) return Promise.resolve(window.echarts);
    return loading ??= new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = scriptUrl;
        script.onload = () => resolve(window.echarts);
        script.onerror = () => {
            loading = null;
            reject(new Error("ECharts failed to load."));
        };
        document.head.appendChild(script);
    });
}

function resolveTokens(value, style) {
    if (typeof value === "string") {
        return value.startsWith("var(--") ? style.getPropertyValue(value.slice(4, -1)).trim() : value;
    }
    if (Array.isArray(value)) return value.map(item => resolveTokens(item, style));
    if (value && typeof value === "object") {
        return Object.fromEntries(Object.entries(value).map(([key, item]) => [key, resolveTokens(item, style)]));
    }
    return value;
}

function baseOption() {
    return {
        animation: !reducedMotion.matches,
        textStyle: { fontFamily: "Inter, ui-sans-serif, system-ui, sans-serif", color: "var(--ink-muted)" },
        tooltip: {
            backgroundColor: "var(--sheet-raised)",
            borderColor: "var(--rule)",
            textStyle: { color: "var(--ink)", fontFamily: "Inter, ui-sans-serif, system-ui, sans-serif" },
        },
    };
}

// Numbers on value axes and in tooltips follow the page language (ECharts formats them as en-US).
function localize(option) {
    const lang = document.documentElement.lang || undefined;
    const number = new Intl.NumberFormat(lang, { maximumFractionDigits: 2 });
    const percent = new Intl.NumberFormat(lang, { style: "percent", minimumFractionDigits: 1, maximumFractionDigits: 1 });
    const axes = [option.yAxis, option.xAxis].flat().filter(axis => axis?.type === "value");
    for (const axis of axes) {
        axis.axisLabel = { formatter: value => number.format(value), ...axis.axisLabel };
    }

    option.tooltip = { valueFormatter: value => (typeof value === "number" ? number.format(value) : value), ...option.tooltip };
    const template = option.tooltip.formatter;
    if (typeof template === "string" && template.includes("{c}")) {
        option.tooltip.formatter = params => template
            .replace("{b}", params.name)
            .replace("{c}", number.format(params.value))
            .replace("{d}", percent.format(params.percent / 100).replace("%", ""));
    }
    return option;
}

function render(entry) {
    if (!entry.option) return;
    const style = getComputedStyle(entry.element);
    entry.chart.setOption(localize(resolveTokens({ ...baseOption(), ...entry.option }, style)), true);
}

function redrawAll() {
    charts.forEach(render);
}

new MutationObserver(redrawAll).observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme", "data-amounts", "data-accent"] });
systemDark.addEventListener("change", redrawAll);
reducedMotion.addEventListener("change", redrawAll);

export async function create(element, dotnet) {
    const echarts = await loadLibrary();
    const entry = { element, chart: echarts.init(element, null, { renderer: "svg" }), option: null };
    entry.chart.on("click", params => dotnet.invokeMethodAsync("OnClick", String(params.data?.id ?? params.name ?? "")));
    entry.observer = new ResizeObserver(() => entry.chart.resize());
    entry.observer.observe(element);
    charts.add(entry);

    return {
        setOption(option) {
            entry.option = option;
            render(entry);
        },
        resize() {
            entry.chart.resize();
        },
        dispose() {
            charts.delete(entry);
            entry.observer.disconnect();
            entry.chart.dispose();
        },
    };
}

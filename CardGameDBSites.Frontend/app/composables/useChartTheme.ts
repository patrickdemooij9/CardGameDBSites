import { useColorMode } from "~/composables/useColorMode";

export function useChartTheme() {
  const { isDark } = useColorMode();

  return computed(() =>
    isDark.value
      ? { title: "#f1f0ee", text: "#cac6c2", muted: "#9b968f", grid: "rgba(255,255,255,0.08)" }
      : { title: "#111827", text: "#374151", muted: "#64748b", grid: "rgba(0,0,0,0.06)" }
  );
}

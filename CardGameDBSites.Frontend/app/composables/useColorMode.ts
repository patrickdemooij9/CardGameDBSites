export type ColorModePreference = "light" | "dark" | "system";

export const ColorModeCookie = "color-mode";

export function useColorMode() {
  // No default: a default gets written as Set-Cookie, which SWR caches and replays to every visitor.
  const cookie = useCookie<ColorModePreference | null>(ColorModeCookie, {
    maxAge: 60 * 60 * 24 * 365,
    sameSite: "lax",
  });
  const systemPrefersDark = useState("system-prefers-dark", () => false);

  const preference = computed<ColorModePreference>(() => cookie.value ?? "system");
  const isDark = computed(
    () =>
      preference.value === "dark" ||
      (preference.value === "system" && systemPrefersDark.value)
  );

  function toggle() {
    cookie.value = isDark.value ? "light" : "dark";
  }

  return { preference, systemPrefersDark, isDark, toggle };
}

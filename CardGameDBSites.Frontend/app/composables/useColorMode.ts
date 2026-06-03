const COLOR_MODE_KEY = 'color-mode';

export function useColorMode() {
  const isDark = useState<boolean>('color-mode', () => false);

  function initialize() {
    if (import.meta.server) return;

    const stored = localStorage.getItem(COLOR_MODE_KEY);
    if (stored === 'dark') {
      isDark.value = true;
    } else if (stored === 'light') {
      isDark.value = false;
    } else {
      isDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches;
    }
    applyClass();
  }

  function toggle() {
    isDark.value = !isDark.value;
    if (!import.meta.server) {
      localStorage.setItem(COLOR_MODE_KEY, isDark.value ? 'dark' : 'light');
    }
    applyClass();
  }

  function applyClass() {
    if (import.meta.server) return;
    document.documentElement.classList.toggle('dark', isDark.value);
  }

  return {
    isDark,
    initialize,
    toggle,
  };
}

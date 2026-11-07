/** @type {import('tailwindcss').Config} */
export default {
  content: [],
  darkMode: 'selector',
  theme: {
    extend: {
      colors: {
        canvas: 'rgb(var(--color-canvas) / <alpha-value>)',
        surface: 'rgb(var(--color-surface) / <alpha-value>)',
        line: 'rgb(var(--color-line) / <alpha-value>)',
        ink: {
          DEFAULT: 'rgb(var(--color-ink) / <alpha-value>)',
          muted: 'rgb(var(--color-ink-muted) / <alpha-value>)',
        },
        positive: 'rgb(var(--color-positive) / <alpha-value>)',
        negative: 'rgb(var(--color-negative) / <alpha-value>)',
      },
    },
    container: {
        center: true,
        screens: {
            sm: '100%',
            md: '100%',
            lg: '100%',
            xl: '100%',
            '2xl': '1536px'
        },
    },
  },
  safelist: [
    'grid-cols-6',
    'grid-cols-5',
    'grid-cols-3',
    'grid-cols-4',
    'lg:grid-cols-4',
    'lg:grid-cols-5',
    'lg:grid-cols-3',
    'lg:grid-cols-6',
    'border-red-300'
  ]
}


/** @type {import('tailwindcss').Config} */
const plugin = require('tailwindcss/plugin')

module.exports = {
    content: ['./../Views/**/*.cshtml', "./../Components/**/*.razor"],
    theme: {
        extend: {},
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
    plugins: [
        plugin(function ({ addVariant }) {
            addVariant('scrollbar', '&::-webkit-scrollbar')
            addVariant('scrollbar-track', '&::-webkit-scrollbar-track')
            addVariant('scrollbar-thumb', '&::-webkit-scrollbar-thumb')
        })
    ],
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

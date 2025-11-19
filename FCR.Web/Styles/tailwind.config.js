/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Views/**/*.cshtml',
        './Pages/**/*.cshtml',
        './Areas/**/*.cshtml',
    ],
    theme: {
        extend: {
            colors: {
                'fcr-blue': '#3b82f6',
                'fcr-dark': '#1e293b',
                'fcr-accent': '#f59e0b',
            },
        },
    },
    plugins: [],
}

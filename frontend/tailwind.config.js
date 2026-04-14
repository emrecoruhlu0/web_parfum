/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        serif: ['"Playfair Display"', 'Georgia', 'serif'],
        sans: ['Inter', 'sans-serif'],
      },
      colors: {
        'bg-light': '#F8F5F0',
        'text-gray': '#9CA3AF',
        'text-dark': '#111827',
      },
    },
  },
  plugins: [],
}


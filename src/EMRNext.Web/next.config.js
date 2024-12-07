/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  reactStrictMode: true,
  // Add any additional configuration here
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api',
  },
  // Optional: Configure webpack if needed
  webpack: (config, { isServer }) => {
    // Add any custom webpack configuration
    return config;
  }
}

module.exports = nextConfig

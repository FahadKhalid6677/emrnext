import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'EMRNext',
  description: 'Electronic Medical Record Management System',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  )
}

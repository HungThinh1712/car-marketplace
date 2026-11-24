import type { Metadata } from "next";
import { Poppins } from "next/font/google";
import { AntdProvider } from "@/components/AntdProvider";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { getCurrentSeller } from "@/lib/api";
import "./globals.css";

const body = Poppins({ subsets: ["latin"], weight: ["300", "400", "500", "600", "700"], variable: "--font-body" });

export const metadata: Metadata = {
  title: "Cars for sale",
  description: "Browse cars for sale.",
};

export default async function RootLayout({ children }: { children: React.ReactNode }) {
  const seller = await getCurrentSeller();

  return (
    <html lang="en" className={body.variable}>
      <body>
        <AntdProvider>
          <SiteHeader seller={seller} />
          <main>{children}</main>
          <SiteFooter />
        </AntdProvider>
        <script src="/photo-review.js" defer />
      </body>
    </html>
  );
}

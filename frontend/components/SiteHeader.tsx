"use client";

import { MenuOutlined } from "@ant-design/icons";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import type { Seller } from "@/lib/types";
import { SignOutButton } from "./SignOutButton";

export function SiteHeader({ seller }: { seller: Seller | null }) {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  const home = pathname === "/";

  return (
    <header className="site-header">
      <div className="nav">
        <Link className="logo" href="/" onClick={() => setOpen(false)}>
          Cars <span>for sale</span>
        </Link>
        <button className="menu-icon" type="button" aria-expanded={open} aria-label="Menu" onClick={() => setOpen((value) => !value)}>
          <MenuOutlined />
        </button>
        <nav className={open ? "navbar active" : "navbar"} aria-label="Account">
          <Link className={home ? "active" : undefined} href="/" onClick={() => setOpen(false)}>
            Home
          </Link>
          {seller ? (
            <>
              <Link className={pathname.startsWith("/sell/new") ? "active" : undefined} href="/sell/new" onClick={() => setOpen(false)}>
                Sell a car
              </Link>
              <Link className={pathname.startsWith("/sell/listings") ? "active" : undefined} href="/sell/listings" onClick={() => setOpen(false)}>
                My listings
              </Link>
              <span className="seller-name">{seller.displayName}</span>
              <SignOutButton />
            </>
          ) : (
            <>
              <Link className={pathname === "/register" ? "active" : undefined} href="/register" onClick={() => setOpen(false)}>
                Register
              </Link>
              <Link className={pathname === "/sign-in" ? "active" : undefined} href="/sign-in" onClick={() => setOpen(false)}>
                Sign in
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  );
}

import { cookies } from "next/headers";
import type { Listing, ListingList, Seller } from "./types";

function origin(): string {
  return process.env.API_ORIGIN ?? "http://localhost:5080";
}

export async function apiFetch(path: string): Promise<Response> {
  const cookieStore = await cookies();
  const cookie = cookieStore.toString();
  const headers = new Headers();
  if (cookie) {
    headers.set("cookie", cookie);
  }

  return fetch(`${origin()}${path}`, {
    headers,
    cache: "no-store",
  });
}

export async function getCurrentSeller(): Promise<Seller | null> {
  const response = await apiFetch("/api/v1/sellers/me");
  if (!response.ok) {
    return null;
  }

  return (await response.json()) as Seller;
}

export async function sellerOwnsListing(listingId: string): Promise<boolean> {
  let page = 1;
  for (let attempt = 0; attempt < 20; attempt += 1) {
    const response = await apiFetch(`/api/v1/sellers/me/listings?page=${page}`);
    if (!response.ok) {
      return false;
    }

    const list = (await response.json()) as ListingList;
    if (list.items.some((item) => item.id === listingId)) {
      return true;
    }

    if (page * list.pageSize >= list.totalCount || list.items.length === 0) {
      return false;
    }

    page += 1;
  }

  return false;
}

export async function getListing(id: string): Promise<Listing | null> {
  const response = await apiFetch(`/api/v1/listings/${id}`);
  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error("The listing could not be loaded.");
  }

  return (await response.json()) as Listing;
}

export function money(amount: number): string {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(amount);
}

export function firstValue(value: string | string[] | undefined): string {
  if (Array.isArray(value)) {
    return value[0] ?? "";
  }

  return value ?? "";
}

import Link from "next/link";
import { redirect } from "next/navigation";
import { apiFetch, money } from "@/lib/api";
import type { ListingList } from "@/lib/types";

export default async function MyListingsPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const params = await searchParams;
  const pageValue = Array.isArray(params.page) ? params.page[0] : params.page;
  const page = Math.max(1, Number(pageValue) || 1);
  const response = await apiFetch(`/api/v1/sellers/me/listings?page=${page}`);
  if (response.status === 401) {
    redirect("/sign-in?next=/sell/listings");
  }

  if (!response.ok) {
    return <p className="error">Your listings could not be loaded.</p>;
  }

  const list = (await response.json()) as ListingList;

  return (
    <>
      <h1>My listings</h1>
      <p>
        <Link href="/sell/new">Publish a car</Link>
      </p>
      {list.items.length === 0 ? <p>You have not published a car yet.</p> : null}
      <div className="results">
        {list.items.map((item) => (
          <article className="card" key={item.id}>
            <div>
              <h2>
                {item.year} {item.make} {item.model}
              </h2>
              <p>{money(item.price)} USD</p>
              <p className={`status status-${item.status.toLowerCase()}`}>{item.status}</p>
              <p className="actions">
                <Link href={`/listings/${item.id}`}>View</Link>
                <Link href={`/sell/listings/${item.id}/edit`}>Edit</Link>
                <Link href={`/sell/listings/${item.id}/inquiries`}>Inquiries</Link>
              </p>
            </div>
          </article>
        ))}
      </div>
    </>
  );
}

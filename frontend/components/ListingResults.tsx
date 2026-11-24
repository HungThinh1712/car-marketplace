"use client";

import { Empty, Pagination } from "antd";
import Link from "next/link";
import { useRouter } from "next/navigation";

type Item = {
  id: string;
  year: number;
  make: string;
  model: string;
  price: string;
  mileage: string;
  location: string;
  status: string;
  coverPhotoUrl: string | null;
  href: string;
};

export function ListingResults({
  items,
  page,
  pageSize,
  total,
  query,
  filtersActive,
}: {
  items: Item[];
  page: number;
  pageSize: number;
  total: number;
  query: string;
  filtersActive: boolean;
}) {
  const router = useRouter();

  function openPage(nextPage: number) {
    const next = new URLSearchParams(query);
    next.set("page", String(nextPage));
    router.push(`/?${next.toString()}`);
  }

  return (
    <section>
      <div className="heading">
        <span>All cars</span>
        <h2>Cars for sale</h2>
        <p>{total === 1 ? "1 car" : `${total} cars`}. Prices are in USD and mileage is in kilometers.</p>
      </div>
      {total === 0 && !filtersActive ? <Empty description="There are no cars for sale yet." /> : null}
      {total === 0 && filtersActive ? (
        <Empty description="No cars match these filters.">
          <a href="/">Clear filters</a>
        </Empty>
      ) : null}
      <div className="cars-container">
        {items.map((item) => (
          <Link className="car-tile" key={item.id} href={item.href}>
            {item.coverPhotoUrl ? (
              <img src={item.coverPhotoUrl} alt={`${item.year} ${item.make} ${item.model}`} />
            ) : (
              <div className="photo-fallback" />
            )}
            <span className="car-chip">
              {item.year} {item.make} {item.model}
            </span>
          </Link>
        ))}
      </div>
      <Pagination className="pager" current={page} pageSize={pageSize} total={total} showSizeChanger={false} onChange={openPage} />
    </section>
  );
}

import { ListingFilters, type FilterValues } from "@/components/ListingFilters";
import { ListingResults } from "@/components/ListingResults";
import { apiFetch, firstValue, money } from "@/lib/api";
import type { ListingList } from "@/lib/types";

type Search = Record<string, string | string[] | undefined>;

export default async function BrowsePage({ searchParams }: { searchParams: Promise<Search> }) {
  const params = await searchParams;
  const values: FilterValues = {
    make: firstValue(params.make),
    model: firstValue(params.model),
    minYear: firstValue(params.minYear),
    maxYear: firstValue(params.maxYear),
    minPrice: firstValue(params.minPrice),
    maxPrice: firstValue(params.maxPrice),
    maxMileage: firstValue(params.maxMileage),
    location: firstValue(params.location),
    status: firstValue(params.status),
  };
  const page = Math.max(1, Number(firstValue(params.page)) || 1);
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(values)) {
    if (value && !(key === "status" && value === "available")) {
      query.set(key, value);
    }
  }
  query.set("page", String(page));
  const response = await apiFetch(`/api/v1/listings?${query.toString()}`);
  if (!response.ok) {
    const problem = (await response.json()) as { detail?: string };
    return <p className="error">{problem.detail ?? "The catalog could not be loaded."}</p>;
  }

  const list = (await response.json()) as ListingList;
  const filtersActive = Object.entries(values).some(([key, value]) => value && !(key === "status" && value === "available"));
  const detailQuery = query.toString();

  return (
    <>
      <section className="home">
        <div className="home-text">
          <h1>
            We have every <span>car</span> you need
          </h1>
          <p>Browse cars for sale, open a listing, and send an inquiry. Prices are in USD. Mileage is in kilometers.</p>
          <a className="btn" href="#cars">
            Browse cars
          </a>
        </div>
        <div className="home-visual" aria-hidden="true">
          <img src="/images/hero-car.png" alt="" />
        </div>
      </section>
      <ListingFilters values={values} />
      <div id="cars">
        <ListingResults
        items={list.items.map((item) => ({
          id: item.id,
          year: item.year,
          make: item.make,
          model: item.model,
          price: money(item.price),
          mileage: `${item.mileage.toLocaleString("en-US")} ${item.mileageUnit}`,
          location: item.location,
          status: item.status,
          coverPhotoUrl: item.coverPhotoUrl,
          href: detailQuery ? `/listings/${item.id}?${detailQuery}` : `/listings/${item.id}`,
        }))}
        page={list.page}
        pageSize={list.pageSize}
        total={list.totalCount}
        query={query.toString()}
        filtersActive={filtersActive}
        />
      </div>
    </>
  );
}

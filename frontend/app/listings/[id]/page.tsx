import Link from "next/link";
import { notFound } from "next/navigation";
import { InquiryForm } from "@/components/InquiryForm";
import { firstValue, getListing, money, sellerOwnsListing } from "@/lib/api";

type Search = Record<string, string | string[] | undefined>;

export default async function ListingDetailPage({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<Search>;
}) {
  const { id } = await params;
  const query = await searchParams;
  const listing = await getListing(id);
  if (!listing) {
    notFound();
  }

  const back = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    const text = firstValue(value);
    if (text) {
      back.set(key, text);
    }
  }
  const backHref = back.size > 0 ? `/?${back.toString()}` : "/";
  const owner = await sellerOwnsListing(listing.id);

  return (
    <>
      <p>
        <Link href={backHref}>Back to results</Link>
      </p>
      <div className="sticker">
        <div>
          <h1>
            {listing.year} {listing.make} {listing.model}
          </h1>
          <p className={`status status-${listing.status.toLowerCase()}`}>{listing.status}</p>
          <p className="price">
            {money(listing.price)} {listing.currencyCode}
          </p>
        </div>
        <p className="specs">
          <span>{listing.mileage.toLocaleString("en-US")} {listing.mileageUnit}</span>
          <span>{listing.location}</span>
          <span>Seller: {listing.sellerDisplayName}</span>
        </p>
      </div>
      <div className="gallery">
        {listing.photos.map((photo) => (
          <img key={photo.id} src={photo.url} alt={`${listing.year} ${listing.make} ${listing.model}`} />
        ))}
      </div>
      <p>{listing.description}</p>
      {owner ? (
        <p className="actions">
          <Link href={`/sell/listings/${listing.id}/edit`}>Edit this listing</Link>
          <Link href={`/sell/listings/${listing.id}/inquiries`}>Inquiries</Link>
        </p>
      ) : null}
      {listing.status === "Available" && !owner ? (
        <InquiryForm listingId={listing.id} make={listing.make} model={listing.model} year={listing.year} />
      ) : null}
    </>
  );
}

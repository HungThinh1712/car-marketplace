"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { problemText, readProblem } from "@/lib/problem";
import type { Listing, ListingPhoto } from "@/lib/types";

export default function EditListingPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const [listing, setListing] = useState<Listing | null>(null);
  const [error, setError] = useState("");
  const [removed, setRemoved] = useState<string[]>([]);

  useEffect(() => {
    let active = true;
    async function load() {
      const me = await fetch("/api/v1/sellers/me");
      if (me.status === 401) {
        router.push(`/sign-in?next=/sell/listings/${params.id}/edit`);
        return;
      }

      const response = await fetch(`/api/v1/listings/${params.id}`);
      if (response.status === 404) {
        setError("That listing was not found.");
        return;
      }

      const body = (await response.json()) as Listing;
      if (active) {
        setListing(body);
      }
    }

    void load();
    return () => {
      active = false;
    };
  }, [params.id, router]);

  async function saveFacts(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!listing) {
      return;
    }

    setError("");
    const data = new FormData(event.currentTarget);
    const response = await fetch(`/api/v1/listings/${listing.id}`, {
      method: "PUT",
      headers: { "content-type": "application/json", accept: "application/json" },
      body: JSON.stringify({
        make: data.get("make"),
        model: data.get("model"),
        year: Number(data.get("year")),
        price: Number(data.get("price")),
        mileage: Number(data.get("mileage")),
        location: data.get("location"),
        description: data.get("description"),
      }),
    });
    await showResult(response);
  }

  async function savePhotos(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!listing) {
      return;
    }

    setError("");
    const data = new FormData(event.currentTarget);
    const form = new FormData();
    const kept = listing.photos.filter((photo) => !removed.includes(photo.id)).sort((a, b) => a.sortOrder - b.sortOrder);
    for (const photo of kept) {
      const file = await fetch(photo.url);
      const blob = await file.blob();
      form.append("photos", blob, `${photo.id}.img`);
    }

    for (const value of data.getAll("photos")) {
      if (value instanceof File && value.size > 0) {
        form.append("photos", value);
      }
    }

    if (form.getAll("photos").length === 0) {
      setError("Add 1 to 10 still photos. The first photo is the cover.");
      return;
    }

    const response = await fetch(`/api/v1/listings/${listing.id}/photos`, { method: "PUT", body: form });
    await showResult(response);
  }

  async function changeStatus(status: "Available" | "Sold") {
    if (!listing) {
      return;
    }

    setError("");
    const response = await fetch(`/api/v1/listings/${listing.id}/status`, {
      method: "PATCH",
      headers: { "content-type": "application/json", accept: "application/json" },
      body: JSON.stringify({ status }),
    });
    await showResult(response);
  }

  async function showResult(response: Response) {
    if (response.status === 401) {
      router.push(`/sign-in?next=/sell/listings/${params.id}/edit`);
      return;
    }

    if (!response.ok) {
      setError(problemText(await readProblem(response)));
      return;
    }

    setListing((await response.json()) as Listing);
    setRemoved([]);
    router.refresh();
  }

  if (!listing) {
    return error ? <p className="error">{error}</p> : <p>Loading the listing.</p>;
  }

  return (
    <>
      <h1>
        Edit {listing.year} {listing.make} {listing.model}
      </h1>
      {error ? <p className="error">{error}</p> : null}
      <form className="form" onSubmit={saveFacts}>
        <h2>Facts</h2>
        <label>
          Make
          <input name="make" defaultValue={listing.make} required maxLength={50} />
        </label>
        <label>
          Model
          <input name="model" defaultValue={listing.model} required maxLength={50} />
        </label>
        <label>
          Year
          <input name="year" defaultValue={listing.year} required />
        </label>
        <label>
          Price (USD)
          <input name="price" defaultValue={listing.price} required />
        </label>
        <label>
          Mileage (km)
          <input name="mileage" defaultValue={listing.mileage} required />
        </label>
        <label>
          Location
          <input name="location" defaultValue={listing.location} required minLength={2} maxLength={80} />
        </label>
        <label>
          Description
          <textarea name="description" defaultValue={listing.description} required maxLength={2000} rows={6} />
        </label>
        <button type="submit">Save facts</button>
      </form>
      <form className="form" onSubmit={savePhotos}>
        <h2>Photos</h2>
        <p>The first kept photo, then any new files, becomes the cover-first set.</p>
        <ul>
          {listing.photos.map((photo: ListingPhoto) => (
            <li key={photo.id}>
              <label>
                <input
                  type="checkbox"
                  checked={!removed.includes(photo.id)}
                  onChange={(event) => {
                    setRemoved((current) =>
                      event.target.checked ? current.filter((id) => id !== photo.id) : [...current, photo.id],
                    );
                  }}
                />
                Keep photo {photo.sortOrder + 1}
              </label>
            </li>
          ))}
        </ul>
        <label>
          Add photos
          <input name="photos" type="file" accept="image/jpeg,image/png,image/webp" multiple />
        </label>
        <button type="submit">Save photos</button>
      </form>
      <div className="actions">
        <button type="button" onClick={() => changeStatus("Sold")}>
          Mark sold
        </button>
        <button type="button" onClick={() => changeStatus("Available")}>
          Mark available
        </button>
      </div>
      <p className={`status status-${listing.status.toLowerCase()}`}>{listing.status}</p>
    </>
  );
}

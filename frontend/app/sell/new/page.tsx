"use client";

import { Alert, Button, Input } from "antd";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { problemText, readProblem } from "@/lib/problem";
import type { Listing } from "@/lib/types";

type PhotoPreview = { url: string; name: string };

export default function PublishPage() {
  const router = useRouter();
  const [error, setError] = useState("");
  const [previews, setPreviews] = useState<PhotoPreview[]>([]);
  const previewsRef = useRef<PhotoPreview[]>([]);

  function onPhotos(event: React.ChangeEvent<HTMLInputElement>) {
    const input = event.currentTarget;
    input.dataset.reviewed = "react";
    const files = [...(input.files ?? [])].filter((file) => file.size > 0).slice(0, 10);
    for (const preview of previewsRef.current) {
      URL.revokeObjectURL(preview.url);
    }
    const next = files.map((file) => ({ url: URL.createObjectURL(file), name: file.name }));
    previewsRef.current = next;
    setPreviews(next);
  }

  useEffect(() => {
    return () => {
      for (const preview of previewsRef.current) {
        URL.revokeObjectURL(preview.url);
      }
    };
  }, []);

  useEffect(() => {
    void fetch("/api/v1/sellers/me").then((response) => {
      if (response.status === 401) {
        router.push("/sign-in?next=/sell/new");
      }
    });
  }, [router]);

  async function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    const form = event.currentTarget;
    const data = new FormData(form);
    const photos = data.getAll("photos").filter((value) => value instanceof File && value.size > 0);
    data.delete("photos");
    for (const photo of photos) {
      data.append("photos", photo);
    }

    const response = await fetch("/api/v1/listings", { method: "POST", body: data });
    if (response.status === 401) {
      router.push("/sign-in?next=/sell/new");
      return;
    }

    if (!response.ok) {
      setError(problemText(await readProblem(response)));
      return;
    }

    const listing = (await response.json()) as Listing;
    router.push(`/listings/${listing.id}`);
    router.refresh();
  }

  return (
    <form className="form" onSubmit={onSubmit}>
      {error ? <Alert type="error" message={error} /> : null}
      <h1>Publish a car</h1>
      <label>
        Make
        <Input name="make" required maxLength={50} />
      </label>
      <label>
        Model
        <Input name="model" required maxLength={50} />
      </label>
      <label>
        Year
        <Input name="year" inputMode="numeric" required />
      </label>
      <label>
        Price (USD)
        <Input name="price" inputMode="decimal" required />
      </label>
      <label>
        Mileage (km)
        <Input name="mileage" inputMode="numeric" required />
      </label>
      <label>
        Location
        <Input name="location" required minLength={2} maxLength={80} />
      </label>
      <label>
        Description
        <Input.TextArea name="description" required maxLength={2000} rows={6} />
      </label>
      <label>
        Photos (1 to 10, first is the cover)
        <input name="photos" type="file" accept="image/jpeg,image/png,image/webp" multiple required onChange={onPhotos} />
      </label>
      {previews.length > 0 ? (
        <div className="photo-review" aria-label="Selected photos">
          {previews.map((preview, index) => (
            <figure key={preview.url}>
              <img src={preview.url} alt={preview.name} />
              <figcaption>{index === 0 ? "Cover" : `Photo ${index + 1}`}</figcaption>
            </figure>
          ))}
        </div>
      ) : null}
      <Button type="primary" htmlType="submit">
        Publish
      </Button>
    </form>
  );
}

import Link from "next/link";
import { redirect } from "next/navigation";
import { apiFetch } from "@/lib/api";
import type { Inquiry } from "@/lib/types";

export default async function InquiriesPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const response = await apiFetch(`/api/v1/listings/${id}/inquiries`);
  if (response.status === 401) {
    redirect(`/sign-in?next=/sell/listings/${id}/inquiries`);
  }

  if (response.status === 403) {
    const problem = (await response.json()) as { detail?: string };
    return <p className="error">{problem.detail ?? "Only the owner can change this listing."}</p>;
  }

  if (!response.ok) {
    return <p className="error">The inquiries could not be loaded.</p>;
  }

  const body = (await response.json()) as { items: Inquiry[] };

  return (
    <>
      <p>
        <Link href={`/sell/listings/${id}/edit`}>Back to the listing</Link>
      </p>
      <h1>Inquiries</h1>
      {body.items.length === 0 ? <p>No inquiries yet.</p> : null}
      <div className="results">
        {body.items.map((inquiry) => (
          <article className="card" key={inquiry.id}>
            <div>
              <h2>{inquiry.name}</h2>
              <p>{inquiry.email}</p>
              {inquiry.phone ? <p>{inquiry.phone}</p> : null}
              <p>{inquiry.message}</p>
              <p>{new Date(inquiry.submittedAt).toLocaleString("en-US")}</p>
            </div>
          </article>
        ))}
      </div>
    </>
  );
}

# HTTP Contract: Public Car Marketplace

The Next.js app calls this API. Browser calls use same-origin paths under `/api/v1` (rewritten to the API). JSON property names are camelCase. Enums are the strings `Available` and `Sold`. Timestamps are ISO-8601 `DateTimeOffset` values in UTC. Null properties are omitted. Prices are JSON numbers with at most two decimal places, never strings. `currencyCode` is always `USD`. `mileageUnit` is always `km`.

Authentication is the `marketplace_session` cookie. Send it automatically from the browser. Anonymous calls omit it.

## Shared errors

Failures use RFC 7807 problem details (`application/problem+json`). Validation uses `400` and an `errors` object. The `detail` text and the field messages are English and safe to show in the page.

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The listing was not saved.",
  "instance": "/api/v1/listings",
  "errors": {
    "price": [
      "Price must be greater than 0 and at most 99,999,999.99, with at most two decimal places."
    ]
  }
}
```

| Status | When | Title / detail the page can show |
| --- | --- | --- |
| 400 | Field rules failed | Field messages below. Nothing is saved. |
| 401 | A signed-in seller is required | "Sign in to continue." |
| 403 | Signed in, but not allowed | "Only the owner can change this listing." or "You cannot send an inquiry about your own listing." |
| 404 | Id does not exist | "That listing was not found." |
| 409 | State conflict | "That email is already in use." or "This car is sold and is not accepting inquiries." |

Mutating requests that send an `Origin` header must match the configured site origin. A mismatch returns `403` with detail "This action is not allowed from that site." Requests with no `Origin` are accepted so server-side fetches and `CarMarketplace.http` work.

Field messages, used verbatim:

| Field | Message |
| --- | --- |
| displayName | Display name must be 1 to 80 characters. |
| email (format) | Enter a valid email address. |
| email (duplicate) | That email is already in use. |
| password | Password must be at least 8 and at most 128 characters. |
| sign-in | The email or password is incorrect. |
| make | Make must be 1 to 50 characters. |
| model | Model must be 1 to 50 characters. |
| year | Year must be a whole number from 1900 through {maxYear}. |
| price | Price must be greater than 0 and at most 99,999,999.99, with at most two decimal places. |
| mileage | Mileage must be a whole number of kilometers from 0 through 2,000,000. |
| location | Location must be 2 to 80 characters. |
| description | Description must be 1 to 2000 characters. |
| photos (count) | Add 1 to 10 still photos. The first photo is the cover. |
| photos (type) | Each photo must be a still image (JPEG, PNG, or WebP). Video is not accepted. |
| photos (size) | Each photo must be 8 MB or smaller. |
| name | Name must be 1 to 80 characters. |
| message | Message must be 10 to 1000 characters. |
| phone | Phone must contain 7 to 20 digits and may include spaces, hyphens, parentheses, and a leading plus sign. |

`{maxYear}` is the UTC calendar year after the current year. On 2026-11-15 that sentence ends in 2027.

## Listing representations

`ListingSummary` (browse row):

| Field | Type | Notes |
| --- | --- | --- |
| id | string (guid) | |
| make, model | string | |
| year | number | |
| price | number | |
| currencyCode | string | `USD` |
| mileage | number | kilometers |
| mileageUnit | string | `km` |
| location | string | |
| status | string | `Available` or `Sold` |
| coverPhotoUrl | string | `/api/v1/photos/{photoId}` |
| publishedAt | string (date-time) | Original publication time |

`Listing` (detail and write responses) adds `description`, `sellerDisplayName`, `updatedAt`, and `photos`: `{ id, url, sortOrder }[]` ordered by `sortOrder`. It does not include seller email, seller phone, street address, latitude, or longitude.

`ListingList`:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

`pageSize` is always 20. An empty catalog and a filter with no matches are both `200` with `totalCount` 0. They are not errors.

## Listings

### List listings

`GET /api/v1/listings`

Authentication: none.

Query (all optional):

| Name | Rule |
| --- | --- |
| make, model, location | Case-insensitive contains. Empty or whitespace is ignored. |
| minYear, maxYear | Inclusive whole years. |
| minPrice, maxPrice | Inclusive decimals. |
| maxMileage | Inclusive kilometers. |
| status | `available` (default), `sold`, or `all`. |
| page | Whole number, default 1. Values below 1 are `400`. |

Selected filters apply together. Sort is `publishedAt` descending, then `id` descending. A page past the end returns `200`, `items: []`, and the real `totalCount`.

`200` `ListingList`.

### Get one listing

`GET /api/v1/listings/{id}`

Authentication: none. Available and sold listings both return `200` `Listing`. Unknown id returns `404`.

### Publish a listing

`POST /api/v1/listings`

Authentication: seller cookie. `Content-Type: multipart/form-data`.

Fields: `make`, `model`, `year`, `price`, `mileage`, `location`, `description`. Files: repeated part name `photos`, 1 to 10, in cover-first order. The client cannot set status. The stored status is `Available`.

`201 Created`, `Location: /api/v1/listings/{id}`, body `Listing`. Invalid fields or photos return `400` and create no listing and no leftover public files. No cookie returns `401`.

### Replace listing facts

`PUT /api/v1/listings/{id}`

Authentication: owner cookie. JSON body: `make`, `model`, `year`, `price`, `mileage`, `location`, `description`. Does not change photos, status, or `publishedAt`.

`200` `Listing`. Not the owner: `403` and the row is unchanged. Unknown id: `404`. Invalid body: `400` and the row is unchanged.

### Replace photos

`PUT /api/v1/listings/{id}/photos`

Authentication: owner cookie. `multipart/form-data` with 1 to 10 `photos` parts, cover first. Replaces the whole set in one transaction. Zero files or more than 10 is `400` and the previous photos stay. `200` `Listing`. The first stored photo is the cover (`sortOrder` 0).

### Change status

`PATCH /api/v1/listings/{id}/status`

Authentication: owner cookie. JSON: `{ "status": "Sold" }` or `{ "status": "Available" }`.

`200` `Listing`. `publishedAt` is unchanged. Setting the status the listing already has returns `200` and the same listing. Not the owner: `403` and the row is unchanged. There is no DELETE.

### List the signed-in seller's listings

`GET /api/v1/sellers/me/listings?page=1`

Authentication: seller cookie. Same page shape as the public list, but every status is included and only listings owned by the caller are returned. Sort is `publishedAt` descending. `401` when anonymous.

## Photos

### Get photo bytes

`GET /api/v1/photos/{id}`

Authentication: none. `200` with the image `Content-Type` and the bytes. `404` when the id is unknown. This is how browse and detail load the cover and the gallery, including for a sold car opened directly.

## Seller accounts and sessions

### Register

`POST /api/v1/sellers`

Authentication: none. JSON:

```json
{
  "displayName": "North Yard Motors",
  "email": "seller@example.com",
  "password": "test-pass-8"
}
```

`201 Created`, `Location: /api/v1/sellers/me`, `Set-Cookie: marketplace_session`, body:

```json
{
  "id": "guid",
  "displayName": "North Yard Motors",
  "email": "seller@example.com"
}
```

The caller is signed in. Duplicate email, compared case-insensitively, returns `409` and does not create an account. The body does not include a password or a hash.

### Current seller

`GET /api/v1/sellers/me`

Authentication: seller cookie. `200` with `id`, `displayName`, and `email`. `401` when there is no valid session. This email is the caller's own. It is not part of `Listing`.

### Sign in

`POST /api/v1/sessions`

Authentication: none. JSON `{ "email", "password" }`.

Success: `200` with the same seller body as `GET /api/v1/sellers/me`, plus `Set-Cookie`. The cookie's sliding lifetime is 14 days.

Failure: `401` and detail "The email or password is incorrect." The detail does not say whether the email exists.

### Sign out

`DELETE /api/v1/sessions/current`

Clears `marketplace_session`. `204` whether or not a session was present.

## Inquiries

### Send an inquiry

`POST /api/v1/listings/{id}/inquiries`

Authentication: none required. A signed-in owner of `{id}` is rejected. JSON:

```json
{
  "name": "Amina Cole",
  "email": "amina@example.com",
  "phone": "+1 (415) 555-0134",
  "message": "Is the mileage in kilometers, and is the car still available?"
}
```

`phone` may be omitted. Success: `201 Created`, `Location: /api/v1/listings/{id}`, body:

```json
{
  "id": "guid",
  "listingId": "guid",
  "make": "Honda",
  "model": "Civic",
  "year": 2018,
  "submittedAt": "2026-11-15T12:00:00+00:00"
}
```

The confirmation names the car with `make`, `model`, and `year`. The response does not echo the message back as a thread, and there is no reply URL.

Refusals, and no row is inserted:

| Condition | Status | Detail |
| --- | --- | --- |
| Listing missing | 404 | That listing was not found. |
| Listing is Sold | 409 | This car is sold and is not accepting inquiries. |
| Caller owns the listing | 403 | You cannot send an inquiry about your own listing. |
| Invalid fields | 400 | Field messages in the table above. |

### Read inquiries

`GET /api/v1/listings/{id}/inquiries`

Authentication: owner cookie. `200` with:

```json
{
  "items": [
    {
      "id": "guid",
      "name": "Amina Cole",
      "email": "amina@example.com",
      "phone": "+1 (415) 555-0134",
      "message": "Is the mileage in kilometers, and is the car still available?",
      "submittedAt": "2026-11-15T12:00:00+00:00"
    }
  ]
}
```

Newest `submittedAt` first. `phone` is omitted when it was not provided. Anonymous: `401`. A different seller: `403` with "Only the owner can change this listing." and an empty change to stored data. There is no POST for a reply and no chat resource.

## Endpoints that must not exist

`DELETE /api/v1/listings/{id}`, any payments, financing, shipping, auctions, message replies, password-reset, email-verification, or buyer-registration route.

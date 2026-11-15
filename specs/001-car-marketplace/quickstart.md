# Quickstart: Public Car Marketplace

Validation guide for the four accepted stories. Implementation details live in [tasks.md](tasks.md). Request and response shapes live in [contracts/marketplace-api.md](contracts/marketplace-api.md). Column rules live in [data-model.md](data-model.md).

## Prerequisites

- .NET 10 SDK
- Node.js 22 LTS
- MySQL 8.0 or 8.4, reachable from the API
- Databases `car_marketplace` and `car_marketplace_test`, charset `utf8mb4`, collation `utf8mb4_0900_ai_ci`

Create them with the MySQL client before the first migration:

```sql
CREATE DATABASE IF NOT EXISTS car_marketplace CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
CREATE DATABASE IF NOT EXISTS car_marketplace_test CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
```

The API connection string is configuration (`ConnectionStrings:Marketplace`), not a secret committed in source. Development listens on `http://localhost:5080`. The site listens on `http://localhost:3000` and rewrites `/api/*` to the API. Photo files use `car-marketplace/backend/storage/photos`.

## Setup

From `car-marketplace/backend`:

```text
dotnet tool restore
dotnet ef database update --project src/CarMarketplace.Api --startup-project src/CarMarketplace.Api
dotnet test tests/CarMarketplace.Api.Tests
dotnet run --project src/CarMarketplace.Api
```

From `car-marketplace/frontend`:

```text
npm install
npm run dev
```

Open `http://localhost:3000`. OpenAPI in Development is `/openapi/v1.json` on the API origin. `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.http` calls the same routes.

Do not seed listings from `Program.cs`. The scenarios below create their own data.

## Scenario 1 — Browse (User Story 1)

Prerequisite: one Available listing and one Sold listing, which Scenarios 2 and 3 create.

1. Open `http://localhost:3000` while signed out.
2. Expected: only the available car, newest published first, at most 20 rows, and a count of matches. The sold car is absent. Copy is English. Status is the word Available, not color alone.
3. Set make, model, year range, price range, maximum mileage, location, and status together.
4. Expected: every selected filter applies. A filter with no match says that no cars match and offers a way to clear the filters. An empty catalog, with no filters and no rows, says there are no cars for sale yet and is not an error page.
5. Open a car, then go back.
6. Expected: the detail page shows make, model, year, price with USD, mileage with km, location, at least one photo, description, the seller display name, and Available or Sold. It does not show the seller email, a phone, a street, or a map. The browse filters are still applied.
7. Open the sold car's URL directly.
8. Expected: the facts and the word Sold. A fresh available browse still omits it.

API check: `GET /api/v1/listings` returns `pageSize` 20 and `totalCount`. `GET /api/v1/listings/{id}` returns the detail contract.

## Scenario 2 — Publish (User Story 2)

1. Register with a display name, an unused email, and a password of at least 8 characters.
2. Expected: the account exists and the seller is signed in (`201` and `marketplace_session`).
3. Register again with the same email using different letter case.
4. Expected: no second account, and the text "That email is already in use."
5. Sign out. Sign in with a wrong password.
6. Expected: still signed out, and the text "The email or password is incorrect." The text does not say which field failed.
7. Sign in with the right email and password. Publish make, model, year, a price greater than zero, mileage, city and region, a description, and 1 to 10 still photos.
8. Expected: the car is public as Available, owned by that seller, and visible in the available browse.
9. Repeat publish with a missing fact, a price that is not greater than zero, a year outside 1900 through next calendar year, a mileage outside 0 through 2,000,000, no photo, an 11th photo, a non-image, or a file larger than 8 MB.
10. Expected: nothing new appears in browse, and the page explains the invalid fact in text.
11. Sign out.
12. Expected: browse still works, and publish or edit asks the seller to sign in.

## Scenario 3 — Correct and mark sold (User Story 3)

1. As the owner, change the price and save.
2. Expected: the detail page shows the new price. The car does not jump ahead of a newer listing in browse (`publishedAt` unchanged).
3. Save photos with 1 to 10 images.
4. Expected: the first image is the cover in the list.
5. Save with zero photos.
6. Expected: the change is rejected and the previous photos remain.
7. Mark the car sold.
8. Expected: the detail page says Sold. A fresh available browse omits it.
9. Mark it available.
10. Expected: the detail page says Available and the car returns to the available browse.
11. As a visitor, or as a different seller, try to edit or change status.
12. Expected: the listing is unchanged, and the page says only the owner can change it.

## Scenario 4 — Inquire (User Story 4)

1. Signed out, on someone else's Available car, submit a name, an email, a message of 10 to 1000 characters, and an optional phone.
2. Expected: a confirmation that names the car. The owner can later read the name, email, message, phone if it was sent, and the submission time, newest first.
3. Submit an inquiry on a Sold car, and submit one while signed in as the owner.
4. Expected: no inquiry is stored, and the page says why.
5. Submit a message shorter than 10 characters or longer than 1000, or omit the name or email.
6. Expected: no inquiry is stored, and the invalid fact is explained in text.
7. Look for a reply box or a chat pane as the buyer and as the seller.
8. Expected: neither is present. The owner only reads the inquiries.

## Done when

- `dotnet test` passes the four story tests.
- `dotnet build` reports zero warnings.
- Scenarios 1 through 4 match the expected outcomes in the browser.
- The network calls match [contracts/marketplace-api.md](contracts/marketplace-api.md).
- No payment, finance, shipping, auction, chat, reply, password-reset, or listing-delete control is present.

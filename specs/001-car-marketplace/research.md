# Research: Public Car Marketplace

All Technical Context items are resolved. Assumptions A-01 through A-15 are accepted defaults and were not reopened.

## 1. Runtime and web framework

- **Decision**: API targets `net10.0` and uses ASP.NET Core minimal APIs, `AddValidation()`, `AddOpenApi()`, and `MapOpenApi()` in Development. No `Swashbuckle.*` package.
- **Rationale**: The API is new, so the Web API skill defaults to minimal APIs. .NET 10 is the current supported runtime on 2026-11-15 and is the version that includes minimal-API validation and built-in OpenAPI. .NET 9's standard-term support has ended.
- **Alternatives considered**: MVC controllers were rejected because the project has no existing controller style. Swashbuckle was rejected for .NET 10. .NET 9 was rejected because it is out of standard support.

## 2. MySQL provider

- **Decision**: `Microting.EntityFrameworkCore.MySql` 10.0.14 with EF Core 10 packages in the `10.0.14`–`10.0.999` range. Namespaces are `Microting.EntityFrameworkCore.MySql`. `Microsoft.EntityFrameworkCore.Design` is a private asset. Migrations are created with `dotnet ef`. The database is MySQL 8.0 or 8.4, `utf8mb4` / `utf8mb4_0900_ai_ci`.
- **Rationale**: Official `Pomelo.EntityFrameworkCore.MySql` 9.0.0 is still the newest Pomelo release and depends on EF Core 9 (`[9.0.0, 9.0.999]`). Pomelo's EF Core 10 upgrade (issue 2007, PR 2019) is not merged or published. Combining Pomelo 9 with EF Core 10 produces restore errors (`NU1107` / `NU1608`) and can fail inside `UseMySql`. The Microting 10.0.14 package is the published EF Core 10 provider and tracks MySQL through MySqlConnector. Do not suppress those NuGet warnings to force Pomelo 9 onto EF Core 10.
- **Alternatives considered**: Pomelo 9 plus EF Core 9 on the .NET 10 runtime was rejected because the shared ASP.NET Core 10 framework aligns with EF Core 10. Waiting for official Pomelo 10 was rejected because it is unpublished. Dapper-only access was rejected because this slice needs EF Core migrations.

## 3. Identity and session

- **Decision**: Cookie authentication, cookie name `marketplace_session`, `HttpOnly`, `SameSite=Lax`, `Path=/`, sliding expiration of 14 days, `Secure` in Production only. The password is hashed with `PasswordHasher<SellerAccount>` from `Microsoft.Extensions.Identity.Core`. The API does not use `AddIdentity`, Identity roles, passkeys, or an Identity EF model. Email is stored trimmed and lowercased. A failed sign-in always returns the same sentence: "The email or password is incorrect."
- **Rationale**: FR-009 is a sliding 14-day session after the last sign-in. A cookie with sliding expiration matches that without a refresh-token resource. The constitution allows only one account role, and .NET 10 Identity's passkey model is a poor fit for MySQL. The spec's password rule is a minimum of 8 characters, not a complexity policy. An implementation guard rejects passwords longer than 128 characters so hashing input stays bounded.
- **Alternatives considered**: A bearer JWT in `localStorage` was rejected because the security rules require an HttpOnly cookie and because a fixed `exp` does not slide for 14 days of use. Full ASP.NET Identity was rejected because it adds roles, email-confirmation hooks, and passkey storage this release forbids. Extra password-complexity rules were rejected because they would change A-09 and FR-007.

## 4. How the Next.js app calls the API

- **Decision**: Next.js 16.4.6 App Router. `next.config.ts` rewrites `/api/:path*` to `API_ORIGIN` (development: `http://localhost:5080`). Browser code calls same-origin `/api/v1/...`. Server Components call `API_ORIGIN` and forward the request `Cookie` header. Mutating API requests that include an `Origin` header are rejected unless `Origin` is the configured site origin (`http://localhost:3000` in development). Requests with no `Origin` (server-side fetch and the `.http` file) are allowed. Pin `next@16.4.6` or a later 16.4 patch; 16.4.5 fixed response-forwarding and HttpOnly cookie disclosure, and 16.4.6 is the current stable on 2026-11-15.
- **Rationale**: Same-site cookies plus the rewrite keep the session cookie on the site the visitor is using. The Origin check is the CSRF control for browser mutations. The patched Next.js line matters because this app forwards API responses and cookies.
- **Alternatives considered**: CORS with `Access-Control-Allow-Origin: *` was rejected because credentialed cookies cannot use a wildcard origin. A separate antiforgery token was rejected as a second mechanism once SameSite, the rewrite, and the Origin check are in place. The React instruction set under `instructions/react/` was not used; those notes are not the Next.js App Router guide.

## 5. API style and errors

- **Decision**: Resource URLs under `/api/v1`. JSON uses camelCase, strict numbers, no duplicate properties, and nulls omitted. Enums are strings. Errors are RFC 7807 problem details. Validation errors include an `errors` object whose messages are the English sentences in the contract. Domain failures map as: unknown resource `404`, anonymous mutation `401`, not the owner `403`, duplicate email or illegal inquiry state `409`.
- **Rationale**: The Web API skill requires problem details, string enums, `DateTimeOffset` on DTOs, and sealed-record DTOs. A custom `{ "error": { "code" } }` envelope would fight the framework's validation response.
- **Alternatives considered**: The generic API-rule error envelope was rejected for this stack. Integer enums were rejected because the spec requires the words Available and Sold.

## 6. Keys, time, and money columns

- **Decision**: Primary keys are GUID strings (`char(36)`). API date-times are `DateTimeOffset` in UTC. MySQL columns are `datetime(6)` storing UTC, because MySQL has no offset type; the service maps them to `DateTimeOffset` with offset zero. Price is `decimal(10,2)`. Currency code `USD` and mileage unit `km` are constants on the DTO, not columns.
- **Rationale**: Public IDs should not be sequential. `decimal(10,2)` holds the spec maximum 99,999,999.99. A-03 forbids a per-listing currency.
- **Alternatives considered**: Auto-increment integers were rejected for public URLs. Storing `DateTimeOffset` as an EF column against MySQL was rejected because the column cannot keep an offset. A currency column was rejected by A-03.

## 7. Photos

- **Decision**: Files live under a configured directory outside the site and the API's `wwwroot` (development default `car-marketplace/backend/storage/photos`). The stored name is the photo id plus a generated extension. JPEG, PNG, and WebP only, after ImageSharp decodes the bytes and re-encodes them so EXIF is removed. Reject anything that does not decode as those still images, including video. Maximum 8 MB per file before storage. Create sends facts and 1 to 10 files in one request and commits the row and the files together, or neither. Replacing photos is a separate request that swaps the whole ordered set, or leaves the previous set unchanged when validation fails. Sort order 0 is the cover.
- **Rationale**: FR-011 forbids a public listing with no photo or a partial public listing. Bytes do not belong in MySQL rows at 8 MB times 10. Re-encoding satisfies the still-image rule and the metadata-stripping guidance.
- **Alternatives considered**: A `LONGBLOB` column was rejected for size and backups. Object storage was rejected as extra infrastructure for the first local slice. Accepting GIF or SVG was rejected because GIF may be animated and SVG is not a photo. A public static file path using the original filename was rejected.

## 8. Browse query

- **Decision**: Offset pages of exactly 20, `page` starting at 1. The client cannot change the page size. Default status filter is available only. `status=sold` and `status=all` are explicit. Sort is `publishedAt` descending, then `id` descending. Edits do not change `publishedAt`. Text filters are case-insensitive contains, with `LIKE` wildcards escaped. Empty filter values are ignored. Numeric bounds are inclusive. The supporting index is `(status, published_at, id)`.
- **Rationale**: This is A-11 and FR-002. The collation `utf8mb4_0900_ai_ci` is case-insensitive. Offset paging matches "next page" in the spec at the 500-listing target. Keyset paging would not let the visitor jump to a page number.
- **Alternatives considered**: Keyword search was rejected by FR-018. A client-supplied page size was rejected by the fixed page size of 20.

## 9. Schema delivery

- **Decision**: One initial migration creates seller accounts, listings, photos, and inquiries before story behavior is added. Story phases add endpoints and screens, not a second schema for the same entities.
- **Rationale**: Every story reads or writes the listing, and the inquiry row references it. One migration keeps the vertical slices from rebuilding the same tables.
- **Alternatives considered**: A migration per story was rejected because the listing table would be edited by later stories that only add behavior.

## 10. Tests in this plan

- **Decision**: Each user story has one MSTest class hosted with `WebApplicationFactory` and the MySQL test database `car_marketplace_test`. The test applies migrations and inserts only its own rows. It covers that story's independent test. Wider case design stays with the test agent.
- **Rationale**: The delivery workflow asks the developer for a happy-path test per slice. The specification does not ask for a full automated matrix.
- **Alternatives considered**: EF InMemory was rejected because it does not apply MySQL collation, decimals, or check constraints. SQLite was rejected because the constitution fixes the database as MySQL.

## 11. Out of scope reminders

Payments, financing, shipping, auctions, in-app chat, in-product replies, password reset, email verification, listing deletion, staff moderation, keyword search, video, maps, street addresses, and buyer accounts are not designed and must not appear as endpoints or screens.

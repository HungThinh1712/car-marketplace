# Implementation Plan: Public Car Marketplace

**Branch**: `001-car-marketplace` | **Date**: 2026-11-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-car-marketplace/spec.md`

## Summary

A visitor can browse and filter cars, and open a listing, without an account. A seller can register, sign in, publish a car, correct it, and mark it sold or available. A buyer can send one inquiry, and the owner can read it. The first release stops there.

The public site is a Next.js 16 App Router application. It calls a .NET 10 minimal API. The API stores seller accounts, listings, photos, and inquiries in MySQL through EF Core 10. Assumptions A-01 through A-15 are closed planning defaults and are not reopened. A-16 is applied as well because it matches the constitution's personal-information limit.

## Technical Context

**Language/Version**: .NET 10 (`net10.0`) for the API. TypeScript on Next.js 16.4.6 (App Router) for the site. Node.js 22 LTS for the frontend toolchain.

**Primary Dependencies**: ASP.NET Core minimal APIs (built-in OpenAPI and `AddValidation()`). EF Core 10 with `Microting.EntityFrameworkCore.MySql` 10.0.14. `Microsoft.EntityFrameworkCore.Design` in the same 10.0 band, `PrivateAssets="all"`. `Microsoft.Extensions.Identity.Core` only for `PasswordHasher<T>` (no Identity database, no roles, no passkeys). SixLabors.ImageSharp to accept still images and strip metadata. No Swashbuckle package.

**Storage**: MySQL 8.0 or 8.4, database `car_marketplace`, charset `utf8mb4`, collation `utf8mb4_0900_ai_ci`. Photo bytes on local disk outside the web root. EF Core migrations only. Do not call `EnsureCreated` and do not seed in `Program.cs`.

**Testing**: MSTest with `WebApplicationFactory` against MySQL for one happy-path test per user story. Playwright and the broader test suite belong to the test agent after implementation. Tests are included because this delivery workflow requires a happy-path test with each story, not because the specification asked for test-first coverage of every edge.

**Target Platform**: Local Windows or Linux development. The API listens on `http://localhost:5080`. The site listens on `http://localhost:3000`. Production hosting is out of this plan.

**Project Type**: Web application. Next.js site plus .NET HTTP API plus MySQL.

**Performance Goals**: With 500 published listings, the first browse page and a listing detail response are targets of under 3 seconds (SC-006, assumption A-15). No separate load-test harness is in this plan.

**Constraints**: English copy only (A-02, FR-020). Prices are USD with no per-listing currency column (A-03, FR-019). Mileage is kilometers (A-04). Location is city and region text (A-05). A valid listing is public immediately (A-06). Sold can return to available (A-07). No listing delete, no email notification, no reply thread (A-08, FR-017, FR-018). Session lasts 14 days after the last sign-in (A-09, FR-009). Photos are 1 to 10 still images, 8 MB each, first is the cover (A-10). Browse is 20 per page, available only, newest published first (A-11). Field bounds in the spec edge cases are the validation rules (A-12).

**Scale/Scope**: Four user stories, one public catalog, seller accounts only, one role. Screens: browse, listing detail, register, sign in, publish, my listings, edit listing, inquiries for one listing.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Result |
| --- | --- | --- |
| I. Honest Listings | Detail shows make, model, year, price, mileage, location, photos, description, and the word Available or Sold. Default browse hides sold cars. The owner can correct a listing and mark it sold. | Pass |
| II. Public Discovery, Accountable Selling | Browse and detail require no account. Publish, edit, and status changes require the signed-in owner. | Pass |
| III. Inquiry Without a Transaction | The API accepts and lists inquiries only. No payment, financing, shipping, auction, chat, or reply endpoint. | Pass |
| IV. Vertical Slice Value | Each story phase ships the screen, the API behavior, and the stored facts for that outcome. | Pass |
| V. Specify What and Why | This plan does not add product behavior beyond the accepted spec. A-01 through A-15 stay closed. | Pass |
| Product constraints | Next.js, .NET, and MySQL are the stack. Personal data is account email, display name, and password, plus inquiry name, email, optional phone, and message. Public listing JSON omits seller email and phone. | Pass |
| Exclusions | No password reset, email verification, deletion, moderation, keyword search, video, maps, street address, or buyer account. | Pass |

No gate failed. Complexity Tracking is empty.

## Post-design Constitution Check

Reviewed again after `research.md`, `data-model.md`, `contracts/marketplace-api.md`, and `quickstart.md`.

The public list and detail contracts expose status as the words `Available` and `Sold`, sort by original `publishedAt`, and omit seller email, phone, and street address. Cookie authentication is required for every mutation except inquiry creation. Inquiry creation is refused for a sold car and for the owner. There is no reply resource. The Next.js routes cover browse, detail, account, publish, edit, and reading inquiries. Schema work stays in this plan (the application database belongs to the developer).

Post-design result: pass. No constitution change is required.

## Project Structure

### Documentation (this feature)

```text
specs/001-car-marketplace/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── marketplace-api.md
└── tasks.md
```

### Source Code (repository root)

```text
car-marketplace/
├── backend/
│   ├── CarMarketplace.sln
│   ├── src/CarMarketplace.Api/
│   │   ├── Program.cs
│   │   ├── CarMarketplace.Api.csproj
│   │   ├── CarMarketplace.http
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Data/MarketplaceDbContext.cs
│   │   ├── Data/MarketplaceDbContextFactory.cs
│   │   ├── Entities/SellerAccount.cs
│   │   ├── Entities/Listing.cs
│   │   ├── Entities/ListingPhoto.cs
│   │   ├── Entities/Inquiry.cs
│   │   ├── Contracts/
│   │   ├── Endpoints/SellerEndpoints.cs
│   │   ├── Endpoints/SessionEndpoints.cs
│   │   ├── Endpoints/ListingEndpoints.cs
│   │   ├── Endpoints/PhotoEndpoints.cs
│   │   ├── Endpoints/InquiryEndpoints.cs
│   │   ├── Services/
│   │   ├── Storage/LocalPhotoStorage.cs
│   │   ├── Middleware/ApiExceptionHandler.cs
│   │   └── Migrations/
│   └── tests/CarMarketplace.Api.Tests/
└── frontend/
    ├── package.json
    ├── next.config.ts
    ├── app/layout.tsx
    ├── app/page.tsx
    ├── app/listings/[id]/page.tsx
    ├── app/register/page.tsx
    ├── app/sign-in/page.tsx
    ├── app/sell/new/page.tsx
    ├── app/sell/listings/page.tsx
    ├── app/sell/listings/[id]/edit/page.tsx
    ├── app/sell/listings/[id]/inquiries/page.tsx
    ├── components/
    └── lib/api.ts
```

**Structure Decision**: The marketplace is a new `car-marketplace/` tree so the site and API are not mixed into `.cursor/`, `agents/`, or `instructions/`. `backend/` and `frontend/` follow the web-application layout. The Next.js app is the only UI. Do not add a Blazor host and do not apply `instructions/react/`.

## Next.js note

Use the App Router. Read pages (browse and detail) are Server Components that call the API. Forms (filters, register, sign-in, publish, edit, inquiry) are Client Components. Filter state lives in the browse URL search params so the detail page can link back to the same filters (FR-004). `next.config.ts` rewrites `/api/:path*` to the API origin so the browser stores the session cookie on the site origin.

Do not use the Pages Router. Do not add a buyer account UI, a chat pane, a map, or a payment control.

## API and data shape

The MySQL model and the HTTP contract the site calls are specified in [data-model.md](data-model.md) and [contracts/marketplace-api.md](contracts/marketplace-api.md). Decisions and rejected options are in [research.md](research.md).

Implementation rules taken from the .NET skills, without writing the code in this phase:

- New API, so use minimal APIs, one static `Map*` class per resource. Do not add MVC controllers.
- Request and response types are `sealed record` DTOs with XML `<summary>` comments. Do not return EF entities.
- Date-time values in DTOs are `DateTimeOffset`. Enums serialize as strings (`Available`, `Sold`).
- Handlers take `CancellationToken`, use `TypedResults` with an explicit `Results<...>` return type, and set OpenAPI name, summary, description, and `Produces`.
- POST create returns `201 Created` with a `Location` header. Validation failures are RFC 7807 problem details with field text. Unhandled errors use `Middleware/ApiExceptionHandler.cs` and do not copy `exception.Message` into the response.
- Each service has an interface registered with `AddScoped`. Endpoints do not take `MarketplaceDbContext`.
- Built-in `AddOpenApi()` and `MapOpenApi()` in Development. Do not add any `Swashbuckle.*` package.
- `builder.Services.AddValidation()` for data annotations on minimal APIs.
- Persist with a migration. Parent seller rows exist before listings. The `.http` file creates a seller and a listing before an inquiry.

`csharp-expert` does not add another marketplace plugin for this plan. Endpoint shape is owned by `dotnet-webapi`. EF Core persistence is owned by `create-datadriven-aspnetcore`, narrowed by the rules above where the scaffolder samples would return entity types or emit a DELETE endpoint the spec forbids.

## Complexity Tracking

No constitution violations to justify.

| Violation | Why Needed | Simpler Alternative Rejected Because |
| --- | --- | --- |
| — | — | — |

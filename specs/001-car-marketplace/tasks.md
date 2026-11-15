---
description: "Task list for the public car marketplace"
---

# Tasks: Public Car Marketplace

**Input**: Design documents from `/specs/001-car-marketplace/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/marketplace-api.md, quickstart.md

**Tests**: One happy-path MSTest class per user story. The specification does not ask for a full matrix. These four tests are here because the delivery workflow requires a happy-path test with each slice. Write each test first and confirm it fails before that story's implementation. Broader cases stay with the test agent.

**Organization**: Tasks are grouped by user story so each story can be implemented and checked on its own.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Paths are under `car-marketplace/` as in plan.md

## Path Conventions

- **API**: `car-marketplace/backend/src/CarMarketplace.Api/`
- **API tests**: `car-marketplace/backend/tests/CarMarketplace.Api.Tests/`
- **Site**: `car-marketplace/frontend/`
- Do not place the app in the repository root. Do not add a Blazor project. Do not follow `instructions/react/`.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the Next.js site and the .NET API projects. No feature behavior yet.

- [ ] T001 Create `car-marketplace/backend/src/CarMarketplace.Api/` and `car-marketplace/backend/tests/CarMarketplace.Api.Tests/` per the tree in `specs/001-car-marketplace/plan.md`
- [ ] T002 Create `car-marketplace/backend/CarMarketplace.sln` and `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.Api.csproj` (`net10.0` Microsoft.NET.Sdk.Web, `Nullable` enable, `ImplicitUsings` enable, `GenerateDocumentationFile` true, `TreatWarningsAsErrors` true) and a compiling `car-marketplace/backend/src/CarMarketplace.Api/Program.cs`
- [ ] T003 Create `car-marketplace/backend/tests/CarMarketplace.Api.Tests/CarMarketplace.Api.Tests.csproj` (MSTest, `Microsoft.NET.Test.Sdk`, `Microsoft.AspNetCore.Mvc.Testing`) referencing the API project
- [ ] T004 [P] Scaffold the App Router TypeScript app in `car-marketplace/frontend/` with `next@16.4.6` pinned in `car-marketplace/frontend/package.json`, `app/layout.tsx` setting `lang="en"`, and no Pages Router
- [ ] T005 [P] Add `car-marketplace/.gitignore` ignoring `bin/`, `obj/`, `node_modules/`, `.next/`, `.env`, `.env.local`, and `backend/storage/photos/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Database, error pipeline, cookie scheme, photo storage, and the site-to-API rewrite. No story is startable until this phase is done.

**Critical**: No user story work begins until this phase is complete.

- [ ] T006 Add `Microting.EntityFrameworkCore.MySql` 10.0.14, `Microsoft.EntityFrameworkCore.Design` in the provider's `10.0.14`–`10.0.999` range with `PrivateAssets="all"`, `Microsoft.Extensions.Identity.Core`, and `SixLabors.ImageSharp` to `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.Api.csproj`. Do not add `Pomelo.EntityFrameworkCore.MySql` or any `Swashbuckle.*` package. Do not suppress `NU1107` or `NU1608`.
- [ ] T007 [P] Create `SellerAccount` in `car-marketplace/backend/src/CarMarketplace.Api/Entities/SellerAccount.cs`. `email` is required, stored trimmed and lowercased, unique, and "A registration email comparison ignores letter case." `display_name` is required and "Display name and inquiry name are 1 to 80 characters." `password_hash` is required and is never an API field. "The submitted password is at least 8 characters and at most 128 characters."
- [ ] T008 [P] Create `Listing` in `car-marketplace/backend/src/CarMarketplace.Api/Entities/Listing.cs`. Required facts: "Make and model are 1 to 50 characters." "Year is a whole number from 1900 through the calendar year after the current year." "Price is a number greater than 0 and at most 99,999,999.99, with at most two decimal places." "Mileage is a whole number of kilometers from 0 through 2,000,000." "Location is 2 to 80 characters." "Description is 1 to 2000 characters after trimming blank space." "Status is the word Available or Sold." `published_at` is set once. "Edits and status changes must not update it."
- [ ] T009 [P] Create `ListingPhoto` in `car-marketplace/backend/src/CarMarketplace.Api/Entities/ListingPhoto.cs`. "The first photo is the cover photo." Sort order `0` through `9` is unique per listing. "A photo that is not a still image, or a still image larger than 8 MB, is rejected. Video is rejected." Content type after re-encode is `image/jpeg`, `image/png`, or `image/webp`.
- [ ] T010 [P] Create `Inquiry` in `car-marketplace/backend/src/CarMarketplace.Api/Entities/Inquiry.cs`. `buyer_name` follows "Display name and inquiry name are 1 to 80 characters." `message` follows "a message of 10 to 1000 characters." `buyer_phone` is optional and "Optional phone, when present, contains 7 to 20 digits and may include spaces, hyphens, parentheses, and a leading plus sign." There is no reply column.
- [ ] T011 Configure `car-marketplace/backend/src/CarMarketplace.Api/Data/MarketplaceDbContext.cs` with the relationships, unique indexes, check constraints, and `ix_listings_status_published_at_id` from `specs/001-car-marketplace/data-model.md`. Foreign keys use `ON DELETE RESTRICT`. Charset `utf8mb4`.
- [ ] T012 Add `car-marketplace/backend/src/CarMarketplace.Api/Data/MarketplaceDbContextFactory.cs` and the initial EF Core migration under `car-marketplace/backend/src/CarMarketplace.Api/Migrations/` for all four tables. Do not call `EnsureCreated` and do not seed in `Program.cs`.
- [ ] T013 [P] Add RFC 7807 handling in `car-marketplace/backend/src/CarMarketplace.Api/Middleware/ApiExceptionHandler.cs`. Map not-found to `404`, forbidden to `403`, and conflict to `409`. Do not copy `exception.Message` into a 500 response. Log the exception without passwords, hashes, cookies, or inquiry message text.
- [ ] T014 [P] Add `car-marketplace/backend/src/CarMarketplace.Api/Storage/IPhotoStorage.cs` and `car-marketplace/backend/src/CarMarketplace.Api/Storage/LocalPhotoStorage.cs`. Store files under a configured root outside `wwwroot`, default `car-marketplace/backend/storage/photos`, using a generated file name. Decode with ImageSharp and re-encode JPEG, PNG, or WebP so EXIF is removed.
- [ ] T015 [P] Add `car-marketplace/backend/src/CarMarketplace.Api/Services/IClock.cs` and `SystemClock.cs` returning UTC, so the year upper bound can be tested.
- [ ] T016 Wire `car-marketplace/backend/src/CarMarketplace.Api/Program.cs` with MySQL `AddDbContext`, `AddValidation()`, strict JSON options, `JsonStringEnumConverter`, `AddProblemDetails()`, `UseExceptionHandler()`, cookie authentication named `marketplace_session` (`HttpOnly`, `SameSite=Lax`, sliding 14 days, `Secure` only in Production), Development-only `AddOpenApi()` and `MapOpenApi()`, and an Origin allow-list on mutating requests. Do not register MVC controllers.
- [ ] T017 Add `car-marketplace/backend/src/CarMarketplace.Api/appsettings.json` and `appsettings.Development.json` with `ConnectionStrings:Marketplace`, `PhotoStorage:RootPath`, `WebOrigin`, and Kestrel on `http://localhost:5080`. Do not commit a database password.
- [ ] T018 [P] Rewrite `/api/:path*` to `API_ORIGIN` in `car-marketplace/frontend/next.config.ts` and add a cookie-forwarding fetch helper in `car-marketplace/frontend/lib/api.ts` for Server Components.
- [ ] T019 Create `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.http` with `@baseUrl = http://localhost:5080` and no feature requests yet.

**Checkpoint**: The solution builds, the migration applies to MySQL, and the site can reach the API through `/api`. User stories can start.

---

## Phase 3: User Story 1 - Browse cars for sale (Priority: P1)

**Goal**: A signed-out visitor sees available cars, filters them, and opens a car that shows every required fact and the word Available or Sold.

**Independent Test**: With at least one available listing and one sold listing present, a visitor who is not signed in can filter to the available car, open it, and see every required fact and the word Available.

### Tests for User Story 1

> Write this test first and confirm it fails before T021–T027.

- [ ] T020 [US1] Add `car-marketplace/backend/tests/CarMarketplace.Api.Tests/BrowseListingsTests.cs`. Insert one Available and one Sold listing through the test database. `GET /api/v1/listings` with no cookie returns only the available car, `pageSize` 20, and a `totalCount`. `GET /api/v1/listings/{id}` returns make, model, year, price, `currencyCode` `USD`, mileage, `mileageUnit` `km`, location, description, `sellerDisplayName`, status `Available`, and at least one photo, and the JSON does not contain the seller email.

### Implementation for User Story 1

- [ ] T021 [P] [US1] Add sealed-record `ListingSummary`, `ListingResponse`, `ListingPhotoResponse`, and `ListingListResponse` with XML `<summary>` comments in `car-marketplace/backend/src/CarMarketplace.Api/Contracts/ListingResponses.cs`. Date-times are `DateTimeOffset`. Do not expose seller email, phone, or street address.
- [ ] T022 [US1] Implement `IListingBrowseService` in `car-marketplace/backend/src/CarMarketplace.Api/Services/ListingBrowseService.cs`. "Text filters on make, model, and location: case-insensitive contains. Empty values are not predicates." "Year, price, and mileage bounds are inclusive when the query parameter is present." Default browse is status Available, `published_at desc, id desc`, limit 20. `status` query values are `available`, `sold`, and `all`. Use `AsNoTracking`. Map cover photo sort order `0` to `coverPhotoUrl`.
- [ ] T023 [US1] Map `GET /api/v1/listings` and `GET /api/v1/listings/{id}` in `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/ListingEndpoints.cs` and call it from `Program.cs`. Use `TypedResults` with an explicit `Results<...>` type, `CancellationToken`, `.WithName()`, `.WithSummary()`, `.WithDescription()`, and `.Produces()`. Unknown id returns `404` "That listing was not found." Page values below 1 return `400`. An empty list is `200` with `totalCount` 0.
- [ ] T024 [US1] Map `GET /api/v1/photos/{id}` in `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/PhotoEndpoints.cs`. Return the stored bytes and content type, or `404`.
- [ ] T025 [P] [US1] Build the browse page in `car-marketplace/frontend/app/page.tsx` and `car-marketplace/frontend/components/ListingFilters.tsx`. Read and write filters in the URL search params. Show the match count, 20 rows, the word Available or Sold, and the cover photo. When `totalCount` is 0 and no filter is set, show "There are no cars for sale yet." When filters match nothing, show "No cars match these filters." and a control that clears the query string.
- [ ] T026 [P] [US1] Build `car-marketplace/frontend/app/listings/[id]/page.tsx` showing make, model, year, price with USD, mileage with km, location, photos, description, seller display name, and the word Available or Sold. Link back to `/` with the same search params. Do not render seller email, phone, a street, or a map.
- [ ] T027 [US1] Append the list, detail, and photo requests from `specs/001-car-marketplace/contracts/marketplace-api.md` to `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.http`.

**Checkpoint**: Browse and detail work signed out, including the empty catalog and a sold car opened by URL.

---

## Phase 4: User Story 2 - Publish a car for sale (Priority: P1)

**Goal**: A person creates a seller account, signs in, and publishes one complete car that appears in the available browse list.

**Independent Test**: A new person can create an account, sign in, publish one complete listing, and see that listing in the public available list while signed out.

### Tests for User Story 2

> Write this test first and confirm it fails before T029–T039.

- [ ] T028 [US2] Add `car-marketplace/backend/tests/CarMarketplace.Api.Tests/PublishListingTests.cs`. Register, publish one valid multipart listing, and assert the public available list contains it. A second register with the same email in different letter case returns `409` and "That email is already in use." A wrong password returns `401` and "The email or password is incorrect."

### Implementation for User Story 2

- [ ] T029 [P] [US2] Add sealed-record request and response types with XML `<summary>` comments in `car-marketplace/backend/src/CarMarketplace.Api/Contracts/SellerContracts.cs` and `car-marketplace/backend/src/CarMarketplace.Api/Contracts/ListingWriteContracts.cs`.
- [ ] T030 [US2] Implement `ISellerAccountService` in `car-marketplace/backend/src/CarMarketplace.Api/Services/SellerAccountService.cs`. Hash passwords with `PasswordHasher<SellerAccount>`. "The submitted password is at least 8 characters and at most 128 characters." "Display name and inquiry name are 1 to 80 characters." Store email trimmed and lowercased. "A registration email comparison ignores letter case." Duplicate email throws a conflict with "That email is already in use." A mismatch signs nobody in and uses "The email or password is incorrect." Issue the sliding 14-day `marketplace_session` cookie on register and on successful sign-in. Update `last_signed_in_at` on those successes. Sign-out clears the cookie.
- [ ] T031 [US2] Map `POST /api/v1/sellers`, `GET /api/v1/sellers/me`, `POST /api/v1/sessions`, and `DELETE /api/v1/sessions/current` in `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/SellerEndpoints.cs` and `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/SessionEndpoints.cs`. Register returns `201` with `Location: /api/v1/sellers/me`. Anonymous `GET /api/v1/sellers/me` returns `401` "Sign in to continue." Forward `CancellationToken`. Add OpenAPI metadata. Register both maps from `Program.cs`.
- [ ] T032 [US2] Implement publish in `car-marketplace/backend/src/CarMarketplace.Api/Services/ListingCommandService.cs` (`IListingCommandService`). Require a signed-in seller. Persist status `Available` only. Reject, with no listing row and no leftover file, when any required fact is missing or when the bounds fail: "Make and model are 1 to 50 characters." "Year is a whole number from 1900 through the calendar year after the current year." "Price is a number greater than 0 and at most 99,999,999.99, with at most two decimal places." "Mileage is a whole number of kilometers from 0 through 2,000,000." "Location is 2 to 80 characters." "Description is 1 to 2000 characters after trimming blank space." "Add 1 to 10 still photos. The first photo is the cover." "Each photo must be a still image (JPEG, PNG, or WebP). Video is not accepted." "Each photo must be 8 MB or smaller." Save the listing and its photos in one transaction. Set `published_at` once.
- [ ] T033 [US2] Map `POST /api/v1/listings` (`multipart/form-data`, `201` plus `Location`) and `GET /api/v1/sellers/me/listings` in `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/ListingEndpoints.cs`. No cookie returns `401` "Sign in to continue." Validation returns `400` problem details using the field messages in `specs/001-car-marketplace/contracts/marketplace-api.md`. The seller list includes every status for the caller only, 20 per page, `publishedAt` descending.
- [ ] T034 [P] [US2] Add `car-marketplace/frontend/app/register/page.tsx`. On success the seller is signed in. Show "That email is already in use." when the API returns it.
- [ ] T035 [P] [US2] Add `car-marketplace/frontend/app/sign-in/page.tsx`. Show "The email or password is incorrect." without saying which field failed.
- [ ] T036 [US2] Add `car-marketplace/frontend/app/sell/new/page.tsx` posting make, model, year, price, mileage, location, description, and 1 to 10 photos. Show the API field text when publish is refused. After success, show the new listing on the public detail page.
- [ ] T037 [US2] Add `car-marketplace/frontend/app/sell/listings/page.tsx` listing the signed-in seller's cars, including sold cars.
- [ ] T038 [US2] Add sign-out to `car-marketplace/frontend/components/SiteHeader.tsx` and render it from `car-marketplace/frontend/app/layout.tsx`. After sign-out, publish and edit links send the visitor to `/sign-in`.
- [ ] T039 [US2] Append register, duplicate email, sign-in, failed sign-in, sign-out, and publish requests to `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.http`.

**Checkpoint**: A new seller can register, publish, sign out, and see the car in the public available list.

---

## Phase 5: User Story 3 - Correct a listing and mark it sold (Priority: P2)

**Goal**: The owner can change their listing, replace photos while keeping at least one, and move status between Available and Sold. Nobody else can.

**Independent Test**: The owner can change the price, replace photos while leaving at least one, mark the car sold so it leaves the available list, and mark it available again. A different person cannot change it.

### Tests for User Story 3

> Write this test first and confirm it fails before T041–T044.

- [ ] T040 [US3] Add `car-marketplace/backend/tests/CarMarketplace.Api.Tests/EditListingTests.cs`. As the owner, change the price and assert `publishedAt` is unchanged. `PATCH` status to `Sold` removes the car from `GET /api/v1/listings` and the detail status is `Sold`. A different seller's `PUT` returns `403` "Only the owner can change this listing." and the price is unchanged.

### Implementation for User Story 3

- [ ] T041 [US3] Extend `car-marketplace/backend/src/CarMarketplace.Api/Services/ListingCommandService.cs` with fact replace, photo replace, and status change. Fact replace and status change must not update `published_at` ("Edits and status changes must not update it."). Photo replace accepts 1 to 10 files and "The first photo is the cover photo." Zero photos is rejected and "the previous photos stay." A non-owner changes nothing. Allowed statuses are only "Available" and "Sold".
- [ ] T042 [US3] Map `PUT /api/v1/listings/{id}`, `PUT /api/v1/listings/{id}/photos`, and `PATCH /api/v1/listings/{id}/status` in `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/ListingEndpoints.cs`. Owner success is `200`. Anonymous is `401` "Sign in to continue." Not the owner is `403` "Only the owner can change this listing." Invalid photos or facts are `400` and the stored listing is unchanged. Do not add a DELETE route.
- [ ] T043 [US3] Add `car-marketplace/frontend/app/sell/listings/[id]/edit/page.tsx`. Save facts without forcing a photo upload. Save a new 1-to-10 photo set cover-first, re-sending kept images. Mark the car Sold or Available. Show "Only the owner can change this listing." when the API returns `403`.
- [ ] T044 [US3] Append the fact update, photo replace, zero-photo rejection, and both status changes to `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.http`.

**Checkpoint**: The owner can correct a car and take it out of the available list. A second account cannot.

---

## Phase 6: User Story 4 - Inquire about a listed car (Priority: P2)

**Goal**: A buyer sends one inquiry on someone else's available car and sees a confirmation that names the car. The owner reads it. There is no reply thread.

**Independent Test**: A person who is not signed in can send an inquiry on someone else's available car, see a confirmation, and the owner can read the buyer's name, email, message, and phone if it was provided.

### Tests for User Story 4

> Write this test first and confirm it fails before T046–T051.

- [ ] T045 [US4] Add `car-marketplace/backend/tests/CarMarketplace.Api.Tests/InquiryTests.cs`. An anonymous `POST` on someone else's Available listing returns `201` with that car's make, model, and year. The owner's `GET` returns name, email, message, phone, and `submittedAt`, newest first. A `POST` on a Sold listing returns `409` "This car is sold and is not accepting inquiries." and adds no row.

### Implementation for User Story 4

- [ ] T046 [P] [US4] Add sealed-record inquiry contracts with XML `<summary>` comments in `car-marketplace/backend/src/CarMarketplace.Api/Contracts/InquiryContracts.cs`. The create response includes `make`, `model`, and `year`. The owner read model includes name, email, message, optional phone, and `submittedAt` as `DateTimeOffset`.
- [ ] T047 [US4] Implement `IInquiryService` in `car-marketplace/backend/src/CarMarketplace.Api/Services/InquiryService.cs`. Accept only an Available listing the caller does not own. "Display name and inquiry name are 1 to 80 characters." "a message of 10 to 1000 characters." "Optional phone, when present, contains 7 to 20 digits and may include spaces, hyphens, parentheses, and a leading plus sign." Sold listings return the conflict "This car is sold and is not accepting inquiries." The owner receives "You cannot send an inquiry about your own listing." Invalid fields save nothing. Owner reads are newest `submitted_at` first. Do not add a reply method.
- [ ] T048 [US4] Map `POST /api/v1/listings/{id}/inquiries` and `GET /api/v1/listings/{id}/inquiries` in `car-marketplace/backend/src/CarMarketplace.Api/Endpoints/InquiryEndpoints.cs` and register the map from `Program.cs`. Create returns `201` and `Location`. Owner list is `200`. Anonymous list is `401`. Another seller's list is `403` "Only the owner can change this listing." Add OpenAPI metadata and `CancellationToken`.
- [ ] T049 [US4] Add `car-marketplace/frontend/components/InquiryForm.tsx` and render it from `car-marketplace/frontend/app/listings/[id]/page.tsx` only when the listing is Available and the viewer is not the owner. On success, confirm with the car's make, model, and year. Show the API's refusal text for a sold car, the owner's car, and invalid fields.
- [ ] T050 [US4] Add `car-marketplace/frontend/app/sell/listings/[id]/inquiries/page.tsx` listing the owner's inquiries newest first with name, email, message, phone when present, and the submission date. Do not add a reply box, a chat pane, or a reply route.
- [ ] T051 [US4] Append a valid inquiry, a sold-listing inquiry, an owner inquiry refusal, and the owner read request to `car-marketplace/backend/src/CarMarketplace.Api/CarMarketplace.http`.

**Checkpoint**: A buyer can reach the seller about an available car, and the product still has no conversation.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Hardening that spans the stories. Do not add excluded product behavior.

- [ ] T052 [P] Add a fixed-window rate limit in `car-marketplace/backend/src/CarMarketplace.Api/Program.cs` for `POST /api/v1/sellers`, `POST /api/v1/sessions`, and `POST /api/v1/listings/{id}/inquiries` (10 requests per minute per IP). Return `429` problem details.
- [ ] T053 [P] Review `car-marketplace/backend/src/CarMarketplace.Api/Services/SellerAccountService.cs`, `car-marketplace/backend/src/CarMarketplace.Api/Services/ListingCommandService.cs`, and `car-marketplace/backend/src/CarMarketplace.Api/Services/InquiryService.cs` so logs omit passwords, password hashes, session cookies, and inquiry message bodies.
- [ ] T054 Confirm the browse query in `car-marketplace/backend/src/CarMarketplace.Api/Services/ListingBrowseService.cs` filters and sorts on `status`, `published_at`, and `id` so `ix_listings_status_published_at_id` can serve the 500-listing target. Do not add a cache.
- [ ] T055 Walk through `specs/001-car-marketplace/quickstart.md` Scenarios 1–4. Fix only defects in the tasks above. Record any new gap as a task and wait for confirmation before building it.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup. Blocks every user story.
- **User Stories (Phases 3–6)**: Depend on Foundational. For one implementer, use priority order US1 → US2 → US3 → US4.
- **Polish (Phase 7)**: Depends on US1 through US4.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational. The happy-path test inserts its own rows, so the browse slice does not require the publish UI.
- **User Story 2 (P1)**: Starts after Foundational. Uses the User Story 1 public list to show a published car. Independently testable through the API.
- **User Story 3 (P2)**: Starts after User Story 2 publish exists, because edit needs an owned listing. The test creates that listing through the publish API.
- **User Story 4 (P2)**: Starts after User Story 2, because an inquiry needs an owned Available listing. It does not require User Story 3, except for the sold-listing refusal, which the test can set through the status API if US3 is already present. Implement US3 before US4 so the sold refusal is real.

### Within Each User Story

- The story test is written and failing before implementation.
- DTOs before services.
- Services before endpoints.
- Endpoints before the page that calls them.
- `Program.cs`, `ListingEndpoints.cs`, and `CarMarketplace.http` are shared. Do not work on two tasks that edit the same one of those files at the same time.

### Parallel Opportunities

- T004 and T005 can run alongside T001–T003.
- T007, T008, T009, T010, T013, T014, and T015 can run together after T006.
- T018 can run with the backend foundational tasks after T004.
- T021 can run in parallel with the failing T020 test once the test file exists.
- T025 and T026 can run together after T023.
- T034 and T035 can run together after T031.
- T046 can run in parallel with the failing T045 test.
- T052 and T053 can run together.

---

## Parallel Example: User Story 1

```text
Task: "T021 [US1] Listing response records in car-marketplace/backend/src/CarMarketplace.Api/Contracts/ListingResponses.cs"
Task: "T025 [US1] Browse page in car-marketplace/frontend/app/page.tsx"
Task: "T026 [US1] Detail page in car-marketplace/frontend/app/listings/[id]/page.tsx"
```

T025 and T026 wait until T023 has defined the routes. T025 and T026 do not edit the same files.

## Parallel Example: User Story 2

```text
Task: "T034 [US2] Register page in car-marketplace/frontend/app/register/page.tsx"
Task: "T035 [US2] Sign-in page in car-marketplace/frontend/app/sign-in/page.tsx"
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Finish Phase 1 and Phase 2.
2. Finish Phase 3.
3. Stop and run `BrowseListingsTests` plus the browse and detail pages.
4. The rows in that test are inserted in the test database. A clickable catalog with a real seller is User Story 2.

### Incremental Delivery

1. Setup and foundational work.
2. User Story 1: a visitor can browse and open a car.
3. User Story 2: a seller can register and publish. This is the first demo in which the catalog is filled through the product.
4. User Story 3: the owner can correct a car and mark it sold.
5. User Story 4: a buyer can inquire, and the owner can read the inquiry.
6. Polish, then the quickstart scenarios.

The accepted spec includes all four stories in the first release. Stopping after User Story 1 is a checkpoint, not a release cut.

### Parallel Team Strategy

After Phase 2, one person can build User Story 1 while another starts User Story 2 service and account pages. User Story 3 and User Story 4 wait until publish exists. They should not edit `Program.cs` in the same pass.

---

## Notes

- [P] tasks use different files and do not depend on an unfinished task.
- Story labels map to spec.md user stories.
- Field bounds in task text are copied from `data-model.md` and `contracts/marketplace-api.md`. Do not widen them.
- Do not add payments, financing, shipping, auctions, chat, replies, password reset, email verification, listing delete, moderation, keyword search, video, maps, street addresses, or buyer accounts.
- Commit after each task or logical group only when the user asks for a commit.

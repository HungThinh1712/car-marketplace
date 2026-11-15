# Feature Specification: Public Car Marketplace

**Feature Branch**: `001-car-marketplace`

**Created**: 2026-11-15

**Status**: Draft

**Input**: User description: "A public website where people can sell cars. A visitor can browse cars for sale, filter them, and open a car detail page. A seller can create an account, sign in, list a car, edit it, and mark it sold. A buyer can submit an inquiry to the seller about a listed car. Each listing has make, model, year, price, mileage, location, photos, description, and status (available or sold). Out of scope for this first release: payments, financing, shipping, auctions, and in-app chat."

## User Scenarios & Testing *(mandatory)*

The first release includes every story below. P1 and P2 describe delivery order, not a later release. A visitor browses without an account. A seller is a person with an account who publishes and maintains listings. A buyer is any person who sends an inquiry; a buyer does not need an account.

### User Story 1 - Browse cars for sale (Priority: P1)

A visitor opens the marketplace and looks through cars that are for sale. They narrow the list with filters and open one car to read the facts they need before deciding whether to contact the seller.

**Why this priority**: Finding a truthful listing is the reason a buyer comes to the site. This journey can be demonstrated with listings that already exist.

**Independent Test**: With at least one available listing and one sold listing present, a visitor who is not signed in can filter to the available car, open it, and see every required fact and the word Available.

**Acceptance Scenarios**:

1. **Given** available and sold listings exist, **When** a visitor opens the marketplace without signing in, **Then** the visitor sees available listings only, newest published first, up to 20 per page, and a count of how many cars match.
2. **Given** the visitor is browsing, **When** they set make, model, year range, price range, maximum mileage, location text, and status together, **Then** the list shows only cars that match every selected filter.
3. **Given** a filter match exists, **When** the visitor opens that car, **Then** the page shows make, model, year, price with its currency, mileage with its unit, location, at least one photo, description, the seller display name, and the status word Available or Sold.
4. **Given** the visitor opened a car from a filtered list, **When** they return to the list, **Then** the same filters are still applied.
5. **Given** no car matches the filters, **When** the visitor views the list, **Then** they see that no cars match and can clear the filters.

---

### User Story 2 - Publish a car for sale (Priority: P1)

A person creates a seller account, signs in, and publishes a car with the facts a buyer needs. The car then appears in the public browse list as available.

**Why this priority**: The marketplace has nothing to sell until a seller can publish. Account creation and sign-in exist so that publication is accountable to that seller.

**Independent Test**: A new person can create an account, sign in, publish one complete listing, and see that listing in the public available list while signed out.

**Acceptance Scenarios**:

1. **Given** the email is not already registered, **When** a person registers with a display name, email, and a password of at least 8 characters, **Then** they have a seller account and are signed in.
2. **Given** the email is already registered, **When** a person tries to register with it, **Then** the account is not created and they are told that the email is already in use.
3. **Given** a seller account exists, **When** the person signs in with the correct email and password, **Then** they are signed in. **When** the email or password is wrong, **Then** they are not signed in and the message does not reveal which of the two was wrong.
4. **Given** a seller is signed in, **When** they enter make, model, year, a price greater than zero, mileage, city and region, a description, and 1 to 10 photos, **Then** the car is published as Available, owned by that seller, and visible in the public available list.
5. **Given** a seller is signed in, **When** they try to publish with any required fact missing, a price that is not greater than zero, a year outside the allowed range, a mileage outside the allowed range, no photo, or more than 10 photos, **Then** the car is not published and the missing or invalid facts are explained in text.
6. **Given** a seller is signed in, **When** they sign out, **Then** they can browse as a visitor and can no longer publish or change listings until they sign in again.

---

### User Story 3 - Correct a listing and mark it sold (Priority: P2)

A seller updates their own car after publication and can record that it has been sold, or that it is for sale again.

**Why this priority**: This is part of the first release. It follows publish because buyers need a way to trust that a sold car is no longer offered as available.

**Independent Test**: The owner can change the price, replace photos while leaving at least one, mark the car sold so it leaves the available list, and mark it available again. A different person cannot change it.

**Acceptance Scenarios**:

1. **Given** a seller is signed in and owns an available listing, **When** they change any listing fact and save, **Then** the public detail page shows the saved facts and the original publication order is unchanged.
2. **Given** the owner is editing photos, **When** they save with at least one photo and no more than 10, **Then** the first photo is the cover photo buyers see in the list. **When** they try to save with zero photos, **Then** the change is rejected.
3. **Given** the owner is signed in, **When** they mark the car sold, **Then** the detail page shows Sold and a fresh browse of available cars does not include it.
4. **Given** the car is sold and the owner is signed in, **When** they mark it available, **Then** the detail page shows Available and the car returns to the available browse list.
5. **Given** a visitor, a buyer, or a different seller is viewing the car, **When** they attempt to edit it or change its status, **Then** the listing is unchanged and they are shown that only the owner can change it.

---

### User Story 4 - Inquire about a listed car (Priority: P2)

A buyer who wants to know more sends one inquiry to the seller of an available car. The seller later reads that inquiry while signed in. The product does not host a conversation.

**Why this priority**: This is part of the first release. It follows publish because an inquiry needs a listed car and a seller who can read it.

**Independent Test**: A person who is not signed in can send an inquiry on someone else's available car, see a confirmation, and the owner can read the buyer's name, email, message, and phone if it was provided.

**Acceptance Scenarios**:

1. **Given** an available listing owned by someone else, **When** a person submits their name, email, a message of 10 to 1000 characters, and an optional phone number, **Then** they see a confirmation that names the car, and the inquiry is waiting for that listing's owner.
2. **Given** the owner is signed in, **When** they open inquiries for that car, **Then** they see each inquiry newest first with the buyer's name, email, message, phone if provided, and the date it was submitted.
3. **Given** a listing is sold, or the signed-in viewer is the owner, **When** they try to send an inquiry, **Then** the inquiry is not accepted and they are told why.
4. **Given** a required inquiry fact is missing or the message is outside 10 to 1000 characters, **When** the person submits it, **Then** the inquiry is not accepted and the problem is explained in text.
5. **Given** an inquiry was accepted, **When** the buyer or seller looks for a reply thread, **Then** the product offers no in-product reply and no chat.

---

### Edge Cases

- A catalog with no listings shows that there are no cars for sale yet. It does not show an error.
- A sold car opened directly still shows its facts and the word Sold. It does not appear in the default available list.
- Status is the word Available or Sold. Color alone is not the only signal.
- The public car page shows the seller display name. It does not show the seller's email, phone, or a street address.
- Location is the city and region the seller typed. The product does not place the car on a map.
- Two sellers saving is not a shared edit. Only the owner can save. If the owner saves twice, the later save is the one buyers see.
- A registration email comparison ignores letter case. "Seller@example.com" and "seller@example.com" are the same email.
- A signed-in seller who tries to publish or edit after signing out is asked to sign in. Unsaved entries can be lost; nothing partial is published.
- A photo that is not a still image, or a still image larger than 8 MB, is rejected. Video is rejected.
- Price is a number greater than 0 and at most 99,999,999.99, with at most two decimal places. Zero, negative, and "contact for price" are rejected.
- Year is a whole number from 1900 through the calendar year after the current year. On 2026-11-15 the allowed years are 1900 through 2027.
- Mileage is a whole number of kilometers from 0 through 2,000,000.
- Make and model are 1 to 50 characters. Description is 1 to 2000 characters after trimming blank space. Display name and inquiry name are 1 to 80 characters. Location is 2 to 80 characters.
- Optional phone, when present, contains 7 to 20 digits and may include spaces, hyphens, parentheses, and a leading plus sign.
- The first release has no staff review. A listing that meets the rules becomes public immediately, including from a new account.
- Filters for make, model, and location match if the listing contains the typed text, ignoring letter case. Year, price, and mileage bounds are inclusive. Empty filter fields are ignored. Selected filters all apply together.
- Page size is 20 listings. The visitor can move to the next page when more matches exist.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A visitor MUST be able to browse listings and open a listing detail page without creating an account or signing in.
- **FR-002**: The default browse MUST list only Available listings, newest published first, 20 per page, and MUST show the number of matches. Sorting MUST use the original publication time, not the last edit.
- **FR-003**: A visitor MUST be able to filter by make, model, minimum year, maximum year, minimum price, maximum price, maximum mileage, location text, and status. Status choices MUST be available only, sold only, or both. All selected filters MUST apply together.
- **FR-004**: Returning from a detail page to browse MUST keep the filters used to get there.
- **FR-005**: A detail page MUST show make, model, year, price with currency code, mileage in kilometers, location, description, status as the word Available or Sold, the cover photo and any other photos, and the owner's display name.
- **FR-006**: A public listing MUST NOT show the owner's email, the owner's phone, a street address, or a map.
- **FR-007**: A person MUST be able to create one seller account with a unique email, a display name, and a password of at least 8 characters, and MUST be signed in immediately afterward.
- **FR-008**: The marketplace MUST reject a duplicate email and MUST reject a sign-in that does not match, without revealing whether the email or the password was the part that failed.
- **FR-009**: A signed-in seller MUST be able to sign out. Publishing and editing MUST require the owner to be signed in. A seller MUST remain signed in until they sign out or until 14 days have passed since they last signed in.
- **FR-010**: A signed-in seller MUST be able to publish a listing they own. The listing MUST include make, model, year, price, mileage, location, description, 1 to 10 still photos, and status Available. The first photo MUST be the cover photo.
- **FR-011**: The marketplace MUST refuse to publish or save a listing that breaks the boundaries in the Edge Cases section, and MUST explain the problem in text. A refused attempt MUST NOT create a public listing or leave a partial public listing.
- **FR-012**: Only the signed-in owner MUST be able to edit their listing facts and photos or change status between Available and Sold. Every other person MUST be prevented from doing so, and the listing MUST stay as it was.
- **FR-013**: Marking a listing Sold MUST remove it from the default available browse and MUST keep the detail page readable with the word Sold. Marking it Available again MUST return it to the default browse.
- **FR-014**: A person MUST be able to submit one inquiry on an Available listing they do not own, without an account. The inquiry MUST include name, email, and a message of 10 to 1000 characters, and MAY include a phone number.
- **FR-015**: The marketplace MUST refuse an inquiry on a Sold listing, on the viewer's own listing, or when the inquiry facts are invalid, and MUST explain why in text.
- **FR-016**: The signed-in owner MUST be able to read inquiries for their own listings, newest first, including the buyer's name, email, message, phone when provided, and submission date.
- **FR-017**: The first release MUST NOT take payment, offer financing, arrange shipping, run an auction, host in-app chat, or provide an in-product reply to an inquiry.
- **FR-018**: The first release MUST NOT include password reset, email verification before publishing, listing deletion, staff moderation, keyword search, video, buyer accounts, or more than one account role.
- **FR-019**: Every price MUST be shown in one marketplace currency code. For this release that code is USD.
- **FR-020**: The marketplace copy that a visitor, seller, or buyer must read to complete these journeys MUST be in English.

### Key Entities *(include if feature involves data)*

- **Seller account**: A person who can publish and maintain listings. Identified to the business by a unique email and a display name. Owns zero or more listings. The password is a secret used only to sign in.
- **Listing**: One car offered by one seller. Holds make, model, year, price, mileage, location, description, status, publication time, and the ordered photos. Status is Available or Sold, and the owner may move it either way. Has zero or more inquiries.
- **Photo**: A still image attached to one listing. Listings show one to ten photos. The first photo is the cover shown in browse results.
- **Inquiry**: One message from a buyer about one listing. Holds the buyer's name, email, optional phone, message, and submission date. Readable by the listing owner. It is not a conversation thread.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When a matching available car exists within the first five pages, a first-time visitor can filter to it and open the detail page in under 2 minutes without creating an account.
- **SC-002**: A first-time seller can create an account, sign in, and publish a complete listing in under 10 minutes.
- **SC-003**: In a review of any 10 published listings, all 10 detail pages show make, model, year, price, mileage, location, at least one photo, description, and the word Available or Sold.
- **SC-004**: A buyer can submit a valid inquiry in under 3 minutes and sees a confirmation that names the car. The owner can read that same inquiry after signing in.
- **SC-005**: After the owner marks a car sold, the next fresh browse of available cars does not include it, while a direct open of that car shows Sold.
- **SC-006**: With 500 published listings, a visitor sees either the first browse page or a requested detail page within 3 seconds of asking for it.
- **SC-007**: Across publish and edit attempts, zero public listings are missing a required fact, and zero listings are changed by someone other than the signed-in owner.
- **SC-008**: In a moderated task review of these four journeys, at least 90% of participants complete the journey they were given on the first attempt without help.

## Assumptions

Assumptions below are decisions made so this specification is testable. Each one is marked **Assumption** and should be confirmed before planning. If an assumption is wrong, the matching requirement changes.

- **A-01**: **Assumption.** One public catalog lists cars from every seller. Accounts are seller accounts only. A buyer does not register. Confirmed by the request: visitors browse, sellers list, buyers inquire.
- **A-02**: **Assumption.** The first release is English only (**FR-020**).
- **A-03**: **Assumption.** The only currency is USD, shown as a code on every price (**FR-019**). There is no per-listing currency.
- **A-04**: **Assumption.** Mileage is entered and shown as kilometers. The allowed range is 0 through 2,000,000.
- **A-05**: **Assumption.** Location is free-text city and region, 2 to 80 characters. Street address and maps are excluded to avoid publishing a home address.
- **A-06**: **Assumption.** A listing goes live as soon as it passes the field rules. There is no draft status and no staff approval (**FR-018**).
- **A-07**: **Assumption.** The owner may mark a sold car available again (**FR-013**). The first release does not delete listings.
- **A-08**: **Assumption.** An inquiry is read by the owner inside the product (**FR-016**). The first release does not email or otherwise notify the seller, and it does not include a reply.
- **A-09**: **Assumption.** Sign-in uses email and password. Social sign-in, email verification, and password reset are outside this release (**FR-018**). A seller stays signed in for 14 days after the last sign-in unless they sign out (**FR-009**).
- **A-10**: **Assumption.** Photo rules are 1 to 10 still images, maximum 8 MB each, first photo is the cover. Video is out of scope.
- **A-11**: **Assumption.** Text filters are case-insensitive "contains" matches. Numeric bounds are inclusive. Page size is 20. Default status filter is available only. Default sort is newest published first.
- **A-12**: **Assumption.** Numeric and length bounds in Edge Cases are the validation rules for this release, including model year through the following calendar year.
- **A-13**: **Assumption.** If the owner saves more than once, the later save is the listing buyers see. Two people never edit the same listing because only the owner can edit.
- **A-14**: **Assumption.** The delivery stack is already recorded in the project constitution as a developer constraint. This specification does not restate that stack and does not choose pages, components, interfaces, or storage design.
- **A-15**: **Assumption.** Success-criteria times and the 500-listing check are planning targets, not figures supplied by the stakeholder. No legal, tax, title, or vehicle-history requirement was requested, so none is included.
- **A-16**: **Assumption.** Personal information is limited to account email, display name, and password, plus the name, email, optional phone, and message on an inquiry. Inquiry contact details are visible to the listing owner and are not placed on the public listing.

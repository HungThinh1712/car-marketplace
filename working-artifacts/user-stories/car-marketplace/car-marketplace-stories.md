# Car marketplace stories

**Epic**: Public car marketplace, first release
**Status**: Draft
**Source**: Stakeholder request on 2026-11-15, and `specs/001-car-marketplace/spec.md`
**Mockup / wireframe**: TBD
**Priority note**: All four stories are in the first release. P1 is the path that puts a truthful catalog in front of a visitor. P2 completes honesty of status and contact with the seller.

Shared out of scope for every story: payments, financing, shipping, auctions, in-app chat, in-product replies, password reset, email verification, listing deletion, staff moderation, keyword search, video, maps, street addresses, and buyer accounts.

No numeric non-functional threshold was stated by the stakeholder. Timing and volume targets live in the specification as assumptions A-15 and success criteria SC-001 through SC-008. They are not signed stakeholder targets.

## US-001 Browse cars for sale

**Priority**: P1
**Traceability**: FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, SC-001, SC-003, SC-006

As a visitor,
I want to browse cars for sale, filter them, and open a car,
so that I can decide whether it is worth contacting the seller.

### Preconditions

- At least one listing can exist so the journey can be shown. The visitor is not signed in.

### Assumptions

| ID | Assumption | Impact if wrong |
| --- | --- | --- |
| A-03 | Prices are shown in USD. | Browse and detail labels change. |
| A-04 | Mileage is shown in kilometers. | The filter and the detail label change. |
| A-05 | Location is city and region text, with no map and no street address. | Browse location and privacy rules change. |
| A-11 | Default list is available cars only, newest published first, 20 per page. Text filters contain the typed words. | The first screen a visitor sees changes. |

### Workflow

The visitor opens the catalog, narrows it, opens one car, and can return to the same filters. A sold car is not in the default list. Its detail page, if opened, says Sold in words. The page shows the seller's display name and does not show the seller's email or phone.

### Acceptance criteria

```gherkin
AC1: Browse available cars without an account
  Given available and sold listings exist
  And the visitor is not signed in
  When the visitor opens the marketplace
  Then they see only available cars, newest published first, up to 20 per page
  And they see how many cars match

AC2: Apply every selected filter together
  Given the visitor is browsing
  When they set make, model, year range, price range, maximum mileage, location, and status
  Then the list shows only cars that match all of those filters

AC3: Read the facts needed to decide
  Given a listed car matches the filters
  When the visitor opens it
  Then the page shows make, model, year, price with currency, mileage with unit, location, at least one photo, description, seller display name, and the word Available or Sold
  And it does not show the seller email, seller phone, or a street address

AC4: Keep filters when returning from a car
  Given the visitor opened a car from a filtered list
  When they return to the list
  Then the same filters are still applied

AC5: Show an empty match clearly
  Given no car matches the filters
  When the visitor views the list
  Then they see that no cars match
  And they can clear the filters
```

### Out of scope

Keyword search, maps, and saving favorite cars.

## US-002 Publish a car for sale

**Priority**: P1
**Traceability**: FR-007, FR-008, FR-009, FR-010, FR-011, SC-002, SC-007

As a seller,
I want to create an account, sign in, and publish a car,
so that buyers can find a listing that is tied to me.

### Preconditions

- The person has an email address and the facts and photos of one car.

### Assumptions

| ID | Assumption | Impact if wrong |
| --- | --- | --- |
| A-06 | A valid listing goes public immediately. There is no draft and no staff approval. | Publish may need a holding state. |
| A-09 | Sign-in is email and password. The seller stays signed in for 14 days unless they sign out. Password reset and email verification are out of this release. | Account recovery and when a seller may publish both change. |
| A-10 | One to ten still photos, up to 8 MB each. The first photo is the cover. | The publish rules change. |
| A-12 | Year, price, mileage, and text length limits in the specification edge cases are the publish rules. | Invalid listings would be judged differently. |

### Workflow

The person registers, is signed in, and publishes one complete car as Available. A duplicate email or a wrong password does not open an account session. An incomplete car is not published. Signing out returns the person to visitor behavior.

### Acceptance criteria

```gherkin
AC1: Create a seller account and stay signed in
  Given the email is not already registered
  When the person registers with a display name, email, and a password of at least 8 characters
  Then a seller account exists
  And they are signed in

AC2: Reject a duplicate email
  Given the email is already registered
  When a person tries to register with it
  Then no additional account is created
  And they are told that the email is already in use

AC3: Sign in only with a matching email and password
  Given a seller account exists
  When the person signs in with the correct email and password
  Then they are signed in
  When the email or password does not match
  Then they are not signed in
  And the message does not reveal which value failed

AC4: Publish a complete car
  Given the seller is signed in
  When they enter make, model, year, a price greater than zero, mileage, city and region, a description, and 1 to 10 photos
  Then the car is public with the word Available
  And it is owned by that seller
  And it appears in the available browse list

AC5: Refuse an incomplete or invalid car
  Given the seller is signed in
  When they try to publish without a required fact, with an invalid price, year, or mileage, or with no photo or more than 10 photos
  Then the car is not public
  And the problem is explained in text

AC6: Stop publishing after sign-out
  Given the seller is signed in
  When they sign out
  Then they browse as a visitor
  And they cannot publish until they sign in again
```

### Out of scope

Password reset, email verification, social sign-in, and dealer-versus-private roles.

## US-003 Correct a listing and mark it sold

**Priority**: P2, still required for the first release
**Traceability**: FR-012, FR-013, SC-005, SC-007

As a seller,
I want to edit my car and mark it sold or available,
so that buyers do not pursue a car I no longer offer, and I can correct a mistake.

### Preconditions

- The seller is signed in and owns the listing.

### Assumptions

| ID | Assumption | Impact if wrong |
| --- | --- | --- |
| A-07 | The owner may mark a sold car available again. The owner cannot delete a listing in this release. | Sold may be final, or removal may be required. |
| A-11 | Editing does not change the original publication order. | A corrected listing may need to jump to the top of browse. |
| A-13 | The later save by the owner is the listing buyers see. | Conflict handling would be required only if non-owners could edit. |

### Workflow

The owner changes facts or photos and saves. The public page shows the saved car. Marking it sold removes it from the available list and leaves the detail page readable as Sold. Marking it available puts it back. Anyone who is not the signed-in owner cannot change it.

### Acceptance criteria

```gherkin
AC1: Save a correction
  Given the signed-in seller owns an available listing
  When they change a listing fact and save
  Then the public detail page shows the saved facts
  And the listing keeps its original publication order

AC2: Keep at least one photo
  Given the owner is editing photos
  When they save with 1 to 10 photos
  Then the first photo is the cover buyers see in the list
  When they try to save with zero photos
  Then the change is rejected

AC3: Mark the car sold
  Given the owner is signed in
  When they mark the car sold
  Then the detail page shows the word Sold
  And a fresh browse of available cars does not include it

AC4: Offer the car again
  Given the car is sold and the owner is signed in
  When they mark it available
  Then the detail page shows the word Available
  And the car returns to the available browse list

AC5: Block a change by anyone else
  Given a visitor, a buyer, or a different seller is viewing the car
  When they attempt to edit it or change its status
  Then the listing is unchanged
  And they see that only the owner can change it
```

### Out of scope

Deleting a listing, sale price negotiation, and proof of sale.

## US-004 Ask the seller about a car

**Priority**: P2, still required for the first release
**Traceability**: FR-014, FR-015, FR-016, FR-017, SC-004

As a buyer,
I want to send an inquiry about an available car to its seller,
so that I can ask a question without paying or chatting inside the site.

### Preconditions

- An available listing exists and is owned by someone other than the person sending the inquiry.

### Assumptions

| ID | Assumption | Impact if wrong |
| --- | --- | --- |
| A-01 | The buyer does not need an account. | Inquiry would start with registration. |
| A-08 | The owner reads the inquiry in the product. This release does not email the seller and does not provide a reply. | A notification channel or a conversation would be added. |
| A-16 | The buyer's name, email, message, and optional phone are visible to the listing owner and are not shown on the public listing. | Public contact details or a narrower inquiry would change the privacy rule. |

### Workflow

The buyer submits one inquiry and sees a confirmation that names the car. The owner signs in and reads inquiries for that car, newest first. Sold cars and the owner's own car do not accept an inquiry. There is no reply thread.

### Acceptance criteria

```gherkin
AC1: Send an inquiry without an account
  Given an available listing owned by someone else
  When a person submits their name, email, a message of 10 to 1000 characters, and an optional phone number
  Then they see a confirmation that names the car
  And the inquiry is waiting for that listing's owner

AC2: Let the owner read the inquiry
  Given the owner is signed in
  When they open inquiries for that car
  Then they see each inquiry newest first
  And each inquiry shows the buyer's name, email, message, phone if provided, and the date it was submitted

AC3: Refuse an inquiry the seller should not receive
  Given a listing is sold, or the signed-in viewer owns it
  When that person tries to send an inquiry
  Then the inquiry is not accepted
  And they are told why

AC4: Refuse an incomplete inquiry
  Given a required inquiry fact is missing or the message length is outside 10 to 1000 characters
  When the person submits it
  Then the inquiry is not accepted
  And the problem is explained in text

AC5: Do not start a conversation
  Given an inquiry was accepted
  When the buyer or the seller looks for a reply
  Then the product offers no in-product reply and no chat
```

### Out of scope

Payments, financing, shipping, auctions, in-app chat, and seller replies inside the product.

## Open questions to confirm before planning

- Business: Is USD the marketplace currency, and is mileage in kilometers (A-03, A-04)?
- UX: May the owner mark a sold car available again, or is sold final (A-07)?
- UX: Is reading inquiries while signed in enough, or must the seller also receive an email (A-08)?
- Security: May a new seller publish before proving they control the email (A-09)?
- Operations: Is leaving out password reset, listing deletion, and staff moderation acceptable for the first release (A-09, A-06, A-07)?

## Readiness

- Each story has a role, a goal, and a reason.
- Each story is a vertical outcome a stakeholder can watch, not a technical layer.
- Happy paths and refusal, permission, or status paths are covered.
- Assumptions are marked and traced to the specification.
- The stories do not choose an implementation design.

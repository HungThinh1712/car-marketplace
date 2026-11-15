<!--
Sync Impact Report
- Version change: unversioned template → 1.0.0
- Rationale: Initial ratification. The prior file was the untouched scaffold
  and had no semantic version. All principles are newly defined, so the first
  governed version is 1.0.0.
- Modified principles:
  - Placeholder principle 1 → I. Honest Listings
  - Placeholder principle 2 → II. Public Discovery, Accountable Selling
  - Placeholder principle 3 → III. Inquiry Without a Transaction
  - Placeholder principle 4 → IV. Vertical Slice Value
  - Placeholder principle 5 → V. Specify What and Why
- Added sections: none. Template section 2 renamed to Product Constraints.
  Template section 3 renamed to Delivery Workflow.
- Removed sections: none. Placeholder comments were removed after replacement.
- Follow-up TODOs: none. No placeholders were deferred.
-->

# Car Marketplace Constitution

## Core Principles

### I. Honest Listings

Every public listing MUST show the facts a buyer needs to decide whether to
inquire: make, model, year, price, mileage, location, photos, description, and
status. Status MUST be either available or sold. A listing marked sold MUST
NOT be presented as available. A seller MUST be able to correct their own
listing and MUST be able to mark it sold.

Rationale: Buyers trust the marketplace only when the listing matches the car
that is actually for sale.

### II. Public Discovery, Accountable Selling

A visitor MUST be able to browse listings, filter them, and open a listing
detail page without creating an account. Creating, editing, or marking a
listing sold MUST require a signed-in seller, and that seller MUST be able to
change only listings they own. A seller MUST be able to create an account and
sign in before listing a car.

Rationale: Discovery is the public value of the marketplace. Changing a
listing is an accountable act and MUST be tied to the seller who owns it.

### III. Inquiry Without a Transaction

A buyer MUST be able to submit an inquiry to the seller about a listed car.
The first release MUST stop at that inquiry. Payments, financing, shipping,
auctions, and in-app chat are out of scope. The product MUST NOT present
itself as collecting payment, arranging delivery, transferring title, or
hosting a live conversation.

Rationale: The first release proves that buyers can find a car and reach the
seller. Taking money or moving cars is a different product promise.

### IV. Vertical Slice Value

Each approved increment MUST deliver one complete user outcome a person can
demonstrate: the screen they use, the behavior they trigger, and the facts
that are stored. An increment that only draws a screen, only exposes a
service, or only stores data is not a finished outcome.

Rationale: Stakeholders can accept a marketplace only by using a real path
from visitor, seller, or buyer through to a visible result.

### V. Specify What and Why

Feature specifications MUST describe user-visible behavior, business rules,
and the reason they matter. They MUST NOT choose implementation design,
data structures, or interface contracts. Planning and implementation MUST
wait until the user accepts the specification. Test design MUST trace to
accepted behavior.

Rationale: The constitution separates product decisions from build decisions
so the team does not invent scope while designing the system.

## Product Constraints

The product is a public website where people sell cars.

The delivery stack is a constraint recorded for the developer, not a design
choice made in business analysis:

- The public site MUST be built with Next.js.
- The backend API MUST be built with .NET.
- The application database MUST be MySQL.

Business specifications MUST treat that stack as a boundary. They MUST NOT
prescribe pages, components, endpoints, schemas, or query design.

Personal information collected for an account or an inquiry MUST be limited
to what that outcome needs. Contact details a person submits MUST NOT be
shown publicly except where that person is sending them to the seller as part
of an inquiry.

The first release excludes payments, financing, shipping, auctions, and
in-app chat. Adding any of those requires a constitution amendment and a new
accepted specification.

## Delivery Workflow

Work for this product MUST follow this order:

1. Business analysis writes or updates the specification and resolves blocking
   ambiguity before planning.
2. The user accepts the specification before planning begins.
3. The developer produces a plan and tasks for the accepted behavior only.
4. Test analysis reviews that plan and those tasks before implementation.
5. The developer implements only tasks the user has accepted.
6. Test design traces back to the accepted behavior.
7. Any later gap found in the product MUST be added as a task and confirmed by
   the user before it is built.

The application database belongs to the developer. Transactional data design
MUST NOT be routed to an analytics data-engineering role.

## Governance

This constitution supersedes conflicting local practice for the car
marketplace. If a specification, plan, or task conflicts with these
principles, the constitution wins until it is amended.

An amendment MUST be written into this file, MUST state what changed, and
MUST update the version and the last-amended date. Versioning follows
semantic versioning:

- MAJOR: A principle is removed or redefined in a way earlier specifications
  can no longer satisfy.
- MINOR: A principle or governed section is added or materially expanded.
- PATCH: Wording is clarified without changing the rule.

The user MUST accept a MAJOR or MINOR amendment before new planning uses it.
Every specification and plan MUST be checked against this constitution before
implementation starts. A reviewer MUST reject work that adds an excluded
capability, hides listing status, or lets someone change a listing they do
not own.

**Version**: 1.0.0 | **Ratified**: 2026-11-15 | **Last Amended**: 2026-11-15

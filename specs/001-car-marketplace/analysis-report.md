# Specification Analysis Report

Planning review for the car marketplace. This is not a product release decision. No application was running.

The plan matches the four journeys, the constitution, and the Next.js, .NET, and MySQL stack. There is no critical constitution gap. Five high findings change acceptance behavior or task coverage and need to be accepted or edited before implementation.

| ID | Category | Severity | Location(s) | Summary | Recommendation |
| --- | --- | --- | --- | --- | --- |
| I1 | Inconsistency | HIGH | `tasks.md` T023; `contracts/marketplace-api.md` list-listings rule | T023 says an empty list returns `totalCount` 0. The contract says a page past the end returns an empty `items` array and the real match count. | Make T023 preserve `totalCount` when the page is past the end. |
| I2 | Inconsistency | HIGH | `spec.md` edge cases; `research.md` §7; `tasks.md` T009, T032 | The spec rejects non-still images and video. The plan and tasks accept only JPEG, PNG, and WebP, so a GIF, BMP, or HEIC still image fails the plan and passes the spec wording. | Accept that narrower set in the spec, or widen the plan to the spec’s still-image rule. |
| I3 | Inconsistency | HIGH | `spec.md` FR-007; `contracts/marketplace-api.md`; `tasks.md` T007, T030 | Registration acceptance is “at least 8 characters.” The contract and tasks also reject passwords longer than 128 characters. | Record the ceiling as an accepted requirement, or stop treating it as a product failure. |
| I4 | Ambiguity | HIGH | `spec.md` FR-009; `research.md` §3; `data-model.md` SellerAccount | FR-009 ends the session 14 days after the last sign-in. The plan uses a sliding cookie and does not renew `last_signed_in_at` on later use, so continued use can extend the session beyond 14 days from sign-in. | State whether activity renews the 14 days. |
| C1 | Coverage | HIGH | `spec.md` FR-002 and edge cases; `tasks.md` T025 | The spec says the visitor can move to the next page of 20. The API tasks page results, but the browse-page task does not require a next-page control. | Add that control to the browse task before implementation. |
| U1 | Underspecification | MEDIUM | `tasks.md` T025; `spec.md` FR-003 | T025 says “filters” without listing make, model, year, price, mileage, location, and status. | Name the FR-003 filter set in T025. |
| U2 | Ambiguity | MEDIUM | `spec.md` FR-014, User Story 4 | “One inquiry” can mean a single non-threaded message or one inquiry per person per listing. The plan allows repeated inquiries. | State that repeated inquiries are allowed. |
| U3 | Underspecification | MEDIUM | `spec.md` FR-007, FR-014; `contracts/marketplace-api.md` | The contract rejects an invalid email. The spec does not define a valid email. | Define the email rule or mark the contract message as a planning addition. |
| U4 | Coverage | MEDIUM | `spec.md` SC-006; `plan.md` Performance Goals; `tasks.md` T054 | The 3-second target at 500 listings has an index task and no measurement task. The plan explicitly omits a load-test harness. | Keep it as a planning target, or add a measurement task if the number is a gate. |
| U5 | Coverage | MEDIUM | `tasks.md` T052 | A 10-request-per-minute limit on register, sign-in, and inquiry is user-visible and is not in the spec. | Accept it as hardening, or add it to the spec. |
| T1 | Inconsistency | LOW | `spec.md` FR-003; `contracts/marketplace-api.md` | The spec calls the third status filter “both.” The API value is `all`. | Map “both” to `all` in the task text. |
| D1 | Underspecification | LOW | `spec.md` header; `plan.md` Summary | The spec status is still Draft, while the plan treats the specification as accepted. | Update the status if the spec was accepted. |
| A1 | Duplication | LOW | `spec.md` User Story 1, FR-002, FR-003, edge cases, A-11 | Browse, filter, and page-size rules are repeated. The copies agree. | Keep FR-002 and FR-003 as the source of truth. |

## Coverage

| Requirement key | Has task? | Task IDs | Notes |
| --- | --- | --- | --- |
| FR-001 browse-and-detail-without-account | Yes | T020, T023, T025, T026 | |
| FR-002 available-only-newest-20 | Yes | T020, T022, T023, T025 | Next-page control missing from T025 |
| FR-003 combined-filters | Yes | T022, T025 | Filter set not named in T025 |
| FR-004 keep-filters-on-return | Yes | T025, T026 | |
| FR-005 detail-facts | Yes | T020, T021, T026 | |
| FR-006 hide-seller-contact | Yes | T021, T026 | |
| FR-007 register-and-sign-in | Yes | T028, T030, T034 | 128-character ceiling is extra |
| FR-008 duplicate-email-and-generic-failure | Yes | T028, T030, T035 | |
| FR-009 sign-out-and-14-day-session | Yes | T016, T030, T038 | Sliding renewal is ambiguous |
| FR-010 publish-complete-listing | Yes | T028, T032, T036 | |
| FR-011 reject-invalid-listing | Yes | T032, T036 | |
| FR-012 owner-only-edit | Yes | T040, T041, T042, T043 | |
| FR-013 sold-and-available-again | Yes | T040, T041, T043, T044 | |
| FR-014 inquiry-without-account | Yes | T045, T047, T049 | |
| FR-015 refuse-invalid-inquiry | Yes | T045, T047, T049 | |
| FR-016 owner-reads-inquiries | Yes | T045, T047, T050 | |
| FR-017 no-transaction-or-chat | Yes | T047, T050 | Expressed as exclusions |
| FR-018 no-reset-delete-search-video | Yes | T042 and exclusion notes | No delete route |
| FR-019 usd-price | Yes | T020, T026 | |
| FR-020 english-copy | Yes | T004 | `lang="en"` |
| SC-006 500-listing-3-seconds | Partial | T054 | Index only; no measurement |

## Metrics

- Total requirements: 20 functional requirements, plus 1 buildable success criterion (SC-006)
- Total tasks: 55 (T001–T055)
- Coverage: 20/20 functional requirements have at least one task
- Critical issues: 0
- High issues that block implementation until accepted or edited: I1, I2, I3, I4, C1

## Constitution alignment

None of the findings break the constitution. Honest listings, public browse, owner-only changes, inquiry without payment or chat, vertical slices, and the Next.js, .NET, and MySQL boundary match the constitution.

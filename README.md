# Car Marketplace

Public car marketplace: specification, Next.js frontend, and .NET API with MySQL.

## Specification

- `.specify/memory/constitution.md` — product principles
- `specs/001-car-marketplace/spec.md` — first-release behavior
- `specs/001-car-marketplace/plan.md` — implementation plan
- `specs/001-car-marketplace/tasks.md` — task list
- `specs/001-car-marketplace/contracts/marketplace-api.md` — API contract
- `working-artifacts/user-stories/car-marketplace/car-marketplace-stories.md` — user stories

## Application

- `frontend/` — Next.js site
- `backend/` — ASP.NET Core API
- `docker-compose.yml` — MySQL 8.4 for local development

Copy `.env.example` to `.env` and set the database passwords before starting MySQL. The `.env` file is not committed. Copy `API_ORIGIN=http://localhost:5080` into `frontend/.env.local`.

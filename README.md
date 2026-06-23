# Dotnet-Task-Queue

A production-grade **distributed background job processing system** built with .NET 10, Hangfire, Redis, and PostgreSQL. Demonstrates retry logic with exponential backoff, dead-letter queue with audit trail, structured logging with correlation IDs, and a Hangfire monitoring dashboard.

---

## Tech stack

| Concern | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core |
| Job processing | Hangfire 1.8 |
| Job storage | PostgreSQL (Hangfire + EF Core) |
| Fast queue broker | Redis |
| ORM | Entity Framework Core 9 + Npgsql |
| Logging | Serilog (console + file, structured) |
| API docs | Scalar / OpenAPI |
| Health checks | ASP.NET Core Health Checks |
| Containerization | Docker Compose |
| CI | GitHub Actions |
| Test data | Bogus |
| Testing | xUnit, FluentAssertions, Testcontainers |

---

## Architecture

Clean Architecture — four layers with strict dependency flow:

```
TaskQueue.Domain        ← no dependencies
TaskQueue.Application   ← depends on Domain
TaskQueue.Infrastructure← depends on Application + Domain
TaskQueue.Api           ← depends on Application + Infrastructure
```

---

## Quick start

**Requirements:** Docker Desktop (or Docker Engine + Compose plugin)

```bash
git clone https://github.com/faizkhan005/dotnet-task-queue
cd dotnet-task-queue

cp .env.example .env          # optional: edit credentials

docker compose up -d
```

| URL | Description |
|---|---|
| http://localhost:5143/scalar | Interactive API docs |
| http://localhost:5143/hangfire | Job dashboard (admin / admin) |
| http://localhost:5143/health | Health check (JSON) |

---

## Job types

### Notification (`POST /api/jobs/notifications`)

Sends an email or SMS notification. Configured to fail ~20% of the time (via `NOTIFICATION_FAILURE_RATE` env var) to demonstrate retry and dead-letter behaviour in development.

- Queue: `notifications`
- Max retries: 4
- Backoff: 30s → 2m → 10m → 1h

### Report Generation (`POST /api/jobs/reports`)

Simulates generating a CSV or PDF report (data aggregation → formatting → delivery). Multi-phase with progress logging.

- Queue: `reports`
- Max retries: 3
- Backoff: 1m → 5m → 15m

### Data Sync (`POST /api/jobs/sync`)

Idempotent batch sync between two tables. Safe to re-run — uses upsert semantics, tracks upserted vs skipped counts per batch.

- Queue: `sync`
- Max retries: 3
- Backoff: 30s → 3m → 15m

---

## Key features

**Retry with exponential backoff** — `[AutomaticRetry]` attribute per job type, custom delay arrays, `AttemptsExceededAction.Fail` routing to dead-letter.

**Dead-letter queue** — `DeadLetterJobFilter` (`IApplyStateFilter`) intercepts permanently failed jobs and writes a `job_failures` record to PostgreSQL with full payload, error, stack trace, and attempt count. Survives Hangfire data purges.

**Job audit trail** — `job_records` table tracks every state transition (Enqueued → Processing → Succeeded / Retrying → DeadLettered) with full timestamps.

**Structured logging** — Serilog with `CorrelationId` enrichment across HTTP requests and job execution. Every log line for a given request/job shares the same correlation ID.

**Hangfire dashboard** — Real-time job monitoring at `/hangfire`, protected by HTTP Basic Auth with credentials from environment variables.

**Health checks** — `/health`, `/health/ready`, `/health/live` covering PostgreSQL and Redis.

**Database seeder** — Bogus generates realistic sample data on first startup (20 succeeded notifications, 10 reports, 5 dead-lettered jobs).

---

## API reference

See [wiki/API-Reference.md](../../wiki/API-Reference) for all endpoints with curl examples.

```bash
# Enqueue a notification
curl -X POST http://localhost:5000/api/jobs/notifications \
  -H "Content-Type: application/json" \
  -d '{"recipientEmail":"user@example.com","recipientName":"Jane","subject":"Hello","body":"World"}'

# Check job status
curl http://localhost:5000/api/jobs/{jobRecordId}

# View dead-letter queue
curl http://localhost:5000/api/dead-letter

# Requeue all failed jobs
curl -X POST http://localhost:5000/api/dead-letter/requeue-all
```

---

## Running tests

```bash
# Unit tests (no Docker needed)
dotnet test tests/TaskQueue.UnitTests/

# Integration tests (requires Docker — spins up Postgres + Redis via Testcontainers)
dotnet test tests/TaskQueue.IntegrationTests/
```

---

## Wiki

Full documentation in [wiki/](wiki/):

- [Architecture](../../wiki/Architecture.md)
- [Job Lifecycle](wiki/Job-Lifecycle.md)
- [Retry Policy](wiki/Retry-Policy.md)
- [Dead-Letter Queue](wiki/Dead-Letter-Queue.md)
- [Hangfire Dashboard](wiki/Hangfire-Dashboard.md)
- [Database Schema](wiki/Database-Schema.md)
- [Docker Setup](wiki/Docker-Setup.md)
- [API Reference](wiki/API-Reference.md)
- [Interview Prep](wiki/Interview-Prep.md)

---

## Environment variables

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_PASSWORD` | `postgres` | PostgreSQL password |
| `HANGFIRE_DASHBOARD_USER` | `admin` | Dashboard username |
| `HANGFIRE_DASHBOARD_PASS` | `admin` | Dashboard password |
| `NOTIFICATION_FAILURE_RATE` | `0.2` | Simulated failure rate (0.0–1.0) |

# Security Audit Log — FinanceSap.Enterprise

---

## Task 0: CVE Fix — SQLite Package Upgrade

- **Vulnerability**: GHSA-2m69-gcr7-jv3q (SQLite < 3.50.2 — memory corruption via aggregate terms overflow)
- **File**: `FinanceSap.Infrastructure/FinanceSap.Infrastructure.csproj`
- **Action**: Updated `Microsoft.Data.Sqlite` from `3.50.2` (non-existent, resolved to 5.0.0 with NU1603 warning) to `9.0.5`, aligned with the full .NET 9 stack. This bundles SQLite ≥ 3.50.2 and eliminates the CVE.
- **Status**: ✅ Completed

---

## Task 1: GET Endpoints — Placeholders Connected

All three GET endpoints were previously returning `NotFound()` unconditionally. They are now fully implemented with real MediatR queries and IDOR protection.

### New files created

| File | Purpose |
|---|---|
| `GetCustomerByIdQuery.cs` | MediatR query record carrying `Id` + `UserId` |
| `GetCustomerByIdQueryHandler.cs` | Validates JWT ownership before returning Customer |
| `GetLoanByIdQuery.cs` | MediatR query record carrying `Id` + `UserId` |
| `GetLoanByIdQueryHandler.cs` | Fetches Loan, validates CustomerId ownership |
| `GetLoanApplicationByIdQuery.cs` | MediatR query record carrying `Id` + `UserId` |
| `GetLoanApplicationByIdQueryHandler.cs` | Fetches LoanApplication, validates CustomerId ownership |

### IDOR Protection Pattern

All three handlers follow the same pattern:
1. Fetch the resource from the repository.
2. Resolve the `CustomerId` linked to the authenticated `UserId` via `IUserContext`.
3. Compare the resource's `CustomerId` against the resolved owner.
4. Return `null` (→ HTTP 404) on mismatch — **does not reveal resource existence to unauthorized callers**.

### Controllers updated

- `GET /api/customers/{id}` — `[Authorize]`, extracts `UserId` from JWT `sub` claim, dispatches `GetCustomerByIdQuery`
- `GET /api/loans/{id}` — `[Authorize]`, extracts `UserId` from JWT `sub` claim, dispatches `GetLoanByIdQuery`
- `GET /api/loanapplications/{id}` — `[Authorize]`, extracts `UserId` from JWT `sub` claim, dispatches `GetLoanApplicationByIdQuery`

All controllers also had duplicate `[HttpGet]` attribute declarations (left by the previous agent) removed.

- **Status**: ✅ Completed

---

## Task 2: Loan Approval / Rejection Flow

The `Loan` aggregate already had `Approve()` and `Reject()` state-transition methods in the Domain layer. This task wired them to the Application and Presentation layers.

### New files created

| File | Purpose |
|---|---|
| `ApproveLoan/ApproveLoanCommand.cs` | MediatR command carrying `LoanId` + `UserId` |
| `ApproveLoan/ApproveLoanHandler.cs` | IDOR check → `loan.Approve()` → `CommitAsync` |
| `RejectLoan/RejectLoanCommand.cs` | MediatR command carrying `LoanId`, `UserId`, optional `Reason` |
| `RejectLoan/RejectLoanHandler.cs` | IDOR check → `loan.Reject(reason)` → `CommitAsync` |

### Interface & Repository changes

- `ILoanRepository` — added `GetByIdTrackedAsync` (without `AsNoTracking`) so EF Core change tracking can persist state transitions.
- `LoanRepository` — implemented `GetByIdTrackedAsync`.

### New API endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| `PUT` | `/api/loans/{id}/approve` | `[Authorize]` | Transitions Loan from `Pending` → `Approved` |
| `PUT` | `/api/loans/{id}/reject` | `[Authorize]` | Transitions Loan from `Pending` → `Rejected` with optional reason |

Both endpoints return `204 No Content` on success, `404` if the loan does not exist or does not belong to the caller, and `400` if the loan is already in a final state.

- **Status**: ✅ Completed

---

## Test Infrastructure Fix

- **Root cause**: `appsettings.json` had `Jwt:Key` set to an empty string (correct for production — forces env var injection). However, `CustomWebApplicationFactory` was not providing a test key, causing `IDX10703: key length is zero` on every integration test request.
- **Fix**: Added `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience` settings to `CustomWebApplicationFactory.ConfigureWebHost` using a 32-character test-only key that never reaches production.
- **Result**: All 101 tests pass (87 previously passing + 14 previously broken by the missing key).

---

## Final Test Results

```
Total:   101
Passed:  101
Failed:    0
Skipped:   0
```

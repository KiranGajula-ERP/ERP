# AuditFlow — .NET Local LAN ERP

AuditFlow is an ASP.NET Core MVC application targeting **.NET 10**, with PostgreSQL persistence through Entity Framework Core and Npgsql. It is intended to run on one Windows host computer and be accessed through a trusted office LAN by 10+ browser users.

## Quick start

1. Install and start Docker Desktop for Windows (WSL 2 backend recommended).
2. Extract this folder to `C:\Users\DELL\Audit-ERP`.
3. Double-click `SETUP_AuditFlow.bat`.
4. On first run, the setup script creates `.env` and opens it in Notepad. Replace both `CHANGE_ME` passwords; save and run `SETUP_AuditFlow.bat` again.
5. Sign in at `http://localhost:8000` with the administrator username/password set in `.env`.
6. Change the initial password, create projects, create/assign staff, configure masters, then test with sample transactions before loading live data.
7. On the host, run `ipconfig` and share `http://<host-LAN-IPv4>:8000` only with authorized computers on the trusted office network.

Read `docs/INSTALLATION.md` for network, firewall, startup, backup and troubleshooting instructions.

## Stack

- ASP.NET Core MVC / C# on .NET 10
- PostgreSQL 17 in Docker, Entity Framework Core 10 / Npgsql provider
- Browser UI for desktop and tablet/mobile layout
- PostgreSQL data persisted in Docker volume `auditflow-db-data`
- Uploaded documents and photographs persisted under `media/`

## Main modules

- Authentication, password change, role switching, user management and project/tower assignments
- Projects, towers, Work Orders and BOQ lines
- Masters with CSV import and templates
- MR, RFQ, supplier quotations, quotation comparison, PO approval, supplier invoices, delivery challans, GRN verification, supplier payments
- Project and central-store stock, opening balances, stock ledger, adjustments/wastage, transfers, reservation-aware balances
- Indent approvals, partial issues, work tasks, photos, supervisor review, engineer checklist/client approval, JMR and client billing task
- Client RA bills, configurable additions/deductions, receipts and outstanding balances
- Linked document storage, expenses, audits/findings, reports and audit trail
- PowerShell backup and restore scripts

## Important beta notes

- This is a first runnable beta and **has not been compiled or end-to-end tested on a Windows Docker host in this creation environment**. Static source checks only were possible here because Docker and the .NET SDK/runtime are not installed in the build environment. Follow `docs/TEST_PLAN.md` on the target host before using live data.
- The initial schema is created by EF Core `EnsureCreatedAsync`, not by a checked-in EF migration baseline. This is suitable for a fresh beta database, but it is **not a safe schema-upgrade strategy** for future versions. Create and test a formal EF Core migration baseline before evolving an existing production database. Never delete the database volume to solve a schema mismatch without a verified backup.
- RFQ/quotation/PO and MR forms in this beta do not yet cover every desired multi-line purchasing case. Validate the project workflow and document requirements before relying on the current entries as statutory procurement records.
- RA billing currently supports bill creation, adjustments and receipts but requires finance-team validation of every tax/deduction formula before real accounting use.
- Notifications, e-signatures, automated JMR collection, complete PDF templates, barcode integration and scheduled backup rotation are not part of this beta.
- LAN HTTP is configured for trusted local networks. Do not expose it directly to the public internet. For remote access, deploy behind HTTPS with suitable firewall, identity, backup and security controls.

## Key files

- `SETUP_AuditFlow.bat` — first-time setup
- `START_AuditFlow.bat`, `STOP_AuditFlow.bat`, `STATUS_AuditFlow.bat` — service controls
- `REBUILD_AuditFlow.bat` — rebuild app image (does not intentionally delete data)
- `BACKUP_AuditFlow.bat`, `RESTORE_AuditFlow.bat` — database/document backup and database restore
- `.env.example` — configuration template; setup copies this to `.env`
- `docs/USER_GUIDE.md` — roles and workflow
- `docs/TEST_PLAN.md` — test before live data
- `docs/PRODUCTION_READINESS.md` — known gaps and launch checklist

## Security essentials

- Use unique long passwords in `.env`; avoid `#`, quotes and shell-sensitive special characters until you understand dotenv escaping.
- Change the bootstrap administrator password at first sign-in, then remove `AUDITFLOW_BOOTSTRAP_ADMIN_PASSWORD` from `.env` and restart the web service. Never commit or share `.env`.
- The `media/` folder may contain confidential documents and worksite photos. Restrict Windows folder permissions and back it up.
- Keep PostgreSQL port private; Compose does not publish the database port on the host.

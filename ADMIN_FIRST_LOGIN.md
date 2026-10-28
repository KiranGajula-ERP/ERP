# Administrator first-login checklist

1. Use the username/password configured in `.env` and sign in at `http://localhost:8000`.
2. Open **Change password** and set a unique password of at least 12 characters.
3. Remove `AUDITFLOW_BOOTSTRAP_ADMIN_PASSWORD` from `.env`; restart `START_AuditFlow.bat`. Do not retain or share the initial bootstrap password.
4. Create Units before items; create relevant categories/brands/suppliers/customers.
5. Create project records and tower names.
6. Create users with unique credentials. Assign each user only the projects, towers and roles required for their duties.
7. Create Work Orders and BOQ lines. Test stock transactions with sample item quantities.
8. Run through Indent, approval, partial issue, work photo, supervisor review, Engineer checklist, JMR and billing scenarios.
9. Configure daily backups and verify backup files can be restored in a separate test environment.
10. Run `docs/TEST_PLAN.md` before loading live data.

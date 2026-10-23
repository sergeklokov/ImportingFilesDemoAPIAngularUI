# Copilot Instructions

## Project Guidelines
- For this project, prefer straightforward import logic: avoid extra SQL application-lock and backup-table collision-check logic, trust a timestamp-based backup table name, and use TRUNCATE TABLE after backup instead of DELETE when replacing all rows.
- Prefer keeping database-side decisions in SQL rather than returning simple existence checks to .NET; perform the data-exists check, timestamped backup, and TRUNCATE in one SQL batch.
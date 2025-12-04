# pgAdmin Configuration

pgAdmin 4 is included as a database administration tool when using the `db-view` profile.

## Quick Start

1. Enable the profile in your `.env`:

   ```
   COMPOSE_PROFILES=db-view
   ```

2. Start services:

   ```
   make run
   ```

3. Access pgAdmin at `http://localhost:5050`

4. Login with credentials from `.env`:
   - Email: `PGADMIN_EMAIL`
   - Password: `PGADMIN_PASSWORD`

## Adding Database Connection

When you first access pgAdmin, you'll need to add the database server:

1. Click "Add New Server" or use the dashboard
2. In the "General" tab, enter:
   - **Name**: `Afina Database` (or any preferred name)
3. In the "Connection" tab, enter:

   - **Host name/address**: Use value from `DB_HOST` in `.env` (default: `postgres`)
   - **Port**: Use value from `DB_PORT` in `.env` (default: `5432`)
   - **Maintenance database**: Use value from `DB_NAME` in `.env` (default: `afina_db`)
   - **Username**: Use value from `DB_USER` in `.env` (default: `postgres`)
   - **Password**: Use value from `DB_PASSWORD` in `.env` (default: `postgres`)

4. Click "Save"

The connection will be remembered for future sessions.

## Using Environment Variables

The following `.env` variables control pgAdmin:

- `PGADMIN_EMAIL` - Login email address
- `PGADMIN_PASSWORD` - Login password
- `PGADMIN_PORT` - Port to access pgAdmin (default: `5050`)

Database connection details use the standard database variables:

- `DB_HOST` - Database hostname
- `DB_NAME` - Database name
- `DB_USER` - Database user
- `DB_PASSWORD` - Database password
- `DB_PORT` - Database port

## Notes

- pgAdmin data is persisted in a Docker volume (`pgadmin-data`) so your configurations survive container restarts
- For development environments, the connection can be added manually through the UI
- To reset pgAdmin (clear all configurations and users), run:
  ```
  docker volume rm afina_pgadmin-data
  make run
  ```

## Troubleshooting

If pgAdmin doesn't start:

1. Check port `5050` is available or modify `PGADMIN_PORT` in `.env`
2. Verify postgres container is running: `docker-compose ps`
3. Check logs: `docker-compose logs pgadmin`

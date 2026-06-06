# Render Deployment

This app deploys to Render as:

- `communityhub-db`: Render Postgres database
- `communityhub-api`: private Docker API service
- `communityhub-web`: public Docker web service

The app can still use SQL Server locally, but Render uses PostgreSQL through the `DatabaseProvider=PostgreSQL` setting in `render.yaml`.

## Deploy With Blueprint

1. Push this repo to GitHub.
2. Go to Render Dashboard.
3. Click `New +`.
4. Select `Blueprint`.
5. Connect `https://github.com/persadenis/UBB-SE-2026-CommunityHub`.
6. Select branch `main`.
7. Render should detect `render.yaml`.
8. Apply the blueprint.

Render will create the Postgres database and automatically pass its connection string to both services.

The public app URL is the `communityhub-web` service URL.

## Persistent Uploads

Uploaded files are stored under `/var/data/uploads` on the web service disk. Keep the disk attached, otherwise uploaded profile pictures, banners, group icons, and matchmaking photos will be lost after redeploys.


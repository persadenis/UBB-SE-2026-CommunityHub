# Render Deployment

This app deploys to Render as:

- an existing free Render Postgres database
- `communityhub-api-persadenis`: public free Docker API web service
- `communityhub-web-persadenis`: public free Docker web service

The app can still use SQL Server locally, but Render uses PostgreSQL through the `DatabaseProvider=PostgreSQL` setting in `render.yaml`.

## Free Tier Limitations

This Blueprint avoids paid Render resources, but the free tier has limitations:

- Free Render Postgres expires after 30 days.
- Free web services sleep after inactivity and can be slow on the first request.
- Free web services cannot receive private-network traffic, so the API is deployed as a public web service.
- Free web services cannot use persistent disks, so uploaded images can disappear after redeploys or restarts.

## Deploy With Blueprint

1. Open your existing free Render Postgres database.
2. Copy its internal connection string.
3. Push this repo to GitHub.
4. Go to Render Dashboard.
5. Click `New +`.
6. Select `Blueprint`.
7. Connect `https://github.com/persadenis/UBB-SE-2026-CommunityHub`.
8. Select branch `main`.
9. Render should detect `render.yaml`.
10. When Render asks for `ConnectionStrings__ChatAndEventsDB`, paste the same Postgres connection string for both services.
11. Apply the blueprint.

The Blueprint reuses your existing free Postgres database instead of creating a second one.

The public app URL is the `communityhub-web-persadenis` service URL.

## Persistent Uploads

On the free Render setup, uploaded profile pictures, banners, group icons, and matchmaking photos are stored on the service filesystem. They work while the instance is running, but they are not guaranteed to survive redeploys or restarts.

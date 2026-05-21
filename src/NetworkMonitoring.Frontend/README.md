# Network Monitoring — Frontend

React + TypeScript + Vite SPA for operator-facing device inventory (`GET /devices`, `POST /devices`) against the Device Inventory backend.

## Prerequisites

- Node.js **current LTS**
- Backend reachable at `http://localhost:5090` (host) when developing with the default Vite proxy

## Configure API base URL

| Scenario | Setting |
|----------|---------|
| **Local dev** (recommended) | Leave `VITE_BACKEND_BASE_URL` unset. The UI calls relative URLs (`/devices`). Vite proxies `/devices` to `http://localhost:5090` (override with `VITE_DEV_PROXY_TARGET`). |
| **Direct backend URL** | Set `VITE_BACKEND_BASE_URL=http://localhost:5090` — requires **CORS** on the backend if the browser origin differs from the API origin. |

Copy `.env.example` to `.env` when you need explicit overrides.

## Scripts

```bash
npm install
npm run dev      # http://localhost:5173 — proxies /devices → localhost:5090
npm run build
npm run test
npm run lint
```

## Docker image

The production image serves static files with **nginx** and **proxies `/devices` to `network-monitoring-backend:8080`** so the browser stays same-origin on port **80** inside the container (published as **3000** in `docker-compose.reference-stack.yml`).

```bash
docker compose -f docker-compose.reference-stack.yml up -d --build network-monitoring-backend network-monitoring-device-management-ui
```

Open `http://localhost:3000` — inventory requests hit nginx → backend without browser CORS configuration.

## Seed data

See `specs/006-device-management/quickstart.md` for `curl` examples against `POST /devices`.

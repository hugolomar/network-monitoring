# Expected Signals

Use this checklist after running a traffic scenario.

## Backend API

- `GET http://localhost:5090/devices` returns `200`
- `GET http://localhost:5090/api/graph/devices/all?limit=200` returns `200`

## Frontend

- UI responds on `http://localhost:3000`
- Device list and graph pages load without transport errors

## Graph consistency

- Graph snapshot endpoint returns non-empty nodes after replay
- Relationship count should increase for mixed/TCP captures (`http.cap`, `SkypeIRC.cap`)

## Scenario-specific notes

- `one-hour-realistic`: baseline replay with periodic DHCP noise, burst overlays, and ARP instability tail

# Tasks: Device Management

**Input**: Design documents from `/specs/006-device-management/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Incluye tareas de prueba enfocadas (Vitest + RTL) alineadas con `research.md` y `quickstart.md`; no es TDD estricto—implementación primero donde tenga sentido.

**Organization**: Fases por historia de usuario para implementación y validación incremental.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Paralelizable (archivos distintos, sin dependencias cruzadas).
- **[Story]**: Historia de usuario (US1–US4) en fases de historia; omitir en Setup, Foundational y Polish.

## Path Conventions

Código UI bajo `src/NetworkMonitoring.Frontend/` según `plan.md`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicializar el SPA React + TypeScript + Vite.

- [x] T001 Create `src/NetworkMonitoring.Frontend/` scaffold with Vite React TypeScript template (`package.json`, `vite.config.ts`, `tsconfig.json`, `index.html`, `public/`) per plan.md structure
- [x] T002 [P] Configure ESLint for React and TypeScript in `src/NetworkMonitoring.Frontend/eslint.config.js`
- [x] T003 [P] Configure Vitest, React Testing Library, and jsdom in `src/NetworkMonitoring.Frontend/vite.config.ts` plus `src/NetworkMonitoring.Frontend/src/test/setup.ts`
- [x] T004 Add npm scripts `dev`, `build`, `preview`, and `test` in `src/NetworkMonitoring.Frontend/package.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Configuración de API, tipos DTO y estado compartidos; **bloquea** las historias de usuario hasta completarse.

**⚠️ CRITICAL**: No comenzar trabajo de US1–US4 hasta terminar esta fase.

- [x] T005 Implement backend base URL resolution from `import.meta.env` (e.g. `VITE_BACKEND_BASE_URL`) in `src/NetworkMonitoring.Frontend/src/config/runtimeConfig.ts`
- [x] T006 Define TypeScript DTOs matching `GET /devices` and `POST /devices` payloads in `src/NetworkMonitoring.Frontend/src/models/deviceDtos.ts`
- [x] T007 Implement Fetch-based `listDevices` and `createDevice` with HTTP status mapping to typed outcomes in `src/NetworkMonitoring.Frontend/src/api/devicesApi.ts`
- [x] T008 Define screen-level state shapes aligned with `data-model.md` in `src/NetworkMonitoring.Frontend/src/models/deviceManagementState.ts`

**Checkpoint**: Cliente HTTP y tipos listos para montar la página de inventario.

---

## Phase 3: User Story 1 — View Device Inventory (Priority: P1) 🎯 MVP

**Goal**: Operador ve inventario en navegador con columnas requeridas; estados loading, vacío y backend no disponible.

**Independent Test**: Backend con ≥1 dispositivo almacenado; abrir UI y ver filas sin Probe/Kafka durante la prueba.

### Implementation for User Story 1

- [x] T009 [P] [US1] Implement inventory table rendering in `src/NetworkMonitoring.Frontend/src/components/DeviceInventoryTable.tsx`
- [x] T010 [P] [US1] Add formatting helpers for MAC, placeholders for missing hostname/IP, and timestamp display in `src/NetworkMonitoring.Frontend/src/components/deviceFormat.ts`
- [x] T011 [US1] Implement initial load flow with loading, loaded, empty, and backend-unavailable UI in `src/NetworkMonitoring.Frontend/src/pages/DeviceManagementPage.tsx`
- [x] T012 [US1] Wire root render path through `src/NetworkMonitoring.Frontend/src/App.tsx` and `src/NetworkMonitoring.Frontend/src/main.tsx`

**Checkpoint**: US1 verificable sola (lista + estados de primera carga).

---

## Phase 4: User Story 2 — Refresh Device Inventory (Priority: P1)

**Goal**: Refresco explícito sin recargar el navegador; inventario previo visible durante refresh; fallo de refresh no borra datos previos.

**Independent Test**: Con UI abierta, añadir dispositivo vía API/backend y refrescar; lista actualizada sin reload completo.

### Implementation for User Story 2

- [x] T013 [US2] Add refresh control and `refreshing` / `refreshFailed` behavior while retaining previous rows in `src/NetworkMonitoring.Frontend/src/pages/DeviceManagementPage.tsx`

**Checkpoint**: US1 + US2 completas y probables en secuencia.

---

## Phase 5: User Story 3 — Create Device Manually (Priority: P2)

**Goal**: Formulario manual que llama `POST /devices` con `Idempotency-Key`; manejar 201, 200 idempotente, 400 y 503 según contratos.

**Independent Test**: Enviar dispositivo válido; aparece en lista; repetir MAC muestra éxito consolidado sin filas duplicadas.

### Implementation for User Story 3

- [x] T014 [US3] Implement manual draft form with required fields and lightweight client validation in `src/NetworkMonitoring.Frontend/src/components/ManualDeviceForm.tsx`
- [x] T015 [US3] Integrate submit handler invoking `devicesApi.createDevice` with `Idempotency-Key` header derived from MAC in `src/NetworkMonitoring.Frontend/src/pages/DeviceManagementPage.tsx`
- [x] T016 [US3] Map intake outcomes to created/idempotent-success/validation-error states and refresh inventory without duplicate MAC rows in `src/NetworkMonitoring.Frontend/src/pages/DeviceManagementPage.tsx`

**Checkpoint**: Alta manual verificable contra backend existente.

---

## Phase 6: User Story 4 — Run the UI Independently (Priority: P2)

**Goal**: Imagen Docker serviendo estáticos y servicio en Compose; README de env para dev local vs stack.

**Independent Test**: Levantar stack con backend + UI y URL documentada; luego `npm run dev` apuntando al backend contenedor.

### Implementation for User Story 4

- [x] T017 [US4] Add production-oriented multi-stage `Dockerfile` serving `dist/` with nginx (or equivalent static server) in `src/NetworkMonitoring.Frontend/Dockerfile`
- [x] T018 [US4] Register `network-monitoring-device-management-ui` service with published port and backend base URL env in `docker-compose.reference-stack.yml`
- [x] T019 [US4] Document env vars, ports, and dev-vs-compose URLs in `src/NetworkMonitoring.Frontend/README.md`

**Checkpoint**: UI desplegable y reproducible fuera del proceso backend.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Pruebas enfocadas y validación manual según quickstart.

- [x] T020 [P] Add Vitest unit tests for HTTP outcome mapping and errors in `src/NetworkMonitoring.Frontend/src/api/devicesApi.test.ts`
- [x] T021 [P] Add React Testing Library tests for loading, empty, and unavailable inventory states in `src/NetworkMonitoring.Frontend/src/pages/DeviceManagementPage.test.tsx`
- [x] T022 Walk through validation scenarios in `specs/006-device-management/quickstart.md` (curl seed, dev server, compose UI, backend stopped/restarted) and align docs if gaps appear

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Sin dependencias—arrancar de inmediato.
- **Phase 2 (Foundational)**: Depende de Phase 1—**bloquea** todas las historias.
- **Phases 3–6 (US1–US4)**: Dependen de Phase 2; orden recomendado **US1 → US2 → US3 → US4** por acoplamiento en `DeviceManagementPage.tsx` (misma superficie).
- **Phase 7 (Polish)**: Depende de existir `devicesApi` y página (idealmente tras US3 para tests de página completos).

### User Story Dependencies

- **US1**: Tras Foundational—sin dependencia de otras historias.
- **US2**: Tras US1 (misma página; añade refresco).
- **US3**: Tras US1 (y prácticamente tras US2 si se evita conflicto en el mismo archivo).
- **US4**: Puede iniciar tras Foundational para Dockerfile aislado; **integración Compose** razonable tras US1 para probar contra backend real.

### Parallel Opportunities

- **Phase 1**: T002 y T003 en paralelo.
- **Phase 3**: T009 y T010 en paralelo (tabla vs formateo).
- **Phase 7**: T020 y T021 en paralelo.
- Historias distintas en paralelo solo con cuidado en `DeviceManagementPage.tsx` (riesgo de conflictos de merge).

---

## Parallel Example: User Story 1

```bash
# Tras Foundational, en paralelo:
Task T009 → src/NetworkMonitoring.Frontend/src/components/DeviceInventoryTable.tsx
Task T010 → src/NetworkMonitoring.Frontend/src/components/deviceFormat.ts
# Luego secuencial:
Task T011 → DeviceManagementPage.tsx
Task T012 → App.tsx / main.tsx
```

---

## Parallel Example: Polish

```bash
Task T020 → devicesApi.test.ts
Task T021 → DeviceManagementPage.test.tsx
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Completar Phase 1 y Phase 2.
2. Completar Phase 3 (US1) hasta T012.
3. **STOP y validar**: inventario visible con datos sembrados + estados loading/vacío/error.
4. Demo o entrega incremental.

### Incremental Delivery

1. Setup + Foundational → cimientos listos.
2. US1 → validar solo lectura (MVP).
3. US2 → refresco operativo.
4. US3 → alta manual.
5. US4 → contenedor + Compose + README operador.
6. Polish → tests y pasada quickstart.

### Suggested MVP Scope

- **Incluye**: Phase 1, Phase 2, Phase 3 (US1) — operador ve inventario con buenos estados de carga.
- **Siguiente incremento típico**: US2 + US3, luego US4 + Polish.

---

## Notes

- Cada tarea debe nombrar **ruta de archivo** concreta.
- No modificar contratos backend, Kafka ni `NetworkMonitoring.Domain` en esta feature.
- `[P]` solo si no hay dependencia de orden incompleta en el mismo archivo crítico.

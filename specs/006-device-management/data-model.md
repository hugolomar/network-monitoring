# Data Model: Device Management

## DeviceInventoryResponse

Represents the backend response consumed by the UI when listing devices.

### Fields

- `items`: array of `DeviceInventoryItem`

### Validation / UI Rules

- Empty `items` is a successful empty inventory state.
- The UI treats the response as read-only inventory state.
- The UI must not infer additional backend state from missing optional fields.

## DeviceInventoryItem

Represents one displayed device returned by the existing backend `GET /devices` contract.

### Fields

- `id`: numeric backend inventory identifier.
- `macAddress`: normalized MAC address.
- `primaryIp`: string or null.
- `hostname`: string or null.
- `observedIps`: array of IP address strings.
- `firstSeenUtc`: timestamp string from backend.
- `lastSeenUtc`: timestamp string from backend.
- `discoverySource`: backend discovery source string.

### Display Rules

- `macAddress` is the primary identity shown to the operator.
- `hostname` and `primaryIp` should show an explicit placeholder when absent.
- `observedIps` should remain visible even when `primaryIp` is absent.
- Timestamps should be displayed consistently without changing backend semantics.
- Items are displayed once per normalized MAC as returned by the backend.

## ManualDeviceDraft

Represents operator-entered form state before submitting to backend `POST /devices`.

### Fields

- `macAddress`: required normalized or normalizable MAC input.
- `primaryIp`: optional IP input.
- `hostname`: optional text input.
- `observedIps`: zero or more IP inputs.
- `firstSeenUtc`: required timestamp input.
- `lastSeenUtc`: required timestamp input.
- `discoverySource`: required discovery source value for manual submission.

### Validation / UI Rules

- UI may perform lightweight required-field and formatting checks for immediate feedback.
- Backend remains authoritative for MAC normalization, IP validity, timestamp ordering, idempotency, and persistence.
- The UI submits an idempotency identity consistent with the entered MAC as required by the backend contract.
- Backend rejection reasons must be surfaced without claiming the device was created.

## DeviceIntakeResponse

Represents the backend response after manual creation or consolidation.

### Fields

- `status` or equivalent backend outcome indicator.
- `reason`: optional backend explanation.
- `device`: optional resulting `DeviceInventoryItem`.

### UI State Mapping

- Created outcome -> success state and inventory refresh or update.
- Updated/consolidated/idempotent outcome -> idempotent-success state and one displayed device.
- Validation rejection -> validation-error state with reason.
- Persistence unavailable -> backend-unavailable state.
- Unexpected response -> generic recoverable error state.

## DeviceManagementViewState

Represents screen-level state for the device management page.

### States

- `initial`: UI has not loaded inventory yet.
- `loading`: inventory request is in progress.
- `loaded`: inventory loaded with one or more items.
- `empty`: inventory loaded successfully with no items.
- `refreshing`: existing inventory remains visible while refresh is in progress.
- `refreshFailed`: existing inventory remains visible and latest refresh failed.
- `backendUnavailable`: backend cannot be reached or reports unavailable.
- `submitting`: manual create request is in progress.
- `validationError`: backend or lightweight UI validation rejected manual input.
- `idempotentSuccess`: backend accepted a duplicate/consolidated request as successful.
- `createdSuccess`: backend created a new device.

### State Transitions

- `initial` -> `loading` -> `loaded` or `empty` or `backendUnavailable`.
- `loaded` or `empty` -> `refreshing` -> `loaded` or `empty` or `refreshFailed`.
- Form submit -> `submitting` -> `createdSuccess`, `idempotentSuccess`, `validationError`, or `backendUnavailable`.
- Any recoverable error state can transition back to `loading` or `refreshing` through retry/refresh.

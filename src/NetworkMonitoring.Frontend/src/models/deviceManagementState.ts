/** Initial load / full reload of inventory (GET /devices). */
export type InventoryLoadPhase =
  | "initial"
  | "loading"
  | "loaded"
  | "empty"
  | "unavailable";

/** Explicit refresh action state (inventory may stay visible). */
export type RefreshPhase = "idle" | "refreshing" | "failed";

/** Manual POST /devices lifecycle on the form. */
export type ManualSubmitPhase =
  | "idle"
  | "submitting"
  | "createdSuccess"
  | "idempotentSuccess"
  | "validationError"
  | "backendUnavailable";

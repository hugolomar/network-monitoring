/**
 * Browser telemetry hygiene: strip query strings and forbid end-user identity fields (FR-003, FR-019).
 */

/** Attribute keys that must never leave the browser in telemetry payloads. */
export const FORBIDDEN_TELEMETRY_ATTRIBUTES = [
  "enduser.id",
  "enduser.name",
  "enduser.email",
  "user.id",
  "user.email",
  "user.name",
  "http.request.header.authorization",
  "http.request.header.cookie",
] as const;

/**
 * Returns a URL safe for telemetry: origin + pathname only, no query or fragment.
 *
 * @param rawUrl - Absolute or relative URL.
 * @returns Scrubbed URL string, or a placeholder when parsing fails.
 */
export function scrubUrlForTelemetry(rawUrl: string): string {
  try {
    const base = typeof window !== "undefined" ? window.location.origin : "http://localhost";
    const url = new URL(rawUrl, base);
    return `${url.origin}${url.pathname}`;
  } catch {
    return "[unparseable-url]";
  }
}

/**
 * Removes forbidden keys and scrubs URL-like attribute values.
 *
 * @param attributes - Candidate span/log attributes.
 * @returns A new attribute map safe to export.
 */
export function sanitizeTelemetryAttributes(
  attributes: Record<string, unknown>
): Record<string, string | number | boolean> {
  const result: Record<string, string | number | boolean> = {};
  for (const [key, value] of Object.entries(attributes)) {
    if (FORBIDDEN_TELEMETRY_ATTRIBUTES.includes(key as (typeof FORBIDDEN_TELEMETRY_ATTRIBUTES)[number])) {
      continue;
    }
    if (typeof value === "string" && (key.includes("url") || key.includes("href"))) {
      result[key] = scrubUrlForTelemetry(value);
      continue;
    }
    if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
      result[key] = value;
    }
  }
  return result;
}

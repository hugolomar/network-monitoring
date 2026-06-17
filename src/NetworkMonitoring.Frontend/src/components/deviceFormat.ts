/**
 * Formats a value that might be null, undefined, or empty.
 * 
 * @param value - The raw string value to format.
 * @returns The original string if it contains content, otherwise an em-dash ("—").
 */
export function formatOptional(value: string | null | undefined): string {
  if (value === null || value === undefined || value.trim() === "") return "—";
  return value;
}

/**
 * Formats an ISO date-time string into a human-readable format.
 * 
 * @param iso - The ISO date-time string.
 * @returns A localized date and time string, or the original string if parsing fails.
 */
export function formatIsoDateTime(iso: string): string {
  const d = Date.parse(iso);
  if (Number.isNaN(d)) return iso;
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(d));
}

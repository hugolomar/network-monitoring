export function formatOptional(value: string | null | undefined): string {
  if (value === null || value === undefined || value.trim() === "") return "—";
  return value;
}

export function formatIsoDateTime(iso: string): string {
  const d = Date.parse(iso);
  if (Number.isNaN(d)) return iso;
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(d));
}

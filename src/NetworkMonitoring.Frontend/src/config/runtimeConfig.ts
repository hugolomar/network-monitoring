/** Backend API base URL without trailing slash. Empty string = same-origin (Vite proxy or nginx proxy to backend). */
export function getBackendBaseUrl(): string {
  const raw = import.meta.env.VITE_BACKEND_BASE_URL?.trim();
  if (!raw) return "";
  return raw.replace(/\/+$/, "");
}

export function apiUrl(path: string): string {
  const base = getBackendBaseUrl();
  const p = path.startsWith("/") ? path : `/${path}`;
  return base ? `${base}${p}` : p;
}

/** 
 * Backend API base URL without trailing slash. 
 * Empty string = same-origin (Vite proxy or nginx proxy to backend). 
 * 
 * @returns The normalized base URL for the backend API.
 */
export function getBackendBaseUrl(): string {
  const raw = import.meta.env.VITE_BACKEND_BASE_URL?.trim();
  if (!raw) return "";
  return raw.replace(/\/+$/, "");
}

/**
 * Constructs a full API URL for the given path.
 * 
 * @param path - The relative path of the endpoint.
 * @returns The absolute or relative URL for the API endpoint.
 */
export function apiUrl(path: string): string {
  const base = getBackendBaseUrl();
  const p = path.startsWith("/") ? path : `/${path}`;
  return base ? `${base}${p}` : p;
}

import { apiUrl } from "../config/runtimeConfig";
import type { DeviceGraphResponseDto } from "../models/graphDtos";

/**
 * Successful graph retrieval result.
 */
export type GetDeviceGraphSuccess = {
  ok: true;
  data: DeviceGraphResponseDto;
};

/**
 * Failed graph retrieval result.
 */
export type GetDeviceGraphFailure = {
  ok: false;
  kind: "invalid" | "unauthorized" | "forbidden" | "unavailable" | "http" | "network";
  status?: number;
  message: string;
};

/**
 * Union type for graph API outcomes.
 */
export type GetDeviceGraphResult = GetDeviceGraphSuccess | GetDeviceGraphFailure;

/**
 * Graph query controls accepted by the backend contract.
 */
export interface DeviceGraphQuery {
  rootDeviceId: string;
  depth?: number;
  limit?: number;
}

/**
 * Graph snapshot controls accepted by the backend full-graph endpoint.
 */
export interface DeviceGraphSnapshotQuery {
  limit?: number;
}

/**
 * Retrieves a bounded graph neighborhood for one root device identity.
 *
 * @param query - The requested graph query controls.
 * @param signal - Optional request cancellation token.
 * @returns Graph data or contract-aligned error information.
 */
export async function getDeviceGraph(
  query: DeviceGraphQuery,
  signal?: AbortSignal
): Promise<GetDeviceGraphResult> {
  const rootDeviceId = query.rootDeviceId.trim();
  if (!rootDeviceId) {
    return {
      ok: false,
      kind: "invalid",
      message: "Root device id is required.",
    };
  }

  const params = new URLSearchParams({ rootDeviceId });
  if (query.depth !== undefined) params.set("depth", String(query.depth));
  if (query.limit !== undefined) params.set("limit", String(query.limit));

  let response: Response;
  try {
    response = await fetch(apiUrl(`/api/graph/devices?${params.toString()}`), {
      method: "GET",
      headers: {
        Accept: "application/json",
        Authorization: "Bearer ui-local",
        "X-Role": "analyst",
      },
      signal,
    });
  } catch {
    return {
      ok: false,
      kind: "network",
      message: "Could not reach the graph backend (network error).",
    };
  }

  if (response.status === 400) {
    return {
      ok: false,
      kind: "invalid",
      status: 400,
      message: "Graph query is invalid.",
    };
  }

  if (response.status === 401) {
    return {
      ok: false,
      kind: "unauthorized",
      status: 401,
      message: "Graph query requires authentication.",
    };
  }

  if (response.status === 403) {
    return {
      ok: false,
      kind: "forbidden",
      status: 403,
      message: "Current role is not authorized for graph reads.",
    };
  }

  if (response.status === 503) {
    return {
      ok: false,
      kind: "unavailable",
      status: 503,
      message: "Communication graph is temporarily unavailable.",
    };
  }

  if (!response.ok) {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: `Unexpected graph response (${response.status}).`,
    };
  }

  let payload: DeviceGraphResponseDto;
  try {
    payload = (await response.json()) as DeviceGraphResponseDto;
  } catch {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: "Invalid graph response payload.",
    };
  }

  if (!Array.isArray(payload.nodes) || !Array.isArray(payload.edges) || typeof payload.truncated !== "boolean") {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: "Invalid graph response payload.",
    };
  }

  return {
    ok: true,
    data: payload,
  };
}

/**
 * Retrieves a bounded full graph snapshot.
 *
 * @param query - Optional snapshot controls.
 * @param signal - Optional request cancellation token.
 * @returns Graph data or contract-aligned error information.
 */
export async function getDeviceGraphSnapshot(
  query: DeviceGraphSnapshotQuery = {},
  signal?: AbortSignal
): Promise<GetDeviceGraphResult> {
  const params = new URLSearchParams();
  if (query.limit !== undefined) {
    params.set("limit", String(query.limit));
  }

  let response: Response;
  try {
    const path = params.size > 0 ? `/api/graph/devices/all?${params.toString()}` : "/api/graph/devices/all";
    response = await fetch(apiUrl(path), {
      method: "GET",
      headers: {
        Accept: "application/json",
        Authorization: "Bearer ui-local",
        "X-Role": "analyst",
      },
      signal,
    });
  } catch {
    return {
      ok: false,
      kind: "network",
      message: "Could not reach the graph backend (network error).",
    };
  }

  if (response.status === 400) {
    return {
      ok: false,
      kind: "invalid",
      status: 400,
      message: "Graph query is invalid.",
    };
  }

  if (response.status === 401) {
    return {
      ok: false,
      kind: "unauthorized",
      status: 401,
      message: "Graph query requires authentication.",
    };
  }

  if (response.status === 403) {
    return {
      ok: false,
      kind: "forbidden",
      status: 403,
      message: "Current role is not authorized for graph reads.",
    };
  }

  if (response.status === 503) {
    return {
      ok: false,
      kind: "unavailable",
      status: 503,
      message: "Communication graph is temporarily unavailable.",
    };
  }

  if (!response.ok) {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: `Unexpected graph response (${response.status}).`,
    };
  }

  let payload: DeviceGraphResponseDto;
  try {
    payload = (await response.json()) as DeviceGraphResponseDto;
  } catch {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: "Invalid graph response payload.",
    };
  }

  if (!Array.isArray(payload.nodes) || !Array.isArray(payload.edges) || typeof payload.truncated !== "boolean") {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: "Invalid graph response payload.",
    };
  }

  return {
    ok: true,
    data: payload,
  };
}


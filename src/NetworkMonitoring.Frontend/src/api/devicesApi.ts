import { apiUrl } from "../config/runtimeConfig";
import type {
  DeviceIntakeRequestDto,
  DeviceIntakeResponseDto,
  DeviceInventoryItem,
  DeviceInventoryResponseDto,
} from "../models/deviceDtos";

export type ListDevicesSuccess = {
  ok: true;
  data: DeviceInventoryResponseDto;
};

export type ListDevicesFailure = {
  ok: false;
  kind: "unavailable" | "http" | "network";
  status?: number;
  message: string;
};

export type ListDevicesResult = ListDevicesSuccess | ListDevicesFailure;

export type CreateDeviceSuccess = {
  ok: true;
  outcome: "Created" | "Updated" | "Idempotent";
  device: DeviceInventoryItem | null;
  raw: DeviceIntakeResponseDto;
};

export type CreateDeviceFailure = {
  ok: false;
  kind: "rejected" | "unavailable" | "network" | "parse";
  status?: number;
  reason?: string;
  message: string;
};

export type CreateDeviceResult = CreateDeviceSuccess | CreateDeviceFailure;

async function readJson<T>(response: Response): Promise<T | undefined> {
  const text = await response.text();
  if (!text) return undefined;
  try {
    return JSON.parse(text) as T;
  } catch {
    return undefined;
  }
}

export async function listDevices(signal?: AbortSignal): Promise<ListDevicesResult> {
  let response: Response;
  try {
    response = await fetch(apiUrl("/devices"), {
      method: "GET",
      headers: { Accept: "application/json" },
      signal,
    });
  } catch {
    return {
      ok: false,
      kind: "network",
      message: "Could not reach the backend (network error).",
    };
  }

  if (response.status === 503) {
    return {
      ok: false,
      kind: "unavailable",
      status: 503,
      message: "Device inventory backend is unavailable.",
    };
  }

  if (!response.ok) {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: `Unexpected response (${response.status}).`,
    };
  }

  const data = await readJson<DeviceInventoryResponseDto>(response);
  if (!data || !Array.isArray(data.items)) {
    return {
      ok: false,
      kind: "http",
      status: response.status,
      message: "Invalid inventory response payload.",
    };
  }

  return { ok: true, data };
}

export async function createDevice(
  body: DeviceIntakeRequestDto,
  idempotencyKey: string,
  signal?: AbortSignal
): Promise<CreateDeviceResult> {
  let response: Response;
  try {
    response = await fetch(apiUrl("/devices"), {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        "Idempotency-Key": idempotencyKey,
      },
      body: JSON.stringify(body),
      signal,
    });
  } catch {
    return {
      ok: false,
      kind: "network",
      message: "Could not reach the backend (network error).",
    };
  }

  const payload = await readJson<DeviceIntakeResponseDto>(response);

  if (response.status === 503) {
    return {
      ok: false,
      kind: "unavailable",
      status: 503,
      reason: payload?.reason ?? undefined,
      message: payload?.reason ?? "Device inventory backend is unavailable.",
    };
  }

  if (response.status === 400 || response.status === 415) {
    return {
      ok: false,
      kind: "rejected",
      status: response.status,
      reason: payload?.reason ?? undefined,
      message: payload?.reason ?? "Request was rejected.",
    };
  }

  if (response.status === 201 || response.status === 200) {
    if (!payload) {
      return {
        ok: false,
        kind: "parse",
        message: "Empty response from backend.",
      };
    }
    const outcome = String(payload.outcome);
    if (
      outcome !== "Created" &&
      outcome !== "Updated" &&
      outcome !== "Idempotent"
    ) {
      return {
        ok: false,
        kind: "parse",
        message: `Unexpected intake outcome: ${outcome}`,
      };
    }
    return {
      ok: true,
      outcome,
      device: payload.device ?? null,
      raw: payload,
    };
  }

  return {
    ok: false,
    kind: "http",
    status: response.status,
    message: `Unexpected response (${response.status}).`,
  };
}

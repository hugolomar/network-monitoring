/** Mirrors backend DeviceInventoryItem + JSON camelCase. */
export interface DeviceInventoryItem {
  id: number;
  macAddress: string;
  primaryIp: string | null;
  hostname: string | null;
  observedIps: string[];
  firstSeenUtc: string;
  lastSeenUtc: string;
  discoverySource: string;
}

export interface DeviceInventoryResponseDto {
  items: DeviceInventoryItem[];
}

/** Backend DeviceIntakeOutcomeKind.ToString() values */
export type DeviceIntakeOutcomeKind =
  | "Created"
  | "Updated"
  | "Idempotent"
  | "Rejected"
  | "PersistenceFailure";

export interface DeviceIntakeResponseDto {
  outcome: DeviceIntakeOutcomeKind | string;
  reason: string | null;
  device: DeviceInventoryItem | null;
}

export interface DeviceIntakeRequestDto {
  macAddress: string | null;
  primaryIp: string | null;
  hostname: string | null;
  observedIps: string[] | null;
  firstSeenUtc: string | null;
  lastSeenUtc: string | null;
  discoverySource: string | null;
  sourceEvent: null;
}

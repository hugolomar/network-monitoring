import { useMemo, useState, type FormEvent } from "react";
import type { DeviceIntakeRequestDto } from "../models/deviceDtos";

export interface ManualDeviceFormProps {
  disabled?: boolean;
  onSubmit: (payload: DeviceIntakeRequestDto, idempotencyKey: string) => Promise<void>;
}

function emptyDraft(): Record<string, string> {
  const now = new Date();
  const local = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
    .toISOString()
    .slice(0, 16);
  return {
    macAddress: "",
    primaryIp: "",
    hostname: "",
    observedIps: "",
    firstSeenUtc: local,
    lastSeenUtc: local,
    discoverySource: "MANUAL",
  };
}

export default function ManualDeviceForm({ disabled, onSubmit }: ManualDeviceFormProps) {
  const [draft, setDraft] = useState(emptyDraft);
  const [localError, setLocalError] = useState<string | null>(null);

  const idempotencyPreview = useMemo(() => draft.macAddress.trim(), [draft.macAddress]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setLocalError(null);
    const mac = draft.macAddress.trim();
    if (!mac) {
      setLocalError("MAC address is required.");
      return;
    }

    const observed =
      draft.observedIps.trim() === ""
        ? []
        : draft.observedIps.split(",").map((s) => s.trim()).filter(Boolean);

    const firstSeen = draft.firstSeenUtc ? new Date(draft.firstSeenUtc).toISOString() : null;
    const lastSeen = draft.lastSeenUtc ? new Date(draft.lastSeenUtc).toISOString() : null;

    if (!firstSeen || !lastSeen) {
      setLocalError("First seen and last seen timestamps are required.");
      return;
    }

    const payload: DeviceIntakeRequestDto = {
      macAddress: mac,
      primaryIp: draft.primaryIp.trim() === "" ? null : draft.primaryIp.trim(),
      hostname: draft.hostname.trim() === "" ? null : draft.hostname.trim(),
      observedIps: observed.length === 0 ? null : observed,
      firstSeenUtc: firstSeen,
      lastSeenUtc: lastSeen,
      discoverySource: draft.discoverySource.trim() || "MANUAL",
      sourceEvent: null,
    };

    await onSubmit(payload, mac);
  }

  return (
    <form className="form-grid" onSubmit={(e) => void handleSubmit(e)}>
      <label>
        MAC address *
        <input
          required
          autoComplete="off"
          value={draft.macAddress}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, macAddress: e.target.value }))}
          className="mono"
          placeholder="AA:BB:CC:DD:EE:FF"
        />
      </label>
      <label>
        Primary IP
        <input
          autoComplete="off"
          value={draft.primaryIp}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, primaryIp: e.target.value }))}
          className="mono"
        />
      </label>
      <label>
        Hostname
        <input
          autoComplete="off"
          value={draft.hostname}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, hostname: e.target.value }))}
        />
      </label>
      <label>
        Observed IPs (comma-separated)
        <input
          autoComplete="off"
          value={draft.observedIps}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, observedIps: e.target.value }))}
          className="mono"
        />
      </label>
      <label>
        First seen (UTC/local) *
        <input
          type="datetime-local"
          required
          value={draft.firstSeenUtc}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, firstSeenUtc: e.target.value }))}
        />
      </label>
      <label>
        Last seen (UTC/local) *
        <input
          type="datetime-local"
          required
          value={draft.lastSeenUtc}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, lastSeenUtc: e.target.value }))}
        />
      </label>
      <label>
        Discovery source *
        <input
          required
          value={draft.discoverySource}
          disabled={disabled}
          onChange={(e) => setDraft((d) => ({ ...d, discoverySource: e.target.value }))}
        />
      </label>
      <div className="form-actions">
        <button type="submit" className="primary" disabled={disabled}>
          Submit device
        </button>
        <span className="mono" style={{ fontSize: "0.8rem", opacity: 0.85 }}>
          Idempotency-Key → MAC: {idempotencyPreview || "(enter MAC)"}
        </span>
      </div>
      {localError ? (
        <div className="form-actions">
          <div className="message err">{localError}</div>
        </div>
      ) : null}
    </form>
  );
}

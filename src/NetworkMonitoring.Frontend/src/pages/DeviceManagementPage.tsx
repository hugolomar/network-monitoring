import { useCallback, useEffect, useState } from "react";
import { createDevice, listDevices } from "../api/devicesApi";
import DeviceInventoryTable from "../components/DeviceInventoryTable";
import ManualDeviceForm from "../components/ManualDeviceForm";
import { getBackendBaseUrl } from "../config/runtimeConfig";
import type { DeviceIntakeRequestDto, DeviceInventoryItem } from "../models/deviceDtos";
import type { InventoryLoadPhase, RefreshPhase } from "../models/deviceManagementState";

export default function DeviceManagementPage() {
  const [items, setItems] = useState<DeviceInventoryItem[]>([]);
  const [inventoryPhase, setInventoryPhase] = useState<InventoryLoadPhase>("initial");
  const [inventoryError, setInventoryError] = useState<string | null>(null);

  const [refreshPhase, setRefreshPhase] = useState<RefreshPhase>("idle");
  const [refreshError, setRefreshError] = useState<string | null>(null);

  const [manualBusy, setManualBusy] = useState(false);
  const [manualBanner, setManualBanner] = useState<{ kind: "ok" | "err"; text: string } | null>(
    null
  );

  const loadInventory = useCallback(async (mode: "initial" | "refresh") => {
    if (mode === "initial") {
      setInventoryPhase("loading");
      setInventoryError(null);
      setRefreshError(null);
    } else {
      setRefreshPhase("refreshing");
      setRefreshError(null);
    }

    const result = await listDevices();

    if (!result.ok) {
      const msg =
        result.kind === "network"
          ? result.message
          : result.kind === "unavailable"
            ? result.message
            : `${result.message}${result.status ? ` (${result.status})` : ""}`;
      if (mode === "initial") {
        setItems([]);
        setInventoryPhase("unavailable");
        setInventoryError(msg);
      } else {
        setRefreshPhase("failed");
        setRefreshError(msg);
      }
      return;
    }

    const next = result.data.items;
    setItems(next);
    setInventoryPhase(next.length === 0 ? "empty" : "loaded");
    if (mode === "refresh") {
      setRefreshPhase("idle");
    }
    setInventoryError(null);
  }, []);

  useEffect(() => {
    void loadInventory("initial");
  }, [loadInventory]);

  async function handleManualSubmit(payload: DeviceIntakeRequestDto, idempotencyKey: string) {
    setManualBanner(null);
    setManualBusy(true);
    try {
      const outcome = await createDevice(payload, idempotencyKey);
      if (!outcome.ok) {
        const text =
          outcome.kind === "rejected"
            ? outcome.reason ?? outcome.message
            : outcome.kind === "unavailable"
              ? outcome.message
              : outcome.message;
        setManualBanner({ kind: "err", text });
        return;
      }
      const label =
        outcome.outcome === "Created"
          ? "Device created."
          : outcome.outcome === "Updated"
            ? "Device updated (consolidated)."
            : "Request succeeded (idempotent / consolidated).";
      setManualBanner({ kind: "ok", text: label });
      await loadInventory("refresh");
    } finally {
      setManualBusy(false);
    }
  }

  const backendHint =
    getBackendBaseUrl() === ""
      ? "Same-origin `/devices` (Vite proxy or nginx → backend)."
      : `Backend base: ${getBackendBaseUrl()}`;

  const showTable =
    inventoryPhase === "loaded" ||
    inventoryPhase === "empty" ||
    (inventoryPhase === "unavailable" && items.length > 0) ||
    refreshPhase === "refreshing" ||
    refreshPhase === "failed";

  return (
    <>
      <header style={{ marginBottom: "1rem" }}>
        <h1 style={{ margin: "0 0 0.35rem", fontSize: "1.35rem" }}>Device inventory</h1>
        <p style={{ margin: 0, opacity: 0.82, fontSize: "0.9rem" }}>{backendHint}</p>
      </header>

      {(inventoryPhase === "loading" || inventoryPhase === "initial") && (
        <div className="panel">
          <p style={{ margin: 0 }}>Loading inventory…</p>
        </div>
      )}

      {inventoryPhase === "unavailable" && items.length === 0 && inventoryError && (
        <div className="panel">
          <div className="message err">{inventoryError}</div>
          <div className="toolbar" style={{ marginTop: "0.75rem" }}>
            <button type="button" onClick={() => void loadInventory("initial")}>
              Retry
            </button>
          </div>
        </div>
      )}

      {showTable && (
        <section className="panel">
          <div className="toolbar">
            <h2 style={{ flex: "1 1 auto", margin: 0 }}>Stored devices</h2>
            <button
              type="button"
              disabled={refreshPhase === "refreshing" || inventoryPhase === "loading"}
              onClick={() => void loadInventory("refresh")}
            >
              {refreshPhase === "refreshing" ? "Refreshing…" : "Refresh"}
            </button>
          </div>
          {refreshError ? (
            <div className="message warn" style={{ marginBottom: "0.65rem" }}>
              Latest refresh failed: {refreshError}. Showing previously loaded inventory.
            </div>
          ) : null}
          {inventoryPhase === "empty" && items.length === 0 ? (
            <p style={{ margin: 0, opacity: 0.88 }}>No devices in inventory yet.</p>
          ) : (
            <DeviceInventoryTable items={items} />
          )}
        </section>
      )}

      <section className="panel">
        <h2>Manual device intake</h2>
        {manualBanner ? (
          <div
            className={`message ${manualBanner.kind === "ok" ? "ok" : "err"}`}
            style={{ marginBottom: "0.65rem" }}
          >
            {manualBanner.text}
          </div>
        ) : null}
        <ManualDeviceForm disabled={manualBusy} onSubmit={handleManualSubmit} />
      </section>
    </>
  );
}

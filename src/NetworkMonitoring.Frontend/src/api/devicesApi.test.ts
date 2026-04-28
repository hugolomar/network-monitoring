import { afterEach, describe, expect, it, vi } from "vitest";
import { createDevice, listDevices } from "./devicesApi";

describe("devicesApi", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("listDevices parses items on 200", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        Promise.resolve({
          ok: true,
          status: 200,
          text: async () =>
            JSON.stringify({
              items: [
                {
                  id: 1,
                  macAddress: "AA:BB:CC:DD:EE:FF",
                  primaryIp: "10.0.0.1",
                  hostname: "h",
                  observedIps: ["10.0.0.1"],
                  firstSeenUtc: "2026-04-27T12:00:00.0000000+00:00",
                  lastSeenUtc: "2026-04-27T12:05:00.0000000+00:00",
                  discoverySource: "MANUAL",
                },
              ],
            }),
        }) as Response
      )
    );

    const r = await listDevices();
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.data.items).toHaveLength(1);
  });

  it("listDevices maps 503 to unavailable", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        Promise.resolve({
          ok: false,
          status: 503,
          text: async () => "",
        }) as Response
      )
    );

    const r = await listDevices();
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.kind).toBe("unavailable");
  });

  it("createDevice maps 201 Created", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        Promise.resolve({
          ok: true,
          status: 201,
          text: async () =>
            JSON.stringify({
              outcome: "Created",
              reason: null,
              device: {
                id: 2,
                macAddress: "AA:BB:CC:DD:EE:FF",
                primaryIp: null,
                hostname: null,
                observedIps: [],
                firstSeenUtc: "2026-04-27T12:00:00.0000000+00:00",
                lastSeenUtc: "2026-04-27T12:05:00.0000000+00:00",
                discoverySource: "MANUAL",
              },
            }),
        }) as Response
      )
    );

    const r = await createDevice(
      {
        macAddress: "AA:BB:CC:DD:EE:FF",
        primaryIp: null,
        hostname: null,
        observedIps: [],
        firstSeenUtc: "2026-04-27T12:00:00.0000000Z",
        lastSeenUtc: "2026-04-27T12:05:00.0000000Z",
        discoverySource: "MANUAL",
        sourceEvent: null,
      },
      "AA:BB:CC:DD:EE:FF"
    );
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.outcome).toBe("Created");
  });
});

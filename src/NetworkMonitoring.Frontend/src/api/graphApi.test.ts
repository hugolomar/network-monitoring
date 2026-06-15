import { afterEach, describe, expect, it, vi } from "vitest";
import { getDeviceGraph, getDeviceGraphSnapshot } from "./graphApi";

describe("graphApi", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("returns graph payload on 200", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async (): Promise<Response> => {
        const partial = {
          ok: true,
          status: 200,
          json: async () => ({
            nodes: [{ id: "a", kind: "InternalDevice" }],
            edges: [],
            truncated: false,
          }),
        };
        return partial as unknown as Response;
      })
    );

    const outcome = await getDeviceGraph({ rootDeviceId: "a", depth: 1, limit: 50 });
    expect(outcome.ok).toBe(true);
    if (outcome.ok) {
      expect(outcome.data.nodes).toHaveLength(1);
    }
  });

  it("maps 503 to unavailable", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async (): Promise<Response> => {
        const partial = {
          ok: false,
          status: 503,
        };
        return partial as unknown as Response;
      })
    );

    const outcome = await getDeviceGraph({ rootDeviceId: "a" });
    expect(outcome.ok).toBe(false);
    if (!outcome.ok) {
      expect(outcome.kind).toBe("unavailable");
    }
  });

  it("returns snapshot payload on 200", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async (): Promise<Response> => {
        const partial = {
          ok: true,
          status: 200,
          json: async () => ({
            nodes: [{ id: "device-1", kind: "InternalDevice" }],
            edges: [],
            truncated: false,
          }),
        };
        return partial as unknown as Response;
      })
    );

    const outcome = await getDeviceGraphSnapshot({ limit: 50 });
    expect(outcome.ok).toBe(true);
    if (outcome.ok) {
      expect(outcome.data.nodes[0]?.id).toBe("device-1");
    }
  });
});


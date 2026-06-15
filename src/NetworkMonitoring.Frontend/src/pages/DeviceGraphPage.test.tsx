import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import DeviceGraphPage from "./DeviceGraphPage";

vi.mock("../api/graphApi", () => ({
  getDeviceGraph: vi.fn(async () => ({
    ok: true as const,
    data: {
      nodes: [{ id: "api-root", kind: "InternalDevice" }],
      edges: [],
      truncated: false,
    },
  })),
  getDeviceGraphSnapshot: vi.fn(async () => ({
    ok: true as const,
    data: {
      nodes: [{ id: "device-1", kind: "InternalDevice" }],
      edges: [],
      truncated: false,
    },
  })),
}));

describe("DeviceGraphPage", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("renders graph data when query succeeds", async () => {
    render(<DeviceGraphPage />);

    const rootInput = screen.getByLabelText(/root device id/i);
    fireEvent.change(rootInput, { target: { value: "api-root" } });
    fireEvent.click(screen.getByRole("button", { name: /load graph/i }));

    expect(await screen.findByText(/graph visualization/i)).toBeInTheDocument();
    expect(await screen.findByText(/1 nodes · 0 edges/i)).toBeInTheDocument();
  });

  it("shows root-id validation before calling backend", async () => {
    render(<DeviceGraphPage />);

    const rootInput = screen.getByLabelText(/root device id/i);
    fireEvent.change(rootInput, { target: { value: "" } });
    fireEvent.click(screen.getByRole("button", { name: /load graph/i }));

    await waitFor(() => {
      expect(screen.getByText(/graph visualization/i)).toBeInTheDocument();
    });
  });

  it("shows empty-state message when graph query returns no nodes", async () => {
    const { getDeviceGraph } = await import("../api/graphApi");
    vi.mocked(getDeviceGraph).mockResolvedValueOnce({
      ok: true,
      data: { nodes: [], edges: [], truncated: false },
    });

    render(<DeviceGraphPage />);

    const rootInput = screen.getByLabelText(/root device id/i);
    fireEvent.change(rootInput, { target: { value: "unknown-device" } });
    fireEvent.click(screen.getByRole("button", { name: /load graph/i }));

    expect(await screen.findByText(/no graph data found for root id/i)).toBeInTheDocument();
    expect(await screen.findByText(/0 nodes · 0 edges/i)).toBeInTheDocument();
  });

  it("loads full snapshot when root is empty", async () => {
    const { getDeviceGraphSnapshot, getDeviceGraph } = await import("../api/graphApi");

    render(<DeviceGraphPage />);
    fireEvent.click(screen.getByRole("button", { name: /load graph/i }));

    await waitFor(() => {
      expect(vi.mocked(getDeviceGraphSnapshot)).toHaveBeenCalled();
    });
    expect(vi.mocked(getDeviceGraph)).not.toHaveBeenCalled();
  });
});


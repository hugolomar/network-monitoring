import { render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import DeviceManagementPage from "./DeviceManagementPage";

vi.mock("../api/devicesApi", () => ({
  listDevices: vi.fn(async () => ({
    ok: true as const,
    data: { items: [] },
  })),
  createDevice: vi.fn(),
}));

describe("DeviceManagementPage", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("shows loading then empty inventory message", async () => {
    render(<DeviceManagementPage />);
    expect(screen.getByText(/loading inventory/i)).toBeInTheDocument();
    expect(await screen.findByText(/no devices in inventory yet/i)).toBeInTheDocument();
  });
});

import { fireEvent, render, screen } from "@testing-library/react";
import { createElement } from "react";
import { describe, expect, it } from "vitest";
import DiagnosticsPage, { buildCorrelationKql } from "../pages/DiagnosticsPage";

describe("Diagnostics correlation lookup", () => {
  it("builds kibana correlation query", () => {
    expect(buildCorrelationKql(" abc-123 ")).toBe('correlationId : "abc-123"');
  });

  it("renders generated lookup when correlation id is provided", () => {
    render(createElement(DiagnosticsPage));

    const input = screen.getByPlaceholderText("example: 6f8f57715090da2632453988d9a1501b");
    fireEvent.change(input, { target: { value: "corr-42" } });
    fireEvent.click(screen.getByRole("button", { name: "Generate lookup" }));

    expect(screen.getByText(/correlationId : "corr-42"/)).toBeInTheDocument();
    expect(screen.getByText("corr-42")).toBeInTheDocument();
  });
});

import DeviceManagementPage from "./pages/DeviceManagementPage";
import DeviceGraphPage from "./pages/DeviceGraphPage";
import DiagnosticsPage from "./pages/DiagnosticsPage";
import { useState } from "react";

/**
 * Root component of the Network Monitoring application.
 * 
 * @returns The rendered application layout.
 */
export default function App() {
  const [page, setPage] = useState<"inventory" | "graph" | "diagnostics">("inventory");

  return (
    <div className="app-root">
      <nav className="app-nav">
        <button
          type="button"
          className={page === "inventory" ? "primary" : ""}
          onClick={() => setPage("inventory")}
        >
          Inventory
        </button>
        <button
          type="button"
          className={page === "graph" ? "primary" : ""}
          onClick={() => setPage("graph")}
        >
          Graph
        </button>
        <button
          type="button"
          className={page === "diagnostics" ? "primary" : ""}
          onClick={() => setPage("diagnostics")}
        >
          Diagnostics
        </button>
      </nav>
      {page === "inventory" ? (
        <DeviceManagementPage />
      ) : page === "graph" ? (
        <DeviceGraphPage />
      ) : (
        <DiagnosticsPage />
      )}
    </div>
  );
}

import DeviceManagementPage from "./pages/DeviceManagementPage";
import DeviceGraphPage from "./pages/DeviceGraphPage";
import DiagnosticsPage from "./pages/DiagnosticsPage";
import { useState } from "react";
import { trackInAppNavigation } from "./telemetry/browserTelemetry";

type AppPage = "inventory" | "graph" | "diagnostics";

/**
 * Root component of the Network Monitoring application.
 * 
 * @returns The rendered application layout.
 */
export default function App() {
  const [page, setPage] = useState<AppPage>("inventory");

  const navigate = (next: AppPage) => {
    setPage(next);
    trackInAppNavigation(next);
  };

  return (
    <div className="app-root">
      <nav className="app-nav">
        <button
          type="button"
          className={page === "inventory" ? "primary" : ""}
          onClick={() => navigate("inventory")}
        >
          Inventory
        </button>
        <button
          type="button"
          className={page === "graph" ? "primary" : ""}
          onClick={() => navigate("graph")}
        >
          Graph
        </button>
        <button
          type="button"
          className={page === "diagnostics" ? "primary" : ""}
          onClick={() => navigate("diagnostics")}
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

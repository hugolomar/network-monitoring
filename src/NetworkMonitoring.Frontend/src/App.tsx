import DeviceManagementPage from "./pages/DeviceManagementPage";
import DeviceGraphPage from "./pages/DeviceGraphPage";
import { useState } from "react";

/**
 * Root component of the Network Monitoring application.
 * 
 * @returns The rendered application layout.
 */
export default function App() {
  const [page, setPage] = useState<"inventory" | "graph">("inventory");

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
      </nav>
      {page === "inventory" ? <DeviceManagementPage /> : <DeviceGraphPage />}
    </div>
  );
}

import DeviceManagementPage from "./pages/DeviceManagementPage";

/**
 * Root component of the Network Monitoring application.
 * 
 * @returns The rendered application layout.
 */
export default function App() {
  return (
    <div className="app-root">
      <DeviceManagementPage />
    </div>
  );
}

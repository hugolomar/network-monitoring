import type { DeviceInventoryItem } from "../models/deviceDtos";
import { formatIsoDateTime, formatOptional } from "./deviceFormat";

/**
 * Props for the {@link DeviceInventoryTable} component.
 */
export interface DeviceInventoryTableProps {
  /** The list of devices to display in the table. */
  items: DeviceInventoryItem[];
}

/**
 * Component that renders a table of discovered network devices.
 * 
 * @param props - The component props.
 * @returns The rendered inventory table.
 */
export default function DeviceInventoryTable(props: DeviceInventoryTableProps) {
  const { items } = props;
  return (
    <div className="inventory-table-wrap">
      <table className="inventory">
        <thead>
          <tr>
            <th>MAC</th>
            <th>Hostname</th>
            <th>Primary IP</th>
            <th>Observed IPs</th>
            <th>Discovery</th>
            <th>First seen</th>
            <th>Last seen</th>
          </tr>
        </thead>
        <tbody>
          {items.map((row) => (
            <tr key={row.id}>
              <td className="mono">{row.macAddress}</td>
              <td className={row.hostname ? "" : "placeholder-cell"}>
                {formatOptional(row.hostname)}
              </td>
              <td className={row.primaryIp ? "mono" : "placeholder-cell mono"}>
                {formatOptional(row.primaryIp)}
              </td>
              <td className="mono">{row.observedIps.join(", ") || "—"}</td>
              <td>{row.discoverySource}</td>
              <td>{formatIsoDateTime(row.firstSeenUtc)}</td>
              <td>{formatIsoDateTime(row.lastSeenUtc)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

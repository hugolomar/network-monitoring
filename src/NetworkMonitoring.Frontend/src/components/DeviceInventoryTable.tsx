import type { DeviceInventoryItem } from "../models/deviceDtos";
import { formatIsoDateTime, formatOptional } from "./deviceFormat";

export interface DeviceInventoryTableProps {
  items: DeviceInventoryItem[];
}

export default function DeviceInventoryTable({ items }: DeviceInventoryTableProps) {
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

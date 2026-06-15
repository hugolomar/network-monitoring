import { formatIsoDateTime } from "./deviceFormat";
import type { GraphEdgeDto, GraphNodeDto } from "../models/graphDtos";
import type { DeviceInventoryItem } from "../models/deviceDtos";

/**
 * Props for rendering graph tabular details.
 */
export interface DeviceGraphTableProps {
  nodes: GraphNodeDto[];
  edges: GraphEdgeDto[];
  inventoryMatchesByNodeId?: Record<string, DeviceInventoryItem | null>;
}

/**
 * Renders graph nodes and edges in tables for exact inspection.
 *
 * @param props - The graph payload to display.
 * @returns Table-based graph details.
 */
export default function DeviceGraphTable(props: DeviceGraphTableProps) {
  const { nodes, edges, inventoryMatchesByNodeId } = props;

  return (
    <>
      <section className="panel">
        <h2>Graph nodes</h2>
        <div className="inventory-table-wrap">
          <table className="inventory">
            <thead>
              <tr>
                <th>ID</th>
                <th>Kind</th>
                <th>Inventory match</th>
              </tr>
            </thead>
            <tbody>
              {nodes.map((node) => {
                const match = inventoryMatchesByNodeId?.[node.id] ?? null;
                return (
                  <tr key={node.id}>
                    <td className="mono">{node.id}</td>
                    <td>{node.kind}</td>
                    <td>
                      {match ? (
                        <>
                          inv:{match.id} · {match.macAddress}
                          {match.primaryIp ? ` · ${match.primaryIp}` : ""}
                          {match.hostname ? ` · ${match.hostname}` : ""}
                        </>
                      ) : (
                        <span className="placeholder-cell">No match</span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>

      <section className="panel">
        <h2>Graph edges</h2>
        <div className="inventory-table-wrap">
          <table className="inventory">
            <thead>
              <tr>
                <th>Source</th>
                <th>Destination</th>
                <th>Protocol</th>
                <th>Weight</th>
                <th>First seen</th>
                <th>Last seen</th>
              </tr>
            </thead>
            <tbody>
              {edges.map((edge, idx) => (
                <tr key={`${edge.sourceId}-${edge.destinationId}-${edge.protocol}-${idx}`}>
                  <td className="mono">{edge.sourceId}</td>
                  <td className="mono">{edge.destinationId}</td>
                  <td>{edge.protocol}</td>
                  <td>{edge.weight}</td>
                  <td>{formatIsoDateTime(edge.firstSeenUtc)}</td>
                  <td>{formatIsoDateTime(edge.lastSeenUtc)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </>
  );
}


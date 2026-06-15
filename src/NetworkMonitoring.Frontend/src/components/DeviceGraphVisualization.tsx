import { useMemo } from "react";
import type { GraphEdgeDto, GraphNodeDto } from "../models/graphDtos";

/**
 * Props for the graph visualization component.
 */
export interface DeviceGraphVisualizationProps {
  nodes: GraphNodeDto[];
  edges: GraphEdgeDto[];
  rootNodeId: string;
}

interface PositionedNode extends GraphNodeDto {
  x: number;
  y: number;
}

/**
 * Renders a lightweight interactive SVG graph view.
 *
 * @param props - Graph data and the selected root id.
 * @returns SVG-based graph rendering.
 */
export default function DeviceGraphVisualization(props: DeviceGraphVisualizationProps) {
  const { nodes, edges, rootNodeId } = props;
  const width = 860;
  const height = 480;
  const cx = width / 2;
  const cy = height / 2;
  const ring = Math.min(width, height) * 0.35;

  const positions = useMemo(() => {
    const rootIndex = nodes.findIndex((n) => n.id === rootNodeId);
    const root = rootIndex >= 0 ? nodes[rootIndex] : nodes[0];
    const others = nodes.filter((n) => n.id !== root?.id);

    const mapped: PositionedNode[] = [];
    if (root) {
      mapped.push({ ...root, x: cx, y: cy });
    }

    others.forEach((node, idx) => {
      const angle = (idx / Math.max(1, others.length)) * Math.PI * 2;
      mapped.push({
        ...node,
        x: cx + Math.cos(angle) * ring,
        y: cy + Math.sin(angle) * ring,
      });
    });

    return mapped;
  }, [nodes, rootNodeId, cx, cy, ring]);

  const byId = useMemo(() => {
    const m = new Map<string, PositionedNode>();
    positions.forEach((n) => m.set(n.id, n));
    return m;
  }, [positions]);

  return (
    <section className="panel">
      <h2>Graph visualization</h2>
      <div className="graph-canvas-wrap">
        <svg viewBox={`0 0 ${width} ${height}`} className="graph-canvas" role="img" aria-label="Device graph">
          <defs>
            <marker
              id="arrow"
              markerWidth="10"
              markerHeight="8"
              refX="9"
              refY="4"
              orient="auto"
              markerUnits="strokeWidth"
            >
              <path d="M0,0 L10,4 L0,8 z" fill="#70839d" />
            </marker>
          </defs>

          {edges.map((edge, idx) => {
            const source = byId.get(edge.sourceId);
            const target = byId.get(edge.destinationId);
            if (!source || !target) return null;
            const stroke = Math.max(1, Math.min(6, edge.weight));
            return (
              <g key={`${edge.sourceId}-${edge.destinationId}-${edge.protocol}-${idx}`}>
                <line
                  x1={source.x}
                  y1={source.y}
                  x2={target.x}
                  y2={target.y}
                  stroke="#70839d"
                  strokeWidth={stroke}
                  markerEnd="url(#arrow)"
                  opacity={0.8}
                />
                <text x={(source.x + target.x) / 2} y={(source.y + target.y) / 2 - 6} className="graph-edge-label">
                  {edge.protocol} x{edge.weight}
                </text>
              </g>
            );
          })}

          {positions.map((node) => {
            const isRoot = node.id === rootNodeId;
            const isExternal = node.kind === "ExternalHost";
            const radius = isRoot ? 24 : 18;
            const fill = isExternal ? "#8b5f2d" : "#2d4f7a";
            const stroke = isRoot ? "#d6e4ff" : "#9eb6d3";
            return (
              <g key={node.id}>
                <circle cx={node.x} cy={node.y} r={radius} fill={fill} stroke={stroke} strokeWidth={isRoot ? 3 : 2} />
                <text x={node.x} y={node.y + radius + 14} textAnchor="middle" className="graph-node-label">
                  {node.id}
                </text>
                <title>{`${node.id} (${node.kind})`}</title>
              </g>
            );
          })}
        </svg>
      </div>
    </section>
  );
}


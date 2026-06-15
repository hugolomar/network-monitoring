/**
 * One node returned by the graph API.
 */
export interface GraphNodeDto {
  id: string;
  kind: string;
}

/**
 * One directed edge returned by the graph API.
 */
export interface GraphEdgeDto {
  sourceId: string;
  destinationId: string;
  protocol: string;
  weight: number;
  firstSeenUtc: string;
  lastSeenUtc: string;
}

/**
 * Graph neighborhood payload returned by the backend.
 */
export interface DeviceGraphResponseDto {
  nodes: GraphNodeDto[];
  edges: GraphEdgeDto[];
  truncated: boolean;
}


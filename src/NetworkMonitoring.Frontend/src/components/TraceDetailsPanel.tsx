export interface TraceDetailsPanelProps {
  correlationId: string;
  traceId?: string;
  impactedComponent?: string;
}

/**
 * Renders trace and correlation context details for diagnostics workflows.
 *
 * @param props - Component properties.
 * @returns A panel with distributed trace context fields.
 */
export default function TraceDetailsPanel(props: TraceDetailsPanelProps) {
  const { correlationId, traceId, impactedComponent } = props;

  return (
    <section className="panel">
      <h2>Trace context</h2>
      <dl style={{ margin: 0, display: "grid", gridTemplateColumns: "180px 1fr", rowGap: "0.35rem" }}>
        <dt>Correlation ID</dt>
        <dd className="mono" style={{ margin: 0 }}>
          {correlationId}
        </dd>
        <dt>Trace ID</dt>
        <dd className="mono" style={{ margin: 0 }}>
          {traceId && traceId.trim() ? traceId : "N/A"}
        </dd>
        <dt>Impacted component</dt>
        <dd style={{ margin: 0 }}>{impactedComponent && impactedComponent.trim() ? impactedComponent : "Unknown"}</dd>
      </dl>
    </section>
  );
}

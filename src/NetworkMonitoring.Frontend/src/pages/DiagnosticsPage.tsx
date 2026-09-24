import { useMemo, useState } from "react";
import TraceDetailsPanel from "../components/TraceDetailsPanel";

/**
 * Builds a Kibana KQL query from a correlation identifier.
 *
 * @param correlationId - Correlation identifier entered by the operator.
 * @returns Baseline KQL query string.
 */
export function buildCorrelationKql(correlationId: string): string {
  return `correlationId : "${correlationId.trim()}"`;
}

/**
 * Diagnostics page focused on correlation-first incident lookup.
 *
 * @returns The rendered diagnostics page.
 */
export default function DiagnosticsPage() {
  const [correlationId, setCorrelationId] = useState("");
  const [traceId, setTraceId] = useState("");
  const [impactedComponent, setImpactedComponent] = useState("");
  const [submitted, setSubmitted] = useState(false);

  const query = useMemo(() => buildCorrelationKql(correlationId), [correlationId]);
  const isReady = correlationId.trim().length > 0;

  return (
    <>
      <header style={{ marginBottom: "1rem" }}>
        <h1 style={{ margin: "0 0 0.35rem", fontSize: "1.35rem" }}>Diagnostics</h1>
        <p style={{ margin: 0, opacity: 0.82, fontSize: "0.9rem" }}>
          Locate incidents by correlation ID and jump to Kibana log search.
        </p>
      </header>

      <section className="panel">
        <h2>Correlation lookup</h2>
        <div className="form-grid">
          <label>
            Correlation ID
            <input
              value={correlationId}
              onChange={(event) => setCorrelationId(event.target.value)}
              placeholder="example: 6f8f57715090da2632453988d9a1501b"
            />
          </label>
          <label>
            Trace ID (optional)
            <input
              value={traceId}
              onChange={(event) => setTraceId(event.target.value)}
              placeholder="optional distributed trace id"
            />
          </label>
          <label>
            Impacted component (optional)
            <input
              value={impactedComponent}
              onChange={(event) => setImpactedComponent(event.target.value)}
              placeholder="NetworkMonitoring.Backend.Graph"
            />
          </label>
          <div className="form-actions">
            <button type="button" className="primary" disabled={!isReady} onClick={() => setSubmitted(true)}>
              Generate lookup
            </button>
          </div>
        </div>
        {isReady ? (
          <p style={{ marginTop: "0.8rem", marginBottom: 0 }}>
            Kibana KQL: <span className="mono">{query}</span>
          </p>
        ) : (
          <p style={{ marginTop: "0.8rem", marginBottom: 0, opacity: 0.75 }}>
            Enter a correlation ID to generate the baseline lookup query.
          </p>
        )}
      </section>

      {submitted && isReady ? (
        <TraceDetailsPanel
          correlationId={correlationId.trim()}
          traceId={traceId.trim()}
          impactedComponent={impactedComponent.trim()}
        />
      ) : null}
    </>
  );
}

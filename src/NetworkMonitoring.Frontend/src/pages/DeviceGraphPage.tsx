import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { getDeviceGraph, getDeviceGraphSnapshot } from "../api/graphApi";
import type { DeviceGraphResponseDto } from "../models/graphDtos";
import DeviceGraphVisualization from "../components/DeviceGraphVisualization";
import DeviceGraphTable from "../components/DeviceGraphTable";

/**
 * Page component for graph exploration against `GET /api/graph/devices`.
 *
 * @returns The rendered graph exploration page.
 */
export default function DeviceGraphPage() {
  const [rootDeviceId, setRootDeviceId] = useState("");
  const [depth, setDepth] = useState("1");
  const [limit, setLimit] = useState("50");

  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<DeviceGraphResponseDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [refreshError, setRefreshError] = useState<string | null>(null);

  const stats = useMemo(() => {
    if (!result) return null;
    return `${result.nodes.length} nodes · ${result.edges.length} edges`;
  }, [result]);

  async function runQuery(mode: "initial" | "refresh") {
    const parsedDepth = Number(depth);
    const parsedLimit = Number(limit);
    const trimmedRoot = rootDeviceId.trim();

    if (mode === "initial") {
      setError(null);
      setRefreshError(null);
    } else {
      setRefreshError(null);
    }

    setLoading(true);
    try {
      const outcome = trimmedRoot
        ? await getDeviceGraph({
            rootDeviceId: trimmedRoot,
            depth: Number.isFinite(parsedDepth) ? parsedDepth : undefined,
            limit: Number.isFinite(parsedLimit) ? parsedLimit : undefined,
          })
        : await getDeviceGraphSnapshot({
            limit: Number.isFinite(parsedLimit) ? parsedLimit : undefined,
          });

      if (!outcome.ok) {
        if (mode === "initial" || !result) {
          setError(outcome.message);
        } else {
          setRefreshError(outcome.message);
        }
        return;
      }

      setResult(outcome.data);
      setError(null);
      setRefreshError(null);
    } finally {
      setLoading(false);
    }
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await runQuery("initial");
  }

  return (
    <>
      <header style={{ marginBottom: "1rem" }}>
        <h1 style={{ margin: "0 0 0.35rem", fontSize: "1.35rem" }}>Device communication graph</h1>
        <p style={{ margin: 0, opacity: 0.82, fontSize: "0.9rem" }}>
          Load the full communication graph (Neo4j-like view) or filter by root identity.
        </p>
      </header>

      <section className="panel">
        <h2>Query controls</h2>
        <form onSubmit={onSubmit} className="form-grid">
          <label>
            Root device id
            <input
              value={rootDeviceId}
              onChange={(e) => setRootDeviceId(e.target.value)}
              placeholder="(optional) device-1"
            />
          </label>

          <label>
            Depth
            <input value={depth} onChange={(e) => setDepth(e.target.value)} placeholder="1" />
          </label>

          <label>
            Limit
            <input value={limit} onChange={(e) => setLimit(e.target.value)} placeholder="50" />
          </label>

          <div className="form-actions">
            <button className="primary" type="submit" disabled={loading}>
              {loading ? "Loading..." : "Load graph"}
            </button>
            <button
              type="button"
              disabled={loading || !result}
              onClick={() => {
                void runQuery("refresh");
              }}
            >
              Refresh
            </button>
            {stats ? <span>{stats}</span> : null}
            {result?.truncated ? <span className="graph-truncated-badge">Truncated</span> : null}
          </div>
        </form>
        <p style={{ margin: "0.65rem 0 0", opacity: 0.78, fontSize: "0.85rem" }}>
          Leave root empty to load the full graph. If provided, root expects graph identity values (for example:{" "}
          <span className="mono">device-1</span>), not inventory numeric IDs.
        </p>
      </section>

      {error ? (
        <section className="panel">
          <div className="message err">{error}</div>
        </section>
      ) : null}

      {refreshError ? (
        <section className="panel">
          <div className="message warn">{refreshError}. Showing previously loaded graph.</div>
        </section>
      ) : null}

      {result ? (
        <>
          {result.nodes.length === 0 ? (
            <section className="panel">
              <div className="message warn">
                {rootDeviceId.trim() ? (
                  <>
                    No graph data found for root id <span className="mono">{rootDeviceId.trim()}</span>.
                  </>
                ) : (
                  <>No graph data available in the current snapshot.</>
                )}
              </div>
            </section>
          ) : null}
          <DeviceGraphVisualization nodes={result.nodes} edges={result.edges} rootNodeId={rootDeviceId.trim()} />
          <DeviceGraphTable nodes={result.nodes} edges={result.edges} />
        </>
      ) : null}
    </>
  );
}


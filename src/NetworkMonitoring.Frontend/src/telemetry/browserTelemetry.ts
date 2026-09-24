import { context, propagation, SpanStatusCode, trace, type Tracer } from "@opentelemetry/api";
import { W3CTraceContextPropagator } from "@opentelemetry/core";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { resourceFromAttributes } from "@opentelemetry/resources";
import {
  BatchSpanProcessor,
  ParentBasedSampler,
  TraceIdRatioBasedSampler,
  WebTracerProvider,
} from "@opentelemetry/sdk-trace-web";
import { ATTR_SERVICE_NAME } from "@opentelemetry/semantic-conventions";
import { sanitizeTelemetryAttributes, scrubUrlForTelemetry } from "./hygiene";

let tracer: Tracer | undefined;
let started = false;

/**
 * Resolves the OTLP/HTTP traces endpoint for the browser exporter.
 *
 * @returns Absolute URL, or undefined when browser export is disabled.
 */
export function resolveOtlpTracesUrl(): string | undefined {
  const configured = import.meta.env.VITE_OTEL_OTLP_HTTP_URL?.trim();
  if (!configured) {
    return undefined;
  }
  return configured.endsWith("/v1/traces")
    ? configured
    : `${configured.replace(/\/$/, "")}/v1/traces`;
}

/**
 * Resolves the client-side sample ratio from runtime config (default 0.2).
 *
 * @returns Ratio in [0, 1].
 */
export function resolveSampleRatio(): number {
  const raw = import.meta.env.VITE_OTEL_SAMPLE_RATIO;
  const parsed = raw === undefined || raw === "" ? 0.2 : Number(raw);
  if (!Number.isFinite(parsed)) {
    return 0.2;
  }
  return Math.min(1, Math.max(0, parsed));
}

/**
 * Starts the browser OpenTelemetry pipeline when an OTLP endpoint is configured.
 * Safe to call multiple times; subsequent calls are no-ops.
 */
export function startBrowserTelemetry(): void {
  if (started) {
    return;
  }
  started = true;

  const endpoint = resolveOtlpTracesUrl();
  if (!endpoint) {
    tracer = trace.getTracer("network-monitoring-frontend");
    return;
  }

  propagation.setGlobalPropagator(new W3CTraceContextPropagator());

  const provider = new WebTracerProvider({
    resource: resourceFromAttributes({
      [ATTR_SERVICE_NAME]: "network-monitoring-frontend",
    }),
    sampler: new ParentBasedSampler({
      root: new TraceIdRatioBasedSampler(resolveSampleRatio()),
    }),
    spanProcessors: [
      new BatchSpanProcessor(
        new OTLPTraceExporter({
          url: endpoint,
        })
      ),
    ],
  });

  provider.register();
  tracer = provider.getTracer("network-monitoring-frontend");

  installClientErrorReporting();
  recordDocumentLoadTiming();
}

/**
 * Returns the application tracer, initializing a no-export tracer when needed.
 *
 * @returns OpenTelemetry tracer.
 */
export function getBrowserTracer(): Tracer {
  if (!tracer) {
    tracer = trace.getTracer("network-monitoring-frontend");
  }
  return tracer;
}

/**
 * Records an in-application navigation between local pages.
 *
 * @param page - Target page name.
 */
export function trackInAppNavigation(page: string): void {
  const span = getBrowserTracer().startSpan("ui.navigation", {
    attributes: sanitizeTelemetryAttributes({
      "ui.page": page,
    }),
  });
  span.end();
}

/**
 * Reports a client-side failure as a span, including failures that never reach a service.
 *
 * @param name - Span name.
 * @param attributes - Non-PII attributes describing the failure.
 */
export function reportClientFailure(
  name: string,
  attributes: Record<string, unknown>
): void {
  const span = getBrowserTracer().startSpan(name, {
    attributes: sanitizeTelemetryAttributes(attributes),
  });
  span.setStatus({ code: SpanStatusCode.ERROR, message: name });
  span.end();
}

function installClientErrorReporting(): void {
  window.addEventListener("error", (event) => {
    reportClientFailure("window.error", {
      "exception.type": event.error?.name ?? "Error",
      "exception.message": truncate(String(event.message ?? "unknown"), 200),
      "code.filepath": scrubUrlForTelemetry(String(event.filename ?? "")),
    });
  });

  window.addEventListener("unhandledrejection", (event) => {
    const reason = event.reason;
    reportClientFailure("window.unhandledrejection", {
      "exception.type": reason instanceof Error ? reason.name : "UnhandledRejection",
      "exception.message": truncate(
        reason instanceof Error ? reason.message : String(reason),
        200
      ),
    });
  });
}

function recordDocumentLoadTiming(): void {
  const record = () => {
    const [nav] = performance.getEntriesByType("navigation") as PerformanceNavigationTiming[];
    if (!nav) {
      return;
    }
    const span = getBrowserTracer().startSpan("document.load", {
      startTime: performance.timeOrigin + nav.startTime,
      attributes: sanitizeTelemetryAttributes({
        "browser.page.url": scrubUrlForTelemetry(window.location.href),
        "browser.timing.dom_content_loaded_ms": Math.round(nav.domContentLoadedEventEnd),
        "browser.timing.load_event_ms": Math.round(nav.loadEventEnd),
      }),
    });
    span.end(performance.timeOrigin + nav.loadEventEnd);
  };

  if (document.readyState === "complete") {
    record();
  } else {
    window.addEventListener("load", () => record(), { once: true });
  }
}

function truncate(value: string, max: number): string {
  return value.length <= max ? value : `${value.slice(0, max)}…`;
}

export { context, propagation, trace };

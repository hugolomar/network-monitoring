import { context, propagation, SpanStatusCode, trace } from "@opentelemetry/api";
import { getBrowserTracer, reportClientFailure } from "../telemetry/browserTelemetry";
import { sanitizeTelemetryAttributes, scrubUrlForTelemetry } from "../telemetry/hygiene";

/**
 * Instrumented fetch that propagates W3C trace context and reports client-visible failures.
 *
 * @param input - Request URL or Request object.
 * @param init - Fetch init options.
 * @returns The fetch Response.
 */
export async function tracedFetch(
  input: RequestInfo | URL,
  init?: RequestInit
): Promise<Response> {
  const url =
    typeof input === "string"
      ? input
      : input instanceof URL
        ? input.toString()
        : input.url;
  const method =
    init?.method ??
    (typeof input === "object" && !(input instanceof URL) ? input.method : "GET") ??
    "GET";
  const scrubbedUrl = scrubUrlForTelemetry(url);

  const span = getBrowserTracer().startSpan("http.client", {
    attributes: sanitizeTelemetryAttributes({
      "http.request.method": method,
      "url.full": scrubbedUrl,
    }),
  });

  const headers = new Headers(
    init?.headers ??
      (typeof input === "object" && !(input instanceof URL) ? input.headers : undefined)
  );
  const activeContext = trace.setSpan(context.active(), span);
  propagation.inject(activeContext, headers, {
    set(carrier, key, value) {
      carrier.set(key, String(value));
    },
  });

  try {
    const response = await fetch(input, { ...init, headers });
    span.setAttribute("http.response.status_code", response.status);
    if (!response.ok) {
      span.setStatus({ code: SpanStatusCode.ERROR, message: `HTTP ${response.status}` });
      reportClientFailure("http.client.failed", {
        "http.request.method": method,
        "url.full": scrubbedUrl,
        "http.response.status_code": response.status,
      });
    } else {
      span.setStatus({ code: SpanStatusCode.OK });
    }
    return response;
  } catch (error) {
    const message = error instanceof Error ? error.message : "network error";
    span.setStatus({ code: SpanStatusCode.ERROR, message });
    reportClientFailure("http.client.unreachable", {
      "http.request.method": method,
      "url.full": scrubbedUrl,
      "exception.message": message.slice(0, 200),
    });
    throw error;
  } finally {
    span.end();
  }
}

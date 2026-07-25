import { afterEach, describe, expect, it, vi } from "vitest";
import { W3CTraceContextPropagator } from "@opentelemetry/core";
import { context, propagation, trace } from "@opentelemetry/api";
import {
  AlwaysOnSampler,
  InMemorySpanExporter,
  SimpleSpanProcessor,
  WebTracerProvider,
} from "@opentelemetry/sdk-trace-web";
import {
  FORBIDDEN_TELEMETRY_ATTRIBUTES,
  sanitizeTelemetryAttributes,
  scrubUrlForTelemetry,
} from "../telemetry/hygiene";
import { resolveOtlpTracesUrl, resolveSampleRatio } from "../telemetry/browserTelemetry";

describe("browser telemetry hygiene", () => {
  it("strips query strings and fragments from URLs", () => {
    expect(scrubUrlForTelemetry("https://example.test/devices?mac=AA:BB&token=secret#x")).toBe(
      "https://example.test/devices"
    );
  });

  it("removes forbidden end-user identity attributes", () => {
    const sanitized = sanitizeTelemetryAttributes({
      "ui.page": "inventory",
      "enduser.id": "user-1",
      "user.email": "a@b.c",
      "http.request.header.authorization": "Bearer secret",
      "url.full": "http://localhost/devices?q=1",
    });

    for (const key of FORBIDDEN_TELEMETRY_ATTRIBUTES) {
      expect(sanitized).not.toHaveProperty(key);
    }
    expect(sanitized["ui.page"]).toBe("inventory");
    expect(sanitized["url.full"]).toBe("http://localhost/devices");
  });
});

describe("browser telemetry config", () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it("appends /v1/traces when only the collector base URL is configured", () => {
    vi.stubEnv("VITE_OTEL_OTLP_HTTP_URL", "http://localhost:4318");
    expect(resolveOtlpTracesUrl()).toBe("http://localhost:4318/v1/traces");
  });

  it("clamps sample ratio into [0, 1]", () => {
    vi.stubEnv("VITE_OTEL_SAMPLE_RATIO", "2");
    expect(resolveSampleRatio()).toBe(1);
    vi.stubEnv("VITE_OTEL_SAMPLE_RATIO", "-1");
    expect(resolveSampleRatio()).toBe(0);
  });
});

describe("browser trace propagation", () => {
  it("injects a W3C traceparent header for an active span", () => {
    propagation.setGlobalPropagator(new W3CTraceContextPropagator());
    const provider = new WebTracerProvider({
      sampler: new AlwaysOnSampler(),
      spanProcessors: [new SimpleSpanProcessor(new InMemorySpanExporter())],
    });
    provider.register();
    const tracer = provider.getTracer("test");
    const span = tracer.startSpan("http.client");
    const carrier: Record<string, string> = {};
    const active = trace.setSpan(context.active(), span);
    propagation.inject(active, carrier, {
      set(bag, key, value) {
        bag[key] = String(value);
      },
    });
    span.end();

    expect(carrier.traceparent).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$/);
  });
});

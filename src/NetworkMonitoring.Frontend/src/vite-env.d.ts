/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_BACKEND_BASE_URL?: string;
  readonly VITE_DEV_PROXY_TARGET?: string;
  /** OTLP/HTTP base URL for browser traces (for example http://localhost:4318). */
  readonly VITE_OTEL_OTLP_HTTP_URL?: string;
  /** Client-side sample ratio in [0, 1]. Defaults to 0.2 when unset. */
  readonly VITE_OTEL_SAMPLE_RATIO?: string;
}

/// <reference types="node" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/devices": {
        target: process.env.VITE_DEV_PROXY_TARGET ?? "http://localhost:5090",
        changeOrigin: true,
      },
    },
  },
});

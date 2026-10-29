import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: "127.0.0.1",
    port: 5173,
    strictPort: true,
    proxy: {
      "/api": {
        target: "http://127.0.0.1:5105",
        changeOrigin: true,
      },
      "/realtime-hub": {
        target: "http://127.0.0.1:5105",
        changeOrigin: true,
        ws: true,
      },
    },
  },
  build: {
    outDir: "../backend/DormitoryManagement/wwwroot",
    emptyOutDir: true,
  },
});

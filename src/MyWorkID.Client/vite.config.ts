import { defineConfig } from "vite";
import fs from "fs";
import path from "path";
import child_process from "child_process";
import { env } from "process";
import react from "@vitejs/plugin-react";

const baseFolder =
  env.APPDATA !== undefined && env.APPDATA !== ""
    ? `${env.APPDATA}/ASP.NET/https`
    : `${env.HOME}/.aspnet/https`;

fs.mkdirSync(baseFolder, { recursive: true });

const certificateName = "MyWorkID.Client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  if (
    0 !==
    child_process.spawnSync(
      "dotnet",
      [
        "dev-certs",
        "https",
        "--export-path",
        certFilePath,
        "--format",
        "Pem",
        "--no-password",
      ],
      { stdio: "inherit" }
    ).status
  ) {
    throw new Error("Could not create certificate.");
  }
}

let target: string;

if (env.ASPNETCORE_HTTPS_PORT) {
  target = `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`;
} else if (env.ASPNETCORE_URLS) {
  target = env.ASPNETCORE_URLS.split(";")[0];
} else {
  target = "https://localhost:7266";
}

const targetWebSocket = target
  .replace("https://", "wss://")
  .replace("http://", "wss://");

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      // "@": fileURLToPath(new URL("./src", import.meta.url)),
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    proxy: {
      "/api": {
        target,
        secure: false,
      },
      "/hubs/verifiedId/negotiate": {
        target: target,
        secure: false,
      },
      "/hubs": {
        target: targetWebSocket,
        ws: true,
        secure: false,
      },
    },
    port: 5173,
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath),
    },
  },
});

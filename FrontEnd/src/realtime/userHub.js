import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { getToken, logout } from "../auth/token";

let connection;

const API_ORIGIN =
  (process.env.REACT_APP_API_URL || "").replace(/\/api\/?$/i, "") ||
  window.location.origin; // last resort

export function startUserHub() {
  if (connection) return connection;

  const hubUrl = `${API_ORIGIN}/hubs/user`;
  console.log("[hub] connecting to", hubUrl);

  connection = new HubConnectionBuilder()
    .withUrl(hubUrl, { accessTokenFactory: () => getToken() || "" })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Error)
    .build();

  connection.on("ForceLogout", (p) => {
    console.log("[hub] ForceLogout", p);
    logout(p?.reason || "Signed out.");
  });

  connection.onreconnected(() => console.log("[hub] reconnected"));
  connection.onclose((e) => console.log("[hub] closed", e));

  connection.start()
    .then(() => console.log("[hub] started"))
    .catch(err => console.error("[hub] start failed", err));

  return connection;
}

export async function stopUserHub() {
  if (!connection) return;
  try { await connection.stop(); } catch {}
  connection = null;
}
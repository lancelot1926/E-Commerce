export function getToken() {
  return localStorage.getItem("token");
}

export function getUserIdFromToken() {
  const t = getToken();
  if (!t) return null;
  const parts = t.split(".");
  if (parts.length !== 3) return null;
  try {
    const payload = JSON.parse(atob(parts[1]));
    // JwtTokenGenerator adds sub = user.Id
    return payload.sub ? Number(payload.sub) : null;
  } catch {
    return null;
  }
}


// optional: read "role" claim if your token includes it
export function getRoleFromToken() {
  const t = getToken();
  if (!t) return null;
  const parts = t.split(".");
  if (parts.length !== 3) return null;
  try {
    const json = JSON.parse(atob(parts[1]));
    // common keys: "role", "roles", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    return json.role || json.roles || json["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null;
  } catch { return null; }
}

export function isAdmin() {
  const r = getRoleFromToken();
  return r === "Admin" || (Array.isArray(r) && r.includes("Admin"));
}

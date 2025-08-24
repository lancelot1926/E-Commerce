export function getToken() {return localStorage.getItem("token");}
export const setToken = (t) => localStorage.setItem("token", t);
export const clearToken = () => localStorage.removeItem("token");

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


export const logout = (reason) => {
  clearToken();
  // broadcast to other tabs (optional)
  localStorage.setItem("force-logout", Date.now().toString());
  // pass reason to login page
  const q = reason ? `?reason=${encodeURIComponent(reason)}` : "";
  window.location.assign(`/login${q}`);
};
import { createContext, useContext, useMemo, useState } from "react";

const AuthCtx = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem("token"));

  const login = (t) => {
    localStorage.setItem("token", t);
    setToken(t);                 // <- triggers rerender
  };

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);              // <- triggers rerender
  };

  const value = useMemo(() => ({
    token,
    isLoggedIn: !!token,
    login,
    logout,
  }), [token]);

  return <AuthCtx.Provider value={value}>{children}</AuthCtx.Provider>;
}

export function useAuth() {
  return useContext(AuthCtx);
}

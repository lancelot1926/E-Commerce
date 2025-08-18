import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getRoleFromToken } from "../auth/token";

export default function ProtectedRoute({ children, requireAdmin = false }) {
  const { isLoggedIn } = useAuth();
  if (!isLoggedIn) return <Navigate to="/login" replace />;

  if (requireAdmin) {
    const role = getRoleFromToken();
    if (role !== "Admin") return <Navigate to="/" replace />;
  }
  return children;
}

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";
import { useAuth } from "../auth/AuthContext";
import { getRoleFromToken } from "../auth/token";
import { mergeGuestCartIntoUser } from "../cart/storage";
import { cartMergeGuestIntoServer } from "../cart/client";

export default function Login() {
  const { login } = useAuth();          // ✅ inside component
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState("");
  const navigate = useNavigate();
  const reason = useState(() => new URLSearchParams(navigate).get("reason"), [navigate]);

  const submit = async (e) => {
    e.preventDefault();
    setErr("");
    try {
      const res = await api.post("/auth/login", { email, password });
      const token = typeof res.data === "string" ? res.data : res.data.token;
      login(token);                      // updates context + localStorage
      await cartMergeGuestIntoServer();
      // merge guest → user cart after token is saved
      //mergeGuestCartIntoUser();
      const role = getRoleFromToken();
      navigate("/"); // redirect to home
    } catch (e2) {
      setErr(e2.response?.data?.error || "Login failed");
    }
  };

  return (
    <div className="container py-4" style={{maxWidth: 420}}>
      {reason && <div className="alert alert-warning mb-3">{reason}</div>}
      <h1 className="mb-3">Login</h1>
      {err && <div className="alert alert-danger">{err}</div>}
      <form onSubmit={submit}>
        <div className="mb-3">
          <label className="form-label">Email</label>
          <input className="form-control" type="email" value={email} onChange={e=>setEmail(e.target.value)} required />
        </div>
        <div className="mb-3">
          <label className="form-label">Password</label>
          <input className="form-control" type="password" value={password} onChange={e=>setPassword(e.target.value)} required />
        </div>
        <button className="btn btn-primary w-100">Login</button>
      </form>
    </div>
  );
}

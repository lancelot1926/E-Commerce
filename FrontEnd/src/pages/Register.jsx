import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";

export default function Register() {
  const [form, setForm] = useState({
    name: "", surname: "", email: "", password: "",
    phoneNumber: "", addressLine1: "", city: "", state: "", postalCode: "", country: ""
  });
  const [err, setErr] = useState("");
  const navigate = useNavigate();

  const change = e => setForm(f => ({ ...f, [e.target.name]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setErr("");
    try {
      // Matches RegisterUserRequest.cs fields
      await api.post("/auth/register", form); // <— adjust if your route differs
      navigate("/login");
    } catch (e) {
      const msg = e.response?.data?.error || e.response?.data || e.message || "Register failed";
      setErr(String(msg));
    }
  };

  return (
    <div className="container py-4" style={{maxWidth: 640}}>
      <h1 className="mb-3">Register</h1>
      {err && <div className="alert alert-danger">{err}</div>}
      <form onSubmit={submit} className="row g-3">
        <div className="col-md-6"><label className="form-label">Name</label><input className="form-control" name="name" value={form.name} onChange={change} required /></div>
        <div className="col-md-6"><label className="form-label">Surname</label><input className="form-control" name="surname" value={form.surname} onChange={change} required /></div>
        <div className="col-md-6"><label className="form-label">Email</label><input className="form-control" type="email" name="email" value={form.email} onChange={change} required /></div>
        <div className="col-md-6"><label className="form-label">Phone</label><input className="form-control" name="phoneNumber" value={form.phoneNumber} onChange={change} required /></div>
        <div className="col-md-6"><label className="form-label">Password</label><input className="form-control" type="password" name="password" value={form.password} onChange={change} required /></div>
        <div className="col-12"><label className="form-label">Address</label><input className="form-control" name="addressLine1" value={form.addressLine1} onChange={change} /></div>
        <div className="col-md-4"><label className="form-label">City</label><input className="form-control" name="city" value={form.city} onChange={change} /></div>
        <div className="col-md-4"><label className="form-label">State</label><input className="form-control" name="state" value={form.state} onChange={change} /></div>
        <div className="col-md-4"><label className="form-label">Postal Code</label><input className="form-control" name="postalCode" value={form.postalCode} onChange={change} /></div>
        <div className="col-md-6"><label className="form-label">Country</label><input className="form-control" name="country" value={form.country} onChange={change} /></div>
        <div className="col-12"><button className="btn btn-primary">Create Account</button></div>
      </form>
    </div>
  );
}

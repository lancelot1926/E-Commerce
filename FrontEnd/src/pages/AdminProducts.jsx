import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api/axios";

export default function AdminProducts() {
  const [items, setItems] = useState([]);
  const [q, setQ] = useState("");
  const [err, setErr] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    api.get("/products")
      .then(res => setItems(res.data))
      .catch(e => setErr(e.response?.data?.error || e.message));
  }, []);

  const filtered = q
    ? items.filter(p => (p.name || "").toLowerCase().includes(q.toLowerCase()))
    : items;

  return (
    <div className="container py-4">
      <div className="d-flex align-items-center justify-content-between mb-3">
        <h1 className="m-0">Admin • Products</h1>
        <button className="btn btn-primary" onClick={() => navigate("/admin/products/create")}>
          + Create Product
        </button>
      </div>

      {err && <div className="alert alert-danger">{err}</div>}

      <div className="mb-3">
        <input
          className="form-control"
          placeholder="Search by name…"
          value={q}
          onChange={e => setQ(e.target.value)}
        />
      </div>

      <div className="table-responsive">
        <table className="table align-middle">
          <thead>
            <tr>
              <th>ID</th>
              <th style={{width: 80}}>Image</th>
              <th>Name</th>
              <th>Price</th>
              <th>Stock</th>
              <th>Active</th>
              <th style={{width: 200}}></th>
            </tr>
          </thead>
          <tbody>
            {filtered.map(p => (
              <tr key={p.id}>
                <td>{p.id}</td>
                <td>
                  {p.mainImageUrl
                    ? <img src={p.mainImageUrl} alt="" style={{width:60, height:60, objectFit:"cover"}}/>
                    : <span className="text-muted">—</span>}
                </td>
                <td>{p.name}</td>
                <td>₺{p.price}</td>
                <td>{p.stockQuantity}</td>
                <td>{p.isActive ? "Yes" : "No"}</td>
                <td className="text-end">
                  <Link to={`/product/${p.id}`} className="btn btn-outline-secondary me-2">View</Link>
                  <Link to={`/admin/products/${p.id}/edit`} className="btn btn-primary">Edit</Link>
                </td>
              </tr>
            ))}
            {filtered.length === 0 && (
              <tr><td colSpan="7" className="text-center text-muted py-4">No products</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

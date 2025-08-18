import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";

export default function AdminCreateProduct() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    name: "", description: "", price: 0, stockQuantity: 0,
    sku: "", brand: "", category: "", mainImageUrl: ""
  });
  const [err, setErr] = useState("");
  const [uploading, setUploading] = useState(false);

  const change = (e) => {
    const { name, value } = e.target;
    setForm(f => ({ ...f, [name]: value }));
  };

  const upload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const fd = new FormData();
    fd.append("file", file);
    setUploading(true);
    setErr("");
    try {
      const res = await api.post("/files/image", fd, {
        headers: { "Content-Type": "multipart/form-data" }
      });
      setForm(f => ({ ...f, mainImageUrl: res.data.url }));
    } catch (e2) {
      setErr(e2.response?.data?.error || e2.message || "Upload failed");
    } finally {
      setUploading(false);
    }
  };

  const submit = async (e) => {
    e.preventDefault();
    setErr("");
    try {
      await api.post("/products", {
        name: form.name,
        description: form.description || null,
        price: Number(form.price),
        stockQuantity: Number(form.stockQuantity),
        sku: form.sku || null,
        brand: form.brand || null,
        category: form.category || null,
        mainImageUrl: form.mainImageUrl || null
      });
      alert("Created");
      navigate("/admin/products");
    } catch (e2) {
      setErr(e2.response?.data?.error || e2.message || "Create failed");
    }
  };

  return (
    <div className="container py-4" style={{maxWidth: 820}}>
      <h1 className="mb-3">Create Product</h1>
      {err && <div className="alert alert-danger">{err}</div>}

      <div className="mb-3">
        <label className="form-label">Image preview</label>
        <div className="border rounded d-flex align-items-center justify-content-center p-2" style={{height: 240}}>
          {form.mainImageUrl
            ? <img src={form.mainImageUrl} alt="preview" className="h-100" />
            : <span className="text-muted">No image</span>}
        </div>
      </div>

      <div className="mb-3">
        <label className="form-label">Upload Image</label>
        <input type="file" accept="image/*" className="form-control" onChange={upload} />
        {uploading && <div className="form-text">Uploading…</div>}
      </div>

      <form onSubmit={submit} className="row g-3">
        <div className="col-md-8">
          <label className="form-label">Name</label>
          <input className="form-control" name="name" value={form.name} onChange={change} required />
        </div>
        <div className="col-md-4">
          <label className="form-label">Price (₺)</label>
          <input className="form-control" type="number" step="0.01" name="price" value={form.price} onChange={change} required />
        </div>
        <div className="col-12">
          <label className="form-label">Description</label>
          <textarea className="form-control" rows="4" name="description" value={form.description} onChange={change} />
        </div>
        <div className="col-md-4">
          <label className="form-label">Stock</label>
          <input className="form-control" type="number" name="stockQuantity" value={form.stockQuantity} onChange={change} />
        </div>
        <div className="col-md-4">
          <label className="form-label">SKU</label>
          <input className="form-control" name="sku" value={form.sku} onChange={change} />
        </div>
        <div className="col-md-4">
          <label className="form-label">Brand</label>
          <input className="form-control" name="brand" value={form.brand} onChange={change} />
        </div>
        <div className="col-md-6">
          <label className="form-label">Category</label>
          <input className="form-control" name="category" value={form.category} onChange={change} />
        </div>
        <div className="col-md-6">
          <label className="form-label">Main Image URL</label>
          <input className="form-control" name="mainImageUrl" value={form.mainImageUrl} onChange={change} placeholder="https://..." />
        </div>
        <div className="col-12">
          <button className="btn btn-primary">Create</button>
        </div>
      </form>
    </div>
  );
}

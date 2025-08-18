import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import api from "../api/axios";

export default function AdminEditProduct() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    name: "",
    description: "",
    price: 0,
    stockQuantity: 0,
    sku: "",
    brand: "",
    category: "",
    mainImageUrl: "",
    isActive: true
  });
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [uploading, setUploading] = useState(false);

  // 1) Load current product
  useEffect(() => {
    setLoading(true);
    api.get(`/products/${id}`)
      .then(res => {
        const p = res.data;
        setForm({
          name: p.name ?? "",
          description: p.description ?? "",
          price: p.price ?? 0,
          stockQuantity: p.stockQuantity ?? 0,
          sku: p.sku ?? "",
          brand: p.brand ?? "",
          category: p.category ?? "",
          mainImageUrl: p.mainImageUrl ?? "",
          isActive: p.isActive ?? true
        });
      })
      .catch(e => setErr(e.response?.data?.error || e.message || "Failed to load"))
      .finally(() => setLoading(false));
  }, [id]);

  const change = (e) => {
    const { name, value, type, checked } = e.target;
    setForm(f => ({ ...f, [name]: type === "checkbox" ? checked : value }));
  };

  // ---- IMAGE UPLOAD ----
  const onFileChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const fd = new FormData();
    fd.append("file", file);

    setUploading(true);
    setErr("");
    try {
      // POST /api/files/image  -> { url: "https://..." }
      const res = await api.post("/files/image", fd, {
        headers: { "Content-Type": "multipart/form-data" }
      });
      setForm(f => ({ ...f, mainImageUrl: res.data.url }));
    } catch (error) {
      setErr(error.response?.data?.error || error.message || "Upload failed");
    } finally {
      setUploading(false);
    }
  };

  // 2) Save (PUT)
  const submit = async (e) => {
    e.preventDefault();
    setErr("");
    try {
      // UpdateProductRequest shape matches our form
      await api.put(`/products/${id}`, {
        name: form.name,
        description: form.description,
        price: Number(form.price),
        stockQuantity: Number(form.stockQuantity),
        sku: form.sku || null,
        brand: form.brand || null,
        category: form.category || null,
        mainImageUrl: form.mainImageUrl || null,
        isActive: Boolean(form.isActive)
      });
      alert("Product updated");
      navigate(`/product/${id}`);
    } catch (e) {
      setErr(e.response?.data?.error || e.message || "Update failed");
    }
  };

  if (loading) return <div className="container py-4">Loading…</div>;

   return (
    <div className="container py-4" style={{ maxWidth: 820 }}>
      <h1 className="mb-3">Edit Product #{id}</h1>
      {err && <div className="alert alert-danger">{err}</div>}

      {/* Image Preview */}
      <div className="mb-3">
        <label className="form-label">Current Image</label>
        <div className="border rounded d-flex align-items-center justify-content-center p-2" style={{ height: 240 }}>
          {form.mainImageUrl
            ? <img src={form.mainImageUrl} alt="preview" className="h-100" />
            : <span className="text-muted">No image</span>
          }
        </div>
      </div>

      {/* File input */}
      <div className="mb-3">
        <label className="form-label">Upload New Image</label>
        <input type="file" accept="image/*" className="form-control" onChange={onFileChange} />
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
          <label className="form-label">Main Image URL (auto-filled after upload)</label>
          <input className="form-control" name="mainImageUrl" value={form.mainImageUrl} onChange={change} placeholder="https://..." />
        </div>
        <div className="col-12 form-check mt-2">
          <input className="form-check-input" id="isActive" type="checkbox" name="isActive" checked={form.isActive} onChange={change} />
          <label className="form-check-label" htmlFor="isActive">Active</label>
        </div>
        <div className="col-12">
          <button className="btn btn-primary">Save Changes</button>
        </div>
      </form>
    </div>
  );
}

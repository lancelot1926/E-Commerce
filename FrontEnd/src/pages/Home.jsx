import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/axios";
import { addToCartItem } from "../cart/storage";
import { cartAddOrUpdate } from "../cart/client";


export default function Home() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  useEffect(() => {
    api.get("/products")
      .then(res => setItems(res.data))
      .catch(e => setErr(e.message || "Failed to load"))
      .finally(() => setLoading(false));
  }, []);

  const addToCart = async (p) => {
    await cartAddOrUpdate({ id: p.id, name: p.name, price: p.price }, 1);
  alert("Added to cart");
  };

  if (loading) return <div className="container py-4">Loading…</div>;
  if (err) return <div className="container py-4 text-danger">Error: {err}</div>;

  return (
    <div className="container py-4">
      <h1 className="mb-3">Products</h1>
      <div className="row">
        {items.map(p => (
          <div className="col-md-4" key={p.id}>
            <div className="card mb-3 h-100">
              {p.mainImageUrl && <img src={p.mainImageUrl} className="card-img-top" alt={p.name} />}
              <div className="card-body d-flex flex-column">
                <h5 className="card-title">{p.name}</h5>
                <p className="card-text flex-grow-1">{p.description}</p>
                <div className="d-flex justify-content-between align-items-center">
                  <div className="fw-bold">₺{p.price}</div>
                  <div>
                    <Link to={`/product/${p.id}`} className="btn btn-outline-secondary me-2">Details</Link>
                    <button className="btn btn-primary" onClick={() => addToCart(p)}>Add</button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        ))}
        {items.length === 0 && <div>No products found.</div>}
      </div>
    </div>
  );
}

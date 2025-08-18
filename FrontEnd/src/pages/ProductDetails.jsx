import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import api from "../api/axios";
import { cartAddOrUpdate } from "../cart/client";

export default function ProductDetails() {
  const { id } = useParams();
  const [p, setP] = useState(null);
  const [err, setErr] = useState("");

  useEffect(() => {
    api.get(`/products/${id}`)
      .then(res => setP(res.data))
      .catch(e => setErr(e.message || "Failed to load"));
  }, [id]);

  const addToCart = async () => {
    await cartAddOrUpdate({ id: p.id, name: p.name, price: p.price }, 1);
  alert("Added to cart");
  };

  if (err) return <div className="container py-4 text-danger">Error: {err}</div>;
  if (!p) return <div className="container py-4">Loading…</div>;

  return (
    <div className="container py-4">
      <div className="row">
        <div className="col-md-6">
          {p.mainImageUrl ? (
            <img alt={p.name} src={p.mainImageUrl} className="img-fluid rounded" />
          ) : (
            <div className="bg-light rounded d-flex align-items-center justify-content-center" style={{height: 300}}>
              No image
            </div>
          )}
        </div>
        <div className="col-md-6">
          <h1>{p.name}</h1>
          <div className="lead mb-2">₺{p.price}</div>
          <p>{p.description}</p>
          <button className="btn btn-primary" onClick={addToCart}>Add to Cart</button>
        </div>
      </div>
    </div>
  );
}

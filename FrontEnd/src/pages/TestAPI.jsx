import { useEffect, useState } from "react";
import api from "../api/axios";

export default function TestAPI() {
  const [items, setItems] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    // Adjust path if your controller route is different
    api.get("/products")
      .then(res => setItems(res.data))
      .catch(err => setError(err.message || "Request failed"));
  }, []);

  if (error) return <div className="container py-4">Error: {error}</div>;

  return (
    <div className="container py-4">
      <h1>Products (test)</h1>
      <ul>
        {items.map(p => (
          <li key={p.id}>{p.name} — ₺{p.price}</li>
        ))}
      </ul>
    </div>
  );
}

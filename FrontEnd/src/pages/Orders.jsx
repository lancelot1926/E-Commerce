import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listOrders } from "../orders/client";

export default function Orders() {
  const [orders, setOrders] = useState([]);
  const [err, setErr] = useState("");

  useEffect(() => {
    listOrders().then(setOrders).catch(e => setErr(e.response?.data?.error || e.message));
  }, []);

  const setStatDet=(s)=>{
    switch (Number(s)) {
    case 1: return "Paid";
    case 2: return "Cancelled";
    case 0:
    default: return "OnHold";
  }
  }

  if (err) return <div className="container py-4 text-danger">Error: {err}</div>;

  return (
    <div className="container py-4">
      <h1 className="mb-3">My Orders</h1>
      {orders.length === 0 && <div>No orders yet.</div>}
      <div className="list-group">
        {orders.map(o => (
          <Link key={o.id} to={`/orders/${o.id}`} className="list-group-item list-group-item-action">
            <div className="d-flex justify-content-between">
              <div>Order #{o.id}</div>
              <div className="fw-semibold">₺{o.total}</div>
            </div>
            <div className="small text-muted">Status: {setStatDet(o.status)}</div>
          </Link>
        ))}
      </div>
    </div>
  );
}

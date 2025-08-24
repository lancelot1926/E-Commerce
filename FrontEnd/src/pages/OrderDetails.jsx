import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getOrder } from "../orders/client";

export default function OrderDetails() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [err, setErr] = useState("");
  const customer=order?.customer?? null;

  useEffect(() => {
    getOrder(id).then(setOrder).catch(e => setErr(e.response?.data?.error || e.message));
  }, [id]);


  const setStatDet=()=>{
    if(order.status===1){
      return "Paid";
    }else if(order.status===2){
      return "Cancelled";
    }else if(order.status===0){
      return "OnHold";
    }
  }

  if (err) return <div className="container py-4 text-danger">Error: {err}</div>;
  if (!order) return <div className="container py-4">Loading…</div>;

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="m-0">Order #{order.id}</h1>
        <Link className="btn btn-outline-secondary" to="/orders">Back to Orders</Link>
      </div>
      <div className="mb-2">Status: <strong>{setStatDet()}</strong></div>
      <div className="table-responsive">
        <table className="table align-middle">
          <thead>
            <tr>
              <th>Product</th>
              <th>Unit</th>
              <th>Qty</th>
              <th>Total</th>              
            </tr>
          </thead>
          <tbody>
            {order.items.map(it => (
              <tr key={it.productId}>
                <td>{it.productName}</td>
                <td>₺{it.unitPrice}</td>
                <td>{it.quantity}</td>
                <td>₺{(it.unitPrice * it.quantity).toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr>
              <td colSpan="3" className="text-end fw-bold">Order Total</td>
              <td className="fw-bold">₺{order.total}</td>
            </tr>
          </tfoot>
        </table>
        {order?.customer && (
  <div className="card mb-3">
    <div className="card-header fw-semibold">Customer</div>
    <div className="card-body">
      <div className="mb-1"><strong>Name:</strong> {order.customer.name} {order.customer.surname}</div>
      <div className="mb-1"><strong>Email:</strong> {order.customer.email}</div>
      <div className="mb-1"><strong>Phone:</strong> {order.customer.phoneNumber}</div>
      <div className="mb-1"><strong>Address:</strong> {order.customer.addressLine1}</div>
      {/* … */}
    </div>
  </div>
)}
      </div>
    </div>
  );
}

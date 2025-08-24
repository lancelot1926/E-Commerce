import api from "../api/axios";



export async function checkout() {
  // returns OrderDto
  const { data } = await api.post("/orders/checkout");
  return data;
}

export async function listOrders() {
  const { data } = await api.get("/orders");
  return data; // OrderDto[]
}

export async function getOrder(id) {
  const { data } = await api.get(`/orders/${id}`);
  return data; // OrderDto
}

export async function listOrdersAdmin() {
  const { data } = await api.get("/orders/all"); // Admin-only
  return data; // OrderDto[]
}

export async function setOrderStatus(id, state) {
  const { data } = await api.put(`/orders/${id}/state`, { state });
  return data; // OrderDto
}

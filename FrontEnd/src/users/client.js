import api from "../api/axios";
import { getUserIdFromToken } from "../auth/token";

export async function listUsersAdmin() {
  const { data } = await api.get("/users/userList"); // hits GET /api/users
  return data; // UserDto[]
}

export async function userDetails(id) {
  const { data } = await api.get(`/users/${id}`); // hits GET /api/users/:id
  return data; // UserDto
}

export async function handleBan(id, banned) {
  const { data } = await api.put(`/users/${id}/ban`, { banned });
  return data;
}
// Get { name, surname, email } or ask the user if not logged in / endpoint missing
export async function getBuyerForPayment() {
  try {
    const uid = getUserIdFromToken && getUserIdFromToken();
    if (!uid) throw new Error("no token");
    const u = await userDetails(uid);
    return { name: u.name, surname: u.surname, email: u.email };
  } catch {
    // Fallback prompts just for sandbox testing
    const name = window.prompt("Your name for payment:", "John");
    const surname = window.prompt("Your surname for payment:", "Doe");
    const email = window.prompt("Your email for payment:", "john@example.com");
    if (!name || !surname || !email) throw new Error("Buyer info required");
    return { name, surname, email };
  }
}
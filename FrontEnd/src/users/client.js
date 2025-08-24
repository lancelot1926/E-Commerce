import api from "../api/axios";

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

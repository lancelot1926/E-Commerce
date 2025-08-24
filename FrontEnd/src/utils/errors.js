export function getErrorMessage(err) {
  // Axios error from our API (we send { error: "..." })
  if (err?.response?.data?.error) return err.response.data.error;

  // Axios network/timeouts etc.
  if (err?.response?.status && err?.message) return err.message;

  // Generic JS Error
  if (err?.message) return err.message;

  // Fallback
  return "Unexpected error";
}
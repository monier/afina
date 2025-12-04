export const baseUrl =
  import.meta.env.VITE_API_URL?.replace(/\/$/, "") ??
  "http://localhost:5100/api/v1";

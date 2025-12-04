import type { ApiError } from "./types";
import { baseUrl } from "./config";

export async function apiPost<TRequest, TResponse>(
  endpoint: string,
  req: TRequest,
  signal?: AbortSignal
): Promise<TResponse> {
  const res = await fetch(`${baseUrl}${endpoint}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(req),
    signal,
  });

  if (!res.ok) {
    const error = await res
      .json()
      .catch(() => ({ code: "UNKNOWN_ERROR", message: "An error occurred" }));
    throw error as ApiError;
  }

  return (await res.json()) as TResponse;
}

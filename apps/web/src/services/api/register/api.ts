import type { Request, Response } from "./models";
import { apiPost } from "../shared";

export async function invoke(
  req: Request,
  signal?: AbortSignal
): Promise<Response> {
  return apiPost<Request, Response>("/auth/register", req, signal);
}

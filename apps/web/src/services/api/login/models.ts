import type { ApiError } from "../shared";

export type Request = {
  username: string;
  authHash: string; // hex sha256 of the plaintext password
};

export type Response = {
  token: string;
  refreshToken: string;
  userId: string;
};

export type { ApiError };

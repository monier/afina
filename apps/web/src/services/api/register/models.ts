import type { ApiError } from "../shared";

export type Request = {
  username: string;
  passwordHash: string; // hex sha256 of the plaintext password
};

export type Response = {
  userId: string;
};

export type { ApiError };

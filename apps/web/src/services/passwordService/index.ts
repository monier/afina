export async function hash(input: string): Promise<string> {
  const enc = new TextEncoder();
  const data = enc.encode(input);
  const digest = await crypto.subtle.digest("SHA-256", data);
  const bytes = new Uint8Array(digest);
  return Array.from(bytes)
    .map((b) => b.toString(16).padStart(2, "0"))
    .join("");
}

export type StrengthResult = { isValid: boolean; errors: string[] };

export function validateStrength(password: string): StrengthResult {
  const errors: string[] = [];
  if (password.length < 8) errors.push("PASSWORD_TOO_SHORT");
  // Minimal checks; can be hardened later
  const hasUpper = /[A-Z]/.test(password);
  const hasLower = /[a-z]/.test(password);
  const hasDigit = /\d/.test(password);
  const hasSpecial = /[^A-Za-z0-9]/.test(password);
  if (!(hasUpper && hasLower && (hasDigit || hasSpecial))) {
    // allow either digit or special to pass; mark weak otherwise
    if (!hasDigit && !hasSpecial) errors.push("PASSWORD_VARIETY_WEAK");
  }
  return { isValid: errors.length === 0, errors };
}

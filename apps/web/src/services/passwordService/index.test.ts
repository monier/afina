import { describe, it, expect } from "vitest";
import { hash, validateStrength } from "./index";

describe("passwordService", () => {
  it("hash is deterministic and 64 hex chars", async () => {
    const pwd = "My$ecretP@ssw0rd";
    const h1 = await hash(pwd);
    const h2 = await hash(pwd);
    expect(h1).toEqual(h2);
    expect(h1).toMatch(/^[0-9a-f]{64}$/);
  });

  it("hash differs for different inputs", async () => {
    const h1 = await hash("password-1");
    const h2 = await hash("password-2");
    expect(h1).not.toEqual(h2);
  });

  it("validateStrength enforces minimum length, allows simple variety", () => {
    expect(validateStrength("abc")).toEqual({
      isValid: false,
      errors: ["PASSWORD_TOO_SHORT", "PASSWORD_VARIETY_WEAK"],
    });
    expect(validateStrength("aaaaaaaaaaaa")).toEqual({
      isValid: false,
      errors: ["PASSWORD_VARIETY_WEAK"],
    });
    expect(validateStrength("Abcdefgh!")).toEqual({
      isValid: true,
      errors: [],
    });
    expect(validateStrength("Abcdefgh1")).toEqual({
      isValid: true,
      errors: [],
    });
    expect(validateStrength("Abcdef1!ghijk")).toEqual({
      isValid: true,
      errors: [],
    });
  });
});

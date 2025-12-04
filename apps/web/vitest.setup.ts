import "./src/i18n/config";

// Provide WebCrypto in test environment (Node)
if (!globalThis.crypto?.subtle) {
  // @ts-expect-error - Node crypto module provides webcrypto for testing
  const { webcrypto } = await import("node:crypto");
  globalThis.crypto = webcrypto as Crypto;
}

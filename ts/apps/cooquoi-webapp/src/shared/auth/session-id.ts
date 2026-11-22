import 'server-only';
import { randomBytes } from 'node:crypto';

/** 256 random bits, URL-safe: an opaque, unguessable session id (ADR 0019 §1). */
export function createSessionId(): string {
  return randomBytes(32).toString('base64url');
}

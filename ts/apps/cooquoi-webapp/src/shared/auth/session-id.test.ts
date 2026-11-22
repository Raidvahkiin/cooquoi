import { describe, expect, it } from 'vitest';
import { createSessionId } from './session-id';

describe('createSessionId', () => {
  it('creates a URL-safe id of 256 random bits', () => {
    const id = createSessionId();

    expect(Buffer.from(id, 'base64url')).toHaveLength(32);
    expect(id).toMatch(/^[\w-]{43}$/);
  });

  it('never repeats an id', () => {
    expect(createSessionId()).not.toBe(createSessionId());
  });
});

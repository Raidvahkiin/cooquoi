type StoredCookie = { name: string; value: string };

const cookies = new Map<string, string>();

/**
 * Stands in for the `cookies()` store of `next/headers`, which only exists inside a
 * Next.js request. iron-session reads and writes the session cookie through it.
 */
export const cookieJar = {
  get(name: string): StoredCookie | undefined {
    const value = cookies.get(name);
    return value === undefined ? undefined : { name, value };
  },
  set(name: string, value: string): void {
    if (value) {
      cookies.set(name, value);
    } else {
      cookies.delete(name);
    }
  },
  clear(): void {
    cookies.clear();
  },
};

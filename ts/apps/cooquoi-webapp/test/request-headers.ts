/** Headers that `next/headers` returns in tests (the proxy-forwarded ones a page would see). */
export const requestHeaders = new Headers();

export function clearRequestHeaders(): void {
  for (const name of [...requestHeaders.keys()]) {
    requestHeaders.delete(name);
  }
}

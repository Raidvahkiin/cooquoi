import { getMe } from '@/features/household';

// Spike, phase 1: plain markup; the Astryx shell comes in phase 4.
export default async function AccountPage() {
  const me = await getMe();

  return (
    <main>
      <h1>Signed in as {me.displayName}</h1>
      <p>{me.email}</p>
      <p>
        Language: {me.locale} · Households: {me.householdCount}
      </p>
      <a href="/auth/logout">Sign out</a>
    </main>
  );
}

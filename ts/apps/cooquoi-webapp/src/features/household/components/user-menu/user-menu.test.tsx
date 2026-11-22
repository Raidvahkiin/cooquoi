import { describe, expect, it } from 'vitest';
import fr from '../../../../../messages/fr.json';
import { renderWithProviders } from '../../../../../test/render';
import { UserMenu } from './user-menu';

describe('UserMenu', () => {
  it('shows the signed-in user name', async () => {
    const screen = await renderWithProviders(
      <UserMenu displayName="Linh Tran" />,
    );

    await expect.element(screen.getByText('Linh Tran')).toBeVisible();
  });

  it('links to sign out', async () => {
    const screen = await renderWithProviders(
      <UserMenu displayName="Linh Tran" />,
    );

    await expect
      .element(screen.getByRole('link', { name: 'Sign out' }))
      .toHaveAttribute('href', '/auth/logout');
  });

  it('labels the sign-out link in French', async () => {
    const screen = await renderWithProviders(
      <UserMenu displayName="Linh Tran" />,
      {
        locale: 'fr',
        messages: fr,
      },
    );

    await expect
      .element(screen.getByRole('link', { name: 'Se déconnecter' }))
      .toBeVisible();
  });
});

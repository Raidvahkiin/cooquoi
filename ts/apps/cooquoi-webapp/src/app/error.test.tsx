import { expect, it, vi } from 'vitest';
import fr from '../../messages/fr.json';
import { renderWithProviders } from '../../test/render';
import ErrorPage from './error';

it('shows the French unexpected-error text and a retry that calls reset', async () => {
  const reset = vi.fn();
  const screen = await renderWithProviders(<ErrorPage reset={reset} />, {
    locale: 'fr',
    messages: fr,
  });

  await expect
    .element(screen.getByText('Une erreur est survenue. Réessayez plus tard.'))
    .toBeInTheDocument();
  await screen.getByRole('button', { name: 'Réessayer' }).click();

  expect(reset).toHaveBeenCalledOnce();
});

import { expect, it } from 'vitest';
import { renderWithProviders } from '../../../../test/render';
import fr from '../../../../messages/fr.json';
import { ErrorMessage } from './error-message';

it('shows the French text of a code with its params', async () => {
  const screen = await renderWithProviders(
    <ErrorMessage
      error={{
        code: 'common.version-mismatch',
        params: { current: 3, expected: 2 },
      }}
    />,
    { locale: 'fr', messages: fr },
  );

  await expect
    .element(
      screen.getByText(
        'Cet élément a changé depuis son ouverture : il est à la version 3, et non 2.',
      ),
    )
    .toBeInTheDocument();
  await expect.element(screen.getByText('Erreur')).toBeInTheDocument();
});

it('shows common.unexpected for an unknown code', async () => {
  const screen = await renderWithProviders(
    <ErrorMessage error={{ code: 'billing.nope' }} />,
  );

  await expect
    .element(screen.getByText('Something went wrong. Try again later.'))
    .toBeInTheDocument();
});

import { expect, it } from 'vitest';
import fr from '../../../../../messages/fr.json';
import { renderWithProviders } from '../../../../../test/render';
import type { IngredientSearchViewModel } from '../../models/ingredient-search.view-model';
import { IngredientSearch } from './ingredient-search';

const search: IngredientSearchViewModel = {
  query: 'sa',
  page: 1,
  totalPages: 3,
  totalCount: 41,
  sort: { key: 'name', direction: 'ascending' },
  rows: [
    { id: '1', code: 'salmon', name: 'Saumon', baseUnit: 'g' },
    { id: '2', code: 'salt', name: 'Sel', baseUnit: 'g' },
  ],
};

it('shows the rows with their name, code and base unit label', async () => {
  const screen = await renderWithProviders(
    <IngredientSearch result={search} />,
    { locale: 'fr', messages: fr },
  );

  await expect
    .element(screen.getByRole('heading', { name: 'Foodpedia' }))
    .toBeInTheDocument();
  await expect
    .element(screen.getByRole('row', { name: /Saumon.*salmon.*gramme/ }))
    .toBeInTheDocument();
  await expect
    .element(screen.getByRole('row', { name: /Sel/ }))
    .toBeInTheDocument();
  await expect
    .element(
      screen.getByRole('searchbox', { name: 'Rechercher un ingrédient' }),
    )
    .toHaveValue('sa');
});

it('marks the sorted column and links to the opposite sort', async () => {
  const screen = await renderWithProviders(
    <IngredientSearch result={search} />,
  );

  await expect
    .element(screen.getByRole('columnheader', { name: 'Name' }))
    .toHaveAttribute('aria-sort', 'ascending');
  await expect
    .element(screen.getByRole('columnheader', { name: 'Code' }))
    .toHaveAttribute('aria-sort', 'none');
  await expect
    .element(screen.getByRole('link', { name: 'Name' }))
    .toHaveAttribute('href', '/foodpedia?q=sa&sort=-name');
});

it('links to the next page keeping the query and sort', async () => {
  const screen = await renderWithProviders(
    <IngredientSearch result={search} sort="name" />,
  );

  await expect
    .element(screen.getByRole('link', { name: /next/i }))
    .toHaveAttribute('href', '/foodpedia?q=sa&sort=name&page=2');
});

it('shows an empty state when nothing matches', async () => {
  const screen = await renderWithProviders(
    <IngredientSearch result={{ ...search, rows: [], totalCount: 0 }} />,
  );

  await expect
    .element(screen.getByText('No ingredient found'))
    .toBeInTheDocument();
});

it('shows a failing search as a translated error', async () => {
  const screen = await renderWithProviders(
    <IngredientSearch result={{ error: { code: 'common.unexpected' } }} />,
    { locale: 'fr', messages: fr },
  );

  await expect.element(screen.getByText('Erreur')).toBeInTheDocument();
  await expect.element(screen.getByRole('table')).not.toBeInTheDocument();
});

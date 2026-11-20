export const productsVmMock = [
  {
    id: 'a1b2c3d4-0001-0000-0000-000000000001',
    name: 'Barilla Spaghetti n°5',
    description: 'Classic Italian durum wheat spaghetti, 500g',
    ingredients: ['durum wheat semolina', 'water'],
    offers: [
      {
        id: 'f1e2d3c4-0001-0000-0000-000000000001',
        updatedAt: new Date('2026-03-10T08:00:00Z').toISOString(),
        vendor: 'Carrefour',
        price: { amount: 1.45, currency: 'EUR' },
        unit: { amount: 500, unit: 'g' },
      },
      {
        id: 'f1e2d3c4-0001-0000-0000-000000000002',
        updatedAt: new Date('2026-04-01T10:30:00Z').toISOString(),
        vendor: 'Intermarché',
        price: { amount: 1.29, currency: 'EUR' },
      },
    ],
  },
  {
    id: 'a1b2c3d4-0001-0000-0000-000000000002',
    name: 'Président Unsalted Butter 250g',
    description: 'French unsalted butter, 82% fat',
    ingredients: ['pasteurized cream'],
    offers: [
      {
        id: 'f1e2d3c4-0002-0000-0000-000000000001',
        updatedAt: new Date('2026-04-10T09:00:00Z').toISOString(),
        vendor: 'Leclerc',
        price: { amount: 2.85, currency: 'EUR' },
      },
      {
        id: 'f1e2d3c4-0002-0000-0000-000000000002',
        updatedAt: new Date('2026-04-15T14:00:00Z').toISOString(),
        vendor: 'Monoprix',
        price: { amount: 3.1, currency: 'EUR' },
      },
      {
        id: 'f1e2d3c4-0002-0000-0000-000000000003',
        updatedAt: new Date('2026-04-17T11:15:00Z').toISOString(),
        vendor: 'Biocoop',
        price: { amount: 3.75, currency: 'EUR' },
      },
    ],
  },
  {
    id: 'a1b2c3d4-0001-0000-0000-000000000003',
    name: 'Heinz Ketchup 570g',
    description: null,
    ingredients: ['tomato', 'sugar', 'vinegar', 'salt', 'spices'],
    offers: [
      {
        id: 'f1e2d3c4-0003-0000-0000-000000000001',
        updatedAt: new Date('2026-04-18T07:45:00Z').toISOString(),
        vendor: 'Auchan',
        price: { amount: 3.49, currency: 'EUR' },
      },
    ],
  },
  {
    id: 'a1b2c3d4-0001-0000-0000-000000000004',
    name: 'Lindt Excellence 70% Dark Chocolate',
    description: '100g dark chocolate bar, intense cocoa taste',
    ingredients: ['cocoa mass', 'sugar', 'cocoa butter', 'vanilla extract'],
    offers: [
      {
        id: 'f1e2d3c4-0004-0000-0000-000000000001',
        updatedAt: new Date('2026-02-20T16:00:00Z').toISOString(),
        vendor: 'Carrefour',
        price: { amount: 2.2, currency: 'EUR' },
      },
      {
        id: 'f1e2d3c4-0004-0000-0000-000000000002',
        updatedAt: new Date('2026-03-05T12:00:00Z').toISOString(),
        vendor: 'Monoprix',
        price: { amount: 2.55, currency: 'EUR' },
      },
    ],
  },
];

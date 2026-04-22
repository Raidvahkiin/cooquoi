import { eq } from 'drizzle-orm';
import { getTestSuit } from '../../../../__tests__/setup';
import { offers, products } from '../../../../domain';
import { CreateOrUpdateOfferCommand } from '../create-or-update-offer.command';

describe('CreateOrUpdateOffer feature', () => {
  let testSuit: Awaited<ReturnType<typeof getTestSuit>>;

  beforeAll(async () => {
    testSuit = await getTestSuit();
  });

  afterAll(async () => {
    await testSuit?.app?.close();
  });

  beforeEach(async () => {
    const { resetDb } = testSuit;
    await resetDb();
  });

  it('should create a new offer when product+vendor combination does not exist', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const [product] = await db
      .select()
      .from(products)
      .where(eq(products.name, 'Spicy Pepper'));

    const newVendor = 'VendorZ';

    // act
    const offer = await commandBus.execute(
      new CreateOrUpdateOfferCommand({
        productId: product.id,
        vendor: newVendor,
        price: { amount: '5.00', currency: 'EUR' },
      }),
    );

    // assert
    expect(offer).toMatchObject({
      productId: product.id,
      vendor: newVendor,
      priceAmount: '5.00',
      priceCurrency: 'EUR',
    });

    const rows = await db.select().from(offers).where(eq(offers.id, offer.id));
    expect(rows).toHaveLength(1);
  });

  it('should update price when product+vendor combination already exists', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const [product] = await db
      .select()
      .from(products)
      .where(eq(products.name, 'Super Salmon'));

    // VendorA already has an offer for Super Salmon in seed data (12.99 EUR)

    // act
    const offer = await commandBus.execute(
      new CreateOrUpdateOfferCommand({
        productId: product.id,
        vendor: 'VendorA',
        price: { amount: '19.99', currency: 'USD' },
      }),
    );

    // assert
    expect(offer).toMatchObject({
      productId: product.id,
      vendor: 'VendorA',
      priceAmount: '19.99',
      priceCurrency: 'USD',
    });

    const rows = await db
      .select()
      .from(offers)
      .where(eq(offers.productId, product.id));
    const vendorAOffer = rows.find((o) => o.vendor === 'VendorA');
    expect(vendorAOffer?.priceAmount).toBe('19.99');
    expect(vendorAOffer?.priceCurrency).toBe('USD');

    // other vendors' offers should be unchanged
    expect(rows.find((o) => o.vendor === 'VendorB')?.priceAmount).toBe('9.99');
  });

  it('should throw when product does not exist', async () => {
    // arrange
    const { commandBus } = testSuit;
    const nonExistentProductId = '00000000-0000-0000-0000-000000000000';

    // act & assert
    await expect(
      commandBus.execute(
        new CreateOrUpdateOfferCommand({
          productId: nonExistentProductId,
          vendor: 'VendorA',
          price: { amount: '10.00', currency: 'EUR' },
        }),
      ),
    ).rejects.toMatchObject({
      cause: {
        message: expect.stringContaining('offers_product_id_products_id_fkey'),
      },
    });
  });
});

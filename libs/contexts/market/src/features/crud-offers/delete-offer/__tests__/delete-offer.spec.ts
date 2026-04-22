import { eq } from 'drizzle-orm';
import { getTestSuit } from '../../../../__tests__/setup';
import { offers, products } from '../../../../domain';
import { DeleteOfferCommand } from '../delete-offer.command';

describe('DeleteOffer feature', () => {
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

  it('should delete an offer', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const [product] = await db
      .select()
      .from(products)
      .where(eq(products.name, 'Super Salmon'));
    const [offer] = await db
      .select()
      .from(offers)
      .where(eq(offers.productId, product.id));

    // act
    await commandBus.execute(new DeleteOfferCommand(offer.id));

    // assert
    const rows = await db.select().from(offers).where(eq(offers.id, offer.id));
    expect(rows).toHaveLength(0);
  });

  it('should do nothing if offer not found', async () => {
    // arrange
    const { commandBus, db } = testSuit;
    const nonExistentId = '00000000-0000-0000-0000-000000000000';
    const totalBefore = (await db.select().from(offers)).length;

    // act
    await commandBus.execute(new DeleteOfferCommand(nonExistentId));

    // assert
    const rows = await db.select().from(offers);
    expect(rows).toHaveLength(totalBefore);
  });
});

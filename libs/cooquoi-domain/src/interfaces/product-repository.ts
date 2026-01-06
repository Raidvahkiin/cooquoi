import { Product } from "../entities";

export abstract class ProductRepository {
	abstract save(product: Product): Promise<void>;
	abstract deleteById(id: string): Promise<void>;
	abstract findById(id: string): Promise<Product | null>;
}

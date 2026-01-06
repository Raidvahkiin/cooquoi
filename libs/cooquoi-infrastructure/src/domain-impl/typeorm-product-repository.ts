import { Product, ProductRepository } from "@cooquoi/domain";
import { Injectable } from "@nestjs/common";
import { InjectRepository } from "@nestjs/typeorm";
import { Repository } from "typeorm";
import { ProductMapper } from "../typeorm/mappers";
import { ProductModel } from "../typeorm/models";

@Injectable()
export class TypeOrmProductRepository implements ProductRepository {
	constructor(
		@InjectRepository(ProductModel)
		private readonly productRepository: Repository<ProductModel>,
	) {}

	async save(Product: Product): Promise<void> {
		const model = ProductMapper.fromEntity(Product).toModel();
		await this.productRepository.save(model);
	}

	async deleteById(id: string): Promise<void> {
		await this.productRepository.delete({ id });
	}

	async findById(id: string): Promise<Product | null> {
		const model = await this.productRepository.findOne({ where: { id } });
		if (!model) {
			return null;
		}
		return ProductMapper.fromModel(model).toEntity();
	}
}

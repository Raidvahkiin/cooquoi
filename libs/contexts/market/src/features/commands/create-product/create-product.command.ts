import { Command } from '@nestjs/cqrs';
import { EnableLogging } from '@utils/nestjs/cqrs';
import { Product } from '../../../domain';

interface CreateProductCommandPayload {
  name: string;
  description?: string;
  ingredients: string[];
}

@EnableLogging()
export class CreateProductCommand extends Command<Product> {
  public readonly name: string;
  public readonly description?: string;
  public readonly ingredients: string[];

  constructor(payload: CreateProductCommandPayload) {
    super();
    this.name = payload.name;
    this.description = payload.description;
    this.ingredients = payload.ingredients;
  }
}

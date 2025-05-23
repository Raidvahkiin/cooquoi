import { Entity } from '@core/share';

export class Ingredient extends Entity {
    private _name : string;

    constructor(id: string, name: string) {
        super(id);
        this._name = name;
    }
    
    get name(): string {
        return this._name;
    }
}
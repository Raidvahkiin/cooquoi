import { Entity, EntityProps, EntityUpdateOptions } from "@libs/core";

export type VendorProps = EntityProps & {
    name: string;
};

export class Vendor extends Entity<Vendor>{
    private  _name: string;

    constructor(props: VendorProps, options?: EntityUpdateOptions) {
        super(props, options);
        this._name = props.name;
    }

    get name(): string {
        return this._name;
    }

    public update(
        newState: Partial<Omit<VendorProps, "id" | "createdAt" | "updatedAt">>,
        options: EntityUpdateOptions,
    ): void {
        this.commitUpdate({
            newState,
            datetimeProvider: options.datetimeProvider,
        });
    }
}
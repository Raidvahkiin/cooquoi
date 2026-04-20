import { Input, SearchIcon } from '@utils/react/ui';
import { productsVmMock as products } from './__mocks__/products-vm.mock';
import { ProductList } from './_components/product-list';

export default async function MarketPage() {
  return (
    <div className="max-w-7xl mx-auto flex flex-col gap-4 min-h-screen p-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-neutral-800">Market</h1>
      </div>
      <Input placeholder="Search products..." startComponent={<SearchIcon />} />
      <ProductList products={products} />
    </div>
  );
}

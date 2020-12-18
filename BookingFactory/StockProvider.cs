using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingFactory
{
    public class TestStockProvider : IStockProvider
    {
        PharmacyStock[] stock;

        public TestStockProvider(PharmacyStock[] stock)
        {
            this.stock = stock;
        }

        public async Task<IEnumerable<PharmacyStock>> GetStock(IEnumerable<string> productIds)
        {
            var products = new HashSet<string>(productIds);
            return stock
                .Where(s => s.Stock.Any(i => products.Contains(i.ProductId)))
                .Select(s => new PharmacyStock
                {
                    PharmacyId = s.PharmacyId,
                    Stock = s.Stock.Where(i => products.Contains(i.ProductId)).ToArray()
                });
        }

        public void Dispose() { }
    }
}

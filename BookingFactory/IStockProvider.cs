using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingFactory
{
    public interface IStockProvider : IDisposable
    {
        public Task<IEnumerable<PharmacyStock>> GetStock(IEnumerable<string> productIds);
    }

    public struct PharmacyStock
    {
        public string PharmacyId { get; set; }
        public ProductItem[] Stock { get; set; }
    }
}

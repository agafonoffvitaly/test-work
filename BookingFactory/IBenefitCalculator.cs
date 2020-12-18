using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingFactory
{
    /// <summary>
    /// Калькулятор выгоды
    /// </summary>
    public interface IBenefitCalculator
    {
        Task<decimal> CaluclateBenefit(int pharmacyId, decimal productPrice);
    }

    public class TestBenefitCalculator : IBenefitCalculator
    {
        Dictionary<int, decimal> pharmacyCommissions;

        public TestBenefitCalculator(Dictionary<int, decimal> pharmacyCommissions) => this.pharmacyCommissions = pharmacyCommissions;

        public Task<decimal> CaluclateBenefit(int pharmacyId, decimal productPrice) => Task.FromResult(CalculateBeneift(pharmacyId, productPrice));

        //Расчет выгоды
        decimal CalculateBeneift(int pharmacyId, decimal productPrice)
            => pharmacyCommissions.TryGetValue(pharmacyId, out var commission)
            ? productPrice * commission
            : 0;
    }
}

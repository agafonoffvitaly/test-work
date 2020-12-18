using BookingFactory;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System;

public class Tests
{
    PharmacyStock[] testStock = new PharmacyStock[]
        {
            new PharmacyStock { PharmacyId = Pharmacies.Apteka911,
                Stock = new []
                {
                    Products.Nurofen().Quantity(20).Price(100),
                    Products.Butirat().Quantity(2).Price(2500)
                }
            },
            new PharmacyStock { PharmacyId = Pharmacies.ANC,
                Stock = new []
                {
                    Products.Nurofen().Quantity(3).Price(80)
                }
            },
            new PharmacyStock { PharmacyId = Pharmacies.Romashka,
                Stock = new []
                {
                    Products.Nurofen().Quantity(20).Price(120),
                    Products.Butirat().Quantity(5).Price(2800)
                }
            }
        };

    [Fact]
    public Task Should_find_cheapest_nurofen()
        => RunTest(
            Products.Nurofen().Quantity(1),
            new Booking { PharmacyId = Pharmacies.ANC, Items = new[] { Products.Nurofen().Quantity(1).Price(80) } });

    [Fact]
    public Task Should_find_all_available_nurofen_for_big_demand()
        => RunTest(
            Products.Nurofen().Quantity(10000),
            new Booking { PharmacyId = Pharmacies.ANC,       Items = new[] { Products.Nurofen().Quantity(3).Price(80) } },
            new Booking { PharmacyId = Pharmacies.Apteka911, Items = new[] { Products.Nurofen().Quantity(20).Price(100) } },
            new Booking { PharmacyId = Pharmacies.Romashka,  Items = new[] { Products.Nurofen().Quantity(20).Price(120) } });

    [Fact]
    public Task Should_work_with_several_products()
        => RunTest(
            new[]
            {
                Products.Nurofen().Quantity(5),
                Products.Butirat().Quantity(5)
            },
            new Booking
            {
                PharmacyId = Pharmacies.ANC,
                Items = new[] 
                {
                    Products.Nurofen().Quantity(3).Price(80)
                }
            },
            new Booking
            {
                PharmacyId = Pharmacies.Apteka911,
                Items = new[]
                {
                    Products.Nurofen().Quantity(2).Price(100),
                    Products.Butirat().Quantity(2).Price(2500)
                }
            },
            new Booking
            {
                PharmacyId = Pharmacies.Romashka,
                Items = new[]
                {
                    Products.Butirat().Quantity(3).Price(2800)
                }
            });

    [Fact]
    public Task Should_return_empty_bookings_for_empty_demand()
    => RunTest(Array.Empty<ProductItem>());

    [Fact]
    public Task Should_return_empty_bookings_for_not_found_product()
        => RunTest(Products.Proktozan().Quantity(1));

    Task RunTest(ProductItem demand, params Booking[] expectedBookings) => RunTest(new[] { demand }, expectedBookings);

    async Task RunTest(ProductItem[] demand, params Booking[] expectedBookings)
    {
        var stockProvider = new TestStockProvider(testStock);
        var booker = new BookingFactory.BookingFactory(stockProvider);
        var bookings = await booker.Book(demand);

        Serialize(bookings).ShouldBe(Serialize(expectedBookings));
    }

    string Serialize(IEnumerable<Booking> bookings)
    {
        var ordered = bookings
            .OrderBy(b => b.PharmacyId)
            .Select(b => new Booking
            {
                PharmacyId = b.PharmacyId,
                Items = b.Items.OrderBy(i => i.ProductId).ThenBy(i => i.Quantity).ThenBy(i => i.Price).ToArray()
            });
        return JsonConvert.SerializeObject(ordered);
    }
}

public static class Products
{
    public static ProductItem Nurofen() => new ProductItem { ProductId = "Nurofen" };
    public static ProductItem Butirat() => new ProductItem { ProductId = "Butirat" };
    public static ProductItem Proktozan() => new ProductItem { ProductId = "Proktozan" };
}

public static class Pharmacies
{
    public const string Apteka911 = nameof(Apteka911);
    public const string ANC = nameof(ANC);
    public const string Romashka = nameof(Romashka);
}

public static class ProductItemExtensions
{
    public static ProductItem Price(this ProductItem p, decimal price) => new ProductItem { ProductId = p.ProductId, Quantity = p.Quantity, Price = price };
    public static ProductItem Quantity(this ProductItem p, decimal qt) => new ProductItem { ProductId = p.ProductId, Price = p.Price, Quantity = qt };
}
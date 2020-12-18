using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingFactory
{
    public class BookingFactory : IDisposable
    {
        IStockProvider stockProvider;

        public BookingFactory(IStockProvider stockProvider) => this.stockProvider = stockProvider;

        public Task<IEnumerable<Booking>> Book(IEnumerable<ProductItem> demand)
        {
            //TODO: should return prosed bookings for incoming demand, resulting bookings should cover as much as possible of the demand and be as cheap as possible

            //получаем аптеки с заданым в критерии продукте
            var listStocks = stockProvider.GetStock(demand.Select(p => p.ProductId).AsEnumerable()).GetAwaiter().GetResult();

            //список заказов
            var listBooking = new List<Booking>();

            //Проходя по критериям продукта формируем заказы из выбранных аптек
            foreach (var productFromDemand in demand)
            {
                //сортируем аптеки по цене по возрастанию
                var stocksOrd = from pharmacyStocks in listStocks
                                from stocks in pharmacyStocks.Stock
                                orderby stocks.Price
                                where stocks.ProductId == productFromDemand.ProductId
                                select new
                                {
                                    pharmacyStocks.PharmacyId,
                                    Product = pharmacyStocks.Stock.Where(i => i.ProductId == productFromDemand.ProductId)
                                };

                //количество товара нужно получить из аптек для заказов
                decimal coutProd = productFromDemand.Quantity;

                //Получаем аптеку с самой низкой ценой
                foreach (var stock in stocksOrd)
                {
                    ProductItem product;
                    
                    //если нужного товара заказе меньше чем остаток в аптеке
                    if (coutProd <= stock.Product.FirstOrDefault().Quantity)
                    {
                        product = new ProductItem
                        {
                            ProductId = productFromDemand.ProductId,
                            Price = stock.Product.First().Price,
                            Quantity = coutProd
                        };

                        CreatingBooking(listBooking, stock.PharmacyId, product);
                        break;
                    }
                    else if (coutProd > stock.Product.FirstOrDefault().Quantity)
                    { //если нужное кол-во товара больше чем есть в аптеке
                        //уменьшаем количесво нужного товара на количество товаров которое есть в аптеке
                        coutProd = coutProd - stock.Product.FirstOrDefault().Quantity;

                        product = new ProductItem
                        {
                            ProductId = productFromDemand.ProductId,
                            Price = stock.Product.First().Price,
                            Quantity = stock.Product.FirstOrDefault().Quantity
                        };

                        CreatingBooking(listBooking, stock.PharmacyId, product);
                    }
                }
            }
            
            return Task.FromResult(listBooking.AsEnumerable());
        }

        /// <summary>
        ///  Проверка на существование заказа в списке заказов,
        ///  Создание нового заказа или добавления продукта в существующий заказ
        /// </summary>
        /// <param name="listBookings"></param>
        /// <param name="PharmacyId"></param>
        /// <param name="product"></param>
        private void CreatingBooking(List<Booking> listBookings, string pharmacyId, ProductItem product)
        {
            //проверяем или есть уже аптека списке заказов
            if (listBookings.Where(b => b.PharmacyId == pharmacyId).Count() > 0)
            { //если есть такой заказ
                Booking existBooking = listBookings.Where(b => b.PharmacyId == pharmacyId).FirstOrDefault();
                
                //Удаляем найденный заказ из списка заказов
                listBookings.Remove(existBooking);

                //добавляем в существующий заказ продукт
                existBooking.Items = existBooking.Items.Append(product).ToArray();
                
                //добавляем модифицированный заказ в список заказов
                listBookings.Add(existBooking);

            }
            else
            {//если нет создаем заказ и добавляем в список заказов
                Booking booking = new Booking { PharmacyId = pharmacyId, Items = new[] { product } };
                listBookings.Add(booking);
            }
        }

        public void Dispose() => stockProvider.Dispose();
    }

    public struct Booking
    {
        public string PharmacyId { get; set; }
        public ProductItem[] Items { get; set; }
    }
}

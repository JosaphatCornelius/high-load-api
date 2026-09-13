namespace high_load_api.Models.Database
{
    public class CartItemsModel
    {
        public long ID { get; set; }
        public int Quantity { get; set; }
        public decimal PriceWhenAdded { get; set; }
        public long CartID { get; set; }
        public required CartsModel Cart { get; set;  }
        public long ProductID { get; set; }
        public required ProductsModel Product { get; set; }
    }
}

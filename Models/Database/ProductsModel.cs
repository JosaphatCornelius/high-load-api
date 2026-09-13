namespace high_load_api.Models.Database
{
    public class ProductsModel
    {
        public long ID { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public List<CategoriesModel> Categories { get; set; } = [];
        public List<CartItemsModel> CartItems { get; set; } = [];
    }
}

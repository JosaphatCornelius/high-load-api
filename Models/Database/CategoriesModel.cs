namespace high_load_api.Models.Database
{
    public class CategoriesModel
    {
        public long ID { get; set; }
        public required string Name { get; set; }

        public List<ProductsModel> Product { get; set; } = [];
    }
}

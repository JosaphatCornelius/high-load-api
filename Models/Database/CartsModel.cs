namespace high_load_api.Models.Database
{
    public class CartsModel
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public required UsersModel User { get; set; }
        public List<CartItemsModel> CartItems { get; set; } = [];
    }
}

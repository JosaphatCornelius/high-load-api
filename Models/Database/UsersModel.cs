namespace high_load_api.Models.Database
{
    public class UsersModel
    {
        public long ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public long CartID { get; set; }
        public required CartsModel Cart { get; set; }
        public List<OrdersModel> Orders { get; set; } = [];
    }
}

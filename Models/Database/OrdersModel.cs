namespace high_load_api.Models.Database
{
    public class OrdersModel
    {
        public long ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public long UserID { get; set; }
        public required UsersModel User { get; set; }
        public List<PaymentsModel> Payments = [];
    }
}

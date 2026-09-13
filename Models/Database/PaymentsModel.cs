namespace high_load_api.Models.Database
{
    public class PaymentsModel
    {
        public long ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PaidAt { get; set; }
        public long OrderID { get; set; }
        public required OrdersModel Order { get; set; }
    }
}

namespace high_load_api.Models.DTO
{
    public class UsersDTO
    {
        public long ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}

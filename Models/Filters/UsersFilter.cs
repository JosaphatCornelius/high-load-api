namespace high_load_api.Models.Filters
{
    public class UsersFilter
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

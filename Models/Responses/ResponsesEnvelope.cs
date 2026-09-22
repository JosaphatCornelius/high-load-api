namespace high_load_api.Models.Responses
{
    public class ResponsesEnvelope<T>
    {
        public string? Status { get; set; }
        public int Code { get; set; }
        public string? Message { get; set; }
        public int Count { get; set; } = 0;
        public IEnumerable<T> Data { get; set; } = [];
    }
}

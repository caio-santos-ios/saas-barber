namespace api_barber.src.Requests
{
    public class ResponseApi<T>
    {
        public ResponseApi(T? data, int status, string message)
        {
            Data = data;
            Status = status;
            Message = message;
        }
        public int Status { get; set; } = 200;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}


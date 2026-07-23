namespace BuildingBlocks.Results
{
    public class ServiceResult<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }

        public int HttpStatusCode { get; set; } = (int)System.Net.HttpStatusCode.OK;

        public T Data { get; set; }

        public List<string> Errors { get; set; } = new();



        public ServiceResult() { }

        public ServiceResult(T data, string message = "Completed Successfully", int statusCode = 200)
        {
            Succeeded = true;
            Message = message;
            Data = data;
            HttpStatusCode = statusCode;
        }

        public ServiceResult(string message, bool succeeded = true, int statusCode = 200)
        {
            Succeeded = succeeded;
            Message = message;
            Data = default;
            HttpStatusCode = statusCode;
        }
    }
}
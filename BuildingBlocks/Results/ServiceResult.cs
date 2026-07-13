using System.Net;

namespace BuildingBlocks.Results
{

    public class ServiceResult<T>
    {
        public ServiceResult() { }
        public ServiceResult(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
        }

        public ServiceResult(string message)
        {
            Succeeded = true;
            Message = message;
        }

        public ServiceResult(string message, bool succeeded)
        {
            Succeeded = succeeded;
            Message = message;
        }
        public HttpStatusCode HttpStatusCode { get; set; }
        public List<string> Errors { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }


    }

}

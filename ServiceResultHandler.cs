
namespace CreationMax.Domain.Base
{
    public class ServiceResultHandler

    {
        public ServiceResult<T> Deleted<T>(string Message = null)
        {
            return new ServiceResult<T>()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = Message == null ? "Deleted Sucessfully" : Message
            };
        }
        public ServiceResult<T> Forbidden<T>(string Message = null)
        {
            return new ServiceResult<T>()
            {
                HttpStatusCode = System.Net.HttpStatusCode.Forbidden,
                Succeeded = false,
                Message = Message == null ? "Access denied." : Message
            };
        }
        public ServiceResult<T> Success<T>(T entity)
        {
            return new ServiceResult<T>()
            {
                Data = entity,
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Completed SuccessFully"
            };

        }
        public ServiceResult<T> Unauthorized<T>()
        {
            return new ServiceResult<T>()
            {
                HttpStatusCode = System.Net.HttpStatusCode.Unauthorized,
                Succeeded = true,
                Message = "Unauthorized"
            };
        }
        public ServiceResult<T> BadRequest<T>(string Message = null)
        {
            return new ServiceResult<T>()
            {
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = Message == null ? "BadRequest" : Message
            };

        }
        public ServiceResult<T> UnProcessableEntity<T>(string Message = null)
        {
            return new ServiceResult<T>()
            {
                HttpStatusCode = System.Net.HttpStatusCode.UnprocessableEntity,
                Succeeded = false,
                Message = Message == null ? "UnProcessable" : Message
            };

        }
        public ServiceResult<T> NotFound<T>(string Message = null)
        {
            return new ServiceResult<T>()
            {
                HttpStatusCode = System.Net.HttpStatusCode.NotFound,
                Succeeded = false,
                Message = Message == null ? "NotFound" : Message
            };
        }
        public ServiceResult<T> Created<T>(T entity)
        {
            return new ServiceResult<T>()
            {
                Data = entity,
                HttpStatusCode = System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = "Created Sucessfully"
            };
        }


    }
}

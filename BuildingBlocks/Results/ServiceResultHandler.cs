using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Results
{
    public class ServiceResultHandler
    {
        // =========================================================
        // 1. SUCCESS RESPONSES (200 OK & 201 Created)
        // =========================================================

        public ServiceResult<T> Success<T>(T entity, string message = "Completed Successfully")
        {
            return new ServiceResult<T>
            {
                Succeeded = true,
                Message = message,
                Data = entity,
                HttpStatusCode = StatusCodes.Status200OK
            };
        }

        public ServiceResult<object> Success(string message)
        {
            return new ServiceResult<object>
            {
                Succeeded = true,
                Message = message,
                Data = null,
                HttpStatusCode = StatusCodes.Status200OK
            };
        }

        public ServiceResult<T> Created<T>(T entity, string message = "Created Successfully")
        {
            return new ServiceResult<T>
            {
                Succeeded = true,
                Message = message,
                Data = entity,
                HttpStatusCode = StatusCodes.Status201Created
            };
        }

        public ServiceResult<T> Deleted<T>(string message = "Deleted Successfully")
        {
            return new ServiceResult<T>
            {
                Succeeded = true,
                Message = message,
                Data = default,
                HttpStatusCode = StatusCodes.Status200OK
            };
        }

        // =========================================================
        // 2. AUTHENTICATION & LOGIN SPECIFIC RESPONSES
        // =========================================================

        public ServiceResult<T> Unauthorized<T>(string message = "User account not found or invalid credentials")
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                Errors = new List<string> { message },
                HttpStatusCode = StatusCodes.Status401Unauthorized
            };
        }

        public ServiceResult<T> AccountInactive<T>(string message = "Account is inactive or email unverified")
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                Errors = new List<string> { message },
                HttpStatusCode = StatusCodes.Status402PaymentRequired
            };
        }

        // =========================================================
        // 3. ERROR RESPONSES (400, 403, 404, 422)
        // =========================================================

        public ServiceResult<T> BadRequest<T>(string message = "Bad Request", List<string> errors = null)
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                HttpStatusCode = StatusCodes.Status400BadRequest,
                Errors = errors ?? new List<string> { message }
            };
        }

        public ServiceResult<T> Forbidden<T>(string message = "Access denied")
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                Errors = new List<string> { message },
                HttpStatusCode = StatusCodes.Status403Forbidden
            };
        }

        public ServiceResult<T> NotFound<T>(string message = "Resource not found")
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                Errors = new List<string> { message },
                HttpStatusCode = StatusCodes.Status404NotFound
            };
        }

        public ServiceResult<T> UnProcessableEntity<T>(string message = "Unprocessable Entity", List<string> errors = null)
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                HttpStatusCode = StatusCodes.Status422UnprocessableEntity,
                Errors = errors ?? new List<string> { message }
            };
        }

        // =========================================================
        // 4. SERVER ERROR (500)
        // =========================================================

        public ServiceResult<T> InternalServerError<T>(string message = "Internal Server Error")
        {
            return new ServiceResult<T>
            {
                Succeeded = false,
                Message = message,
                Data = default,
                Errors = new List<string> { message },
                HttpStatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
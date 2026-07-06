using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Results
{
   
        public class ValidationResultModel
        {
            public List<ValidationError> Errors { get; set; }

            public ValidationResultModel()
            {
                Errors = new List<ValidationError>();
            }

            
            public ValidationResultModel(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
            {
                Errors = failures.Select(f => new ValidationError
                {
                    Property = f.PropertyName,
                    Message = f.ErrorMessage,
                    AttemptedValue = f.AttemptedValue
                }).ToList();
            }
        }

        public class ValidationError
        {
            public string Property { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public object? AttemptedValue { get; set; }
        }
    
}

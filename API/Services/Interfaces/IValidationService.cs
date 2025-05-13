using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services.Interfaces
{
    public interface IValidationService<T>
    {
        Task<ValidationResult> ValidateAsync(T dto);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(List<string> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }
}
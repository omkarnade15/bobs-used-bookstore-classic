using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Helpers
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is not IFormFile file) return base.IsValid(value);

            return file.Length <= _maxFileSize;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot exceed {_maxFileSize.ToStorageSize()}";
        }
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Web.Helpers
{
    public class ImageTypesAttribute : ValidationAttribute
    {
        private readonly string[] _imageTypes;

        public ImageTypesAttribute(string[] imageTypes)
        {
            _imageTypes = imageTypes;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is not IFormFile file) return base.IsValid(value);

            var extension = Path.GetExtension(file.FileName);

            return _imageTypes.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a PNG or JPG image.";
        }
    }
}

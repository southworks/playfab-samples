using Microsoft.AspNetCore.Http;

namespace FantasySoccer.Models
{
    public class FileUpload
    {
        public IFormFile file { get; set; }
    }
}

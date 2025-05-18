using System.ComponentModel.DataAnnotations;

namespace Web.Models.Request
{
    public class EmailRequest
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Email { get; set; }
    }
}

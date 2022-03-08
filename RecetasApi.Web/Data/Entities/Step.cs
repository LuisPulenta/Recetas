using System.ComponentModel.DataAnnotations;

namespace RecetasApi.Web.Data.Entities
{
    public class Step
    {
        public int Id { get; set; }

        [Display(Name = "Paso")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }
        public Recipe Recipe { get; set; }
    }
}

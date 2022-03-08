using System.ComponentModel.DataAnnotations;

namespace RecetasApi.Web.Data.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }

        [Display(Name = "Ingrediente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }
        public Recipe Recipe { get; set; }
    }
}

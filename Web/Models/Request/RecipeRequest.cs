using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Models.Request
{
    public class RecipeRequest
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }

        public string UserId { get; set; }

        public ICollection<string> Ingredients { get; set; }

        public ICollection<StepRequest> Steps { get; set; }
        
        public byte[] Image { get; set; }
    }
}

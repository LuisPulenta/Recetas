using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Data.Entities
{
    public class Recipe
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }

        public User User { get; set; }

        public ICollection<Ingredient> Ingredients { get; set; }

        public ICollection<Step> Steps { get; set; }

        public string Photo { get; set; }

        [Display(Name = "Foto")]
        public string PhotoFullPath => string.IsNullOrEmpty(Photo)
                     ? "https://keypress.serveftp.net/RecetasApi/images/recipes/noimage.png"
            : $"https://keypress.serveftp.net/RecetasApi{Photo.Substring(1)}";


    }
}

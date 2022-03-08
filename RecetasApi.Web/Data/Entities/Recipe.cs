using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecetasApi.Web.Data.Entities
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

        public string ImageId { get; set; }

        [Display(Name = "Foto")]
        public string ImageFullPath => string.IsNullOrEmpty(ImageId)
                     ? "http://keypress.serveftp.net:99/Images/nouser.png"
            : $"http://keypress.serveftp.net:99{ImageId.Substring(1)}";


    }
}

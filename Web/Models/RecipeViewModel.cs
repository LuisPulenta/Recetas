using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.Models.Request;

namespace Web.Models
{
    public class RecipeViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public string UserId { get; set; }
        
        public string UserName { get; set; }

        public ICollection<string> Ingredients { get; set; }

        public ICollection<StepRequest> Steps { get; set; }

        public string Photo { get; set; }

        [Display(Name = "Foto")]
        public string PhotoFullPath => string.IsNullOrEmpty(Photo)
                     ? "https://keypress.serveftp.net/RecetasApi/images/recipes/noimage.png"
            : $"https://keypress.serveftp.net/RecetasApi{Photo.Substring(1)}";

    }
}

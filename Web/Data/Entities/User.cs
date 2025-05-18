using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Web.Data.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "Nombre")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; }

        [Display(Name = "Apellido")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; }

        [Display(Name = "Documento")]
        [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Document { get; set; }

        [Display(Name = "Foto")]
        public string Photo { get; set; }


        [Display(Name = "Foto")]
        public string PhotoFullPath => string.IsNullOrEmpty(Photo)
                    ? "https://keypress.serveftp.net/RecetasApi/images/users/nouser.png"
           : $"https://keypress.serveftp.net/RecetasApi{Photo.Substring(1)}";

        [Display(Name = "Tipo de usuario")]
        public UserType UserType { get; set; }

        [Display(Name = "Usuario")]
        public string FullName => $"{FirstName} {LastName}";
    }
}

using System.ComponentModel.DataAnnotations;

namespace RecetasApi.Web.Models.Request
{
    public class UserRequest
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Email { get; set; }

        [Display(Name = "Módulo")]
        [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Modulo { get; set; }

        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; }

        [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Document { get; set; }

        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        public string Address1 { get; set; }

        public float Latitude1 { get; set; }
        public float Longitude1 { get; set; }
        public string Street1 { get; set; }
        public string AdministrativeArea1 { get; set; }
        public string Country1 { get; set; }
        public string IsoCountryCode1 { get; set; }
        public string Locality1 { get; set; }
        public string SubAdministrativeArea1 { get; set; }
        public string SubLocality1 { get; set; }

        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        public string Address2 { get; set; }

        public float Latitude2 { get; set; }
        public float Longitude2 { get; set; }
        public string Street2 { get; set; }
        public string AdministrativeArea2 { get; set; }
        public string Country2 { get; set; }
        public string IsoCountryCode2 { get; set; }
        public string Locality2 { get; set; }
        public string SubAdministrativeArea2 { get; set; }
        public string SubLocality2 { get; set; }

        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        public string Address3 { get; set; }

        public float Latitude3 { get; set; }
        public float Longitude3 { get; set; }
        public string Street3 { get; set; }
        public string AdministrativeArea3 { get; set; }
        public string Country3 { get; set; }
        public string IsoCountryCode3 { get; set; }
        public string Locality3 { get; set; }
        public string SubAdministrativeArea3 { get; set; }
        public string SubLocality3 { get; set; }

        [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string PhoneNumber { get; set; }

        public byte[] Image { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Timely.Models
{
    // Datos que la persona puede cambiar de su propio perfil.
    // Adrede NO incluye Rol ni Contrasena: así nadie puede cambiarse el rol enviando campos extra en el POST.
    public class EditarPerfilViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El usuario no puede superar los 50 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public string Fecha { get; set; } = string.Empty;
    }

    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "Escribe tu contraseña actual")]
        [DataType(DataType.Password)]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe la nueva contraseña")]
        [DataType(DataType.Password)]
        public string ContrasenaNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la nueva contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(ContrasenaNueva), ErrorMessage = "Las contraseñas no coinciden")]
        public string Confirmacion { get; set; } = string.Empty;
    }

    // Modelo de la vista Mi Perfil: datos de solo lectura + los dos formularios
    public class MiPerfilViewModel
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;

        public EditarPerfilViewModel Edicion { get; set; } = new EditarPerfilViewModel();
        public CambiarContrasenaViewModel Contrasena { get; set; } = new CambiarContrasenaViewModel();

        // Cuando un POST falla, la vista se vuelve a abrir con el formulario correspondiente visible
        public bool EditandoDatos { get; set; }
        public bool CambiandoContrasena { get; set; }
    }
}
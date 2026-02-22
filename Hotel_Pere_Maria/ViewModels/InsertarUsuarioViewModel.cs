using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Microsoft.Win32;

namespace Hotel_Pere_Maria.ViewModels
{
    public class InsertarUsuarioViewModel : BaseViewModel
    {
        // ==========================================
        // CAMPOS PRIVADOS
        // ==========================================
        private Usuario? _usuarioEdicion;
        private bool _esEdicion = false;

        private string _nombre = "";
        private string _apellido = "";
        private string _dni = "";
        private string _email = "";
        private string _ciudad = "";
        private string _rol = "client";
        private DateTime? _fechaNacimiento;
        private string _genero = "Other";
        private int _ciudadIndex = -1;
        private int _rolIndex = 0;
        private bool _esHombre;
        private bool _esMujer;
        private bool _esOtro = true;
        private bool _adminVisible = true;
        private string? _imagenPath;
        private BitmapImage? _imagenPreview;

        // ==========================================
        // PROPIEDADES PÚBLICAS
        // ==========================================
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); }
        }

        public string DNI
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public int CiudadIndex
        {
            get => _ciudadIndex;
            set { _ciudadIndex = value; OnPropertyChanged(); }
        }

        public int RolIndex
        {
            get => _rolIndex;
            set { _rolIndex = value; OnPropertyChanged(); }
        }

        public DateTime? FechaNacimiento
        {
            get => _fechaNacimiento;
            set { _fechaNacimiento = value; OnPropertyChanged(); }
        }

        public bool EsHombre
        {
            get => _esHombre;
            set { _esHombre = value; OnPropertyChanged(); }
        }

        public bool EsMujer
        {
            get => _esMujer;
            set { _esMujer = value; OnPropertyChanged(); }
        }

        public bool EsOtro
        {
            get => _esOtro;
            set { _esOtro = value; OnPropertyChanged(); }
        }

        public bool EsEdicion => _esEdicion;

        // Controla si la opción admin se muestra
        public bool AdminVisible
        {
            get => _adminVisible;
            set { _adminVisible = value; OnPropertyChanged(); }
        }

        public string? ImagenPath
        {
            get => _imagenPath;
            set { _imagenPath = value; OnPropertyChanged(); }
        }

        public BitmapImage? ImagenPreview
        {
            get => _imagenPreview;
            set { _imagenPreview = value; OnPropertyChanged(); }
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand SeleccionarImagenCommand { get; }

        // Evento para que el code-behind cierre la ventana con DialogResult
        public event Action<bool>? RequestClose;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public InsertarUsuarioViewModel(Usuario? usuario = null)
        {
            _usuarioEdicion = usuario;

            GuardarCommand = new RelayCommand(async () => await ExecuteGuardar());
            CancelarCommand = new RelayCommand(() => RequestClose?.Invoke(false));
            SeleccionarImagenCommand = new RelayCommand(ExecuteSeleccionarImagen);

            // Ocultar opción admin si el usuario logueado no es admin
            if (Session.User.role != "admin")
            {
                AdminVisible = false;
            }

            if (_usuarioEdicion != null)
            {
                _esEdicion = true;
                CargarDatosUsuario();
            }
        }

        // ==========================================
        // MÉTODOS
        // ==========================================
        private void CargarDatosUsuario()
        {
            Nombre = _usuarioEdicion!.name;
            Apellido = _usuarioEdicion.surname;
            DNI = _usuarioEdicion.dni;
            Email = _usuarioEdicion.email;
            FechaNacimiento = _usuarioEdicion.birthDate;

            // Selección de ciudad
            switch (_usuarioEdicion.city)
            {
                case "Benidorm": CiudadIndex = 0; break;
                case "Alicante": CiudadIndex = 1; break;
                case "Valencia": CiudadIndex = 2; break;
                case "Madrid": CiudadIndex = 3; break;
                default: CiudadIndex = 4; break;
            }

            // Selección de rol
            switch (_usuarioEdicion.role)
            {
                case "client": RolIndex = 0; break;
                case "employee": RolIndex = 1; break;
                case "admin": RolIndex = 2; break;
            }

            // Selección de género
            switch (_usuarioEdicion.gender)
            {
                case "M": EsHombre = true; break;
                case "F": EsMujer = true; break;
                default: EsOtro = true; break;
            }
        }

        // El password se pasa desde el code-behind ya que PasswordBox no soporta Binding
        public async System.Threading.Tasks.Task ExecuteGuardarConPassword(string password, string confirmPassword)
        {
            try
            {
                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido) ||
                    string.IsNullOrWhiteSpace(DNI) || string.IsNullOrWhiteSpace(Email))
                {
                    MessageBox.Show("Por favor, rellena todos los campos obligatorios.");
                    return;
                }

                // Validar DNI
                string dniInput = DNI.ToUpper();
                if (!EsDNIValido(dniInput))
                {
                    MessageBox.Show("El DNI no es válido.\nDebe tener 8 números y la letra correcta (Ej: 12345678Z).");
                    return;
                }

                // Validar Email
                if (!EsEmailValido(Email))
                {
                    MessageBox.Show("El formato del correo electrónico no es válido.");
                    return;
                }

                Usuario nuevoUsuario = _esEdicion ? _usuarioEdicion! : new Usuario();

                nuevoUsuario.name = Nombre;
                nuevoUsuario.surname = Apellido;
                nuevoUsuario.dni = dniInput;
                nuevoUsuario.email = Email;
                nuevoUsuario.birthDate = FechaNacimiento ?? DateTime.Now;

                // Obtener ciudad del índice
                string[] ciudades = { "Benidorm", "Alicante", "Valencia", "Madrid", "Otro" };
                if (CiudadIndex >= 0 && CiudadIndex < ciudades.Length)
                    nuevoUsuario.city = ciudades[CiudadIndex];

                // Obtener rol del índice
                string[] roles = { "client", "employee", "admin" };
                if (RolIndex >= 0 && RolIndex < roles.Length)
                    nuevoUsuario.role = roles[RolIndex];

                // Obtener género
                if (EsHombre) nuevoUsuario.gender = "M";
                else if (EsMujer) nuevoUsuario.gender = "F";
                else nuevoUsuario.gender = "Other";

                // Contraseña
                if (!_esEdicion || !string.IsNullOrEmpty(password))
                {
                    if (password != confirmPassword)
                    {
                        MessageBox.Show("Las contraseñas no coinciden.");
                        return;
                    }
                    if (password.Length < 8)
                    {
                        MessageBox.Show("La contraseña debe tener al menos 8 caracteres.");
                        return;
                    }
                    nuevoUsuario.password = password;
                }

                // Llamada a la API
                if (_esEdicion)
                {
                    await UserService.ModifyUserAsync(nuevoUsuario.user_id, nuevoUsuario);
                    MessageBox.Show("Usuario actualizado correctamente.");
                }
                else
                {
                    await UserService.AddUserAsync(nuevoUsuario);
                    MessageBox.Show("Usuario creado correctamente.");
                }

                RequestClose?.Invoke(true);

                // Subir imagen si se seleccionó una
                if (!string.IsNullOrEmpty(ImagenPath))
                {
                    try
                    {
                        string userId = _esEdicion ? nuevoUsuario.user_id : "";
                        // Si es nuevo usuario, necesitamos hacer otra petición con el ID generado
                        // La API ya devuelve el user_id al crear, pero aquí simplificamos
                        // usando el método UpdateProfileImageAsync
                        if (!string.IsNullOrEmpty(userId))
                        {
                            await UserService.UpdateProfileImageAsync(userId, ImagenPath);
                        }
                    }
                    catch (Exception imgEx)
                    {
                        MessageBox.Show($"Usuario guardado, pero hubo un error al subir la imagen: {imgEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ExecuteGuardar()
        {
            // Este command se usa como fallback; el code-behind llama a ExecuteGuardarConPassword
            // porque PasswordBox no soporta Binding por seguridad de WPF
        }

        private bool EsEmailValido(string email)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patron);
        }

        private bool EsDNIValido(string dni)
        {
            string patron = @"^\d{8}[A-Z]$";
            if (!Regex.IsMatch(dni, patron)) return false;

            string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            string numeros = dni.Substring(0, 8);
            char letraDada = dni[8];

            int resto = int.Parse(numeros) % 23;
            return letras[resto] == letraDada;
        }

        private void ExecuteSeleccionarImagen()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen de perfil",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png|Todos los archivos|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                ImagenPath = dialog.FileName;

                // Crear preview
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 150;
                    bitmap.EndInit();
                    ImagenPreview = bitmap;
                }
                catch
                {
                    MessageBox.Show("No se pudo cargar la imagen seleccionada.");
                }
            }
        }
    }
}

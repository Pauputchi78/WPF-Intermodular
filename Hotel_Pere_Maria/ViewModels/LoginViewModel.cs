using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;

namespace Hotel_Pere_Maria.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // ==========================================
        // CAMPOS PRIVADOS
        // ==========================================
        private string _email = "";
        private string _password = "";
        private string _errorMessage = "";
        private string _errorColor = "Red";
        private bool _isLoginEnabled = true;
        private string _loginButtonText = "Iniciar Sesión";
        private bool _mostrarPassword = false;
        private bool _recordarPassword = false;

        // Ruta del archivo donde se guarda el email recordado
        private static readonly string _archivoRecordar = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HotelPereMaria", "recordar.txt");

        // ==========================================
        // PROPIEDADES PÚBLICAS
        // ==========================================
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string ErrorColor
        {
            get => _errorColor;
            set { _errorColor = value; OnPropertyChanged(); }
        }

        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            set { _isLoginEnabled = value; OnPropertyChanged(); }
        }

        public string LoginButtonText
        {
            get => _loginButtonText;
            set { _loginButtonText = value; OnPropertyChanged(); }
        }

        public bool MostrarPassword
        {
            get => _mostrarPassword;
            set { _mostrarPassword = value; OnPropertyChanged(); }
        }

        public bool RecordarPassword
        {
            get => _recordarPassword;
            set { _recordarPassword = value; OnPropertyChanged(); }
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public ICommand LoginCommand { get; }

        // Evento para que el code-behind sepa que debe cerrar la ventana
        public event Action? LoginExitoso;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async () => await ExecuteLogin());
            CargarCredenciales();
        }

        // ==========================================
        // MÉTODOS
        // ==========================================
        private async System.Threading.Tasks.Task ExecuteLogin()
        {
            try
            {
                // Limpiar error previo
                ErrorMessage = "";

                string email = Email?.Trim() ?? "";
                string password = Password ?? "";

                // Validar campos vacíos
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ErrorMessage = "Email y contraseña son obligatorios";
                    ErrorColor = "Orange";
                    return;
                }

                // Validar formato email
                if (!email.Contains("@"))
                {
                    ErrorMessage = "Por favor, ingresa un email válido";
                    ErrorColor = "Orange";
                    return;
                }

                // Desactivar botón durante login
                IsLoginEnabled = false;
                LoginButtonText = "Iniciando sesión...";

                // Llamada al API
                var response = await AuthService.LoginAsync(email, password);

                // Validar rol
                if (response.user.role != "employee" && response.user.role != "admin")
                {
                    ErrorMessage = $"ACCESO DENEGADO\n\n" +
                                    $"Tu rol es: {response.user.role}\n\n" +
                                    $"Solo estos roles pueden entrar:\n" +
                                    $"• employee (Empleado)\n" +
                                    $"• admin (Administrador)";
                    ErrorColor = "Red";
                    IsLoginEnabled = true;
                    LoginButtonText = "Iniciar Sesión";
                    return;
                }

                // Guardar sesión
                Session.Token = response.token;
                Session.User = response.user;
                ApiService._httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", response.token);

                // Guardar o borrar credenciales según checkbox "Recordar contraseña"
                if (RecordarPassword)
                    GuardarCredenciales(email, password);
                else
                    BorrarCredenciales();

                MessageBox.Show(
                    $"¡Bienvenido {response.user.name}!\nRol: {response.user.role.ToUpper()}",
                    "Login Exitoso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Notificar al code-behind que cierre la ventana
                LoginExitoso?.Invoke();
            }
            catch (Exception ex)
            {
                ManejarError(ex);
                IsLoginEnabled = true;
                LoginButtonText = "Iniciar Sesión";
            }
        }

        private void ManejarError(Exception ex)
        {
            string mensaje = ex.Message;

            if (mensaje.Contains("No se puede establecer una conexión") ||
                mensaje.Contains("Connection refused") ||
                mensaje.Contains("refused") ||
                mensaje.Contains("No route to host") ||
                mensaje.Contains("localhost:3000"))
            {
                ErrorMessage = "No hay conexión con el servidor\n\n" +
                                "Verifica que:\n" +
                                "El backend esté corriendo en http://localhost:3000\n" +
                                "La base de datos esté activa\n" +
                                "No haya firewall bloqueando la conexión";
                ErrorColor = "Red";
                return;
            }

            if (mensaje.Contains("Error") && mensaje.Contains("{"))
            {
                try
                {
                    int startIndex = mensaje.IndexOf("{");
                    int endIndex = mensaje.LastIndexOf("}") + 1;

                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        string jsonError = mensaje.Substring(startIndex, endIndex - startIndex);
                        using (JsonDocument doc = JsonDocument.Parse(jsonError))
                        {
                            JsonElement root = doc.RootElement;
                            if (root.TryGetProperty("error", out JsonElement errorProp))
                                ErrorMessage = FormatearMensajeError(errorProp.GetString());
                            else if (root.TryGetProperty("mensaje", out JsonElement mensajeProp))
                                ErrorMessage = FormatearMensajeError(mensajeProp.GetString());
                            else if (root.TryGetProperty("message", out JsonElement messageProp))
                                ErrorMessage = FormatearMensajeError(messageProp.GetString());
                            else
                                ErrorMessage = "Credenciales inválidas";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Credenciales inválidas";
                    }
                }
                catch
                {
                    ErrorMessage = "Credenciales inválidas";
                }
            }
            else if (mensaje.Contains("timeout") || mensaje.Contains("Timeout"))
            {
                ErrorMessage = "Timeout - El servidor tardó demasiado en responder";
            }
            else
            {
                ErrorMessage = mensaje;
            }

            ErrorColor = "Red";
        }

        /// <summary>
        /// Requisito de seguridad: NO especificar si falló el email o la contraseña.
        /// Siempre devolver un mensaje genérico.
        /// </summary>
        private string FormatearMensajeError(string? errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return "Credenciales incorrectas";

            // Cuenta desactivada sí se puede informar (no revela credenciales)
            if (errorMsg.Contains("activo") || errorMsg.Contains("inactivo") || errorMsg.Contains("desactivado"))
                return "Cuenta desactivada. Contacte con el administrador.";

            // Para cualquier otro error de autenticación: mensaje genérico
            return "Credenciales incorrectas";
        }

        // ==========================================
        // RECORDAR CONTRASEÑA (persistencia en archivo)
        // ==========================================
        private void CargarCredenciales()
        {
            try
            {
                if (File.Exists(_archivoRecordar))
                {
                    string contenido = File.ReadAllText(_archivoRecordar).Trim();
                    if (!string.IsNullOrEmpty(contenido))
                    {
                        var partes = contenido.Split('\n');
                        if (partes.Length >= 1)
                        {
                            Email = partes[0].Trim();
                            RecordarPassword = true;
                        }
                        if (partes.Length >= 2)
                        {
                            try
                            {
                                var base64Bytes = Convert.FromBase64String(partes[1].Trim());
                                Password = System.Text.Encoding.UTF8.GetString(base64Bytes);
                            }
                            catch
                            {
                                Password = partes[1].Trim(); // Por si no estaba codificada antes
                            }
                        }
                    }
                }
            }
            catch { /* Si falla la lectura, simplemente no prellenamos */ }
        }

        private void GuardarCredenciales(string email, string password)
        {
            try
            {
                string directorio = Path.GetDirectoryName(_archivoRecordar)!;
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(password);
                string passCodificada = Convert.ToBase64String(plainTextBytes);

                File.WriteAllText(_archivoRecordar, $"{email}\n{passCodificada}");
            }
            catch { /* Si falla el guardado, no es crítico */ }
        }

        private void BorrarCredenciales()
        {
            try
            {
                if (File.Exists(_archivoRecordar))
                    File.Delete(_archivoRecordar);
            }
            catch { }
        }
    }
}

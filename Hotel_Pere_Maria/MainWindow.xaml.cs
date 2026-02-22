using Hotel_Pere_Maria.ViewModels;
using Hotel_Pere_Maria.Views;
using System;
using System.Windows;

namespace Hotel_Pere_Maria
{
    public partial class MainWindow : Window
    {
        private LoginViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Rellenar visualmente las contraseñas si el ViewModel cargó un recordar contraseña
            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                TxtPassword.Password = _viewModel.Password;
                txtPasswordVisible.Text = _viewModel.Password;
            }

            // Suscribirse al evento de login exitoso para cerrar ventana y abrir Inicio
            _viewModel.LoginExitoso += () =>
            {
                Inicio ventanaInicio = new Inicio();
                ventanaInicio.Show();
                this.Close();
            };

            // Sincronizar PasswordBox → ViewModel cada vez que cambie
            // (PasswordBox no soporta Binding por seguridad de WPF)
            TxtPassword.PasswordChanged += (s, e) =>
            {
                _viewModel.Password = TxtPassword.Password;
            };
        }

        // El checkbox de mostrar/ocultar contraseña se queda en code-behind
        // porque es lógica puramente visual (UI)
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkpass.IsChecked.Value)
            {
                txtPasswordVisible.Text = TxtPassword.Password;
                TxtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordVisible.Focus();
                txtPasswordVisible.SelectionStart = txtPasswordVisible.Text.Length;
            }
            else
            {
                TxtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                TxtPassword.Focus();
            }
        }

        private void TexChanget_Contra(object sender, EventArgs e)
        {
            TxtPassword.Password = txtPasswordVisible.Text;
        }
    }
}

using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;
using System.Windows;

namespace Hotel_Pere_Maria.Views
{
    public partial class modRoom : Window
    {
        private readonly ModRoomViewModel _viewModel;

        public modRoom(Room room, bool isCreate)
        {
            InitializeComponent();
            _viewModel = new ModRoomViewModel(room, isCreate);
            DataContext = _viewModel;
        }

        public modRoom(Room room) : this(room, false) { }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

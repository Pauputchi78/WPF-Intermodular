using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hotel_Pere_Maria.Views
{
    public partial class listRoom : Window
    {
        private readonly bool _editMode;
        private bool _closing = false;
        private readonly ListRoomViewModel _viewModel;

        public Room SelectedRoomResult { get; private set; }

        public listRoom() : this(true, null, null) { }

        public listRoom(DateTime? checkIn, DateTime? checkOut) : this(false, checkIn, checkOut) { }

        private listRoom(bool editMode, DateTime? checkIn, DateTime? checkOut)
        {
            InitializeComponent();
            _editMode = editMode;
            _viewModel = new ListRoomViewModel(editMode, checkIn, checkOut);
            DataContext = _viewModel;
        }

        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (!_editMode)
            {
                MessageBox.Show("No se pueden crear habitaciones desde el modo reserva.");
                return;
            }

            var newRoom = new Room
            {
                RoomId = "HAB-",
                Type = "Individual",
                Description = "",
                Image = "",
                PricePerNight = 0,
                Rate = 0,
                MaxOccupancy = 1,
                IsAvailable = true
            };

            var win = new modRoom(newRoom, isCreate: true);

            if (win.ShowDialog() == true)
            {
                await _viewModel.LoadRoomsAsync();
            }
        }

        private async void LvRooms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_closing) return;

            if ((sender as ListViewItem)?.DataContext is not Room room)
                return;

            if (_editMode)
            {
                var editWin = new modRoom(room);
                if (editWin.ShowDialog() == true)
                {
                    await _viewModel.LoadRoomsAsync();
                }
                return;
            }

            _closing = true;
            SelectedRoomResult = room;
            try { DialogResult = true; } catch { }
            Close();
        }
    }
}

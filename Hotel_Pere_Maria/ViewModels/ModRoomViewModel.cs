using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ModRoomViewModel : BaseViewModel
    {
        private string _roomId;
        private string _type;
        private string _price;
        private int _maxOccupancy;
        private string _rate;
        private string _image;
        private string _description;
        private bool _isAvailable;
        private bool _isCreate;
        private bool _isRoomIdReadOnly;

        public string RoomId
        {
            get => _roomId;
            set { _roomId = value; OnPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set 
            { 
                _type = value; 
                OnPropertyChanged(); 
                UpdateMaxOccupancy();
            }
        }

        public string Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public int MaxOccupancy
        {
            get => _maxOccupancy;
            set { _maxOccupancy = value; OnPropertyChanged(); }
        }

        public string Rate
        {
            get => _rate;
            set { _rate = value; OnPropertyChanged(); }
        }

        public string Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set { _isAvailable = value; OnPropertyChanged(); }
        }

        public bool IsRoomIdReadOnly
        {
            get => _isRoomIdReadOnly;
            set { _isRoomIdReadOnly = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public ModRoomViewModel(Room room, bool isCreate)
        {
            _isCreate = isCreate;
            IsRoomIdReadOnly = !isCreate;

            RoomId = room.RoomId ?? "";
            Type = (room.Type ?? "").Trim();
            Price = room.PricePerNight > 0 ? room.PricePerNight.ToString(CultureInfo.InvariantCulture) : "";
            MaxOccupancy = room.MaxOccupancy > 0 ? room.MaxOccupancy : 1;
            Rate = room.Rate.ToString(CultureInfo.InvariantCulture);
            Image = room.Image ?? "";
            Description = room.Description ?? "";
            IsAvailable = room.IsAvailable;

            SaveCommand = new RelayCommand(() => _ = SaveAsync());
        }

        private void UpdateMaxOccupancy()
        {
            MaxOccupancy = Type switch
            {
                "Individual" => 1,
                "Doble" => 2,
                "Suite" => 4,
                _ => MaxOccupancy
            };
        }

        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RoomId))
                {
                    MessageBox.Show("El Room ID es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Type))
                {
                    MessageBox.Show("Debes seleccionar un tipo de habitación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Description))
                {
                    MessageBox.Show("La descripción es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(Price?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double priceValue) || priceValue <= 0)
                {
                    MessageBox.Show("El precio debe ser un número válido mayor que 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(Rate?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double rateValue))
                    rateValue = 0;

                var payload = new
                {
                    room_id = RoomId,
                    type = Type,
                    description = Description,
                    image = Image,
                    price_per_night = priceValue,
                    rate = rateValue,
                    max_occupancy = MaxOccupancy,
                    isAvailable = IsAvailable
                };

                if (_isCreate)
                    await RoomService.CreateRoomAsync(payload);
                else
                    await RoomService.UpdateRoomAsync(payload);

                foreach (Window win in Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.DialogResult = true;
                        win.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la habitación:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

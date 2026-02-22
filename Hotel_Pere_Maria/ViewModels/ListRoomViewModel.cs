using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ListRoomViewModel : BaseViewModel
    {
        private ObservableCollection<Room> _rooms;
        private List<Room> _allRoomsStore = new List<Room>();
        private string _searchText;
        private string _selectedType;
        private bool _onlyAvailable;
        private int _minCapacity = 1;
        private double _minPrice = 0;
        private double _maxPrice = 1000;
        private bool _isEditMode;
        private DateTime? _checkIn;
        private DateTime? _checkOut;

        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set { _rooms = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public string SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public bool OnlyAvailable
        {
            get => _onlyAvailable;
            set { _onlyAvailable = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public int MinCapacity
        {
            get => _minCapacity;
            set { _minCapacity = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public double MinPrice
        {
            get => _minPrice;
            set { _minPrice = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public double MaxPrice
        {
            get => _maxPrice;
            set { _maxPrice = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand LoadRoomsCommand { get; }

        public ListRoomViewModel(bool editMode, DateTime? checkIn = null, DateTime? checkOut = null)
        {
            _isEditMode = editMode;
            _checkIn = checkIn;
            _checkOut = checkOut;

            Rooms = new ObservableCollection<Room>();
            LimpiarFiltrosCommand = new RelayCommand(LimpiarFiltros);
            LoadRoomsCommand = new RelayCommand(() => _ = LoadRoomsAsync());

            _ = LoadRoomsAsync();
        }

        public async Task LoadRoomsAsync()
        {
            try
            {
                List<Room> result;
                if (_checkIn.HasValue && _checkOut.HasValue)
                {
                    result = await RoomService.GetAvailableRoomsAsync(_checkIn.Value, _checkOut.Value, 1);
                }
                else
                {
                    result = await RoomService.GetAllRoomsAsync();
                }

                _allRoomsStore = result ?? new List<Room>();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error cargando habitaciones", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AplicarFiltros()
        {
            var q = _allRoomsStore.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                q = q.Where(r => (r.RoomId ?? "").Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(SelectedType))
                q = q.Where(r => r.Type == SelectedType);

            q = q.Where(r => r.MaxOccupancy >= MinCapacity);

            double pMin = MinPrice;
            double pMax = MaxPrice;
            if (pMin > pMax) (pMin, pMax) = (pMax, pMin);

            q = q.Where(r => r.PricePerNight >= pMin && r.PricePerNight <= pMax);

            if (OnlyAvailable)
                q = q.Where(r => r.IsAvailable);

            Rooms = new ObservableCollection<Room>(q.ToList());
        }

        private void LimpiarFiltros()
        {
            SearchText = "";
            SelectedType = null;
            OnlyAvailable = false;
            MinCapacity = 1;
            MinPrice = 0;
            MaxPrice = 1000;
            AplicarFiltros();
        }
    }
}

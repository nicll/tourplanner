using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;
using TourPlanner.DataProviders.MapQuest;
using TourPlanner.Reporting.PDF;
using log4net;

namespace TourPlanner.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _darkMode = false, _searchBarVisible = false, _includeDescChecked = false;
        private string _searchText = String.Empty;
        private readonly List<Tour> _tours;
        private readonly Configuration _config;
        private Tour _selectedTour;
        private readonly MapQuestApiFactory _mapFactory;
        private IDirectionsProvider _dir;
        private IMapImageProvider _img;
        private readonly IDatabaseClient _db;
        private readonly ILog _log;

        public bool IsDarkMode
        {
            get => _darkMode;
            set => SetProperty(ref _darkMode, value);
        }

        public bool IsSearchBarVisible
        {
            get => _searchBarVisible;
            set
            {
                SetProperty(ref _searchBarVisible, value);
                OnPropertyChanged(nameof(SearchBarVisibility));
            }
        }

        public Visibility SearchBarVisibility => IsSearchBarVisible ? Visibility.Visible : Visibility.Collapsed;

        public bool IsIncludeDescriptionChecked
        {
            get => _includeDescChecked;
            set
            {
                SetProperty(ref _includeDescChecked, value);
                UpdateShownTours();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                UpdateShownTours();
            }
        }

        public Tour SelectedTour
        {
            get => _selectedTour;
            set => SetProperty(ref _selectedTour, value);
        }

        public ObservableCollection<Tour> ShownTours { get; protected set; }

        public ICommand ResetConnectionCommand { get; }

        public ICommand SwitchThemeCommand { get; }

        public ICommand SwitchSearchBarVisibilityCommand { get; }

        public ICommand ExitApplicationCommand { get; }

        public ICommand AddTourCommand { get; }

        public ICommand DeleteTourCommand { get; }

        public ICommand AddTourLogCommand { get; }

        public ICommand DeleteTourLogCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand GenerateSummaryReportCommand { get; }

        public ICommand GenerateTourReportCommand { get; }

        public ICommand ClearSearchTextCommand { get; }

        public MainWindowViewModel()
        {
            _log = LogManager.GetLogger(typeof(MainWindowViewModel));
            _config = Utils.LoadConfig("connection.config");
            _mapFactory = new();
            Task.Run(async () => _dir = await _mapFactory.CreateDirectionsProvider(_config.DirectionsApiConfig));
            Task.Run(async () => _img = await _mapFactory.CreateMapImageProvider(_config.MapImageApiConfig));
            _tours = new();

            ResetConnectionCommand = new RelayCommand(ResetConnection);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            SwitchSearchBarVisibilityCommand = new RelayCommand(SwitchSearchBarVisibility);
            ExitApplicationCommand = new AsyncCommand(ExitApplication);
            AddTourCommand = new AsyncCommand(AddTour);
            DeleteTourCommand = new AsyncCommand(DeleteTour);
            AddTourLogCommand = new RelayCommand(AddTourLog);
            DeleteTourLogCommand = new RelayCommand(DeleteTourLog);
            ImportCommand = new AsyncCommand(ImportData);
            ExportCommand = new AsyncCommand(ExportData);
            GenerateSummaryReportCommand = new AsyncCommand(GenerateSummaryReport);
            GenerateTourReportCommand = new AsyncCommand(GenerateTourReport);
            ClearSearchTextCommand = new RelayCommand(ClearSearchText);
        }

        protected void UpdateLoadedTours(IList<Tour> tours)
        {
            _tours.Clear();
            _tours.AddRange(tours);
        }

        private void ResetConnection()
            => _mapFactory.ResetConnection();

        private void SwitchTheme()
        {
            IsDarkMode = !IsDarkMode;
            AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources,
                IsDarkMode ? AdonisUI.ResourceLocator.DarkColorScheme : AdonisUI.ResourceLocator.LightColorScheme);
        }

        private void SwitchSearchBarVisibility()
            => IsSearchBarVisible = !IsSearchBarVisible;

        private Task ExitApplication()
        {
            Application.Current.Shutdown();
            return Task.CompletedTask;
        }

        private async Task AddTour()
        {
            if (_dir is null || _img is null)
            {
                _log.Error("Tried adding tour when application was not yet initialized.");
                MessageBox.Show("The application is currently not fully initialized.", "Please wait", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new Views.AddTourView();
            var tourVM = new AddTourViewModel(window)
            {
                CancelCommand = new RelayCommand(Cancel),
                FinishCommand = new RelayCommand(Finish)
            };

            if (window.ShowDialog() is not true) // false or null
                return;

            var route = await _dir.GetRoute(tourVM.StartLocation, tourVM.EndLocation);

            if (route is null) // not found
            {
                _log.Error("No possible route was found for the given inputs.");
                MessageBox.Show("No possible route could be found.", "Cannot add tour", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var imagePath = await _img.GetImage(route);

            if (imagePath is null)
            {
                _log.Warn("No image was found for the given route.");
                MessageBox.Show("No image for the route could be loaded.", "Cannot load tour image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            var newTour = new Tour
            {
                TourId = Guid.NewGuid(),
                Name = tourVM.Name,
                CustomDescription = String.Empty,
                ImagePath = imagePath,
                Route = route,
                Log = new List<LogEntry>()
            };

            _log.Info("Adding new tour with TourId=\"" + newTour.TourId + "\".");
            await _db.AddTour(newTour);
            _tours.Add(newTour);
            UpdateShownTours();

            void Cancel()
            {
                window.DialogResult = false;
                window.Close();
            }

            void Finish()
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private async Task DeleteTour()
        {
            var selectedTour = SelectedTour;

            if (selectedTour is null)
                return;

            _log.Info("Deleting tour with TourId=\"" + selectedTour.TourId + "\".");
            SelectedTour = null;
            _tours.Remove(selectedTour);
            ShownTours.Remove(selectedTour);
            await _db.RemoveTour(selectedTour);
        }

        private void AddTourLog()
        {

        }

        private void DeleteTourLog()
        {

        }

        public async Task ImportData()
        {
            var path = Utils.GetOpenFilePath("json", "JSON document (*.json)|*.json");

            if (String.IsNullOrEmpty(path))
                return;

            _tours.Clear();
            var tours = await Utils.ImportToursFromFile(path);
            _tours.AddRange(tours);
            UpdateShownTours();
            await _db.SynchronizeTours(_tours);
        }

        public async Task ExportData()
        {
            var path = Utils.GetSaveFilePath(null, "json", "JSON document (*.json)|*.json");

            if (String.IsNullOrEmpty(path))
                return;

            await Utils.ExportToursFromFile(path, _tours);
        }

        private Task GenerateSummaryReport()
        {
            _log.Debug("Generating summary report.");
            var savePath = GetReportSavePath("summary");

            if (String.IsNullOrEmpty(savePath))
                return Task.CompletedTask;

            _log.Debug("User selected \"" + savePath + "\" for summary report.");
            ReportGenerator.GenerateSummaryReport(_tours, savePath);
            Utils.ShowFile(savePath);
            _log.Info("Generated summary report: " + savePath);
            return Task.CompletedTask;
        }

        private Task GenerateTourReport()
        {
            if (SelectedTour is not Tour selectedTour)
                return Task.CompletedTask;

            _log.Debug("Generating tour report for tour \"" + selectedTour.TourId + "\".");
            var savePath = GetReportSavePath(selectedTour.Name);

            if (String.IsNullOrEmpty(savePath))
                return Task.CompletedTask;

            _log.Debug("User selected \"" + savePath + "\" for tour report for tour \"" + selectedTour.TourId + "\".");
            ReportGenerator.GenerateTourReport(selectedTour, savePath);
            Utils.ShowFile(savePath);
            _log.Info("Generated tour report \"" + savePath + "\" for tour \"" + selectedTour.TourId + "\".");
            return Task.CompletedTask;
        }

        private static string GetReportSavePath(string suggestedName)
            => Utils.GetSaveFilePath(suggestedName, "pdf", "Portable document files (*.pdf)|*.pdf");

        private void ClearSearchText()
            => SearchText = String.Empty;

        private void UpdateShownTours()
        {
            Func<Tour, bool> isContained = IsIncludeDescriptionChecked ? IsInNameOrDesc : IsInName;
            ShownTours.Clear();

            foreach (var tour in _tours)
            {
                if (isContained(tour))
                    ShownTours.Add(tour);
            }

            bool IsInName(Tour tour) => tour.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            bool IsInNameOrDesc(Tour tour) => IsInName(tour) || tour.CustomDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}

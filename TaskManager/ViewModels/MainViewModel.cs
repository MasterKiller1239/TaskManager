using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Input;
using TaskManager.Client.Export;
using TaskManager.Client.Models;

using TaskManager.Client.Utilities;

namespace TaskManager.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient = new() { BaseAddress = new System.Uri("http://localhost:5000/api/") };

        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public ICommand LoadTasksCommand { get; }
        public string NewTaskTitle { get; set; } = "";

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleTaskCompletedCommand { get; }
        public ICommand ExportToJsonCommand { get; }
        public ICommand ExportToXmlCommand { get; }
        public ICommand ExportToPdfCommand { get; }
        private TaskItem? _selectedTask;
        public ObservableCollection<TaskItem> FilteredTasks { get; } = new();

        private string _statusFilter = "All"; // "All", "Completed", "Pending"
        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                _statusFilter = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }

        private string _sortOption = "None"; // "None", "Title", "Id"
        public string SortOption
        {
            get => _sortOption;
            set
            {
                _sortOption = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }

        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
            }
        }
        private string _motivationalQuote;
        public string MotivationalQuote
        {
            get => _motivationalQuote;
            set
            {
                _motivationalQuote = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel()
        {
            
            LoadTasksCommand = new RelayCommand(async _ => await LoadTasks());
            AddTaskCommand = new RelayCommand(async _ => await AddTask());
            DeleteTaskCommand = new RelayCommand(async _ => await DeleteTask(), _ => SelectedTask != null);
            ToggleTaskCompletedCommand = new RelayCommand(async _ => await ToggleCompleted(), _ => SelectedTask != null);
            ExportToJsonCommand = new RelayCommand(_ => ExportTasks(new JsonExporter()));
            ExportToXmlCommand = new RelayCommand(_ => ExportTasks(new XmlExporter()));
            ExportToPdfCommand = new RelayCommand(_ => ExportTasks(new PdfExporter()));
        }
        public MainViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            LoadTasksCommand = new RelayCommand(async _ => await LoadTasks());
            AddTaskCommand = new RelayCommand(async _ => await AddTask());
            DeleteTaskCommand = new RelayCommand(async _ => await DeleteTask(), _ => SelectedTask != null);
            ToggleTaskCompletedCommand = new RelayCommand(async _ => await ToggleCompleted(), _ => SelectedTask != null);
            ExportToJsonCommand = new RelayCommand(_ => ExportTasks(new JsonExporter()));
            ExportToXmlCommand = new RelayCommand(_ => ExportTasks(new XmlExporter()));
            ExportToPdfCommand = new RelayCommand(_ => ExportTasks(new PdfExporter()));
        }
        private async Task LoadTasks()
        {
            var tasks = await _httpClient.GetFromJsonAsync<TaskItem[]>("tasks");
            Tasks.Clear();
            if (tasks != null)
            {
                foreach (var t in tasks)
                    Tasks.Add(t);
            }
            ApplyFilterAndSort();
            await LoadMotivationalQuote();

        }
        private async Task AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            var newTask = new TaskItem { Title = NewTaskTitle, IsCompleted = false };
            var response = await _httpClient.PostAsJsonAsync("tasks", newTask);
            if (response.IsSuccessStatusCode)
            {
                await LoadTasks();
                NewTaskTitle = string.Empty;
                OnPropertyChanged(nameof(NewTaskTitle));
                ApplyFilterAndSort();
            }

        }

        private async Task DeleteTask()
        {
            if (SelectedTask == null) return;
            var response = await _httpClient.DeleteAsync($"tasks/{SelectedTask.Id}");
            if (response.IsSuccessStatusCode)
            {
                await LoadTasks();
            }
        }

        private async Task ToggleCompleted()
        {
            if (SelectedTask == null) return;

            SelectedTask.IsCompleted = !SelectedTask.IsCompleted;
            var response = await _httpClient.PutAsJsonAsync($"tasks/{SelectedTask.Id}", SelectedTask);
            if (response.IsSuccessStatusCode)
            {
                await LoadTasks();
            }
        }
        private void ExportTasks(IExportStrategy exporter)
        {
            exporter.Export(Tasks);
        }
        private void ApplyFilterAndSort()
        {
            var filtered = Tasks.AsEnumerable();

            // Filtering
            if (StatusFilter == "Completed")
                filtered = filtered.Where(t => t.IsCompleted).ToList();
            else if (StatusFilter == "Pending")
                filtered = filtered.Where(t => !t.IsCompleted).ToList();

            // Sorting
            if (SortOption == "Title")
                filtered = filtered.OrderBy(t => t.Title).ToList();
            else if (SortOption == "Id")
                filtered = filtered.OrderBy(t => t.Id).ToList();

            FilteredTasks.Clear();
            foreach (var task in filtered)
                FilteredTasks.Add(task);
        }
        private async Task LoadMotivationalQuote()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<QuoteResponse>>("https://zenquotes.io/api/random");
                if (response != null && response.Count > 0)
                {
                    var quote = response[0];
                    MotivationalQuote = $"\"{quote.q}\" — {quote.a}";
                }
            }
            catch
            {
                MotivationalQuote = "Could not load quote.";
            }
        }
    }
}
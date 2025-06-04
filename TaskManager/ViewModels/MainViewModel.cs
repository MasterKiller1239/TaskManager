using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            LoadTasksCommand = new RelayCommand(async _ => await LoadTasks());
            AddTaskCommand = new RelayCommand(async _ => await AddTask());
            DeleteTaskCommand = new RelayCommand(async _ => await DeleteTask(), _ => SelectedTask != null);
            ToggleTaskCompletedCommand = new RelayCommand(async _ => await ToggleCompleted(), _ => SelectedTask != null);
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
    }
}
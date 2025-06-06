using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Client.Models;
using TaskManager.Client.ViewModels;
using Xunit;

namespace TaskManager.Tests
{
    public class MainViewModelTests
    {
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

            public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_handlerFunc(request));
            }
        }

        [Fact]
        public async Task AddTask_ShouldAddTask_WhenTitleIsValid()
        {
            // Arrange
            var taskList = new[]
            {
                new TaskItem { Id = 1, Title = "Test Task", IsCompleted = false }
            };

            var handler = new MockHttpMessageHandler(req =>
            {
                if (req.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else if (req.Method == HttpMethod.Get)
                {
                    var json = JsonSerializer.Serialize(taskList);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5000/api/")
            };

            var viewModel = new MainViewModel(httpClient);
            viewModel.NewTaskTitle = "Test Task";

            // Act
            var addTasksTask = viewModel.GetType()
                .GetMethod("AddTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(viewModel, null) as Task;

            await addTasksTask;

            // Assert
            Assert.Single(viewModel.Tasks);
            Assert.Contains("Test Task", viewModel.Tasks.Last().Title);
        }

        [Fact]
        public async Task LoadTasks_ShouldLoadTasksFromApi()
        {
            // Arrange
            var taskList = new[]
            {
                new TaskItem { Id = 1, Title = "Task 1", IsCompleted = false },
                new TaskItem { Id = 2, Title = "Task 2", IsCompleted = true }
            };

            var handler = new MockHttpMessageHandler(_ =>
            {
                var json = JsonSerializer.Serialize(taskList);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5000/api/")
            };

            var viewModel = new MainViewModel(httpClient);

            // Act
            var loadTasksTask = viewModel.GetType()
                .GetMethod("LoadTasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(viewModel, null) as Task;

            await loadTasksTask;


            // Assert
            Assert.Equal(2, viewModel.Tasks.Count);
            Assert.Equal("Task 2", viewModel.Tasks.Last().Title);
            Assert.True(viewModel.Tasks.Last().IsCompleted);
        }
        [Fact]
        public async Task DeleteTask_ShouldRemoveSelectedTask()
        {
            // Arrange
            var taskList = new[]
            {
        new TaskItem { Id = 1, Title = "Task 1", IsCompleted = false }
    };

            var handler = new MockHttpMessageHandler(req =>
            {
                if (req.Method == HttpMethod.Delete)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK); 
                }
                else if (req.Method == HttpMethod.Get)
                {
                    var json = JsonSerializer.Serialize(Array.Empty<TaskItem>()); 
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000/api/") };
            var viewModel = new MainViewModel(httpClient);

            // Dodaj zadanie i ustaw jako wybrane
            viewModel.Tasks.Add(taskList[0]);
            viewModel.SelectedTask = taskList[0];

            // Act
            var method = viewModel.GetType().GetMethod("DeleteTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(viewModel, null);

            // Assert
            Assert.Empty(viewModel.Tasks); 
        }

        [Fact]
        public async Task ToggleTaskCompleted_ShouldToggleIsCompletedFlag()
        {
            // Arrange
            var task = new TaskItem { Id = 1, Title = "Task 1", IsCompleted = false };

            var handler = new MockHttpMessageHandler(req =>
            {
                if (req.Method == HttpMethod.Put)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK); 
                }
                else if (req.Method == HttpMethod.Get)
                {
                    var updatedTask = new[] { new TaskItem { Id = 1, Title = "Task 1", IsCompleted = true } };
                    var json = JsonSerializer.Serialize(updatedTask);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000/api/") };
            var viewModel = new MainViewModel(httpClient);

            viewModel.Tasks.Add(task);
            viewModel.SelectedTask = task;

            // Act
            var method = viewModel.GetType().GetMethod("ToggleCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(viewModel, null);

            // Assert
            Assert.Single(viewModel.Tasks);
            Assert.True(viewModel.Tasks[0].IsCompleted); 
        }
        [Fact]
        public void ApplyFilterAndSort_ShouldFilterCompletedTasks()
        {
            var viewModel = new MainViewModel();
            viewModel.Tasks.Add(new TaskItem { Id = 1, Title = "A", IsCompleted = true });
            viewModel.Tasks.Add(new TaskItem { Id = 2, Title = "B", IsCompleted = false });

            viewModel.StatusFilter = "Completed";

            Assert.Single(viewModel.FilteredTasks);
            Assert.True(viewModel.FilteredTasks[0].IsCompleted);
        }

    }

}

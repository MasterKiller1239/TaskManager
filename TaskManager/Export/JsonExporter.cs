using System.IO;
using System.Text.Json;
using TaskManager.Client.Models;
namespace TaskManager.Client.Export
{
    public class JsonExporter : IExportStrategy
    {
        public void Export(IEnumerable<TaskItem> tasks)
        {
            var json = JsonSerializer.Serialize(tasks);
            File.WriteAllText("tasks.json", json);
        }
    }
}
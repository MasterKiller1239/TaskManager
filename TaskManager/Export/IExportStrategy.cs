using TaskManager.Client.Models;
namespace TaskManager.Client.Export
{
    public interface IExportStrategy
    {
        void Export(IEnumerable<TaskItem> tasks);
    }
}
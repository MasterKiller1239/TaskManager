using System.IO;
using System.Xml.Serialization;
using TaskManager.Client.Models;

namespace TaskManager.Client.Export
{
    public class XmlExporter : IExportStrategy
    {
        public void Export(IEnumerable<TaskItem> tasks)
        {
            var serializer = new XmlSerializer(typeof(List<TaskItem>));
            using var writer = new StreamWriter("tasks.xml");
            serializer.Serialize(writer, tasks.ToList());
        }

    }
}

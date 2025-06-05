using TaskManager.Client.Models;
using PdfSharpCore.Pdf;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Drawing;

namespace TaskManager.Client.Export
{
    public class PdfExporter : IExportStrategy
    {
        public void Export(IEnumerable<TaskItem> tasks)
        {
            // Ścieżka zapisu w folderze Downloads użytkownika
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string outputPath = Path.Combine(outputDir, $"Tasks_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            try
            {
                // Sprawdzenie, czy katalog istnieje, jeśli nie - utwórz
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Tworzenie dokumentu MigraDoc
                Document document = new Document();
                document.Info.Title = "Task List PDF";
                Style style = document.Styles["Normal"];
                style.Font.Name = "Arial";
                // Sekcja dokumentu
                Section section = document.AddSection();

                // Nagłówek
                Paragraph header = section.AddParagraph("Task List");
                header.Format.Font.Size = 16;
                header.Format.Font.Bold = true;
                header.Format.Alignment = ParagraphAlignment.Center;

                // Data
                Paragraph date = section.AddParagraph($"Generated: {DateTime.Now}");
                date.Format.Font.Size = 10;
                date.Format.Alignment = ParagraphAlignment.Right;

                // Tabela
                Table table = section.AddTable();
                table.Borders.Width = 0.5;

                // Kolumny
                Column idColumn = table.AddColumn(Unit.FromCentimeter(2));
                idColumn.Format.Alignment = ParagraphAlignment.Center;
                Column titleColumn = table.AddColumn(Unit.FromCentimeter(10));
                Column statusColumn = table.AddColumn(Unit.FromCentimeter(4));
                statusColumn.Format.Alignment = ParagraphAlignment.Center;

                // Wiersz nagłówka
                Row headerRow = table.AddRow();
                headerRow.HeadingFormat = true;
                headerRow.Format.Font.Bold = true;
                headerRow.Cells[0].AddParagraph("ID");
                headerRow.Cells[1].AddParagraph("Title");
                headerRow.Cells[2].AddParagraph("Status");

                // Wiersze z danymi
                foreach (var task in tasks)
                {
                    Row row = table.AddRow();
                    row.Cells[0].AddParagraph(task.Id.ToString());
                    row.Cells[1].AddParagraph(task.Title ?? "No Title");
                    row.Cells[2].AddParagraph(task.IsCompleted ? "Completed" : "Pending");
                }

                // Renderowanie dokumentu do PDF
                PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();

                // Zapis do pliku
                renderer.PdfDocument.Save(outputPath);

                Console.WriteLine($"PDF exported successfully to {outputPath}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Exception: Unable to write to {outputPath}. {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error exporting to PDF: {ex.Message}");
                throw;
            }
        }
    }
}
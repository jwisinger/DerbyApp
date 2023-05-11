using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;

#warning REPORTS: Add ability to generate per racer and overall reports

namespace DerbyApp
{
    internal class GenerateReport
    {
        public static void DefineStyles(Document document)
        {
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Times New Roman";

            // Heading1 to Heading9 are predefined styles with an outline level. An outline level
            // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks)
            // in PDF.

            style = document.Styles["Heading1"];
            style.Font.Size = 20;
            style.Font.Bold = true;
            style.ParagraphFormat.PageBreakBefore = true;
            style.ParagraphFormat.SpaceAfter = 6;

            style = document.Styles["Heading2"];
            style.Font.Size = 16;
            style.ParagraphFormat.PageBreakBefore = false;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 6;

            style = document.Styles["Heading3"];
            style.Font.Size = 14;
            style.Font.Bold = false;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 3;

            // Create a new style called TextBox based on style Normal
            style = document.Styles.AddStyle("TextBox", "Normal");
            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            style.ParagraphFormat.Borders.Width = 2.5;
            style.ParagraphFormat.Borders.Distance = "3pt";
        }

        static Document CreateDocument(Racer r)
        {
            Document document = new Document();
            DefineStyles(document);
            Section section = document.AddSection();
            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            paragraph.AddFormattedText("\r\nName: " + r.RacerName + "\r\n", "Heading1");
            paragraph.AddFormattedText("Troop: " + r.Troop + "\r\n", "Heading2");
            paragraph.AddFormattedText("Level: " + r.Level + "\r\n", "Heading2");
            paragraph.AddFormattedText("Car Weight: " + r.Weight.ToString() + "\r\n", "Heading3");
            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = -110;
            paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(r.Photo, new Size(200, 200)))));
            paragraph.Format.Borders.Top = new Border() { Width = "2pt", Color = Colors.DarkGray };
            paragraph.Format.Borders.Bottom = new Border() { Width = "2pt", Color = Colors.DarkGray };
            paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Race Info", "Normal");
            return document;
        }

        static public void Generate(string EventName, ObservableCollection<Racer> racers, List<ObservableCollection<Racer>> races)
        {
            Directory.CreateDirectory(EventName);
            foreach (Racer r in racers)
            {
                Document document = CreateDocument(r);
                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(false)
                {
                    Document = document
                };
                pdfRenderer.RenderDocument();

                pdfRenderer.PdfDocument.Save(EventName + "/" + r.RacerName + ".pdf");
                Process.Start(Path.Combine(EventName,r.RacerName + ".pdf"));
            }
        }
    }
}

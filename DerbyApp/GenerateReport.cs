using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

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
        }

        static Document CreateDocument(Racer r, List<RaceResults> races)
        {
            Document document = new();
            DefineStyles(document);
            Section section = document.AddSection();
            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            paragraph.AddFormattedText("\r\nName: " + r.RacerName + "\r\n", "Heading1");
            paragraph.AddFormattedText("Troop: " + r.Troop + "\r\n", "Heading2");
            paragraph.AddFormattedText("Level: " + r.Level + "\r\n", "Heading2");
            paragraph.AddFormattedText("Car Weight: " + r.Weight.ToString() + "\r\n", "Heading3");
            paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = -80;
            paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(r.Photo, new Size(224, 168)))));
            paragraph.Format.Borders.Top = new Border() { Width = "4pt", Color = Colors.DarkGray };
            paragraph.Format.Borders.Bottom = new Border() { Width = "4pt", Color = Colors.DarkGray };

            foreach (RaceResults result in races)
            {
                Leaderboard ldr = new(result.Racers, result.HeatCount, result.LaneCount);
                ldr.CalculateResults(result.ResultsTable);
                if (ldr.Board.OrderByDescending(x => x.Score).ToList().FindIndex(x => x.Number == r.Number) > -1)
                {
                    paragraph = section.AddParagraph();
                    paragraph.Format.Borders.Bottom = new Border() { Width = "1pt", Color = Colors.DarkGray };
                    paragraph.AddFormattedText("\r\nRace Name: " + result.RaceName + "\r\n", "Heading3");
                    try
                    {
                        paragraph.AddFormattedText("Overall Race Finish: " + (1 + ldr.Board.OrderByDescending(x => x.Score).ToList().FindIndex(x => x.Number == r.Number)) + "\r\n", "Normal");
                        DataRow resultRow = result.ResultsTable.Select("Number = " + r.Number)[0];
                        DataRow scoreRow = ldr.RaceScoreTable.Select("Number = " + r.Number)[0];
                        for (int i = 0; i < result.HeatCount; i++)
                        {
                            if (resultRow[i + 2] != DBNull.Value)
                            {

                                paragraph.AddFormattedText("Heat " + (i + 1) + " Time: " + resultRow[i + 2] + " seconds (" + (1 + result.LaneCount - (int)scoreRow[i + 2]) + ")\r\n", "Normal");
                            }
                        }
                    }
                    catch { }
                }
            }

            return document;
        }

        static public void Generate(string eventName, ObservableCollection<Racer> racers, List<RaceResults> races)
        {
            string eventPath = Path.Combine(Path.GetDirectoryName(eventName), Path.GetFileNameWithoutExtension(eventName));
            Directory.CreateDirectory(eventPath);
            foreach (Racer r in racers)
            {
                Document document = CreateDocument(r, races);
                PdfDocumentRenderer pdfRenderer = new(false)
                {
                    Document = document
                };
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                pdfRenderer.RenderDocument();

                pdfRenderer.PdfDocument.Save(Path.Combine(eventPath, r.RacerName + ".pdf"));
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(Path.Combine(eventPath, r.RacerName + ".pdf"))
                    {
                        UseShellExecute = true
                    }
                };
                p.Start();
            }
        }
    }
}

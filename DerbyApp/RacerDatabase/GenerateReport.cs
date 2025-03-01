using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
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
using Image = MigraDoc.DocumentObjectModel.Shapes.Image;

namespace DerbyApp.RacerDatabase
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

        static Document CreateDocument(Racer r, List<RaceResults> races, string eventName, bool timeBasedScoring)
        {
            Document document = new();
            DefineStyles(document);
            Section section = document.AddSection();
            Image image = section.Headers.Primary.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(Properties.Resources.GIRL3))));
            image.Height = "2.5cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.Through;

            Paragraph paragraph = section.Headers.Primary.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddFormattedText("\r\n", "Normal");
            paragraph.AddFormattedText(eventName, "Heading1");

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceAfter = 30;

            Table table = section.AddTable();
            Column column = table.AddColumn("8cm");
            column = table.AddColumn("8cm");
            table.Borders.Width = 0;

            Row row = table.AddRow();
            paragraph = row.Cells[0].AddParagraph();
            row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            image = paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(r.Photo, new Size(224, 168)))));

            paragraph = row.Cells[1].AddParagraph();
            row.Cells[1].VerticalAlignment = VerticalAlignment.Top;
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            paragraph.AddFormattedText("Name: " + r.RacerName + "\r\n", "Heading1");
            paragraph.AddFormattedText("Troop: " + r.Troop + "\r\n", "Heading2");
            paragraph.AddFormattedText("Level: " + r.Level + "\r\n", "Heading2");
            paragraph.AddFormattedText("Car Weight: " + r.Weight.ToString() + "\r\n", "Heading3");
            table.SetEdge(0, 0, 1, 1, Edge.Box, BorderStyle.Single, 2);
            table.SetEdge(0, 0, 2, 1, Edge.Box, BorderStyle.Single, 2);

            foreach (RaceResults result in races)
            {
                Leaderboard ldr = new(result.Racers, result.RaceFormat.HeatCount, result.RaceFormat.LaneCount, timeBasedScoring);
                ldr.CalculateResults(result.ResultsTable);
                List<Racer> raceResults;
                if (timeBasedScoring) raceResults = [.. ldr.Board.OrderBy(x => x.Score)];
                else raceResults = [.. ldr.Board.OrderByDescending(x => x.Score)];
                int racerPosition = raceResults.FindIndex(x => x.Number == r.Number);
                if (racerPosition > -1)
                {
                    racerPosition = raceResults.FindIndex(x => x.Score == raceResults[racerPosition].Score);

                    row = table.AddRow();
                    table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 2);
                    paragraph = row.Cells[0].AddParagraph();
                    row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
                    paragraph.AddFormattedText("Race Name: " + result.RaceName + "\r\n", "Heading3");
                    try
                    {
                        bool tie = false;
                        if (raceResults.Where(x => x.Score == raceResults[racerPosition].Score).Count() > 1)
                        {
                            tie = true;
                        }
                        paragraph.AddFormattedText("Overall Race Finish: " + (1 + racerPosition) + (tie ? " (Tie)" : "") + "\r\n", "Normal");

                        DataRow resultRow = result.ResultsTable.Select("Number = " + r.Number)[0];
                        DataRow scoreRow = ldr.RaceScoreTable.Select("Number = " + r.Number)[0];
                        for (int i = 0; i < result.RaceFormat.HeatCount; i++)
                        {
                            if (resultRow[i + 2] != DBNull.Value)
                            {
                                paragraph.AddFormattedText("Heat " + (i + 1) + " Time: " + ((double)resultRow[i + 2]).ToString("0.000") + " seconds (" + (1 + result.RaceFormat.LaneCount - (int)scoreRow[i + 2]) + ")\r\n", "Normal");
                            }
                        }
                    }
                    catch { }

                    switch (racerPosition)
                    {
                        case 0:
                            paragraph = row.Cells[1].AddParagraph();
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                            image = paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(Properties.Resources.first))));
                            image.Height = "2.5cm";
                            image.LockAspectRatio = true;
                            break;
                        case 1:
                            paragraph = row.Cells[1].AddParagraph();
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                            image = paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(Properties.Resources.second))));
                            image.Height = "2.5cm";
                            image.LockAspectRatio = true;
                            break;
                        case 2:
                            paragraph = row.Cells[1].AddParagraph();
                            row.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                            image = paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(Properties.Resources.third))));
                            image.Height = "2.5cm";
                            image.LockAspectRatio = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            return document;
        }

        static public void Generate(string eventName, string eventFile, string outputFolderName, ObservableCollection<Racer> racers, List<RaceResults> races, bool timeBasedScoring)
        {
            string eventPath = Path.Combine(outputFolderName, Path.GetFileNameWithoutExtension(eventFile), "reports");
            Directory.CreateDirectory(eventPath);
            foreach (Racer r in racers)
            {
                Document document = CreateDocument(r, races, eventName, timeBasedScoring);
                PdfDocumentRenderer pdfRenderer = new()
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

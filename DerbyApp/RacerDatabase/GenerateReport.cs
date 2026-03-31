#warning 3-REPORT: Can I make reports go to Google Drive?
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using static DerbyApp.RaceStats.Leaderboard;
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

        static Document CreateDocument(Leaderboard.RacerResults r, Database db)
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
            paragraph.AddFormattedText(db.EventName, "Heading1");

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceAfter = 30;

            Table table = section.AddTable();
            table.AddColumn("8cm");
            table.AddColumn("8cm");
            table.Borders.Width = 0;

            Row row = table.AddRow();
            paragraph = row.Cells[0].AddParagraph();
            row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(r.Racer.Photo, new Size(224, 168)))));

            paragraph = row.Cells[1].AddParagraph();
            row.Cells[1].VerticalAlignment = VerticalAlignment.Top;
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            paragraph.AddFormattedText("Name: " + r.Racer.RacerName + "\r\n", "Heading1");
            paragraph.AddFormattedText("Troop: " + r.Racer.Troop + "\r\n", "Heading2");
            paragraph.AddFormattedText("Level: " + r.Racer.Level + "\r\n", "Heading2");
            paragraph.AddFormattedText("Car Weight: " + r.Racer.Weight.ToString() + "\r\n", "Heading3");
            table.SetEdge(0, 0, 1, 1, Edge.Box, BorderStyle.Single, 2);
            table.SetEdge(0, 0, 2, 1, Edge.Box, BorderStyle.Single, 2);

            foreach (Leaderboard.RaceResults result in r.Results)
            {
                int racerPosition = result.OverallPosition;
                if (racerPosition > -1)
                {
                    row = table.AddRow();
                    table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 2);
                    paragraph = row.Cells[0].AddParagraph();
                    row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
                    paragraph.AddFormattedText("Race Name: " + result.RaceName + "\r\n", "Heading3");
                    paragraph.AddFormattedText("Overall Race Finish: " + (1 + racerPosition) + (result.Tie ? " (Tie)" : "") + "\r\n", "Normal");
                    for (int i = 0; i < result.HeatResults.Count; i++)
                    {
                        paragraph.AddFormattedText("Heat " + result.HeatResults[i].HeatNumber + " Time: " + result.HeatResults[i].Time.ToString("0.000") + " seconds (" + result.HeatResults[i].Position + ")\r\n", "Normal");
                    }

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

        static private List<RacerResults> GetResultsForRaces(Database db)
        {
            List<RacerResults> racerResults = [];
            foreach (string raceName in db.Races)
            {
                db.UiUpdateOnRaceChange = false;

                db.CurrentRaceName = raceName;
                db.LdrBoard.CalculateResults(db.ResultsTable);
                foreach (Racer r in db.Racers)
                {
                    RacerResults rr;
                    int i = racerResults.FindIndex(racerResults => racerResults.Racer == r);
                    if (i >= 0)
                    {
                        rr = racerResults[i];
                    }
                    else
                    {
                        rr = new() { Racer = r, Results = [] };
                        racerResults.Add(rr);
                    }

                    RaceResults results = db.LdrBoard.GetRacerResults(r, db.ResultsTable);
                    results.RaceName = raceName;
                    rr.Results.Add(results);
                }

                db.UiUpdateOnRaceChange = true;
            }
            return racerResults;
        }

        static public void Generate(Database db)
        {
            string reportFolder = Path.Combine(db.EventFolderName, "reports");
            Directory.CreateDirectory(reportFolder);
            List<RacerResults> racerResults = GetResultsForRaces(db);

            foreach (Racer r in db.Racers)
            {
                Document document = CreateDocument(racerResults.Find(racerResults => racerResults.Racer == r), db);
                PdfDocumentRenderer pdfRenderer = new()
                {
                    Document = document
                };
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                pdfRenderer.RenderDocument();

                pdfRenderer.PdfDocument.Save(Path.Combine(reportFolder, r.RacerName + ".pdf"));
            }
        }
    }
}

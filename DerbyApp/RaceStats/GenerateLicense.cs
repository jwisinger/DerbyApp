using DerbyApp.Helpers;
using GemBox.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using QRCoder;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using Image = MigraDoc.DocumentObjectModel.Shapes.Image;

namespace DerbyApp.RaceStats
{
    internal class GenerateLicense
    {
        static Document CreateQrCode(QRCode qr)
        {
            Document document = new();

            Section section = document.AddSection();
            section.PageSetup.PageHeight = Unit.FromInch(1);
            section.PageSetup.PageWidth = Unit.FromInch(2);
            section.PageSetup.TopMargin = Unit.FromInch(0.1);
            section.PageSetup.BottomMargin = Unit.FromInch(0.1);
            section.PageSetup.LeftMargin = Unit.FromInch(0.1);
            section.PageSetup.RightMargin = Unit.FromInch(0.1);

            Table table = section.AddTable();
            table.AddColumn(Unit.FromInch(2));

            Row row = table.AddRow();
            Paragraph paragraph = row.Cells[0].AddParagraph();
            row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;
            Image image = paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(qr.GetGraphic(20))));
            image.Width = Unit.FromInch(0.75);
            image.LockAspectRatio = true;

            return document;
        }

        static Document CreateLicense(Racer r)
        {
            Document document = new();
            
            Section section = document.AddSection();
            section.PageSetup.PageHeight = Unit.FromInch(2.125);
            section.PageSetup.PageWidth = Unit.FromInch(4.0);
            section.PageSetup.TopMargin = Unit.FromInch(0.2);
            section.PageSetup.BottomMargin = Unit.FromInch(0.2);
            section.PageSetup.LeftMargin = Unit.FromInch(0.2);
            section.PageSetup.RightMargin = Unit.FromInch(0.2);

            Table table = section.AddTable();
            table.AddColumn(Unit.FromInch(1.75));
            table.AddColumn(Unit.FromInch(1.95));
            table.Borders.Width = 0;

            Row row = table.AddRow();
            Paragraph paragraph = row.Cells[0].AddParagraph();
            paragraph.AddFormattedText("Driver License\r\n", TextFormat.Bold);
            row.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Center;
            paragraph = row.Cells[0].AddParagraph();
            paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(r.Photo, new Size(168, 126)))));

            Image image;
            paragraph = row.Cells[1].AddParagraph();
            image = paragraph.AddImage(ImageHandler.LoadImageFromBytes(ImageHandler.ImageToByteArray(new Bitmap(Properties.Resources.GIRL3))));
            image.Width = Unit.FromInch(1.8);
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.Through;

            paragraph = row.Cells[1].AddParagraph();
            row.Cells[1].VerticalAlignment = VerticalAlignment.Top;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddFormattedText("Name:\r\n", TextFormat.Bold);
            paragraph.AddFormattedText(r.RacerName + "\r\n", "Normal");
            paragraph.AddFormattedText("Troop:\r\n", TextFormat.Bold);
            paragraph.AddFormattedText(r.Troop + "\r\n", "Normal");
            paragraph.AddFormattedText("Level:\r\n", TextFormat.Bold);
            paragraph.AddFormattedText(r.Level + "\r\n", "Normal");
            table.SetEdge(0, 0, 1, 1, Edge.Box, BorderStyle.Single, 2);
            table.SetEdge(0, 0, 2, 1, Edge.Box, BorderStyle.Single, 2);

            return document;
        }

        static public void Generate(Racer racer, string eventFile, string outputFolderName, string qrCodeLink, string licensePrinter, string qrPrinter)
        {
            string eventPath = Path.Combine(outputFolderName, Path.GetFileNameWithoutExtension(eventFile), "licenses");
            Directory.CreateDirectory(eventPath);
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            Document document = CreateLicense(racer);
            PdfDocumentRenderer pdfRenderer = new() { Document = document };
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(Path.Combine(eventPath, racer.RacerName + ".pdf"));
            using var pdfDoc = PdfDocument.Load(Path.Combine(eventPath, racer.RacerName + ".pdf"));
            pdfDoc.Print(licensePrinter);

            document = CreateQrCode(new(new QRCodeGenerator().CreateQrCode(qrCodeLink, QRCodeGenerator.ECCLevel.M)));
            pdfRenderer = new() { Document = document };
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(Path.Combine(eventPath, "qr.pdf"));
            using var pdfDoc2 = PdfDocument.Load(Path.Combine(eventPath, "qr.pdf"));
            pdfDoc2.Print(qrPrinter);

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(eventPath, /*racer.RacerName + "*/"qr.pdf"))
                {
                    UseShellExecute = true
                }
            };
            p.Start();
        }
    }
}

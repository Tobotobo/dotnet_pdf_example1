using System;
using System.IO;
using iText.Commons.Utils;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

class Watermarking
{
    public static void ManipulatePdf(String dest)
    {
        PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest));
        Document doc = new Document(pdfDoc);
        pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkingEventHandler());

        PdfFont font = PdfFontFactory.CreateFont("./font/Noto_Sans_JP/static/NotoSansJP-Regular.ttf");
        PdfFont bold = PdfFontFactory.CreateFont("./font/Noto_Sans_JP/static/NotoSansJP-Bold.ttf");

        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 3 })).UseAllAvailableWidth();

        String line = "あ;い;う;え;お;";
        ParseTextLine(table, line, bold, true);
        foreach (var i in Enumerable.Range(0, 30))
        {
            ParseTextLine(table, $"あ{i};い{i};う{i};え{i};お{i};", font, false);
        }

        doc.Add(table);

        doc.Close();
    }

    private static void ParseTextLine(Table table, String line, PdfFont font, bool isHeader)
    {
        StringTokenizer tokenizer = new StringTokenizer(line, ";");
        int c = 0;
        while (tokenizer.HasMoreTokens() && c++ < 3)
        {
            Cell cell = new Cell().Add(new Paragraph(tokenizer.NextToken()).SetFont(font));
            if (isHeader)
            {
                table.AddHeaderCell(cell);
            }
            else
            {
                table.AddCell(cell);
            }
        }
    }

    private class WatermarkingEventHandler : IEventHandler
    {
        public void HandleEvent(Event currentEvent)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            PdfFont? font = null;
            try
            {
                font = PdfFontFactory.CreateFont("./font/Noto_Sans_JP/static/NotoSansJP-Bold.ttf");
            }
            catch (IOException e)
            {

                // Such an exception isn't expected to occur,
                // because helvetica is one of standard fonts
                Console.Error.WriteLine(e.Message);
            }

            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
            new Canvas(canvas, page.GetPageSize())
                .SetFontColor(ColorConstants.LIGHT_GRAY)
                .SetFontSize(60)

                // If the exception has been thrown, the font variable is not initialized.
                // Therefore null will be set and iText will use the default font - Helvetica
                .SetFont(font)
                .ShowTextAligned(new Paragraph("これは透かしだよ"), 298, 421, pdfDoc.GetPageNumber(page),
                    TextAlignment.CENTER, VerticalAlignment.MIDDLE, 45)
                .Close();
        }
    }
}
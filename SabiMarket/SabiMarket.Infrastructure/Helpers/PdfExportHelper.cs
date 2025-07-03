using iTextSharp.text.pdf;
using iTextSharp.text;
using SabiMarket.Application.DTOs.Requests;

public static class PdfExportHelper
{
    public static async Task<byte[]> GenerateMarketReport(ReportExportDto reportData)
    {
        return await Task.Run(() => {
            using var memoryStream = new MemoryStream();

            // Create PDF document
            var document = new Document(PageSize.A4, 36, 36, 54, 36);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            // Add metadata
            document.AddTitle("Market Performance Report");
            document.AddCreator("SabiMarket System");
            document.Open();

            // Define fonts
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            // Add title
            var title = new Paragraph("Market Performance Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // Add date range
            var dateRange = new Paragraph($"Period: {reportData.StartDate:d MMM yyyy} - {reportData.EndDate:d MMM yyyy}", normalFont);
            dateRange.Alignment = Element.ALIGN_CENTER;
            dateRange.SpacingAfter = 20;
            document.Add(dateRange);

            // Add overview section header
            var overviewHeader = new Paragraph("Overview Metrics", headerFont);
            overviewHeader.SpacingBefore = 15;
            overviewHeader.SpacingAfter = 10;
            document.Add(overviewHeader);

            // Add overview metrics table
            var overviewTable = new PdfPTable(2);
            overviewTable.WidthPercentage = 100;
            overviewTable.SetWidths(new float[] { 1, 2 });

            // Add table cells for metrics
            AddMetricRow(overviewTable, "Total Markets", reportData.TotalMarkets.ToString(), boldFont, normalFont);
            AddMetricRow(overviewTable, "Total Revenue", $"₦{reportData.TotalRevenue:N2}", boldFont, normalFont);
            AddMetricRow(overviewTable, "Total Traders", reportData.TotalTraders.ToString(), boldFont, normalFont);
            AddMetricRow(overviewTable, "Trader Compliance Rate", $"{reportData.TraderComplianceRate:F1}%", boldFont, normalFont);
            AddMetricRow(overviewTable, "Total Transactions", reportData.TotalTransactions.ToString(), boldFont, normalFont);
            AddMetricRow(overviewTable, "Daily Average Revenue", $"₦{reportData.DailyAverageRevenue:N2}", boldFont, normalFont);

            document.Add(overviewTable);

            // Add market details section
            var marketDetailsHeader = new Paragraph("Market Performance Details", headerFont);
            marketDetailsHeader.SpacingBefore = 20;
            marketDetailsHeader.SpacingAfter = 10;
            document.Add(marketDetailsHeader);

            // Create market details table
            var marketTable = new PdfPTable(6);
            marketTable.WidthPercentage = 100;
            marketTable.SetWidths(new float[] { 2, 2, 1, 1.5f, 1.5f, 1 });

            // Add table headers
            string[] headers = { "Market Name", "Location", "Traders", "Revenue", "Compliance Rate", "Transactions" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, boldFont));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                marketTable.AddCell(cell);
            }

            // Add market data
            foreach (var market in reportData.MarketDetails)
            {
                marketTable.AddCell(new Phrase(market.MarketName, normalFont));
                marketTable.AddCell(new Phrase(market.Location ?? "", normalFont));

                var tradersCell = new PdfPCell(new Phrase(market.TotalTraders.ToString(), normalFont));
                tradersCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                marketTable.AddCell(tradersCell);

                var revenueCell = new PdfPCell(new Phrase($"₦{market.Revenue:N2}", normalFont));
                revenueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                marketTable.AddCell(revenueCell);

                var complianceCell = new PdfPCell(new Phrase($"{market.ComplianceRate:F1}%", normalFont));
                complianceCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                marketTable.AddCell(complianceCell);

                var transactionsCell = new PdfPCell(new Phrase(market.TransactionCount.ToString(), normalFont));
                transactionsCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                marketTable.AddCell(transactionsCell);
            }

            document.Add(marketTable);

            // Add payment methods section
            if (reportData.RevenueByPaymentMethod != null && reportData.RevenueByPaymentMethod.Any())
            {
                document.NewPage();

                var paymentHeader = new Paragraph("Payment Methods Analysis", headerFont);
                paymentHeader.SpacingBefore = 15;
                paymentHeader.SpacingAfter = 10;
                document.Add(paymentHeader);

                var paymentTable = new PdfPTable(3);
                paymentTable.WidthPercentage = 100;
                paymentTable.SetWidths(new float[] { 2, 1, 1 });

                // Add table headers
                string[] paymentHeaders = { "Payment Method", "Amount", "Percentage" };
                foreach (var header in paymentHeaders)
                {
                    var cell = new PdfPCell(new Phrase(header, boldFont));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    paymentTable.AddCell(cell);
                }

                // Calculate total
                decimal total = reportData.RevenueByPaymentMethod.Values.Sum();

                // Add payment data
                foreach (var method in reportData.RevenueByPaymentMethod)
                {
                    paymentTable.AddCell(new Phrase(method.Key, normalFont));

                    var amountCell = new PdfPCell(new Phrase($"₦{method.Value:N2}", normalFont));
                    amountCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    paymentTable.AddCell(amountCell);

                    decimal percentage = total > 0 ? (method.Value / total) * 100 : 0;
                    var percentCell = new PdfPCell(new Phrase($"{percentage:F1}%", normalFont));
                    percentCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    paymentTable.AddCell(percentCell);
                }

                document.Add(paymentTable);
            }

            // Close the document
            document.Close();

            // Return the PDF bytes
            return memoryStream.ToArray();
        });
    }

    private static void AddMetricRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        var labelCell = new PdfPCell(new Phrase(label, labelFont));
        labelCell.Border = Rectangle.NO_BORDER;
        labelCell.PaddingBottom = 5;
        table.AddCell(labelCell);

        var valueCell = new PdfPCell(new Phrase(value, valueFont));
        valueCell.Border = Rectangle.NO_BORDER;
        valueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
        valueCell.PaddingBottom = 5;
        table.AddCell(valueCell);
    }
}
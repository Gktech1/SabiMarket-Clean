using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using SabiMarket.Application.DTOs.Requests;
using LicenseContext = OfficeOpenXml.LicenseContext;
using OfficeOpenXml.Drawing.Chart;

namespace SabiMarket.Infrastructure.Helpers
{
    public static class ExcelExportHelper
    {
        static ExcelExportHelper()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public static async Task<byte[]> GenerateMarketReport(ReportExportDto reportData)
        {
            using var package = new ExcelPackage();

            var summarySheet = CreateSummarySheet(package, reportData);
            var paymentSheet = CreatePaymentAnalysisSheet(package, reportData);

            // Add charts matching the UI
            AddRevenueChart(summarySheet, reportData);

            // Only add compliance charts if data exists
            if (reportData.ComplianceByMarket != null && reportData.ComplianceByMarket.Any())
            {
                AddComplianceRateCharts(package.Workbook.Worksheets.Add("Compliance"), reportData);
            }

            // Only add levy collection donut if market details exist
            if (reportData.MarketDetails != null && reportData.MarketDetails.Any())
            {
                AddLevyCollectionDonut(package.Workbook.Worksheets.Add("Levy Collection"), reportData);
            }

            // Auto-fit columns in all sheets - with null check
            foreach (var sheet in package.Workbook.Worksheets)
            {
                if (sheet.Dimension != null)
                {
                    try
                    {
                        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception - don't let auto-fit failure crash the export
                        System.Diagnostics.Debug.WriteLine($"Error auto-fitting columns in sheet {sheet.Name}: {ex.Message}");
                    }
                }
            }

            return await package.GetAsByteArrayAsync();
        }

        /*public static async Task<byte[]> GenerateMarketReport(ReportExportDto reportData)
        {
            using var package = new ExcelPackage();

            var summarySheet = CreateSummarySheet(package, reportData);
            var paymentSheet = CreatePaymentAnalysisSheet(package, reportData);

            // Auto-fit columns in both sheets
            summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();
            paymentSheet.Cells[paymentSheet.Dimension.Address].AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }*/

        /*public static async Task<byte[]> GenerateMarketReport(ReportExportDto reportData)
        {
            using var package = new ExcelPackage();

            var summarySheet = CreateSummarySheet(package, reportData);
            var paymentSheet = CreatePaymentAnalysisSheet(package, reportData);

            // Add these new methods to create charts matching the UI
            AddRevenueChart(summarySheet, reportData);
            AddComplianceRateCharts(package.Workbook.Worksheets.Add("Compliance"), reportData);
            AddLevyCollectionDonut(package.Workbook.Worksheets.Add("Levy Collection"), reportData);

            // Auto-fit columns in all sheets
            foreach (var sheet in package.Workbook.Worksheets)
            {
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            }

            return await package.GetAsByteArrayAsync();
        }*/

        private static ExcelWorksheet CreateSummarySheet(ExcelPackage package, ReportExportDto reportData)
        {
            var summarySheet = package.Workbook.Worksheets.Add("Summary");

            // Add title and date range
            summarySheet.Cells[1, 1].Value = "Market Performance Report";
            summarySheet.Cells[2, 1].Value = $"Period: {reportData.StartDate:d MMM yyyy} - {reportData.EndDate:d MMM yyyy}";

            // Style the header
            StyleHeader(summarySheet.Cells[1, 1, 1, 6]);

            // Add Overview Section
            int currentRow = 4;
            AddSectionHeader(summarySheet, currentRow, "Overview Metrics");

            currentRow += 2;
            AddMetricRows(summarySheet, ref currentRow, reportData);

            // Add Market Details Table
            currentRow += 2;
            AddSectionHeader(summarySheet, currentRow, "Market Performance Details");
            currentRow += 2;

            AddMarketDetailsTable(summarySheet, ref currentRow, reportData);

            return summarySheet;
        }

        private static void StyleHeader(ExcelRange headerRange)
        {
            headerRange.Merge = true;
            headerRange.Style.Font.Size = 16;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(48, 84, 150));
            headerRange.Style.Font.Color.SetColor(Color.White);
        }

        private static void AddSectionHeader(ExcelWorksheet sheet, int row, string headerText)
        {
            sheet.Cells[row, 1].Value = headerText;
            using var range = sheet.Cells[row, 1, row, 6];
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 225, 242));
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        private static void AddMetricRows(ExcelWorksheet sheet, ref int currentRow, ReportExportDto reportData)
        {
            AddMetricRow(sheet, currentRow++, "Total Markets", reportData.TotalMarkets);
            AddMetricRow(sheet, currentRow++, "Total Revenue", reportData.TotalRevenue.ToString("C"));
            AddMetricRow(sheet, currentRow++, "Total Traders", reportData.TotalTraders);
            AddMetricRow(sheet, currentRow++, "Trader Compliance Rate", $"{reportData.TraderComplianceRate:F1}%");
            AddMetricRow(sheet, currentRow++, "Total Transactions", reportData.TotalTransactions);
            AddMetricRow(sheet, currentRow++, "Daily Average Revenue", reportData.DailyAverageRevenue.ToString("C"));
        }

        private static void AddMetricRow(ExcelWorksheet sheet, int row, string label, object value)
        {
            sheet.Cells[row, 1].Value = label;
            sheet.Cells[row, 2].Value = value;

            sheet.Cells[row, 1].Style.Font.Bold = true;

            if (value is int or decimal or double)
            {
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
        }

        private static void AddMarketDetailsTable(ExcelWorksheet sheet, ref int currentRow, ReportExportDto reportData)
        {
            string[] headers = { "Market Name", "Location", "Traders", "Revenue", "Compliance Rate", "Transactions" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[currentRow, i + 1].Value = headers[i];
                FormatTableHeader(sheet.Cells[currentRow, i + 1]);
            }

            currentRow++;
            AddMarketDetails(sheet, ref currentRow, reportData.MarketDetails);
        }

        private static void AddMarketDetails(ExcelWorksheet sheet, ref int currentRow, List<ReportExportDto.MarketSummary> marketDetails)
        {
            foreach (var market in marketDetails)
            {
                sheet.Cells[currentRow, 1].Value = market.MarketName;
                sheet.Cells[currentRow, 2].Value = market.Location;
                sheet.Cells[currentRow, 3].Value = market.TotalTraders;
                sheet.Cells[currentRow, 4].Value = market.Revenue;
                sheet.Cells[currentRow, 5].Value = $"{market.ComplianceRate:F1}%";
                sheet.Cells[currentRow, 6].Value = market.TransactionCount;

                sheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00";
                currentRow++;
            }
        }

        private static void FormatTableHeader(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(189, 215, 238));
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        private static ExcelWorksheet CreatePaymentAnalysisSheet(ExcelPackage package, ReportExportDto reportData)
        {
            var sheet = package.Workbook.Worksheets.Add("Payment Analysis");

            sheet.Cells[1, 1].Value = "Payment Methods Analysis";
            sheet.Cells[2, 1].Value = $"Period: {reportData.StartDate:d MMM yyyy} - {reportData.EndDate:d MMM yyyy}";

            using (var headerRange = sheet.Cells[1, 1, 1, 3])
            {
                headerRange.Merge = true;
                headerRange.Style.Font.Size = 14;
                headerRange.Style.Font.Bold = true;
            }

            AddPaymentMethodsTable(sheet, reportData);
            return sheet;
        }

        private static void AddPaymentMethodsTable(ExcelWorksheet sheet, ReportExportDto reportData)
        {
            int currentRow = 4;

            // Add headers
            sheet.Cells[currentRow, 1].Value = "Payment Method";
            sheet.Cells[currentRow, 2].Value = "Amount";
            sheet.Cells[currentRow, 3].Value = "Percentage";

            FormatTableHeader(sheet.Cells[currentRow, 1]);
            FormatTableHeader(sheet.Cells[currentRow, 2]);
            FormatTableHeader(sheet.Cells[currentRow, 3]);

            currentRow++;

            // Check if RevenueByPaymentMethod is null or empty
            if (reportData.RevenueByPaymentMethod == null || !reportData.RevenueByPaymentMethod.Any())
            {
                // Add a "No data available" row
                sheet.Cells[currentRow, 1].Value = "No payment method data available";
                sheet.Cells[currentRow, 1, currentRow, 3].Merge = true;
                sheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, 1].Style.Font.Italic = true;
                return;
            }

            decimal total = reportData.RevenueByPaymentMethod.Values.Sum();
            foreach (var method in reportData.RevenueByPaymentMethod)
            {
                sheet.Cells[currentRow, 1].Value = method.Key;
                sheet.Cells[currentRow, 2].Value = method.Value;
                sheet.Cells[currentRow, 3].Value = total > 0 ? (method.Value / total) * 100 : 0;

                sheet.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                sheet.Cells[currentRow, 3].Style.Numberformat.Format = "0.0%";

                currentRow++;
            }
        }

        private static void AddRevenueChart(ExcelWorksheet sheet, ReportExportDto reportData)
        {

            if (reportData.RevenueByMonth == null || !reportData.RevenueByMonth.Any() ||
                reportData.RevenueByMonth.First().MonthlyData == null ||
                !reportData.RevenueByMonth.First().MonthlyData.Any())
            {
                return; // No data to chart
            }

            // Create a data area for the chart first
            int startRow = sheet.Dimension.End.Row + 2;
            int currentRow = startRow;

            // Add a header for months (X-axis labels)
            int col = 2;
            var firstMarket = reportData.RevenueByMonth.First();
            for (int i = 0; i < firstMarket.MonthlyData.Count; i++)
            {
                sheet.Cells[currentRow, col + i].Value = firstMarket.MonthlyData[i].Month;
            }

            // Add data for each market
            currentRow++;
            foreach (var market in reportData.RevenueByMonth)
            {
                sheet.Cells[currentRow, 1].Value = market.MarketName;

                for (int i = 0; i < market.MonthlyData.Count; i++)
                {
                    sheet.Cells[currentRow, col + i].Value = market.MonthlyData[i].Revenue;
                }

                currentRow++;
            }

            // Create a chart from the data we just added
            var chart = sheet.Drawings.AddChart("RevenueChart", eChartType.Line);
            chart.Title.Text = "Levy Payments Breakdown";
            chart.SetPosition(currentRow + 2, 0, 1, 0);
            chart.SetSize(800, 400);

            // Add a series for each market
            for (int i = 0; i < reportData.RevenueByMonth.Count; i++)
            {
                var marketRow = startRow + 1 + i;
                var marketName = reportData.RevenueByMonth[i].MarketName;

                // Values range (revenue data)
                var valuesRange = sheet.Cells[marketRow, col, marketRow, col + firstMarket.MonthlyData.Count - 1];

                // Labels range (month names)
                var labelsRange = sheet.Cells[startRow, col, startRow, col + firstMarket.MonthlyData.Count - 1];

                // Add the series with proper ExcelRangeBase objects
                var series = chart.Series.Add(valuesRange, labelsRange);
                series.Header = marketName;
            }

            // Add axis titles
            chart.XAxis.Title.Text = "Month";
            chart.YAxis.Title.Text = "₦";
        }

        private static void AddComplianceRateCharts(ExcelWorksheet sheet, ReportExportDto reportData)
        {
            if (reportData.ComplianceByMarket == null || !reportData.ComplianceByMarket.Any())
                return;

            int startRow = sheet.Dimension.End.Row + 5;

            // Add section header with better styling
            AddSectionHeader(sheet, startRow, "Compliance Rates");
            startRow += 2;

            // First, add the data to the worksheet with improved formatting
            sheet.Cells[startRow, 1].Value = "Market";
            sheet.Cells[startRow, 2].Value = "Compliance Rate (%)";

            // Style the headers
            var headerRange = sheet.Cells[startRow, 1, startRow, 2];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(189, 215, 238));
            headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            int currentRow = startRow + 1;
            foreach (var market in reportData.ComplianceByMarket)
            {
                sheet.Cells[currentRow, 1].Value = market.MarketName;
                sheet.Cells[currentRow, 2].Value = market.CompliancePercentage / 100; // Convert to decimal for percentage format

                // Format percentages properly
                sheet.Cells[currentRow, 2].Style.Numberformat.Format = "0.0%";

                // Add conditional formatting - for example, color low compliance rates in red
                if (market.CompliancePercentage < 50)
                {
                    sheet.Cells[currentRow, 2].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0)); // Dark red
                }
                else if (market.CompliancePercentage >= 90)
                {
                    sheet.Cells[currentRow, 2].Style.Font.Color.SetColor(Color.FromArgb(0, 128, 0)); // Dark green
                }

                currentRow++;
            }

            // Add a pie chart for compliance rates
            var pieChart = sheet.Drawings.AddChart("ComplianceChart", eChartType.Pie) as ExcelPieChart;
            if (pieChart != null)
            {
                pieChart.Title.Text = "Compliance Rates by Market";
                pieChart.SetPosition(currentRow + 1, 0, 1, 0);
                pieChart.SetSize(500, 350);

                // Add the series
                var series = pieChart.Series.Add(
                    sheet.Cells[startRow + 1, 2, currentRow - 1, 2],
                    sheet.Cells[startRow + 1, 1, currentRow - 1, 1]
                );
            }

            // Add a legend/note explaining the colors
            currentRow += 20; // Move down below the chart
            sheet.Cells[currentRow, 1].Value = "Note: ";
            sheet.Cells[currentRow, 1].Style.Font.Bold = true;
            sheet.Cells[currentRow, 2].Value = "Compliance rates below 50% are highlighted in red, above 90% in green.";
            sheet.Cells[currentRow, 2, currentRow, 5].Merge = true;
            sheet.Cells[currentRow, 2].Style.Font.Italic = true;
        }

        private static void AddLevyCollectionDonut(ExcelWorksheet sheet, ReportExportDto reportData)
        {
            if (reportData.MarketDetails == null || !reportData.MarketDetails.Any())
                return;

            int startRow = sheet.Dimension.End.Row + 5;

            // Add section header with better styling
            AddSectionHeader(sheet, startRow, "Levy Collection Per Market");
            startRow += 2;

            // First, add the data to the worksheet with improved formatting
            sheet.Cells[startRow, 1].Value = "Market";
            sheet.Cells[startRow, 2].Value = "Revenue (₦)";
            sheet.Cells[startRow, 3].Value = "% of Total";

            // Style the headers
            var headerRange = sheet.Cells[startRow, 1, startRow, 3];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(189, 215, 238));
            headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            int currentRow = startRow + 1;
            decimal totalRevenue = reportData.MarketDetails.Sum(m => m.Revenue);

            foreach (var market in reportData.MarketDetails)
            {
                sheet.Cells[currentRow, 1].Value = market.MarketName;
                sheet.Cells[currentRow, 2].Value = market.Revenue;
                sheet.Cells[currentRow, 2].Style.Numberformat.Format = "₦#,##0.00";

                // Calculate and format percentage of total
                decimal percentage = totalRevenue > 0 ? (market.Revenue / totalRevenue) : 0;
                sheet.Cells[currentRow, 3].Value = percentage;
                sheet.Cells[currentRow, 3].Style.Numberformat.Format = "0.0%";

                // Add a light gradient based on revenue contribution
                byte colorIntensity = (byte)(200 - Math.Min(150, (int)(percentage * 150)));
                sheet.Cells[currentRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[currentRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(colorIntensity, 255, colorIntensity));

                currentRow++;
            }

            // Add a summary row
            sheet.Cells[currentRow, 1].Value = "Total";
            sheet.Cells[currentRow, 1].Style.Font.Bold = true;
            sheet.Cells[currentRow, 2].Value = totalRevenue;
            sheet.Cells[currentRow, 2].Style.Numberformat.Format = "₦#,##0.00";
            sheet.Cells[currentRow, 2].Style.Font.Bold = true;
            sheet.Cells[currentRow, 3].Value = 1.0;
            sheet.Cells[currentRow, 3].Style.Numberformat.Format = "0.0%";
            sheet.Cells[currentRow, 3].Style.Font.Bold = true;

            // Add border to the summary row
            sheet.Cells[currentRow, 1, currentRow, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;

            currentRow += 2;

            // Add a doughnut chart
            var doughnutChart = sheet.Drawings.AddChart("LevyDonut", eChartType.Doughnut) as ExcelDoughnutChart;
            if (doughnutChart != null)
            {
                doughnutChart.Title.Text = "Levy Collection Per Market";
                doughnutChart.SetPosition(currentRow, 0, 1, 0);
                doughnutChart.SetSize(500, 350);

                // Add the series - use only the market names and revenue columns
                var series = doughnutChart.Series.Add(
                    sheet.Cells[startRow + 1, 2, currentRow - 4, 2],  // Revenue column
                    sheet.Cells[startRow + 1, 1, currentRow - 4, 1]   // Market names column
                );
            }

            // Add a summary table with top markets
            currentRow += 20; // Move down below the chart

            sheet.Cells[currentRow, 1].Value = "Top Markets by Revenue";
            sheet.Cells[currentRow, 1].Style.Font.Bold = true;
            sheet.Cells[currentRow, 1].Style.Font.Size = 12;
            sheet.Cells[currentRow, 1, currentRow, 3].Merge = true;
            currentRow++;

            // Sort markets by revenue
            var topMarkets = reportData.MarketDetails.OrderByDescending(m => m.Revenue).Take(3).ToList();

            for (int i = 0; i < topMarkets.Count; i++)
            {
                sheet.Cells[currentRow + i, 1].Value = $"{i + 1}. {topMarkets[i].MarketName}";
                sheet.Cells[currentRow + i, 2].Value = topMarkets[i].Revenue;
                sheet.Cells[currentRow + i, 2].Style.Numberformat.Format = "₦#,##0.00";

                // Add a bar visualization using cell background
                sheet.Cells[currentRow + i, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[currentRow + i, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
            }
        }
    }
}
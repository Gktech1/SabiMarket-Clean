using SabiMarket.Application.DTOs.Requests;

public static class CsvExportHelper
{
    public static async Task<byte[]> GenerateMarketReport(ReportExportDto reportData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);

        // Write market summary
        await writer.WriteLineAsync("Market Performance Report");
        await writer.WriteLineAsync($"Period: {reportData.StartDate:d MMM yyyy} - {reportData.EndDate:d MMM yyyy}");
        await writer.WriteLineAsync();

        // Write overview metrics
        await writer.WriteLineAsync("OVERVIEW METRICS");
        await writer.WriteLineAsync($"Total Markets,{reportData.TotalMarkets}");
        await writer.WriteLineAsync($"Total Revenue,{reportData.TotalRevenue:N2}");
        await writer.WriteLineAsync($"Total Traders,{reportData.TotalTraders}");
        await writer.WriteLineAsync($"Trader Compliance Rate,{reportData.TraderComplianceRate:F1}%");
        await writer.WriteLineAsync($"Total Transactions,{reportData.TotalTransactions}");
        await writer.WriteLineAsync($"Daily Average Revenue,{reportData.DailyAverageRevenue:N2}");
        await writer.WriteLineAsync();

        // Write market details
        await writer.WriteLineAsync("MARKET PERFORMANCE DETAILS");
        await writer.WriteLineAsync("Market Name,Location,Traders,Revenue,Compliance Rate,Transactions");

        foreach (var market in reportData.MarketDetails)
        {
            // Escape any commas in text fields
            string marketName = $"\"{market.MarketName.Replace("\"", "\"\"")}\"";
            string location = $"\"{(market.Location?.Replace("\"", "\"\"") ?? "")}\"";

            await writer.WriteLineAsync(
                $"{marketName},{location},{market.TotalTraders}," +
                $"{market.Revenue:N2},{market.ComplianceRate:F1}%,{market.TransactionCount}"
            );
        }
        await writer.WriteLineAsync();

        // Write payment methods analysis
        await writer.WriteLineAsync("PAYMENT METHODS ANALYSIS");
        await writer.WriteLineAsync("Payment Method,Amount,Percentage");

        decimal totalPayments = reportData.RevenueByPaymentMethod?.Values.Sum() ?? 0;
        if (reportData.RevenueByPaymentMethod != null)
        {
            foreach (var method in reportData.RevenueByPaymentMethod)
            {
                decimal percentage = totalPayments > 0 ? (method.Value / totalPayments) * 100 : 0;
                string methodName = $"\"{method.Key.Replace("\"", "\"\"")}\"";
                await writer.WriteLineAsync($"{methodName},{method.Value:N2},{percentage:F1}%");
            }
        }

        // Flush and return the CSV bytes
        await writer.FlushAsync();
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }
}
using DataImporter.Models;

namespace DataImporter.Interfaces
{
    public interface IMarketDataService
    {
        List<MarketData> ReadDataFromCsv(string filePath);

        decimal GetMinimumPrice(List<MarketData> data);

        decimal GetMaximumPrice(List<MarketData> data);

        decimal GetAveragePrice(List<MarketData> data);

        (decimal MaxHourPrice, DateTime StartHour) GetMostExpensiveHourWindow(List<MarketData> data);
    }
}
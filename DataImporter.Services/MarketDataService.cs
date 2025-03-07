using DataImporter.Interfaces;
using DataImporter.Models;
using CsvHelper;
using System.Globalization;

namespace DataImporter.Services
{
    public class MarketDataService : IMarketDataService
    {
        public List<MarketData> ReadDataFromCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("CSV file not found.", filePath);
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<MarketDataMap>();
                var marketDataList = new List<MarketData>();

                while (csv.Read())
                {
                    var record = csv.GetRecord<MarketData>();
                    marketDataList.Add(record);
                }

                return marketDataList;
            }
            catch (HeaderValidationException ex)
            {
                throw new InvalidDataException($"Invalid CSV header: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Unexpected error reading CSV file: {ex.Message}", ex);
            }
        }

        public decimal GetMinimumPrice(List<MarketData> data)
        {
            return data.Min(d => d.MarketPrice);
        }

        public decimal GetMaximumPrice(List<MarketData> data)
        {
            return data.Max(d => d.MarketPrice);
        }

        public decimal GetAveragePrice(List<MarketData> data)
        {
            return data.Average(d => d.MarketPrice);
        }

        public (decimal MaxHourPrice, DateTime StartHour) GetMostExpensiveHourWindow(List<MarketData> data)
        {
            decimal maxHourPrice = 0;
            var startHour = DateTime.MinValue;

            for (var i = 0; i < data.Count - 1; i++)
            {
                var currentHourPrice = data[i].MarketPrice + data[i + 1].MarketPrice;
                if (currentHourPrice <= maxHourPrice) continue;
                maxHourPrice = currentHourPrice;
                startHour = data[i].Date;
            }

            return (maxHourPrice, startHour);
        }
    }

    public sealed class MarketDataMap : CsvHelper.Configuration.ClassMap<MarketData>
    {
        public MarketDataMap()
        {
            Map(m => m.Date).Name("Date").TypeConverterOption.Format(new[]
            {
                "M/d/yyyy h:mm",
                "M/d/yyyy HH:mm",
                "M/d/yyyy",
                "d/M/yyyy",
                "d/M/yyyy h:mm",
                "d/M/yyyy HH:mm",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss"
            });

            Map(m => m.MarketPrice).Name("Market Price EX1").TypeConverterOption
                .CultureInfo(CultureInfo.InvariantCulture);
        }
    }
}
using DataImporter.Interfaces;
using DataImporter.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataImporter.WebApp.Controllers
{
    public class HomeController(IMarketDataService marketDataService, IWebHostEnvironment webHostEnvironment)
        : Controller
    {
        public IActionResult Index()
        {
            try
            {
                var csvFilePath = Path.Combine(webHostEnvironment.WebRootPath, "sampleSheet.csv");
                var marketDataList = marketDataService.ReadDataFromCsv(csvFilePath);

                ViewBag.MarketData = marketDataList;
                ViewBag.MinPrice = marketDataService.GetMinimumPrice(marketDataList);
                ViewBag.MaxPrice = marketDataService.GetMaximumPrice(marketDataList);
                ViewBag.AveragePrice = marketDataService.GetAveragePrice(marketDataList);

                var expensiveHour = marketDataService.GetMostExpensiveHourWindow(marketDataList);
                ViewBag.MostExpensiveHourPrice = expensiveHour.MaxHourPrice;
                ViewBag.MostExpensiveHourStart = expensiveHour.StartHour;
            }
            catch (FileNotFoundException ex)
            {
                ViewBag.ErrorMessage = $"File not found: {ex.Message}";
            }
            catch (InvalidDataException ex)
            {
                ViewBag.ErrorMessage = $"Data error: {ex.Message}";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
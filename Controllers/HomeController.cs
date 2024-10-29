using CarModelsApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Application.IServices;
using Domain.Car;

namespace CarModelsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICarService _carService;  

        public HomeController(ILogger<HomeController> logger, ICarService carService)
        {
            _logger = logger;
            _carService = carService;  
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicleDataResponse = await _carService.GetAllMakesAsync();
                var years = await _carService.GetAvailableYears(); 

                var model = new AllMakeAndYearsDto
                {
                    Makes = vehicleDataResponse.IsSuccess ? vehicleDataResponse.Results : new List<AllMakeDto>(),
                    Years = years.ToList() 
                };

                return View(model); 
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { Message = $"Error fetching data: {ex.Message}" });
            }
        }

        [HttpGet("Home/GetModelsByMake")]
        public async Task<IActionResult> GetModelsByMake(int makeId, int year, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var dataResponse = await _carService.GetModelsByMakeAsync(makeId, year);
                if (dataResponse.IsSuccess)
                {
                    if (dataResponse.Results != null && dataResponse.Results.Any())
                    {
                        var totalCount = dataResponse.Results.Count;
                        var paginatedModels = dataResponse.Results
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                        var paginatedList = new PaginatedList<ModelByMakeDto>(paginatedModels, totalCount, pageNumber, pageSize);
                        return PartialView("~/Views/Shared/CarsResultGrid.cshtml", paginatedList); // Ensure this returns the correct model
                    }
                    else
                    {
                        var emptyPaginatedList = new PaginatedList<ModelByMakeDto>(new List<ModelByMakeDto>(), 0, pageNumber, pageSize);
                        return PartialView("~/Views/Shared/CarsResultGrid.cshtml", emptyPaginatedList);
                    }
                }

                return BadRequest(dataResponse.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { Message = $"Error fetching data: {ex.Message}" });
            }
        }


        [HttpGet("Home/GetVehiclesByMake")]
        public async Task<IActionResult> GetVehiclesByMake(int makeId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var dataResponse = await _carService.GetVehiclesByMakeAsync(makeId);
                if (dataResponse.IsSuccess)
                {
                    if (dataResponse.Results != null && dataResponse.Results.Any())
                    {
                        var totalCount = dataResponse.Results.Count;
                        var paginatedVehicles = dataResponse.Results
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                        var paginatedList = new PaginatedList<VehicleByMakeDto>(paginatedVehicles, totalCount, pageNumber, pageSize);
                        return PartialView("~/Views/Shared/VehiclesResultGrid.cshtml", paginatedList); // Ensure this returns the correct model
                    }
                    else
                    {
                        var emptyPaginatedList = new PaginatedList<VehicleByMakeDto>(new List<VehicleByMakeDto>(), 0, pageNumber, pageSize);
                        return PartialView("~/Views/Shared/VehiclesResultGrid.cshtml", emptyPaginatedList);
                    }
                }

                return BadRequest(dataResponse.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { Message = $"Error fetching data: {ex.Message}" });
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using ElectricityOutagePortal.Models;
using ElectricityOutagePortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace ElectricityOutagePortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7208/";
        }

        public async Task<IActionResult> Index()
        {
            var model = new SearchViewModel();
            await LoadDropdownData(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            await LoadDropdownData(model);
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Build query parameters
                    var queryParams = new List<string>();
                    
                    if (model.SourceCutting.HasValue)
                        queryParams.Add($"channelId={model.SourceCutting}");
                    if (model.ProblemTypeKey.HasValue)
                        queryParams.Add($"cuttingDownProblemTypeKey={model.ProblemTypeKey}");
                    if (model.Status.HasValue)
                        queryParams.Add($"status={model.Status}");
                    if (model.NetworkElementTypeKey.HasValue)
                        queryParams.Add($"networkElementTypeKey={model.NetworkElementTypeKey}");
                    
                    queryParams.Add($"pageNumber={model.PageNumber}");
                    queryParams.Add($"pageSize={model.PageSize}");

                    var queryString = string.Join("&", queryParams);
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}api/CuttingDownHeader?{queryString}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var pagedResult = JsonSerializer.Deserialize<PagedResult<CuttingDownHeaderDto>>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (pagedResult != null)
                        {
                            model.PagedResults = pagedResult;
                            model.Results = pagedResult.Items;
                            model.TotalCount = pagedResult.TotalCount;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to retrieve data from the API.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while searching for cutting down incidents");
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> NetworkHierarchy()
        {
            var model = new NetworkHierarchyViewModel();
            await LoadNetworkHierarchyData(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NetworkHierarchy(NetworkHierarchyViewModel model)
        {
            await LoadNetworkHierarchyData(model);
            
            // Implement search logic here
            // This would call the appropriate API endpoints to get network hierarchy data
            
            return View(model);
        }

        public async Task<IActionResult> IgnoredOutages(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}api/Ignored/GetAllByPage?page={pageNumber}&pageSize={pageSize}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var pagedResult = JsonSerializer.Deserialize<PagedResult<CuttingDownHeaderDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(pagedResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ignored outages");
            }

            return View(new PagedResult<CuttingDownHeaderDto>());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIgnoredOutage(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}api/Ignored/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("IgnoredOutages");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting ignored outage {Id}", id);
            }

            TempData["Error"] = "Failed to delete the ignored incident.";
            return RedirectToAction("IgnoredOutages");
        }

        private async Task LoadDropdownData(SearchViewModel model)
        {
            try
            {
                // Load Sources (A/B)
                model.Sources = new List<SourceDto>
                {
                    new SourceDto { Id = 1, Name = "Source A" },
                    new SourceDto { Id = 2, Name = "Source B" }
                };

                // Load Problem Types
                var problemTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/ProblemType/GetAll");
                if (problemTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await problemTypesResponse.Content.ReadAsStringAsync();
                    model.ProblemTypes = JsonSerializer.Deserialize<List<ProblemTypeDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ProblemTypeDto>();
                }

                // Load Statuses
                model.Statuses = new List<StatusDto>
                {
                    new StatusDto { Id = 0, Name = "Open" },
                    new StatusDto { Id = 1, Name = "Closed" }
                };

                // Load Search Criteria
                model.SearchCriterias = new List<SearchCriteriaDto>
                {
                    new SearchCriteriaDto { Id = 1, Name = "City" },
                    new SearchCriteriaDto { Id = 2, Name = "Zone" },
                    new SearchCriteriaDto { Id = 3, Name = "Sector" },
                    new SearchCriteriaDto { Id = 4, Name = "All" }
                };

                // Load Network Element Types
                var networkTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/NetworkElementType/GetAll");
                if (networkTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await networkTypesResponse.Content.ReadAsStringAsync();
                    model.NetworkElementTypes = JsonSerializer.Deserialize<List<NetworkElementTypeDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<NetworkElementTypeDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading dropdown data");
            }
        }

        private async Task LoadNetworkHierarchyData(NetworkHierarchyViewModel model)
        {
            try
            {
                // Load Problem Types
                var problemTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/ProblemType/GetAll");
                if (problemTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await problemTypesResponse.Content.ReadAsStringAsync();
                    model.ProblemTypes = JsonSerializer.Deserialize<List<ProblemTypeDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ProblemTypeDto>();
                }

                // Load Network Element Types
                var networkTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/NetworkElementType/GetAll");
                if (networkTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await networkTypesResponse.Content.ReadAsStringAsync();
                    model.NetworkElementTypes = JsonSerializer.Deserialize<List<NetworkElementTypeDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<NetworkElementTypeDto>();
                }

                // Load Search Criteria
                model.SearchCriterias = new List<SearchCriteriaDto>
                {
                    new SearchCriteriaDto { Id = 1, Name = "City" },
                    new SearchCriteriaDto { Id = 2, Name = "Zone" },
                    new SearchCriteriaDto { Id = 3, Name = "Sector" },
                    new SearchCriteriaDto { Id = 4, Name = "All" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading network hierarchy data");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

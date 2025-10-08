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

            try
            {   
                var queryParams = new List<string>();

                if (model.SourceCutting.HasValue)
                    queryParams.Add($"sourceKey={model.SourceCutting}");
                if (model.ProblemTypeKey.HasValue)
                    queryParams.Add($"problemTypeKey={model.ProblemTypeKey}");
                if (model.Status.HasValue)
                    queryParams.Add($"statusKey={model.Status}");
                if (model.SearchCriteriaKey.HasValue)
                    queryParams.Add($"searchCriteriaKey={model.SearchCriteriaKey}");
                if (!string.IsNullOrWhiteSpace(model.SearchValue))
                    queryParams.Add($"searchValue={Uri.EscapeDataString(model.SearchValue)}");
                if (model.StartDate.HasValue)
                    queryParams.Add($"fromDate={model.StartDate:O}");
                if (model.EndDate.HasValue)
                    queryParams.Add($"toDate={model.EndDate:O}");
                if (model.NetworkElementTypeKey.HasValue)
                    queryParams.Add($"networkElementTypeKey={model.NetworkElementTypeKey}");

                queryParams.Add($"page={model.PageNumber}");
                queryParams.Add($"pageSize={model.PageSize}");

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}api/CuttingDown/search?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiPaged = JsonSerializer.Deserialize<ElectricityOutagePortal.Models.ApiPagedResult<ElectricityOutagePortal.Models.ApiCuttingDownHeaderDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiPaged != null)
                    {
                        // Map API models to portal view model DTO
                        var mappedItems = apiPaged.Items.Select(x => new CuttingDownHeaderDto
                        {
                            Cutting_Down_Key = int.TryParse(x.CuttingIncidentId, out var cid) ? cid : 0,
                            Cutting_Down_Incident_ID = x.CuttingIncidentId,
                            Channel_Key = null,
                            Cutting_Down_Problem_Type_Key = x.ProblemTypeKey,
                            ActualCreateDate = x.StartDate,
                            ActualEndDate = x.EndDate,
                            SynchCreateDate = null,
                            SynchUpdateDate = null,
                            IsPlanned = null,
                            IsGlobal = null,
                            IsActive = null,
                            PlannedStartDTS = x.StartDate,
                            PlannedEndDTS = x.EndDate,
                            CreateSystemUserID = "",
                            UpdateSystemUserID = "",
                            CableName = x.NetworkElementName,
                            CabinName = x.NetworkElementName
                        }).ToList();

                        model.PagedResults = new PagedResult<CuttingDownHeaderDto>
                        {
                            Items = mappedItems,
                            TotalCount = apiPaged.TotalCount,
                            PageNumber = apiPaged.Page,
                            PageSize = apiPaged.PageSize
                        };
                        model.Results = mappedItems;
                        model.PageNumber = apiPaged.Page;
                        model.PageSize = apiPaged.PageSize;
                        model.TotalCount = apiPaged.TotalCount;
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

            return View(model);
        }

        public async Task<IActionResult> NetworkHierarchy()
        {
            var model = new NetworkHierarchyViewModel();
            await LoadNetworkHierarchyData(model);

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}api/NetworkElement/hierarchy");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiNodes = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiNetworkElementNodeDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiNetworkElementNodeDto>();

                    List<NetworkElementNode> MapNodes(List<ElectricityOutagePortal.Models.ApiNetworkElementNodeDto> nodes)
                    {
                        return nodes.Select(n => new NetworkElementNode
                        {
                            Id = n.Id,
                            Name = n.Name,
                            Type = n.Type,
                            ParentId = null,
                            Children = MapNodes(n.Children),
                            IsExpanded = false,
                            HasIncidents = false,
                            IncidentCount = 0
                        }).ToList();
                    }

                    model.NetworkElements = MapNodes(apiNodes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading network hierarchy");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NetworkHierarchy(NetworkHierarchyViewModel model)
        {
            await LoadNetworkHierarchyData(model);

            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(model.SearchValue)) queryParams.Add($"searchTerm={Uri.EscapeDataString(model.SearchValue)}");
                if (model.NetworkElementTypeKey.HasValue) queryParams.Add($"typeKey={model.NetworkElementTypeKey}");
                // isActive left null for now; can add toggle in UI later
                queryParams.Add($"page={model.PageNumber}");
                queryParams.Add($"pageSize={model.PageSize}");

                var url = $"{_apiBaseUrl}api/NetworkElement/search";
                if (queryParams.Any()) url += $"?{string.Join("&", queryParams)}";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiPaged = JsonSerializer.Deserialize<ElectricityOutagePortal.Models.ApiPagedResult<ElectricityOutagePortal.Models.ApiNetworkElementDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiPaged != null)
                    {
                        model.TotalItems = apiPaged.TotalCount;
                        model.PageNumber = apiPaged.Page;
                        model.PageSize = apiPaged.PageSize;
                        model.TotalPages = apiPaged.TotalPages;

                        // Map to view model's SearchResults list
                        model.SearchResults = apiPaged.Items.Select(x => new NetworkIncidentDto
                        {
                            CuttingIncidentId = x.Key.ToString(),
                            NetworkElement = x.Name,
                            NumberOfImpactedCustomers = 0,
                            Action = string.Empty,
                            Status = x.IsActive ? "Active" : "Inactive"
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching network elements");
            }

            return View(model);
        }

        public async Task<IActionResult> IgnoredOutages(int pageNumber = 1, int pageSize = 20, DateTime? dateFrom = null, DateTime? dateTo = null, string? searchValue = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={pageNumber}",
                    $"pageSize={pageSize}"
                };
                if (dateFrom.HasValue) queryParams.Add($"fromDate={dateFrom:O}");
                if (dateTo.HasValue) queryParams.Add($"toDate={dateTo:O}");
                if (!string.IsNullOrWhiteSpace(searchValue)) queryParams.Add($"searchValue={Uri.EscapeDataString(searchValue)}");

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}api/IgnoredOutages/search?{string.Join("&", queryParams)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiPaged = JsonSerializer.Deserialize<ElectricityOutagePortal.Models.ApiPagedResult<ElectricityOutagePortal.Models.ApiIgnoredOutageDto>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiPaged != null)
                    {
                        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
                        ViewBag.SearchValue = searchValue ?? string.Empty;

                        var mappedItems = apiPaged.Items.Select(x => new CuttingDownHeaderDto
                        {
                            Cutting_Down_Key = int.TryParse(x.CuttingIncidentId, out var cid) ? cid : 0,
                            Cutting_Down_Incident_ID = x.CuttingIncidentId,
                            Channel_Key = null,
                            Cutting_Down_Problem_Type_Key = x.ProblemTypeKey,
                            ActualCreateDate = x.StartDate,
                            ActualEndDate = x.EndDate,
                            SynchCreateDate = x.IgnoredDate,
                            SynchUpdateDate = null,
                            IsPlanned = null,
                            IsGlobal = null,
                            IsActive = null,
                            PlannedStartDTS = x.StartDate,
                            PlannedEndDTS = x.EndDate,
                            CreateSystemUserID = x.IgnoredBy,
                            UpdateSystemUserID = "",
                            CableName = x.NetworkElementName,
                            CabinName = x.NetworkElementName,
                            IgnoreReason = x.IgnoreReason
                        }).ToList();

                        var vm = new SearchViewModel
                        {
                            Results = mappedItems,
                            PagedResults = new PagedResult<CuttingDownHeaderDto>
                            {
                                Items = mappedItems,
                                TotalCount = apiPaged.TotalCount,
                                PageNumber = apiPaged.Page,
                                PageSize = apiPaged.PageSize
                            },
                            PageNumber = apiPaged.Page,
                            PageSize = apiPaged.PageSize,
                            TotalCount = apiPaged.TotalCount
                        };

                        return View(vm);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ignored outages");
            }

            return View(new SearchViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIgnoredOutage(string id)
        {
            try
            {
                // Align with API: Unignore endpoint expects POST to /api/IgnoredOutages/{id}/unignore
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}api/IgnoredOutages/{id}/unignore", null);
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
                // Load Sources from API
                var sourcesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/sources");
                if (sourcesResponse.IsSuccessStatusCode)
                {
                    var json = await sourcesResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.Sources = items.Select(x => new SourceDto { Id = x.Key, Name = x.Name }).ToList();
                }

                var problemTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/problem-types");
                if (problemTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await problemTypesResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.ProblemTypes = items.Select(x => new ProblemTypeDto { Problem_Type_Key = x.Key, Problem_Type_Name = x.Name }).ToList();
                }

                var statusesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/statuses");
                if (statusesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await statusesResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.Statuses = items.Select(x => new StatusDto { Id = x.Key, Name = x.Name }).ToList();
                }

                var criteriaResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/search-criteria");
                if (criteriaResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await criteriaResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.SearchCriterias = items.Select(x => new SearchCriteriaDto { Id = x.Key, Name = x.Name }).ToList();
                }

                var networkTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/network-element-types");
                if (networkTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await networkTypesResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.NetworkElementTypes = items.Select(x => new NetworkElementTypeDto { Network_Element_Type_Key = x.Key, Network_Element_Type_Name = x.Name }).ToList();
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
                var problemTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/problem-types");
                if (problemTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await problemTypesResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.ProblemTypes = items.Select(x => new ProblemTypeDto { Problem_Type_Key = x.Key, Problem_Type_Name = x.Name }).ToList();
                }

                // Load Network Element Types
                var networkTypesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/network-element-types");
                if (networkTypesResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await networkTypesResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.NetworkElementTypes = items.Select(x => new NetworkElementTypeDto { Network_Element_Type_Key = x.Key, Network_Element_Type_Name = x.Name }).ToList();
                }

                // Load Search Criteria from API
                var criteriaResponse = await _httpClient.GetAsync($"{_apiBaseUrl}api/Lookup/search-criteria");
                if (criteriaResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await criteriaResponse.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ElectricityOutagePortal.Models.ApiLookupItemDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<ElectricityOutagePortal.Models.ApiLookupItemDto>();
                    model.SearchCriterias = items.Select(x => new SearchCriteriaDto { Id = x.Key, Name = x.Name }).ToList();
                }
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

using ElectricityOutagePortal.Models;

namespace ElectricityOutagePortal.ViewModels
{
    public class SearchViewModel
    {
        // Dropdown data
        public List<SourceDto> Sources { get; set; } = new List<SourceDto>();
        public List<ProblemTypeDto> ProblemTypes { get; set; } = new List<ProblemTypeDto>();
        public List<StatusDto> Statuses { get; set; } = new List<StatusDto>();
        public List<SearchCriteriaDto> SearchCriterias { get; set; } = new List<SearchCriteriaDto>();
        public List<NetworkElementTypeDto> NetworkElementTypes { get; set; } = new List<NetworkElementTypeDto>();

        // Search parameters
        public int? SourceCutting { get; set; }
        public int? ProblemTypeKey { get; set; }
        public int? Status { get; set; }
        public int? SearchCriteriaKey { get; set; }
        public string SearchValue { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? NetworkElementTypeKey { get; set; }

        // Results
        public List<CuttingDownHeaderDto> Results { get; set; } = new List<CuttingDownHeaderDto>();
        public PagedResult<CuttingDownHeaderDto> PagedResults { get; set; } = new PagedResult<CuttingDownHeaderDto>();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class SourceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ProblemTypeDto
    {
        public int Problem_Type_Key { get; set; }
        public string Problem_Type_Name { get; set; } = string.Empty;
    }

    public class StatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SearchCriteriaDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class NetworkElementTypeDto
    {
        public int Network_Element_Type_Key { get; set; }
        public string Network_Element_Type_Name { get; set; } = string.Empty;
    }

    public class CuttingDownHeaderDto
    {
        public int Cutting_Down_Key { get; set; }
        public string Cutting_Down_Incident_ID { get; set; } = string.Empty;
        public int? Channel_Key { get; set; }
        public int? Cutting_Down_Problem_Type_Key { get; set; }
        public DateTime? ActualCreateDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime? SynchCreateDate { get; set; }
        public DateTime? SynchUpdateDate { get; set; }
        public bool? IsPlanned { get; set; }
        public bool? IsGlobal { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? PlannedStartDTS { get; set; }
        public DateTime? PlannedEndDTS { get; set; }
        public string CreateSystemUserID { get; set; } = string.Empty;
        public string UpdateSystemUserID { get; set; } = string.Empty;
        public string CableName { get; set; } = string.Empty;
        public string CabinName { get; set; } = string.Empty;
        public string IgnoreReason { get; set; } = string.Empty;
    }
}
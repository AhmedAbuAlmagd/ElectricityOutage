namespace ElectricityOutagePortal.ViewModels
{
    public class NetworkHierarchyViewModel
    {
        public List<NetworkElementTypeDto> NetworkElementTypes { get; set; } = new List<NetworkElementTypeDto>();
        public List<ProblemTypeDto> ProblemTypes { get; set; } = new List<ProblemTypeDto>();
        public List<SearchCriteriaDto> SearchCriterias { get; set; } = new List<SearchCriteriaDto>();
        
        // Search parameters
        public int? NetworkElementTypeKey { get; set; }
        public int? ProblemTypeKey { get; set; }
        public int? SearchCriteriaKey { get; set; }
        public string SearchValue { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }

        // Network hierarchy tree
        public List<NetworkElementNode> NetworkElements { get; set; } = new List<NetworkElementNode>();
        
        // Search results
        public List<NetworkIncidentDto> SearchResults { get; set; } = new List<NetworkIncidentDto>();
        public int TotalItems { get; set; }
    }

    public class NetworkElementNode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public List<NetworkElementNode> Children { get; set; } = new List<NetworkElementNode>();
        public bool IsExpanded { get; set; }
        public bool HasIncidents { get; set; }
        public int IncidentCount { get; set; }
    }

    public class NetworkIncidentDto
    {
        public string NetworkElement { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}
namespace STA.Electricity.API.Dtos
{
    public class CuttingDownHeaderDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CuttingDownDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public int NetworkElementKey { get; set; }
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CuttingDownDetailDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public int NetworkElementKey { get; set; }
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
    }
}
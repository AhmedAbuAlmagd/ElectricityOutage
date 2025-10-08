namespace STA.Electricity.API.Dtos
{
    public class IgnoredOutageDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElementName { get; set; } = string.Empty;
        public int NumberOfImpactedCustomers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProblemTypeKey { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? IgnoredDate { get; set; }
        public string? IgnoredBy { get; set; }
        public string? IgnoreReason { get; set; }
    }

    public class IgnoreOutageRequest
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string IgnoredBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
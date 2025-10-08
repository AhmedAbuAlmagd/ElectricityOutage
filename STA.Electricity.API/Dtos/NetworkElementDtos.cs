namespace STA.Electricity.API.Dtos
{
    public class NetworkElementDto
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int TypeKey { get; set; }
        public int? ParentKey { get; set; }
        public bool IsActive { get; set; }
    }

    public class NetworkElementNodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public List<NetworkElementNodeDto> Children { get; set; } = new List<NetworkElementNodeDto>();
    }

    public class NetworkIncidentDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElement { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfImpactedCustomers { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
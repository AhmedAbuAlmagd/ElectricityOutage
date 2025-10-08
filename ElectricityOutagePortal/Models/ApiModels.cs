using System;
using System.Collections.Generic;

namespace ElectricityOutagePortal.Models
{
    // Matches API's paging envelope
    public class ApiPagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // Matches STA.Electricity.API CuttingDownController DTO
    public class ApiCuttingDownHeaderDto
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

    // Matches STA.Electricity.API IgnoredOutagesController DTO
    public class ApiIgnoredOutageDto
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
        public string IgnoredBy { get; set; } = string.Empty;
        public string IgnoreReason { get; set; } = string.Empty;
    }

    // Matches STA.Electricity.API LookupController LookupItemDto
    public class ApiLookupItemDto
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Matches STA.Electricity.API NetworkElementController DTOs
    public class ApiNetworkElementDto
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int TypeKey { get; set; }
        public int? ParentKey { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApiNetworkElementNodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public List<ApiNetworkElementNodeDto> Children { get; set; } = new List<ApiNetworkElementNodeDto>();
    }

    public class ApiNetworkIncidentDto
    {
        public string CuttingIncidentId { get; set; } = string.Empty;
        public string NetworkElement { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfImpactedCustomers { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
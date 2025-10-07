using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class CuttingDownIgnored
{
    public int CuttingDownIncidentId { get; set; }

    public DateTime? ActualCreateDate { get; set; }

    public DateTime? SynchCreateDate { get; set; }

    public string? CabelName { get; set; }

    public string? CabinName { get; set; }

    public string? CreatedUser { get; set; }
}

using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class ProblemType
{
    public int ProblemTypeKey { get; set; }

    public string? ProblemTypeName { get; set; }

    public virtual ICollection<CuttingDownHeader> CuttingDownHeaders { get; set; } = new List<CuttingDownHeader>();
}

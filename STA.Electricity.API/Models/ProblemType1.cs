using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class ProblemType1
{
    public int ProblemTypeKey { get; set; }

    public string? ProblemTypeName { get; set; }

    public virtual ICollection<CuttingDownA> CuttingDownAs { get; set; } = new List<CuttingDownA>();

    public virtual ICollection<CuttingDownB> CuttingDownBs { get; set; } = new List<CuttingDownB>();

    public virtual ICollection<CuttingDownHeader> CuttingDownHeaders { get; set; } = new List<CuttingDownHeader>();
}

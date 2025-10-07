using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class CuttingDownDetail
{
    public int CuttingDownDetailKey { get; set; }

    public int? CuttingDownKey { get; set; }

    public int? NetworkElementKey { get; set; }

    public DateTime? ActualCreateDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public int? ImpactedCustomers { get; set; }

    public virtual CuttingDownHeader? CuttingDownKeyNavigation { get; set; }

    public virtual NetworkElement? NetworkElementKeyNavigation { get; set; }
}

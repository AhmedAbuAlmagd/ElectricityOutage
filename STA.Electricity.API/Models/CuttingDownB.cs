using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class CuttingDownB
{
    public int CuttingDownBIncidentId { get; set; }

    public string? CuttingDownCableName { get; set; }

    public int? ProblemTypeKey { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsPlanned { get; set; }

    public bool? IsGlobal { get; set; }

    public DateTime? PlannedStartDts { get; set; }

    public DateTime? PlannedEndDts { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedUser { get; set; }

    public string? UpdatedUser { get; set; }

    public virtual ProblemType1? ProblemTypeKeyNavigation { get; set; }
}

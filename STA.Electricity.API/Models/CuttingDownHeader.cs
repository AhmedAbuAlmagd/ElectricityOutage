using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class CuttingDownHeader
{
    public int CuttingDownKey { get; set; }

    public int? CuttingDownIncidentId { get; set; }

    public int? ChannelKey { get; set; }

    public int? CuttingDownProblemTypeKey { get; set; }

    public DateTime? ActualCreateDate { get; set; }

    public DateTime? SynchCreateDate { get; set; }

    public DateTime? SynchUpdateDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public bool? IsPlanned { get; set; }

    public bool? IsGlobal { get; set; }

    public DateTime? PlannedStartDts { get; set; }

    public DateTime? PlannedEndDts { get; set; }

    public bool? IsActive { get; set; }

    public int? CreateSystemUserId { get; set; }

    public int? UpdateSystemUserId { get; set; }

    public virtual Channel? ChannelKeyNavigation { get; set; }

    public virtual User? CreateSystemUser { get; set; }

    public virtual ICollection<CuttingDownDetail> CuttingDownDetails { get; set; } = new List<CuttingDownDetail>();

    public virtual ProblemType1? CuttingDownProblemTypeKey1 { get; set; }

    public virtual ProblemType? CuttingDownProblemTypeKeyNavigation { get; set; }

    public virtual User? UpdateSystemUser { get; set; }
}

using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class NetworkElement
{
    public int NetworkElementKey { get; set; }

    public string? NetworkElementName { get; set; }

    public int? NetworkElementTypeKey { get; set; }

    public int? ParentNetworkElementKey { get; set; }

    public virtual ICollection<CuttingDownDetail> CuttingDownDetails { get; set; } = new List<CuttingDownDetail>();

    public virtual ICollection<NetworkElement> InverseParentNetworkElementKeyNavigation { get; set; } = new List<NetworkElement>();

    public virtual NetworkElementType? NetworkElementTypeKeyNavigation { get; set; }

    public virtual NetworkElement? ParentNetworkElementKeyNavigation { get; set; }
}

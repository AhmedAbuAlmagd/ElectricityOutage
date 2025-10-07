using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class NetworkElementType
{
    public int NetworkElementTypeKey { get; set; }

    public string? NetworkElementTypeName { get; set; }

    public int? ParentNetworkElementTypeKey { get; set; }

    public int? NetworkElementHierarchyPathKey { get; set; }

    public virtual ICollection<NetworkElementType> InverseParentNetworkElementTypeKeyNavigation { get; set; } = new List<NetworkElementType>();

    public virtual NetworkElementHierarchyPath? NetworkElementHierarchyPathKeyNavigation { get; set; }

    public virtual ICollection<NetworkElement> NetworkElements { get; set; } = new List<NetworkElement>();

    public virtual NetworkElementType? ParentNetworkElementTypeKeyNavigation { get; set; }
}

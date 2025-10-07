using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class NetworkElementHierarchyPath
{
    public int NetworkElementHierarchyPathKey { get; set; }

    public string? NetwrokElementHierarchyPathName { get; set; }

    public string? Abbreviation { get; set; }

    public virtual ICollection<NetworkElementType> NetworkElementTypes { get; set; } = new List<NetworkElementType>();
}

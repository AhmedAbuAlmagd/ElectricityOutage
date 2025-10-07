using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Governrate
{
    public int GovernrateKey { get; set; }

    public string? GovernrateName { get; set; }

    public virtual ICollection<Sector> Sectors { get; set; } = new List<Sector>();
}

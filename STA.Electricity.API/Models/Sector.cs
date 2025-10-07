using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Sector
{
    public int SectorKey { get; set; }

    public int? GovernrateKey { get; set; }

    public string? SectorName { get; set; }

    public virtual Governrate? GovernrateKeyNavigation { get; set; }

    public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
}

using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Tower
{
    public int TowerKey { get; set; }

    public int? StationKey { get; set; }

    public string? TowerName { get; set; }

    public virtual ICollection<Cabin> Cabins { get; set; } = new List<Cabin>();

    public virtual Station? StationKeyNavigation { get; set; }
}

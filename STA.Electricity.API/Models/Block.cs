using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Block
{
    public int BlockKey { get; set; }

    public int? CableKey { get; set; }

    public string? BlockName { get; set; }

    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    public virtual Cable? CableKeyNavigation { get; set; }
}

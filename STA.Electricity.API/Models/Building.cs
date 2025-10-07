using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Building
{
    public int BuildingKey { get; set; }

    public int? BlockKey { get; set; }

    public string? BuildingName { get; set; }

    public virtual Block? BlockKeyNavigation { get; set; }

    public virtual ICollection<Flat> Flats { get; set; } = new List<Flat>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

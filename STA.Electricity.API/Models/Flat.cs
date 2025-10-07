using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Flat
{
    public int FlatKey { get; set; }

    public int? BuildingKey { get; set; }

    public virtual Building? BuildingKeyNavigation { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

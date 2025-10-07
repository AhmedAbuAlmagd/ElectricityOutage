using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Subscription
{
    public int SubscriptionKey { get; set; }

    public int? FlatKey { get; set; }

    public int? BuildingKey { get; set; }

    public int? MeterKey { get; set; }

    public int? PaletKey { get; set; }

    public virtual Building? BuildingKeyNavigation { get; set; }

    public virtual Flat? FlatKeyNavigation { get; set; }
}

using System;
using System.Collections.Generic;

namespace STA.Electricity.API.Models;

public partial class Channel
{
    public int ChannelKey { get; set; }

    public string? ChannelName { get; set; }

    public virtual ICollection<CuttingDownHeader> CuttingDownHeaders { get; set; } = new List<CuttingDownHeader>();
}

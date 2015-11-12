using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcherC7AccessControl
{
  public class AccessRule
  {
    public string Name { get; set; }
    public int Host { get; set; }
    public int Schedule { get; set; }

    public override string ToString()
    {
      var result = new Dictionary<string, string>
      {
        { "rule_name", Name },
        { "hosts_lists", Host.ToString() },
        { "targets_lists", "255" },
        { "scheds_lists", Schedule.ToString() },
        { "enable", "1" },
        { "Changed", "0" },
        { "SelIndex", "0" },
        { "Page", "1" },
        { "Save", "Save" }
      };

      return string.Join("&", result.Select(x => x.Key + "=" + x.Value));
    }
  }
}

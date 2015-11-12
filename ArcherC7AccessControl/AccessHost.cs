using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcherC7AccessControl
{
  public class AccessHost
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string MacAddress { get; set; }
    public string Schedule { get; set; }

    public override string ToString()
    {
      var result = new Dictionary<string, string>
      {
        { "addr_type", "0" },        // We only support MAC addresses
        { "hosts_lists_name", Name },
        { "src_ip_start", "" },
        { "src_ip_end", "" },
        { "mac_addr", MacAddress },
        { "Changed", "0" },
        { "SelIndex", "0" },
        { "fromAdd", "0" },
        { "Page", "1" },
        { "Save", "Save" }
      };

      return string.Join("&", result.Select(x => x.Key + "=" + x.Value));
    }
  }
}

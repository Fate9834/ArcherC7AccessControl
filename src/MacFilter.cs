using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcherC7AccessControl
{
  public class MacFilter
  {
    public string Name { get; set; }
    public string MacAddress { get; set; }

    public override string ToString()
    {
      var result = new Dictionary<string, string>
      {
        { "Mac", MacAddress },
        { "Desc", Name },
        { "Type", "1" },
        { "entryEnabled", "1" },
        { "Changed", "0" },
        { "SelIndex", "0" },
        { "Page", "1" },
        { "Save", "Save" }
      };

      return string.Join("&", result.Select(x => x.Key + "=" + x.Value));
    }
  }
}

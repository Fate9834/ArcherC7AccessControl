using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcherC7AccessControl
{
  public class AccessSchedule
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public AccessDays Days { get; set; }
    public bool AllHours { get; set; }
    public TimeSpan TimeStart { get; set; }
    public TimeSpan TimeEnd { get; set; }

    public override string ToString()
    {
      var result = new Dictionary<string, string>();

      result.Add("time_sched_name", Name);
      result.Add("day_type", Days == AccessDays.All ? "1" : "0");

      if (Days != AccessDays.All)
      {
        if (Days.HasFlag(AccessDays.Monday))
          result.Add("Mon_select", "on");
        if (Days.HasFlag(AccessDays.Tuesday))
          result.Add("Tue_select", "on");
        if (Days.HasFlag(AccessDays.Wednesday))
          result.Add("Wed_select", "on");
        if (Days.HasFlag(AccessDays.Thursday))
          result.Add("Thu_select", "on");
        if (Days.HasFlag(AccessDays.Friday))
          result.Add("Fri_select", "on");
        if (Days.HasFlag(AccessDays.Saturday))
          result.Add("Sat_select", "on");
        if (Days.HasFlag(AccessDays.Sunday))
          result.Add("Sun_select", "on");
      }

      if (AllHours)
        result.Add("all_hours", "on");
      else
      {
        result.Add("time_sched_start_time", TimeStart.ToString("hhmm"));
        result.Add("time_sched_end_time", TimeEnd.ToString("hhmm"));
      }

      result.Add("Changed", "0");
      result.Add("SelIndex", "0");
      result.Add("fromAdd", "0");
      result.Add("Page", "1");
      result.Add("Save", "Save");

      return string.Join("&", result.Select(x => x.Key + "=" + x.Value));
    }
  }
}

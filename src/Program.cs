using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace ArcherC7AccessControl
{
  class Program
  {
    private static Router _router;
    private static readonly List<AccessHost> _hosts = new List<AccessHost>();
    private static readonly List<AccessRule> _rules = new List<AccessRule>();
    private static readonly List<AccessSchedule> _schedules = new List<AccessSchedule>();
    private static AccessTrafficRule _policy = AccessTrafficRule.DenyRules;
    private static string _username;
    private static string _password;
    private static IPAddress _ip;
    private static string _config;

    static int Main(string[] args)
    {
      try
      {
        if (!ParseCommandLine(args))
          return 1;

        Console.WriteLine("Parsing file " + _config);
        ParseConfig(_config);

        _router = new Router(_ip);
        Console.WriteLine("Connecting to router...");
        _router.Login(_username, _password);
        Console.WriteLine("Logged in with session '" + _router.Session + "'");

        try
        {
          // Disable access policy so we don't mess anything up
          Console.WriteLine("Disabling access controls");
          _router.SetAccessRules(false, AccessTrafficRule.DenyRules);
          _router.SetMacFiltering24GHz(false);
          _router.SetMacFiltering5GHz(false);

          // Delete all rules
          DeletePreviousRules();

          // Add hosts and schedules and assign ID to each
          AddHosts(_hosts);
          AddSchedules(_schedules);

          // Build and add all rules
          BuildRules();
          AddRules(_rules);

          // Add filtering
          AddFiltering(_hosts);

          // Reenable the access policy
          Console.WriteLine("Setting access controls");
          _router.SetAccessRules(true, _policy);
          _router.SetMacExclusiveAccess24Ghz(true);
          _router.SetMacExclusiveAccess5Ghz(true);
          _router.SetMacFiltering24GHz(true);
          _router.SetMacFiltering5GHz(true);
        }
        finally
        {
          _router.Logout();
        }

        return 0;
      }
      catch (Exception ex)
      {
        var fg = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(ex.Message);
        Console.ForegroundColor = fg;
        return 1;
      }
    }

    private static bool ParseCommandLine(string[] args)
    {
      if (args.Length != 4)
      {
        Console.Error.WriteLine("Invalid number of parameters:");
        Console.Error.WriteLine("ArcherAccessControl.exe <router-ip> <username> <password> <config-file>");
        return false;
      }

      _ip = IPAddress.Parse(args[0]);
      _username = args[1];
      _password = args[2];
      _config = args[3];

      return true;
    }

    private static void AddFiltering(List<AccessHost> hosts)
    {
      foreach (var host in hosts)
      {
        Console.WriteLine("Adding MAC filter " + host.MacAddress + " => '" + host.Name + "'");
        var filter = new MacFilter { Name = host.Name, MacAddress = host.MacAddress };
        _router.AddMacFilter24GHz(filter);
        _router.AddMacFilter5GHz(filter);
      }
    }

    private static void AddHosts(List<AccessHost> hosts)
    {
      int id = 0;
      foreach (var host in hosts.Where(x => !string.IsNullOrEmpty(x.Schedule)))
      {
        Console.WriteLine("Adding host " + host.MacAddress + " => '" + host.Name + "'");
        _router.AddHost(host);
        host.Id = id++;
      }
    }

    private static void AddRules(List<AccessRule> rules)
    {
      foreach (var rule in rules)
      {
        Console.WriteLine("Adding rule '" + rule.Name + "'");
        _router.AddRule(rule);
      }
    }

    private static void AddSchedules(List<AccessSchedule> schedules)
    {
      int id = 0;
      foreach (var schedule in schedules)
      {
        Console.WriteLine("Adding schedule '" + schedule.Name + "'");
        _router.AddSchedule(schedule);
        schedule.Id = id++;
      }
    }

    private static void BuildRules()
    {
      var schedules = _schedules.ToLookup(x => x.Group, StringComparer.CurrentCultureIgnoreCase);
      var id = 0;
      foreach (var host in _hosts.Where(x => !string.IsNullOrEmpty(x.Schedule)))
        foreach (var schedule in schedules[host.Schedule])
          _rules.Add(new AccessRule { Name = "rule-" + (++id), Host = host.Id, Schedule = schedule.Id });
    }

    private static void DeletePreviousRules()
    {
      Console.WriteLine("Deleting rules...");
      _router.DeleteAllRules();
      Console.WriteLine("Deleting hosts...");
      _router.DeleteAllHosts();
      Console.WriteLine("Deleting schedules...");
      _router.DeleteAllSchedules();
      Console.WriteLine("Delete all MAC filters");
      _router.DeleteAllMacFilters24GHz();
      _router.DeleteAllMacFilters5GHz();
    }

    private static void ParseConfig(string filename)
    {
      var doc = XDocument.Load(filename);

      var mode = RequireAttr(doc.Elements("xml").Elements("policy").Single(), "mode").ToLower();
      if (mode == "allow")
        _policy = AccessTrafficRule.AllowRules;
      else if (mode == "deny")
        _policy = AccessTrafficRule.DenyRules;
      else
        throw new Exception("Invalid mode '" + mode + ", expected 'deny' or 'allow'");

      foreach (var schedule in doc.Elements("xml").Elements("schedules").Elements("schedule"))
      {
        var name = RequireAttr(schedule, "name");
        var id = 0;
        foreach (var item in schedule.Elements("rule"))
        {
          var rule = new AccessSchedule { Group = name, Name = name + "-" + (++id) };

          var start = GetAttr(item, "start");
          var end = GetAttr(item, "end");
          if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            rule.AllHours = true;
          else
          {
            if (string.IsNullOrEmpty(start))
              start = "00:00:00";
            if (string.IsNullOrEmpty(end))
              end = "23:59:00";

            rule.TimeStart = TimeSpan.Parse(start);
            rule.TimeEnd = TimeSpan.Parse(end);
          }

          rule.Days = ParseDays(RequireAttr(item, "days"));

          _schedules.Add(rule);
        }
      }

      foreach (var item in doc.Elements("xml").Elements("hosts").Elements("host"))
      {
        _hosts.Add(new AccessHost
        {
          Name = RequireAttr(item, "name"),
          MacAddress = RequireAttr(item, "mac"),
          Schedule = GetAttr(item, "schedule")
        });
      }

      // Verify integrity
      var fails = _hosts.GroupBy(x => x.Name).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
      if (fails.Any())
        throw new Exception("Hosts " + string.Join(", ", fails) + " do not have a unique name");

      fails = _hosts.GroupBy(x => x.MacAddress).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
      if (fails.Any())
        throw new Exception("MAC addresses " + string.Join(", ", fails) + " exists more than once");
      
      var schedules = _schedules.Select(x => x.Group).Distinct().ToList();
      foreach(var host in _hosts.Where(h => !string.IsNullOrEmpty(h.Schedule)))
        if (!schedules.Contains(host.Schedule, StringComparer.CurrentCultureIgnoreCase))
          throw new Exception("Host " + host.Name + " has an undefined schedule " + host.Schedule);
    }

    private static AccessDays ParseDays(string days)
    {
      return (AccessDays)days
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .Aggregate(0, (current, day) => current | (int) Enum.Parse(typeof (AccessDays), day, true));
    }

    private static string GetAttr(XElement element, string name)
    {
      return element.Attribute(name) == null ? null : (element.Attribute(name).Value).Trim();
    }

    private static string RequireAttr(XElement element, string name)
    {
      var result = GetAttr(element, name);
      if (string.IsNullOrEmpty(result))
        throw new Exception("Missing attribute '" + name + "' on node " + element.Name);

      return result;
    }
  }
}

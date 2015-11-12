using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcherC7AccessControl
{
  public class Router
  {
    private readonly CookieContainer _cookies = new CookieContainer();
    private readonly IPAddress _ip;
    public string Session { get; private set; }

    public Router(IPAddress ip)
    {
      _ip = ip;
    }

    public void Login(string username, string password)
    {
      var md5 = string.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes(password)).Select(x => x.ToString("x2")));
      var auth = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + md5));
      _cookies.Add(new Cookie("Authorization", "Basic " + auth, "/", _ip.ToString()));

      var response = DoRequest("userRpm/LoginRpm.htm?Save=save");

      var match = Regex.Match(response, "http://" + _ip + "/(.+)/userRpm");
      if (!match.Success)
        throw new Exception("Login failed");

      Session = match.Groups[1].Value;
    }

    public void Logout()
    {
      DoRequest("userRpm/LogoutRpm.htm");
    }

    public void AddHost(AccessHost host)
    {
      DoRequest("userRpm/AccessCtrlHostsListsRpm.htm?" + host);
    }

    public void AddMacFilter24GHz(MacFilter filter)
    {
      DoRequest("userRpm/WlanMacFilterRpm.htm?" + filter);
    }

    public void AddMacFilter5GHz(MacFilter filter)
    {
      DoRequest("userRpm/WlanMacFilterRpm_5g.htm?" + filter);
    }

    public void AddRule(AccessRule rule)
    {
      DoRequest("userRpm/AccessCtrlAccessRulesRpm.htm?" + rule);
    }

    public void AddSchedule(AccessSchedule schedule)
    {
      DoRequest("userRpm/AccessCtrlTimeSchedRpm.htm?" + schedule);
    }

    public void DeleteAllHosts()
    {
      DoRequest("userRpm/AccessCtrlHostsListsRpm.htm?doAll=DelAll&Page=1");
    }

    public void DeleteAllMacFilters24GHz()
    {
      DoRequest("userRpm/WlanMacFilterRpm.htm?Page=1&DoAll=DelAll");
    }

    public void DeleteAllMacFilters5GHz()
    {
      DoRequest("userRpm/WlanMacFilterRpm_5g.htm?Page=1&DoAll=DelAll");
    }

    public void DeleteAllRules()
    {
      DoRequest("userRpm/AccessCtrlAccessRulesRpm.htm?doAll=DelAll&Page=1");
    }

    public void DeleteAllSchedules()
    {
      DoRequest("userRpm/AccessCtrlTimeSchedRpm.htm?doAll=DelAll&Page=1");
    }

    public void SetAccessRules(bool enableAccessControls, AccessTrafficRule trafficRule)
    {
      var enable = enableAccessControls ? 1 : 0;
      var rule = (int)trafficRule;

      DoRequest("userRpm/AccessCtrlAccessRulesRpm.htm?enableCtrl=" + enable + "&defRule=" + rule + "&Page=1");
    }

    public void SetMacFiltering24GHz(bool enable)
    {
      DoRequest("userRpm/WlanMacFilterRpm.htm?Page=1&" + (enable ? "Enfilter=1" : "Disfilter=1"));
    }

    public void SetMacExclusiveAccess24Ghz(bool exclusive)
    {
      DoRequest("userRpm/WlanMacFilterRpm.htm?Page=1&exclusive=" + (exclusive ? "1" : "0"));
    }

    public void SetMacFiltering5GHz(bool enable)
    {
      DoRequest("userRpm/WlanMacFilterRpm_5g.htm?Page=1&" + (enable ? "Enfilter=1" : "Disfilter=1"));
    }

    public void SetMacExclusiveAccess5Ghz(bool exclusive)
    {
      DoRequest("userRpm/WlanMacFilterRpm_5g.htm?Page=1&exclusive=" + (exclusive ? "1" : "0"));
    }

    private string DoRequest(string url)
    {
      var request = WebRequest.CreateHttp("http://" + _ip + "/" + (Session != null ? Session + "/" : "") + url);
      request.CookieContainer = _cookies;
      request.AllowAutoRedirect = false;
      request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
      request.Referer = "http://" + _ip + "/";

      using (var response = (HttpWebResponse)request.GetResponse())
      {
        if (response.StatusCode != HttpStatusCode.OK)
          throw new Exception("HTTP error code " + (int)response.StatusCode + " from " + url);

        using (var stream = response.GetResponseStream())
        {
          if (stream == null)
            return null;

          var buffer = new byte[1024];
          var sb = new StringBuilder();
          int len;

          do
          {
            len = stream.Read(buffer, 0, buffer.Length);
            sb.Append(Encoding.Default.GetString(buffer, 0, len));
          } while (len != 0);

          var result = sb.ToString();

          var errMatch = Regex.Match(result, "errCode\\s*=\\s*\"(\\d+)\"");
          if (errMatch.Success)
            throw new Exception("Error " + errMatch.Groups[1].Value + " while configuring router");

          return result;
        }
      }
    }
  }
}

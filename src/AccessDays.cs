using System;

namespace ArcherC7AccessControl
{
  [Flags]
  public enum AccessDays
  {
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,
    All = 127
  }
}

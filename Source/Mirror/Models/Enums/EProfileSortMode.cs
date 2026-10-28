using System.ComponentModel;

namespace Mirror.Models.Enums;

public enum EProfileSortMode
{
    [Description("Version / name")]
    VersionName,

    [Description("Last used")]
    LastUsed
}

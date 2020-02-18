using System.Collections;
using System.Collections.Generic;

public static class Constants
{
    public const string ServerEnpoint = "http://localhost/repos/BiennaleParcelationServer/parcelation/";
    public static readonly string[] AvatarResourceStrings = { "agent", "builder", "pipsqueak_v2" };
    public static readonly Dictionary<int, string> UnitDefName = new Dictionary<int, string>()
    {
        {0, "One room"},
        {1, "Two room"},
        {2, "Three room"},
        {3, "Four room"}    
    };
    public static readonly Dictionary<int, int> UnitDefSize = new Dictionary<int, int>()
    {
        {0, 2},
        {1, 2},
        {2, 3},
        {3, 3}
    };

    public static int PaxToUnitTypeIndex(int pax)
    {
        switch (pax)
        {
            case 1:
                return 0;
            case 2:
                return 0;
            case 3:
                return 1;
            case 4:
                return 2;
            case 5:
                return 3;
            default:
                return 3;
        }
    }
}

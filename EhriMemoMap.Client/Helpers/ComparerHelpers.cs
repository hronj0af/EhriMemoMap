using EhriMemoMap.Client.Components.Cards.Address;
using EhriMemoMap.Shared;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EhriMemoMap.Client.Helpers
{
    public class VictimComparer : IEqualityComparer<VictimShortInfoModel>
    {
        public bool Equals(VictimShortInfoModel x, VictimShortInfoModel y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return x.Id == y.Id;
        }

        public int GetHashCode(VictimShortInfoModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}

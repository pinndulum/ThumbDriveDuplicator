using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaport.Core
{
    public static class PropertyExtensions
    {
        public static Property[] FindProperties(this IEnumerable<Property> properties, string searchFor)
        {
            if (searchFor.StartsWith("*") && searchFor.EndsWith("*"))
            {
                searchFor = searchFor.Remove(0, 1);
                searchFor = searchFor.Remove(searchFor.Length - 1);
                return properties.Where(p => p.Name.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase) > -1).ToArray();
            }
            else if (searchFor.StartsWith("*"))
            {
                searchFor = searchFor.Remove(0, 1);
                return properties.Where(p => p.Name.EndsWith(searchFor, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }
            else if (searchFor.EndsWith("*"))
            {
                searchFor = searchFor.Remove(searchFor.Length - 1);
                return properties.Where(p => p.Name.StartsWith(searchFor, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }
            return properties.Where(p => p.Name.Equals(searchFor)).ToArray();
        }
    }
}

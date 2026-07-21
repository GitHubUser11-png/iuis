using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Security;

namespace IUIS.Application.Navigation
{
    public static class NavigationFilter
    {
        public static IReadOnlyList<NavigationItemDefinition> Filter(
            IEnumerable<NavigationItemDefinition> items,
            EffectiveAccessSnapshot access)
        {
            return (items ?? Enumerable.Empty<NavigationItemDefinition>())
                .Where(item => item.AlwaysVisible
                    || string.IsNullOrWhiteSpace(item.RequiredPermission)
                    || (access != null && access.HasPermission(item.RequiredPermission)))
                .OrderBy(item => item.DisplayOrder)
                .ThenBy(item => item.DisplayText)
                .ToList()
                .AsReadOnly();
        }
    }
}

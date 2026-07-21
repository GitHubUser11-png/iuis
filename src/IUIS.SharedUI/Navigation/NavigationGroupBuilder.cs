using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Navigation;

namespace IUIS.SharedUI.Navigation
{
    public static class NavigationGroupBuilder
    {
        public static IReadOnlyList<NavigationGroupModel> BuildGroups(
            IEnumerable<NavigationItemDefinition> items)
        {
            return (items ?? Enumerable.Empty<NavigationItemDefinition>())
                .GroupBy(item => item.GroupKey ?? string.Empty)
                .OrderBy(group => group.Min(entry => entry.DisplayOrder))
                .Select(group => new NavigationGroupModel(
                    group.Key,
                    group
                        .OrderBy(entry => entry.DisplayOrder)
                        .ThenBy(entry => entry.DisplayText)
                        .Select(entry => new NavigationEntryModel
                        {
                            Key = entry.Key,
                            GroupKey = entry.GroupKey,
                            DisplayText = entry.DisplayText,
                            PageKey = entry.PageKey,
                            DisplayOrder = entry.DisplayOrder
                        })
                        .ToList()
                        .AsReadOnly()))
                .ToList()
                .AsReadOnly();
        }
    }
}

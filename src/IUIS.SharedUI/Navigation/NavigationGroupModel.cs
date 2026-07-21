using System.Collections.Generic;

namespace IUIS.SharedUI.Navigation
{
    public sealed class NavigationGroupModel
    {
        public NavigationGroupModel(string groupKey, IReadOnlyList<NavigationEntryModel> entries)
        {
            GroupKey = groupKey ?? string.Empty;
            Entries = entries ?? new NavigationEntryModel[0];
        }

        public string GroupKey { get; private set; }
        public IReadOnlyList<NavigationEntryModel> Entries { get; private set; }
    }

    public sealed class NavigationEntryModel
    {
        public string Key { get; set; }
        public string GroupKey { get; set; }
        public string DisplayText { get; set; }
        public string PageKey { get; set; }
        public int DisplayOrder { get; set; }
    }
}

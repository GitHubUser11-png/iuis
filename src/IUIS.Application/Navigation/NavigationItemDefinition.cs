namespace IUIS.Application.Navigation
{
    public sealed class NavigationItemDefinition
    {
        public string Key { get; set; }
        public string GroupKey { get; set; }
        public string DisplayText { get; set; }
        public string PageKey { get; set; }
        public string RequiredPermission { get; set; }
        public int DisplayOrder { get; set; }
        public bool AlwaysVisible { get; set; }
    }
}

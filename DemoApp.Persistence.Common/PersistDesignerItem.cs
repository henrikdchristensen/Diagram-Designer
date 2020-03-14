namespace DemoApp.Persistence.Common
{
    public class PersistDesignerItem : DesignerItemBase
    {
        public PersistDesignerItem(string id, double left, double top, double itemWidth, double itemHeight, string hostUrl) 
            : base(id, left, top, itemWidth, itemHeight)
        {
            this.HostUrl = hostUrl;
        }

        public string HostUrl { get; set; }

    }
}

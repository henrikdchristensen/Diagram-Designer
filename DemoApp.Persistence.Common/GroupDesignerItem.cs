using System.Collections.Generic;

namespace DemoApp.Persistence.Common
{
    public class GroupDesignerItem : DesignerItemBase, IDiagramItem
    {
        public GroupDesignerItem(string id, double left, double top, double itemWidth, double itemHeight) 
            : base(id, left, top, itemWidth, itemHeight)
        {
            this.DesignerItems = new List<DiagramItemData>();
            this.ConnectionIds = new List<string>();
        }

        public List<DiagramItemData> DesignerItems { get; set; }
        public List<string> ConnectionIds { get; set; }

    }
}

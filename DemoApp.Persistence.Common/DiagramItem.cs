using System;
using System.Collections.Generic;

namespace DemoApp.Persistence.Common
{
    public class DiagramItem : PersistableItemBase, IDiagramItem
    {
        public DiagramItem() 
        {
            this.DesignerItems = new List<DiagramItemData>();
            this.ConnectionIds = new List<string>();
        }

        public List<DiagramItemData> DesignerItems { get; set; }
        public List<string> ConnectionIds { get; set; }
    }


    public class DiagramItemData
    {
        public DiagramItemData(string itemId, Type itemType)
        {
            this.ItemId = itemId;
            this.ItemType = itemType;
        }

        public string ItemId { get; set; }
        public Type ItemType { get; set; }
    }  
}

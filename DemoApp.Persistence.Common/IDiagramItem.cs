using System.Collections.Generic;

namespace DemoApp.Persistence.Common
{
    public interface IDiagramItem 
    {
        List<DiagramItemData> DesignerItems { get; set; }
        List<string> ConnectionIds { get; set; }
    }
}

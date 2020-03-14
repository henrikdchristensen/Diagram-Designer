using System.Collections.Generic;

namespace DemoApp.Persistence.Common
{
    public interface IDatabaseAccessService
    {
        //delete methods
        void DeleteConnection(string connectionId);
        void DeletePersistDesignerItem(string persistDesignerId);
        void DeleteSettingDesignerItem(string settingsDesignerItemId);

        //save methods
        string SaveDiagram(DiagramItem diagram);
        //PersistDesignerItem is pecific to the DemoApp example
        string SavePersistDesignerItem(PersistDesignerItem persistDesignerItemToSave);
        //SettingsDesignerItem is pecific to the DemoApp example
        string SaveSettingDesignerItem(SettingsDesignerItem settingsDesignerItemToSave);
        string SaveConnection(Connection connectionToSave);
        string SaveGroupingDesignerItem(GroupDesignerItem groupDesignerItem);
        //Fetch methods
        IEnumerable<DiagramItem> FetchAllDiagram();
        DiagramItem FetchDiagram(string diagramId);
        //PersistDesignerItem is pecific to the DemoApp example
        PersistDesignerItem FetchPersistDesignerItem(string settingsDesignerItemId);
        //SettingsDesignerItem is pecific to the DemoApp example
        SettingsDesignerItem FetchSettingsDesignerItem(string settingsDesignerItemId);
        Connection FetchConnection(string connectionId);
        GroupDesignerItem FetchGroupingDesignerItem(string itemId);
    }
}

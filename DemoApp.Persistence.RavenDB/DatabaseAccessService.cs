using System.Collections.Generic;
using System.Linq;
using DemoApp.Persistence.Common;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace DemoApp.Persistence.RavenDB
{
    /// <summary>
    /// I decided to use RavenDB instead of SQL, to save people having to have SQL Server, and also
    /// it just takes less time to do with Raven. This is ALL the CRUD code. Simple no?
    /// 
    /// Thing is the IDatabaseAccessService and the items it persists could easily be applied to helper methods that
    /// use StoredProcedures or ADO code, the data being stored would be exactly the same. You would just need to store
    /// the individual property values in tables rather than store objects.
    /// </summary>
    public class DatabaseAccessService : IDatabaseAccessService
    {
        DocumentStore documentStore = null;

        public DatabaseAccessService()
        {
            documentStore = new DocumentStore
            {
                Urls = new[] { "http://localhost:8080" },
                Database = "Data"
            };
            documentStore.Initialize();
        }
            
        public void DeleteConnection(string connectionId)
        {
            using IDocumentSession session = documentStore.OpenSession();
            IEnumerable<Connection> conns = session.Query<Connection>().Where(x => x.Id == connectionId);
            foreach (var conn in conns)
            {
                session.Delete<Connection>(conn);
            }
            session.SaveChanges();
        }

        public void DeletePersistDesignerItem(string persistDesignerId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                IEnumerable<PersistDesignerItem> persistItems = session.Query<PersistDesignerItem>().Where(x => x.Id == persistDesignerId);
                foreach (var persistItem in persistItems)
                {
                    session.Delete<PersistDesignerItem>(persistItem);
                }
                session.SaveChanges();
            }
        }

        public void DeleteSettingDesignerItem(string settingsDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                IEnumerable<SettingsDesignerItem> settingItems = session.Query<SettingsDesignerItem>().Where(x => x.Id == settingsDesignerItemId);
                foreach (var settingItem in settingItems)
                {
                    session.Delete<SettingsDesignerItem>(settingItem);
                }
                session.SaveChanges();
            }
        }

        public string SaveDiagram(DiagramItem diagram)
        {
            return SaveItem(diagram);
        }

        public string SavePersistDesignerItem(PersistDesignerItem persistDesignerItemToSave)
        {
            return SaveItem(persistDesignerItemToSave);
        }

        public string SaveSettingDesignerItem(SettingsDesignerItem settingsDesignerItemToSave)
        {
            return SaveItem(settingsDesignerItemToSave);
        }

        public string SaveGroupingDesignerItem(GroupDesignerItem groupDesignerItemToSave)
        {
            return SaveItem(groupDesignerItemToSave);
        }

        public string SaveConnection(Connection connectionToSave)
        {
            return SaveItem(connectionToSave);
        }

        public IEnumerable<DiagramItem> FetchAllDiagram()
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<DiagramItem>().ToList();
            }
        }

        public DiagramItem FetchDiagram(string diagramId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<DiagramItem>().Single(x => x.Id == diagramId);
            }
        }

        public PersistDesignerItem FetchPersistDesignerItem(string settingsDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<PersistDesignerItem>().Single(x => x.Id == settingsDesignerItemId);
            }
        }

        public SettingsDesignerItem FetchSettingsDesignerItem(string settingsDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<SettingsDesignerItem>().Single(x => x.Id == settingsDesignerItemId);
            }
        }

        public GroupDesignerItem FetchGroupingDesignerItem(string groupDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<GroupDesignerItem>().Single(x => x.Id == groupDesignerItemId);
            }
        }
        public Connection FetchConnection(string connectionId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<Connection>().Single(x => x.Id == connectionId);
            }
        }

        private string SaveItem(PersistableItemBase item)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                session.Store(item);
                session.SaveChanges();
            }
            return item.Id;
        }
    }
}

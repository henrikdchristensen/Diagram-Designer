namespace DemoApp.Persistence.Common
{
    public abstract class PersistableItemBase
    {
        public PersistableItemBase()
        {

        }

        public PersistableItemBase(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
    }
}

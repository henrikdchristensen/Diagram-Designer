using System;

namespace DemoApp.Persistence.Common
{
    public class Connection : PersistableItemBase
    {
        public Connection(string id, string sourceId, Orientation sourceOrientation, 
            Type sourceType, string sinkId, Orientation sinkOrientation, Type sinkType) : base(id)
        {
            this.SourceId = sourceId;
            this.SourceOrientation = sourceOrientation;
            this.SourceType = sourceType;
            this.SinkId = sinkId;
            this.SinkOrientation = sinkOrientation;
            this.SinkType = sinkType;
        }

        public string SourceId { get; private set; }
        public Orientation SourceOrientation { get; private set; }
        public Type SourceType { get; private set; }
        public string SinkId { get; private set; }
        public Orientation SinkOrientation { get; private set; }
        public Type SinkType { get; private set; }
    }

}

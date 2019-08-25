namespace DsmSuite.DsmViewer.Model.Data
{
    public class Relation
    {
        private readonly char _typeId;

        private readonly RelationTypes _relationTypes;
        public Element Consumer { get; private set; }
        public Element Provider { get; private set; }
        public string Type { get { return _relationTypes.GetRelationType(_typeId); } }
        public bool IsCyclic;
        public bool IsDerived { get; private set; }
        public int Weight;


        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="provider">Provider side of the relation</param>
        /// <param name="consumer">The module on the depending side of the relation</param>
        public Relation(Element provider, Element consumer, string type, bool isDerived, RelationTypes relationTypes)
        {
            _relationTypes = relationTypes;
            Consumer = consumer;
            Provider = provider;
            _typeId = relationTypes.AddRelationType(type);
            IsCyclic = false;
            IsDerived = isDerived;
            Weight = 0;
        }
    }
}

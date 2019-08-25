namespace DsmSuite.DsmViewer.Model.Data
{
    public class Element
    {
        private readonly int _hash;
        private readonly char _typeId;
        private readonly ElementTypes _elementTypes;

        public int Id { get; set; }

        internal string Namespace { get; }
        public string Type { get { return _elementTypes.GetElementType(_typeId); } }

        public Element(string name, string namespaceName, string type, ElementTypes elementTypes)
        {
            _elementTypes = elementTypes;
            Name = name;
            Namespace = namespaceName;
            _typeId = elementTypes.AddElementType(type);

            _hash = FullName.GetHashCode();
        }

        public string Name { get; }

        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(Namespace))
                {
                    return Namespace + "." + Name;
                }

                return Name;
            }
        }

        public int BuildNumber { set; get; }

        public override int GetHashCode()
        {
            return _hash;
        }

        public override bool Equals(object obj)
        {
            Element other = obj as Element;
            if (other == null) return false;

            if (FullName != null && other.FullName != null)
                return FullName.Equals(other.FullName);

            return FullName == other.FullName;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}

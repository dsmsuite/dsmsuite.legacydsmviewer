using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DsmSuite.DsmViewer.Model.Data
{
    public class ElementTypes
    {
        private readonly Dictionary<char, string> _elementTypes = new Dictionary<char, string>();
        private readonly Dictionary<string, char> _elementTypeIds = new Dictionary<string, char>();

        public char AddElementType(string elementType)
        {
            if (_elementTypeIds.ContainsKey(elementType))
            {
                return _elementTypeIds[elementType];
            }
            else
            {
                char id = (char)_elementTypes.Count;
                _elementTypes[id] = elementType;
                _elementTypeIds[elementType] = id;
                return id;
            }
        }

        public string GetElementType(char id)
        {
            if (_elementTypes.ContainsKey(id))
            {
                return _elementTypes[id];
            }
            else
            {
                return "unknown";
            }
        }
    }
}

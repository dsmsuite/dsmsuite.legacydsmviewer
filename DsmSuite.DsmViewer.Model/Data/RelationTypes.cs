using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DsmSuite.DsmViewer.Model.Data
{
    public class RelationTypes
    {
        private readonly Dictionary<char, string> _relationTypes = new Dictionary<char, string>();
        private readonly Dictionary<string, char> _relationTypeIds = new Dictionary<string, char>();

        public char AddRelationType(string relationType)
        {
            if (_relationTypeIds.ContainsKey(relationType))
            {
                return _relationTypeIds[relationType];
            }
            else
            {
                char id = (char)_relationTypes.Count;
                _relationTypes[id] = relationType;
                _relationTypeIds[relationType] = id;
                return id;
            }
        }

        public string GetRelationType(char id)
        {
            if (_relationTypes.ContainsKey(id))
            {
                return _relationTypes[id];
            }
            else
            {
                return "unknown";
            }
        }
    }
}

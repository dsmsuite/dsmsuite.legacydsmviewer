using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DsmSuite.DsmViewer.Model.Data;
using DsmSuite.DsmViewer.Model.Partioning;
using DsmSuite.DsmViewer.Model.Collections;
using DsmSuite.DsmViewer.Model.Exceptions;
using DsmSuite.DsmViewer.Util;

namespace DsmSuite.DsmViewer.Model.Model
{
    public class DsmModel : IDsmModel
    {
        public int BuildNumber { get; internal set; }

        private readonly List<KeyValuePair<string, List<KeyValuePair<string, string>>>> _metaData;
        private readonly List<KeyValuePair<string, string>> _processStepMetaData;
        private readonly string _processStep;
        private readonly Dictionary<string, TreeNode<Element>> _treeNodes = new Dictionary<string, TreeNode<Element>>();
        private readonly Dictionary<int, Element> _elements = new Dictionary<int, Element>();
        private readonly Dictionary<Element, Dictionary<Element, Relation>> _providerRelations = new Dictionary<Element, Dictionary<Element, Relation>>();
        private readonly Dictionary<Element, Dictionary<Element, Relation>> _consumerRelations = new Dictionary<Element, Dictionary<Element, Relation>>();
        private int _directCycles;
        private int _directRelationCount;
        private ElementTypes _elementTypes = new ElementTypes();
        private RelationTypes _relationTypes = new RelationTypes();

        public DsmModel(string processStep, Assembly executingAssembly)
        {
            _metaData = new List<KeyValuePair<string, List<KeyValuePair<string, string>>>>();

            if (processStep != null)
            {
                _processStepMetaData = new List<KeyValuePair<string, string>>();
                _processStep = processStep;
                AddMetaData("Executable", SystemInfo.GetExecutableInfo(executingAssembly));
            }

            IsModified = false;
        }

        public Tree<Element> Hierarchy
        {
            get;
            private set;
        }

        public void Import(string dsiFilename)
        {
            ModelFile modelFile = new ModelFile(dsiFilename);
            modelFile.ReadFile(ReadDsiXml);
            BuildHierarchy();
            AllocateIds();
            CalculateParentWeights();
            AnalyseCyclicDependencies();

            IsModified = true;
        }

        public void LoadModel(string dsmFilename)
        {
            Element rootElement = new Element("Root", "", "Root", _elementTypes);
            Hierarchy = new Tree<Element>(rootElement);
            BuildNumber = 0;
            ModelFilename = dsmFilename;

            ModelFile modelFile = new ModelFile(dsmFilename);
            IsCompressed = modelFile.IsCompressedFile();
            modelFile.ReadFile(ReadDsmXml);
        }

        public void SaveModel(string dsmFilename, bool compressFile)
        {
            if (_processStepMetaData != null)
            {
                AddMetaData("Total elements found", $"{_elements.Count}");
                AddMetaData("Total relations found", $"{_directRelationCount} (density={CalculateRelationDensity():0.000} %)");
                AddMetaData("Total cycles found", $"{_directCycles} (cycality={CalculateCycalityPercentage():0.000} %)");

                _metaData.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(_processStep, _processStepMetaData));
            }

            ModelFile modelFile = new ModelFile(dsmFilename);
            modelFile.WriteFile(WriteDsmXml, compressFile);
            IsModified = false;
        }

        public string ModelFilename { get; private set; }
        public bool IsModified { get; private set; }
        public bool IsCompressed { get; private set; }

        public Relation GetRelation(Element consumer, Element provider)
        {
            if (_consumerRelations.ContainsKey(consumer) && _consumerRelations[consumer].ContainsKey(provider))
            {
                return _consumerRelations[consumer][provider];
            }
            else
            {
                return null;
            }
        }

        public ICollection<Relation> GetProviderRelations(Element provider)
        {
            if (_providerRelations.ContainsKey(provider))
            {
                return _providerRelations[provider].Values;
            }
            else
            {
                return new List<Relation>();
            }
        }

        public ICollection<Relation> GetConsumerRelations(Element consumer)
        {
            if (_consumerRelations.ContainsKey(consumer))
            {
                return _consumerRelations[consumer].Values;
            }
            else
            {
                return new List<Relation>();
            }
        }

        public IList<Relation> FindRelations(TreeNode<Element> providerNode, TreeNode<Element> consumerNode)
        {
            IList<Relation> relations = new List<Relation>();
            FindRelations(providerNode, consumerNode, relations);
            return relations;
        }

        public IList<Relation> FindProviderRelations(TreeNode<Element> providerNode)
        {
            IList<Relation> relations = new List<Relation>();
            FindProviderRelations(providerNode, providerNode, relations);
            return relations;
        }

        public IList<Relation> FindConsumerRelations(TreeNode<Element> consumerNode)
        {
            IList<Relation> relations = new List<Relation>();
            FindConsumerRelations(consumerNode, consumerNode, relations);
            return relations;
        }

        public bool HasCyclicRelation(Element consumer, Element provider)
        {
            Relation relation = GetRelation(consumer, provider);

            return (relation != null && relation.IsCyclic);
        }

        public void Partition()
        {
            try
            {
                Partitioner p = new Partitioner(Hierarchy, this);
                p.Partition(SelectedTreeNode);

                AllocateIds();

                IsModified = true;
            }
            catch (Exception e)
            {
                throw new DsmException("Matrix partitioning error", e);
            }
        }

        public bool CanMoveNodeDown()
        {
            return (SelectedTreeNode?.NextSibling != null);
        }

        public bool MoveDown()
        {
            bool ok = false;
            if (CanMoveNodeDown())
            {
                // we actually move up the next sibling !
                TreeNode<Element> treeNode = SelectedTreeNode.NextSibling;
                Hierarchy.Remove(treeNode);
                Hierarchy.InsertBefore(treeNode, SelectedTreeNode);

                AllocateIds();
                ok = true;
                IsModified = true;
            }

            return ok;
        }

        public bool CanMoveNodeUp()
        {
            return (SelectedTreeNode?.PreviousSibling != null);
        }

        public bool MoveUp()
        {
            bool ok = false;
            if (CanMoveNodeUp())
            {
                TreeNode<Element> treeNode = SelectedTreeNode.PreviousSibling;
                Hierarchy.Remove(SelectedTreeNode);
                Hierarchy.InsertBefore(SelectedTreeNode, treeNode);

                AllocateIds();

                ok = true;
                IsModified = true;
            }
            return ok;
        }

        public TreeNode<Element> SelectedTreeNode { get; set; }
        public TreeNode<Element> SelectedConsumerTreeNode { get; set; }
        public TreeNode<Element> SelectedProviderTreeNode { get; set; }

        public void AddMetaData(string name, string value)
        {
            Logger.LogUserMessage($"Metadata: processStep={_processStep} name={name} value={value}");
            _processStepMetaData?.Add(new KeyValuePair<string, string>(name, value));
        }

        public ICollection<string> GetMetaDataGroupNames()
        {
            List<string> groupNames = new List<string>();
            foreach (var kv in _metaData)
            {
                groupNames.Add(kv.Key);
            }
            return groupNames;
        }

        public ICollection<KeyValuePair<string, string>> GetMetaData(string groupName)
        {
            ICollection<KeyValuePair<string, string>> metaData = null;
            foreach (var kv in _metaData)
            {
                if (kv.Key == groupName)
                {
                    metaData = kv.Value;
                }
            }

            return metaData;
        }
        
        private void ReadDsiXml(Stream stream)
        {
            using (XmlReader xReader = XmlReader.Create(stream))
            {
                while (xReader.Read())
                {
                    switch (xReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xReader.Name == "metadatagroup")
                            {
                                string groupName = xReader.GetAttribute("name");
                                List<KeyValuePair<string, string>> groupMetaData = new List<KeyValuePair<string, string>>();
                                _metaData.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(groupName, groupMetaData));

                                XmlReader xMetaDataReader = xReader.ReadSubtree();
                                while (xMetaDataReader.Read())
                                {
                                    if (xMetaDataReader.Name == "metadata")
                                    {
                                        string name = xMetaDataReader.GetAttribute("name");
                                        string value = xMetaDataReader.GetAttribute("value");
                                        if ((name != null) && (value != null))
                                        {
                                            groupMetaData.Add(new KeyValuePair<string, string>(name, value));
                                        }
                                    }
                                }
                            }

                            if (xReader.Name == "element")
                            {
                                int id;
                                int.TryParse(xReader.GetAttribute("id"), out id);
                                string name = xReader.GetAttribute("name");
                                string type = xReader.GetAttribute("type");

                                string[] nameParts = name.Split('.');
                                if (nameParts.Length >= 2)
                                {
                                    string typeName = nameParts[nameParts.Length - 1];
                                    string path = name.Substring(0, name.Length - typeName.Length - 1);
                                    Element module = CreateModule(typeName, path, type);
                                    module.Id = id;
                                    if (!_elements.ContainsKey(id))
                                    {
                                        _elements.Add(id, module);
                                    }
                                }
                            }
                            else if (xReader.Name == "relation")
                            {
                                int providerId;
                                int.TryParse(xReader.GetAttribute("providerId"), out providerId);

                                int consumerId;
                                int.TryParse(xReader.GetAttribute("consumerId"), out consumerId);

                                string type = xReader.GetAttribute("type");

                                int weight;
                                int.TryParse(xReader.GetAttribute("weight"), out weight);

                                Element consumer = _elements.ContainsKey(consumerId) ? _elements[consumerId] : null;
                                Element provider = _elements.ContainsKey(providerId) ? _elements[providerId] : null;
                                AddRelation(consumer, provider, type, weight, false, false);
                            }
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.EndElement:
                            break;
                    }
                }
            }
        }

        private void ReadDsmXml(Stream stream)
        {
            Dictionary<string, TreeNode<Element>> nodeMap = new Dictionary<string, TreeNode<Element>>();

            using (XmlReader xReader = XmlReader.Create(stream))
            {
                while (xReader.Read())
                {
                    switch (xReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xReader.Name == "metadatagroup")
                            {
                                string groupName = xReader.GetAttribute("name");
                                List<KeyValuePair<string, string>> groupMetaData = new List<KeyValuePair<string, string>>();
                                _metaData.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(groupName, groupMetaData));

                                XmlReader xMetaDataReader = xReader.ReadSubtree();
                                while (xMetaDataReader.Read())
                                {
                                    if (xMetaDataReader.Name == "metadata")
                                    {
                                        string name = xMetaDataReader.GetAttribute("name");
                                        string value = xMetaDataReader.GetAttribute("value");
                                        if ((name != null) && (value != null))
                                        {
                                            groupMetaData.Add(new KeyValuePair<string, string>(name, value));
                                        }
                                    }
                                }
                            }

                            if (xReader.Name == "element")
                            {
                                string id = xReader.GetAttribute("idref");
                                string nodeName = xReader.GetAttribute("name");
                                string parentId = xReader.GetAttribute("parent");
                                string type = xReader.GetAttribute("type");
                                string nodeNamespace = xReader.GetAttribute("namespace");

                                Element currentModule = new Element(nodeName, nodeNamespace, type, _elementTypes);

                                TreeNode<Element> parentTreeNode = null;
                                if (nodeMap.ContainsKey(parentId))
                                {
                                    parentTreeNode = nodeMap[parentId];
                                }

                                TreeNode<Element> newTreeNode = AddElement(currentModule, currentModule.FullName, parentTreeNode, 0);
                                newTreeNode.NodeValue.Id = int.Parse(id);
                                nodeMap.Add(id, newTreeNode);
                            }

                            if (xReader.Name == "relation")
                            {
                                string consumerId = xReader.GetAttribute("from");
                                string providerId = xReader.GetAttribute("to");
                                int weight = int.Parse(xReader.GetAttribute("weight"));
                                string type = xReader.GetAttribute("type");
                                bool cyclic = xReader.GetAttribute("cyclic") == "True";
                                bool derived = xReader.GetAttribute("derived") == "True";

                                if ((consumerId != null) && nodeMap.ContainsKey(consumerId) &&
                                    (providerId != null) && nodeMap.ContainsKey(providerId))
                                {
                                    Element consumer = nodeMap[consumerId].NodeValue;
                                    Element provider = nodeMap[providerId].NodeValue;
                                    AddRelation(consumer, provider, type, weight, cyclic, derived);
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.EndElement:
                            break;
                    }
                }
            }
        }

        private void WriteDsmXml(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("  ")
            };

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("model");

                foreach (KeyValuePair<string, List<KeyValuePair<string, string>>> metaDataGroup in _metaData)
                {
                    writer.WriteStartElement("metadatagroup");
                    writer.WriteAttributeString("name", metaDataGroup.Key);
                    foreach (KeyValuePair<string, string> metaData in metaDataGroup.Value)
                    {
                        writer.WriteStartElement("metadata");
                        writer.WriteAttributeString("name", metaData.Key);
                        writer.WriteAttributeString("value", metaData.Value);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("elements");
                TreeIterator<Element> it1 = new TreeIterator<Element>(Hierarchy);
                TreeNode<Element> treeNode1 = it1.Next();
                while (treeNode1 != null)
                {
                    Element module = treeNode1.NodeValue;
                    writer.WriteStartElement("element");
                    writer.WriteAttributeString("idref", module.Id.ToString());
                    writer.WriteAttributeString("name", module.Name);
                    writer.WriteAttributeString("type", module.Type);
                    writer.WriteAttributeString("parent", treeNode1.Parent.NodeValue?.Id.ToString() ?? "");
                    writer.WriteAttributeString("namespace", module.Namespace);
                    writer.WriteEndElement();
                    treeNode1 = it1.Next();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("relations");
                TreeIterator<Element> it2 = new TreeIterator<Element>(Hierarchy);
                TreeNode<Element> treeNode2 = it2.Next();
                while (treeNode2 != null)
                {
                    Element module = treeNode2.NodeValue;
                    foreach (Relation rel in GetProviderRelations(module))
                    {
                        writer.WriteStartElement("relation");
                        writer.WriteAttributeString("from", rel.Consumer.Id.ToString());
                        writer.WriteAttributeString("to", rel.Provider.Id.ToString());
                        writer.WriteAttributeString("type", rel.Type);
                        writer.WriteAttributeString("weight", rel.Weight.ToString());
                        writer.WriteAttributeString("cyclic", rel.IsCyclic.ToString());
                        writer.WriteAttributeString("derived", rel.IsDerived.ToString());
                        writer.WriteEndElement();
                    }
                    treeNode2 = it2.Next();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
        }

        private bool Contains(string key)
        {
            return _treeNodes.ContainsKey(key);
        }

        private TreeNode<Element> Get(string key)
        {
            return _treeNodes[key];
        }

        private TreeNode<Element> AddElement(Element module, string key, TreeNode<Element> parentTreeNode, int buildNumber)
        {
            module.BuildNumber = buildNumber;
            TreeNode<Element> node = Hierarchy.CreateNode(module);

            if (parentTreeNode != null)
                node.Depth = parentTreeNode.Depth + 1;

            if (buildNumber == 0)
                Hierarchy.AddLast(parentTreeNode, node);
            else
                Hierarchy.AddFirst(parentTreeNode, node);

            if (Contains(key) == false)
            {
                _treeNodes.Add(key, node);
            }

            return node;
        }

        private void AddRelation(Element consumer, Element provider, string type, int weight, bool isCyclic, bool isDerived)
        {
            if ((provider != null) && (consumer != null))
            {
                if (!_providerRelations.ContainsKey(provider))
                {
                    _providerRelations[provider] = new Dictionary<Element, Relation>();
                }

                if (!_consumerRelations.ContainsKey(consumer))
                {
                    _consumerRelations[consumer] = new Dictionary<Element, Relation>();
                }

                Relation relation;
                if (_providerRelations.ContainsKey(provider) && _providerRelations[provider].ContainsKey(consumer))
                {
                    relation = _providerRelations[provider][consumer];
                }
                else
                {
                    relation = new Relation(provider, consumer, type, isDerived, _relationTypes);
                    _providerRelations[provider][consumer] = relation;
                    _consumerRelations[consumer][provider] = relation;
                }

                relation.Weight += weight;
                relation.IsCyclic = isCyclic;
            }
        }

        private void RemoveNode(TreeNode<Element> treeNode)
        {
            // Remove treeNode from hierarchy and branch lookup and
            // recurse down doing the same for child nodes

            Element m = treeNode.NodeValue;

            if (m != null)
            {
                _treeNodes.Remove(m.FullName);
                foreach (var child in treeNode.Children)
                {
                    RemoveNode(child);
                }

                Hierarchy.Remove(treeNode);
            }
        }

        private void RemoveIfOld(int buildNumber, TreeNode<Element> current)
        {
            Element m = current.NodeValue;
            if (m != null && m.BuildNumber != buildNumber)
            {
                RemoveNode(current);
            }
            else
            {
                foreach (var child in current.Children)
                {
                    RemoveIfOld(buildNumber, child);
                }
            }
        }

        private void RemoveOldItems(int buildNumber)
        {
            TreeNode<Element> current = Hierarchy.RootTree;

            RemoveIfOld(buildNumber, current);
        }

        private void BuildHierarchy()
        {
            IList<Element> types = _elements.Values.ToList();

            if (Hierarchy == null)
            {
                Element rootElement = new Element("Root", "", "Root", _elementTypes);
                Hierarchy = new Tree<Element>(rootElement);
                BuildNumber = 0;
            }
            else
            {
                BuildNumber++;

                _providerRelations.Clear();
                _consumerRelations.Clear();
            }

            BuildTree(types);

            if (BuildNumber > 0)
            {
                RemoveOldItems(BuildNumber);
            }
        }

        private TreeNode<Element> FindNode(string key)
        {
            return Contains(key) ? Get(key) : null;
        }

        private void BuildTree(IList<Element> typeModules)
        {
            foreach (Element module in typeModules)
            {
                TreeNode<Element> parentTreeNode = null;

                bool exists = Contains(module.FullName);
                if (exists)
                {
                    parentTreeNode = Get(module.FullName);
                    parentTreeNode.NodeValue.BuildNumber = BuildNumber;

                    if (parentTreeNode.Parent?.NodeValue != null && "*".Equals(parentTreeNode.Parent.NodeValue.Name))
                    {
                        parentTreeNode.Parent.NodeValue.BuildNumber = BuildNumber;
                    }
                }
                string[] tokens = module.Namespace.Split('.');
                if (tokens[0].Length > 0)  // ignore .<Module> for the moment
                {
                    string namespacePortion = string.Empty;

                    foreach (string token in tokens)
                    {
                        namespacePortion = (namespacePortion.Length > 0)
                            ? namespacePortion + "." + token : token;

                        if (exists || Contains(namespacePortion))
                        {
                            parentTreeNode = Get(namespacePortion);
                            parentTreeNode.NodeValue.BuildNumber = BuildNumber;
                        }
                        else
                        {
                            // create a new module
                            string nspace = null;
                            int pos = namespacePortion.LastIndexOf('.');
                            if (pos != -1)
                            {
                                nspace = namespacePortion.Substring(0, pos);
                            }

                            Element m = CreateModule(token, nspace, "");
                            parentTreeNode = AddElement(m, m.FullName, parentTreeNode, BuildNumber);
                        }
                    }

                    if (!exists)
                    {
                        TreeNode<Element> treeNode = AddElement(module, module.FullName, parentTreeNode, BuildNumber);//tree.CreateNode(module);
                        treeNode.IsHidden = false;
                    }
                }
            }
        }

        private Element CreateModule(string typeName, string namespaceName, string type)
        {
            return new Element(typeName, namespaceName, type, _elementTypes) { BuildNumber = BuildNumber };
        }
        
        private void AllocateIds()
        {
            int i = 1;
            TreeIterator<Element> it = new TreeIterator<Element>(Hierarchy);

            TreeNode<Element> treeNode = it.Next();

            while (treeNode != null)
            {
                treeNode.NodeValue.Id = i;
                i++;
                treeNode = it.Next();
            }
        }

        private void AnalyseCyclicDependencies()
        {
            _directRelationCount = 0;
            _directCycles = 0;
            TreeIterator<Element> it1 = new TreeIterator<Element>(Hierarchy);
            TreeNode<Element> node1 = it1.Next();
            while (node1 != null)
            {
                Element provider = node1.NodeValue;

                foreach (Relation relation in GetProviderRelations(provider))
                {
                    if (!relation.IsDerived)
                    {
                        _directRelationCount++;
                    }
                    Element consumer = relation.Consumer;
                    Relation inverseRelation = GetRelation(provider, consumer);
                    if ((provider.Id != consumer.Id) &&
                        (inverseRelation != null) &&
                        (relation.Weight > 0) &&
                        (inverseRelation.Weight > 0))
                    {
                        relation.IsCyclic = true;
                        inverseRelation.IsCyclic = true;

                        if (!relation.IsDerived && !inverseRelation.IsDerived)
                        {
                            _directCycles++;
                        }
                    }
                }

                node1 = it1.Next();
            }
        }

        private void CalculateParentWeights()
        {
            _directRelationCount = 0;
            TreeIterator<Element> it1 = new TreeIterator<Element>(Hierarchy);
            TreeNode<Element> providerNode = it1.Next();
            while (providerNode != null)
            {
                Relation[] relations = GetProviderRelations(providerNode.NodeValue).ToArray();
                foreach (Relation rel in relations)
                {
                    _directRelationCount++;

                    TreeNode<Element> consumerNode = FindNode(rel.Consumer.FullName);

                    if (consumerNode != null)
                    {
                        TreeNode<Element> currentConsumer = consumerNode;
                        while (currentConsumer != null)
                        {
                            TreeNode<Element> currentProvider = providerNode;
                            while (currentProvider != null)
                            {
                                if ((providerNode.NodeValue != null) &&
                                    (currentProvider.NodeValue != null) &&
                                    (consumerNode.NodeValue != null) &&
                                    (currentConsumer.NodeValue != null))
                                {
                                    if ((currentConsumer.NodeValue.Id != consumerNode.NodeValue.Id) || (currentProvider.NodeValue.Id != providerNode.NodeValue.Id))
                                    {
                                        AddRelation(currentConsumer.NodeValue, currentProvider.NodeValue, "", rel.Weight, false, true);
                                    }
                                }
                                else
                                {
                                    Logger.LogError($"Node value null consumer={rel.Consumer.FullName} provider={rel.Provider.FullName}");
                                }

                                currentProvider = currentProvider.Parent;
                            }

                            currentConsumer = currentConsumer.Parent;
                        }
                    }
                }

                providerNode = it1.Next();
            }
        }

        private void FindRelations(TreeNode<Element> providerNode, TreeNode<Element> consumerNode, IList<Relation> relations)
        {
            foreach (Relation relation in GetProviderRelations(providerNode.NodeValue))
            {
                if (!relation.IsDerived && relation.Consumer.FullName.StartsWith(consumerNode.NodeValue.FullName))
                {
                    relations.Add(relation);
                }
            }

            foreach (TreeNode<Element> providerChildNode in providerNode.Children)
            {
                FindRelations(providerChildNode, consumerNode, relations);
            }
        }

        private void FindProviderRelations(TreeNode<Element> providerNode, TreeNode<Element> providerRootNode, IList<Relation> relations)
        {
            foreach (Relation relation in GetProviderRelations(providerNode.NodeValue))
            {
                if (!relation.IsDerived)
                {
                    if (!relation.Consumer.FullName.StartsWith(providerRootNode.NodeValue.FullName))
                    {
                        relations.Add(relation);
                    }
                }
            }

            foreach (TreeNode<Element> providerChildNode in providerNode.Children)
            {
                FindProviderRelations(providerChildNode, providerRootNode, relations);
            }
        }

        private void FindConsumerRelations(TreeNode<Element> consumerNode, TreeNode<Element> consumerRootNode, IList<Relation> relations)
        {
            foreach (Relation relation in GetConsumerRelations(consumerNode.NodeValue))
            {
                if (!relation.IsDerived)
                {
                    if (!relation.Provider.FullName.StartsWith(consumerRootNode.NodeValue.FullName))
                    {
                        relations.Add(relation);
                    }
                }
            }

            foreach (TreeNode<Element> providerChildNode in consumerNode.Children)
            {
                FindConsumerRelations(providerChildNode, consumerRootNode, relations);
            }
        }

        private double CalculateCycalityPercentage()
        {
            if (_directRelationCount > 0)
            {
                return (_directCycles * 100.0) / _directRelationCount;
            }
            else
            {
                return 0.0;
            }
        }

        private double CalculateRelationDensity()
        {
            return _directRelationCount * 100.0 / (_elements.Count * _elements.Count);
        }
    }
}
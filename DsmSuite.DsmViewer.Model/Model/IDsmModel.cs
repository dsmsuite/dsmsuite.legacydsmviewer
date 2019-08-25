using DsmSuite.DsmViewer.Model.Collections;
using DsmSuite.DsmViewer.Model.Data;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Model.Model
{
    public interface IDsmModel
    {
        Tree<Element> Hierarchy { get; }

        void Import(string dsiFilename);
        void LoadModel(string dsmFilename);
        void SaveModel(string dsmFilename, bool compressFile);
        string ModelFilename { get; }
        bool IsModified { get; }
        bool IsCompressed { get; }
        
        Relation GetRelation(Element consumer, Element provider);
        ICollection<Relation> GetProviderRelations(Element provider);
        ICollection<Relation> GetConsumerRelations(Element provider);

        IList<Relation> FindRelations(TreeNode<Element> providerNode, TreeNode<Element> consumerNode);
        IList<Relation> FindProviderRelations(TreeNode<Element> providerNode);
        IList<Relation> FindConsumerRelations(TreeNode<Element> consumerNode);

        bool HasCyclicRelation(Element consumer, Element provider);

        void Partition();

        bool CanMoveNodeDown();
        bool MoveDown();
        bool CanMoveNodeUp();
        bool MoveUp();

        TreeNode<Element> SelectedTreeNode{get;set;}
        TreeNode<Element> SelectedConsumerTreeNode{get;set;}
        TreeNode<Element> SelectedProviderTreeNode{get;set;}

        void AddMetaData(string name, string value);
        ICollection<string> GetMetaDataGroupNames();
        ICollection<KeyValuePair<string, string>> GetMetaData(string groupName);
    }
}

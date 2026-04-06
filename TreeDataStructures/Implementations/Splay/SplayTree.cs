using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        GetToTheRoot(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (parent == null)
        {
            return;
        }
        GetToTheRoot(parent);
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node != null) 
        {
            GetToTheRoot(node);
            value = node.Value;
            return true;
        }

        value = default;
        return false;
    }

    // Не уверен что так можно)
    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key);
        if (node != null)
        {
            GetToTheRoot(node);
            return true;
        }
        return false;
    }
    private void GetToTheRoot(BstNode<TKey,TValue> node)
    {
        while (Root != node)
        {
            var parent = node.Parent;
            var grandparent = parent?.Parent;

            if (grandparent == null && node.IsLeftChild)
            {
                RotateRight(parent);
            }
            else if (grandparent == null && node.IsRightChild)
            {
                RotateLeft(parent);
            }
            else if (node.IsRightChild && node.Parent.IsLeftChild)
            {
                RotateBigLeft(parent);
            }
            else if (node.IsLeftChild && node.Parent.IsRightChild)
            {
                RotateBigRight(parent);
            }
            else if (node.IsLeftChild && node.Parent.IsLeftChild)
            {
                RotateDoubleRight(grandparent);
            }
            else if (node.IsRightChild && node.Parent.IsRightChild)
            {
                RotateDoubleLeft(grandparent);
            }
        }
    }
}

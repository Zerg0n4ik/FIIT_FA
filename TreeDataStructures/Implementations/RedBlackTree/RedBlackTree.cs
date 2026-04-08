using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        RbNode<TKey, TValue> newNode = new RbNode<TKey, TValue>(key, value);
        newNode.Color = RbColor.Red;
        return newNode;
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        FixAfterAdding(newNode);
    }
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        if (child == null) return;
        if (child.Color == RbColor.Red) return;

        FixAfterDeleting(parent, child);
    }

    private void FixAfterAdding(RbNode<TKey, TValue> node) 
    {
        RbNode<TKey, TValue> current = node;
        while (current != null && current.Parent != null && current.Parent.Color == RbColor.Red)
        {
            RbNode<TKey, TValue> parent = current.Parent;
            RbNode<TKey, TValue> grandparent = current.Parent.Parent;
            if (grandparent == null) break;

            RbNode<TKey, TValue> uncle;
            if (parent.IsLeftChild)
            {
                uncle = grandparent.Right;
            }
            else
            {
                uncle = grandparent.Left;
            }

            if (parent.IsLeftChild)
            {
                if (uncle != null && uncle.Color == RbColor.Red)
                {
                    parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    current = grandparent;
                }
                else
                {
                    if (current.IsLeftChild)
                    {
                        parent.Color = RbColor.Black;
                        grandparent.Color = RbColor.Red;
                        RotateRight(grandparent);
                    }
                    else
                    {
                        current.Color = RbColor.Black;
                        grandparent.Color = RbColor.Red;
                        RotateLeft(parent);
                        RotateRight(grandparent);
                    }
                    break;
                }
            }
            else
            {
                if (uncle != null && uncle.Color == RbColor.Red)
                {
                    parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    current = grandparent;
                }
                else
                {
                    if (current.IsLeftChild)
                    {
                        current.Color = RbColor.Black;
                        grandparent.Color = RbColor.Red;
                        RotateRight(parent);
                        RotateLeft(grandparent);
                    }
                    else
                    {
                        parent.Color = RbColor.Black;
                        grandparent.Color = RbColor.Red;
                        RotateLeft(grandparent);
                    }
                    break;
                }
            }
        }
        Root.Color = RbColor.Black;
    }

    private void FixAfterDeleting(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        while (child != Root && child.Color == RbColor.Black)
        {
            if (parent == null) break;

            if (child.IsLeftChild)
            {
                var brother = parent?.Right;
                if (brother == null) { break; }

                if (brother.Color == RbColor.Red)
                {
                    parent.Color = RbColor.Red;
                    brother.Color = RbColor.Black;
                    RotateLeft(parent);
                    brother = parent?.Right;
                    if (brother == null) break;
                }

                if ((brother.Left?.Color ?? RbColor.Black) == RbColor.Black &&
                    (brother.Right?.Color ?? RbColor.Black) == RbColor.Black)
                {
                    brother.Color = RbColor.Red;
                    child = parent;
                    parent = child?.Parent;
                }

                else
                {
                    if ((brother.Left?.Color ?? RbColor.Black) == RbColor.Red)
                    {
                        brother.Left.Color = RbColor.Black;
                        brother.Color = RbColor.Red;
                        RotateRight(brother);
                        brother = parent.Right;

                        if (brother == null) break; 
                    }

                    if (parent != null)
                    {
                        brother.Color = parent.Color;
                        brother.Right.Color = RbColor.Black;
                        parent.Color = RbColor.Black;
                        RotateLeft(parent);
                        break;
                    }
                }
            }

            else
            {
                var brother = parent?.Left;
                if (brother == null) break; 

                if (brother.Color == RbColor.Red)
                {
                    parent.Color = RbColor.Red;
                    brother.Color = RbColor.Black;
                    RotateRight(parent);
                    brother = parent?.Left;
                    if (brother == null) break;
                }

                if ((brother.Left?.Color ?? RbColor.Black) == RbColor.Black &&
                    (brother.Right?.Color ?? RbColor.Black) == RbColor.Black)
                {
                    brother.Color = RbColor.Red;
                    child = parent;
                    parent = child?.Parent;
                }

                else
                {
                    if ((brother.Right?.Color ?? RbColor.Black) == RbColor.Red)
                    {
                        brother.Right.Color = RbColor.Black;
                        brother.Color = RbColor.Red;
                        RotateLeft(brother);
                        brother = parent.Left;
                        if (brother == null) break;
                    }

                    if (parent != null)
                    {
                        brother.Color = parent.Color;
                        brother.Left.Color = RbColor.Black;
                        parent.Color = RbColor.Black;
                        RotateRight(parent);
                        break;
                    }
                }
            }
        }
    }
}
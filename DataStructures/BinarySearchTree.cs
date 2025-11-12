namespace ST10159832KeenanPROGpart1.DataStructures
{
    
        // Generic Binary Search Tree (BST)
        public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
        {
            // Each node holds a key + value
            private class Node
            {
                public TKey Key;
                public TValue Value;
                public Node Left, Right;

                public Node(TKey key, TValue value)
                {
                    Key = key;
                    Value = value;
                }
            }

            private Node _root;
            public int Count { get; private set; }

            // Add (insert) a new item into the tree
            public void Insert(TKey key, TValue value)
            {
                _root = Insert(_root, key, value);
                Count++;
            }

            // Recursive insertion logic
            private Node Insert(Node node, TKey key, TValue value)
            {
                if (node == null)
                    return new Node(key, value);

                int compare = key.CompareTo(node.Key);

                if (compare < 0)
                    node.Left = Insert(node.Left, key, value);   // smaller → go left
                else if (compare > 0)
                    node.Right = Insert(node.Right, key, value); // larger → go right
                else
                    node.Value = value; // update if key exists

                return node;
            }

            // Find an item by key (search)
            public bool TryGet(TKey key, out TValue value)
            {
                var current = _root;
                while (current != null)
                {
                    int compare = key.CompareTo(current.Key);
                    if (compare == 0)
                    {
                        value = current.Value;
                        return true;
                    }

                    current = (compare < 0) ? current.Left : current.Right;
                }

                value = default;
                return false;
            }

            // Read all items in sorted order (in-order traversal)
            public IEnumerable<KeyValuePair<TKey, TValue>> InOrder()
            {
                var stack = new Stack<Node>();
                var current = _root;

                while (current != null || stack.Count > 0)
                {
                    while (current != null)
                    {
                        stack.Push(current);
                        current = current.Left;
                    }

                    current = stack.Pop();
                    yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    current = current.Right;
                }
            }
        }
    
}
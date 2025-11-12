namespace ST10159832KeenanPROGpart1.DataStructures
{
    // Generic AVL Tree
    public class AVLTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node Left, Right;
            public int Height = 1;

            public Node(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        private Node _root;
        public int Count { get; private set; }

        private int Height(Node n) => n?.Height ?? 0;
        private int Balance(Node n) => (n == null) ? 0 : Height(n.Left) - Height(n.Right);

        private void Update(Node n)
        {
            n.Height = 1 + Math.Max(Height(n.Left), Height(n.Right));
        }

        // Insert a new key-value pair
        public void Insert(TKey key, TValue value)
        {
            _root = Insert(_root, key, value);
            Count++;
        }

        private Node Insert(Node node, TKey key, TValue value)
        {
            if (node == null)
                return new Node(key, value);

            int compare = key.CompareTo(node.Key);

            if (compare < 0)
                node.Left = Insert(node.Left, key, value);
            else if (compare > 0)
                node.Right = Insert(node.Right, key, value);
            else
                node.Value = value;

            Update(node);
            int balance = Balance(node);

            // Rotation cases:
            if (balance > 1 && key.CompareTo(node.Left.Key) < 0)
                return RotateRight(node); // Left Left
            if (balance < -1 && key.CompareTo(node.Right.Key) > 0)
                return RotateLeft(node); // Right Right
            if (balance > 1 && key.CompareTo(node.Left.Key) > 0)
            {
                node.Left = RotateLeft(node.Left); // Left Right
                return RotateRight(node);
            }
            if (balance < -1 && key.CompareTo(node.Right.Key) < 0)
            {
                node.Right = RotateRight(node.Right); // Right Left
                return RotateLeft(node);
            }

            return node;
        }

        private Node RotateRight(Node y)
        {
            var x = y.Left;
            var T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            Update(y);
            Update(x);

            return x;
        }

        private Node RotateLeft(Node x)
        {
            var y = x.Right;
            var T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            Update(x);
            Update(y);

            return y;
        }

        // Read items in order (sorted)
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

        // Try to find a specific key
        public bool TryGet(TKey key, out TValue value)
        {
            var n = _root;
            while (n != null)
            {
                int compare = key.CompareTo(n.Key);
                if (compare == 0)
                {
                    value = n.Value;
                    return true;
                }

                n = (compare < 0) ? n.Left : n.Right;
            }
            value = default;
            return false;
        }
    }
}
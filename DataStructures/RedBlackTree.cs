namespace ST10159832KeenanPROGpart1.DataStructures
{
    public class RedBlackTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private const bool RED = true;
        private const bool BLACK = false;

        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node Left, Right;
            public bool Color = RED;

            public Node(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        private Node _root;
        public int Count { get; private set; }

        private bool IsRed(Node x) => x != null && x.Color == RED;

        private Node RotateLeft(Node h)
        {
            var x = h.Right;
            h.Right = x.Left;
            x.Left = h;
            x.Color = h.Color;
            h.Color = RED;
            return x;
        }

        private Node RotateRight(Node h)
        {
            var x = h.Left;
            h.Left = x.Right;
            x.Right = h;
            x.Color = h.Color;
            h.Color = RED;
            return x;
        }

        private void FlipColors(Node h)
        {
            h.Color = RED;
            if (h.Left != null) h.Left.Color = BLACK;
            if (h.Right != null) h.Right.Color = BLACK;
        }

        // Insert new key/value
        public void Put(TKey key, TValue value)
        {
            _root = Put(_root, key, value);
            _root.Color = BLACK; // root is always black
            Count++;
        }

        private Node Put(Node h, TKey key, TValue value)
        {
            if (h == null) return new Node(key, value);

            int cmp = key.CompareTo(h.Key);

            if (cmp < 0)
                h.Left = Put(h.Left, key, value);
            else if (cmp > 0)
                h.Right = Put(h.Right, key, value);
            else
                h.Value = value;

            // Fix right-leaning links and color flips
            if (IsRed(h.Right) && !IsRed(h.Left)) h = RotateLeft(h);
            if (IsRed(h.Left) && IsRed(h.Left.Left)) h = RotateRight(h);
            if (IsRed(h.Left) && IsRed(h.Right)) FlipColors(h);

            return h;
        }

        // Traverse the tree in sorted order
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

        // Find a value by key
        public bool TryGet(TKey key, out TValue value)
        {
            var node = _root;
            while (node != null)
            {
                int cmp = key.CompareTo(node.Key);
                if (cmp == 0)
                {
                    value = node.Value;
                    return true;
                }
                node = (cmp < 0) ? node.Left : node.Right;
            }
            value = default;
            return false;
        }
    }
}
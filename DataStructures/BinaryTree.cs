namespace ST10159832KeenanPROGpart1.DataStructures
{
    // Generic Binary Tree – can hold any data type (like ServiceRequest)
    public class BinaryTree<T>
    {
        // Node = one piece of data (with optional left/right children)
        public class Node
        {
            public T Value;
            public Node Left;
            public Node Right;

            public Node(T value)
            {
                Value = value;
            }
        }

        // The root is the very top of the tree (like a main issue)
        public Node Root { get; private set; }

        // Sets the root node
        public void SetRoot(T value)
        {
            Root = new Node(value);
        }

        // Traverse (read) all nodes in pre-order (root, left, right)
        public IEnumerable<T> PreOrder()
        {
            var stack = new Stack<Node>();
            if (Root != null) stack.Push(Root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current.Value;

                if (current.Right != null) stack.Push(current.Right);
                if (current.Left != null) stack.Push(current.Left);
            }
        }

        // Traverse nodes in in-order (left, root, right)
        public IEnumerable<T> InOrder()
        {
            var stack = new Stack<Node>();
            var current = Root;

            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                yield return current.Value;
                current = current.Right;
            }
        }
    }
}
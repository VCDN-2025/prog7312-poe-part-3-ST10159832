namespace ST10159832KeenanPROGpart1.DataStructures
{
    // Generic MinHeap – smallest item always stays at the top
    public class MinHeap<T> where T : IComparable<T>
    {
        private readonly List<T> _data = new();  // stores all items
        public int Count => _data.Count;

        // Add an item to the heap
        public void Push(T item)
        {
            _data.Add(item);
            BubbleUp(_data.Count - 1);
        }

        // Look at the smallest item without removing it
        public T Peek() => _data[0];

        // Remove and return the smallest item
        public T Pop()
        {
            var root = _data[0];
            var last = _data[^1];
            _data[0] = last;
            _data.RemoveAt(_data.Count - 1);

            if (_data.Count > 0)
                BubbleDown(0);

            return root;
        }

        // Move item up until heap property is restored
        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_data[index].CompareTo(_data[parent]) >= 0)
                    break;

                (_data[index], _data[parent]) = (_data[parent], _data[index]);
                index = parent;
            }
        }

        // Move item down until heap property is restored
        private void BubbleDown(int index)
        {
            int lastIndex = _data.Count - 1;

            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left <= lastIndex && _data[left].CompareTo(_data[smallest]) < 0)
                    smallest = left;
                if (right <= lastIndex && _data[right].CompareTo(_data[smallest]) < 0)
                    smallest = right;

                if (smallest == index) break;

                (_data[index], _data[smallest]) = (_data[smallest], _data[index]);
                index = smallest;
            }
        }

        // Optional: view all items without popping them
        public IEnumerable<T> Unordered() => _data;
    }
}
namespace ST10159832KeenanPROGpart1.DataStructures
{
    public class Graph<T>
    {
        // Represents an edge (connection between two nodes)
        public class Edge
        {
            public int From;
            public int To;
            public double Weight;

            public Edge(int from, int to, double weight)
            {
                From = from;
                To = to;
                Weight = weight;
            }
        }

        private readonly List<T> _nodes = new(); // list of locations
        private readonly List<List<(int to, double w)>> _adj = new(); // adjacency list

        // Add a new node (like “Durban North”)
        public int AddNode(T value)
        {
            _nodes.Add(value);
            _adj.Add(new());
            return _nodes.Count - 1; // returns index
        }

        // Connect two nodes (undirected edge)
        public void AddUndirectedEdge(int a, int b, double w)
        {
            _adj[a].Add((b, w));
            _adj[b].Add((a, w));
        }

        // Breadth-First Search (BFS)
        public IEnumerable<int> Bfs(int start)
        {
            var visited = new bool[_nodes.Count];
            var queue = new Queue<int>();
            queue.Enqueue(start);
            visited[start] = true;

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;

                foreach (var (neighbor, _) in _adj[node])
                {
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // Depth-First Search (DFS)
        public IEnumerable<int> Dfs(int start)
        {
            var visited = new bool[_nodes.Count];
            var stack = new Stack<int>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (visited[node]) continue;

                visited[node] = true;
                yield return node;

                foreach (var (neighbor, _) in _adj[node].OrderByDescending(x => x.to))
                {
                    stack.Push(neighbor);
                }
            }
        }

        // Kruskal’s Algorithm – Minimum Spanning Tree (shortest total route)
        public (List<Edge> mst, double total) Kruskal()
        {
            var edges = new List<Edge>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                foreach (var (to, w) in _adj[i])
                    if (i < to) edges.Add(new Edge(i, to, w)); // avoid duplicates
            }

            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            var parent = Enumerable.Range(0, _nodes.Count).ToArray();
            var rank = new int[_nodes.Count];

            int Find(int x)
            {
                if (parent[x] != x) parent[x] = Find(parent[x]);
                return parent[x];
            }

            void Union(int a, int b)
            {
                a = Find(a);
                b = Find(b);
                if (a == b) return;

                if (rank[a] < rank[b]) parent[a] = b;
                else if (rank[a] > rank[b]) parent[b] = a;
                else { parent[b] = a; rank[a]++; }
            }

            var mst = new List<Edge>();
            double total = 0;

            foreach (var edge in edges)
            {
                int rootA = Find(edge.From);
                int rootB = Find(edge.To);

                if (rootA != rootB)
                {
                    mst.Add(edge);
                    total += edge.Weight;
                    Union(rootA, rootB);
                }
            }

            return (mst, total);
        }

        // Helpful accessors
        public int NodeCount => _nodes.Count;
        public T GetNode(int index) => _nodes[index];
    }
}
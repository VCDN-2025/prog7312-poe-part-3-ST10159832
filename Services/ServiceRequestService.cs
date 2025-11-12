using ST10159832KeenanPROGpart1.DataStructures;
using ST10159832KeenanPROGpart1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10159832KeenanPROGpart1.Services
{
    public class ServiceRequestService
    {
        // Singleton pattern – only one instance shared
        private static readonly Lazy<ServiceRequestService> _instance =
            new(() => new ServiceRequestService());
        public static ServiceRequestService Instance => _instance.Value;

        // Core storage (acts like your database for now)
        private readonly Dictionary<string, ServiceRequest> _byId = new();

        // Data structures for sorting and performance
        private BinarySearchTree<string, ServiceRequest> _bstById = new();
        private AVLTree<DateTime, ServiceRequest> _avlByCreated = new();
        private RedBlackTree<(int priority, long order), ServiceRequest> _rbtByPriority = new();
        private MinHeap<JobItem> _heap = new();
        private Graph<string> _graph = new();

        private long _sequence = 0; // keeps order consistent for priority sorting

        // Private constructor to seed data
        private ServiceRequestService()
        {
            SeedData();
            RebuildIndexes();
            BuildLocationGraph();
        }

        // --------------- MAIN PUBLIC METHODS ---------------

        // Return all requests
        public IEnumerable<ServiceRequest> GetAll() => _byId.Values;

        // Get one by ID
        public ServiceRequest GetById(string id)
        {
            _byId.TryGetValue(id, out var req);
            return req;
        }

        // Sorting options for your UI
        public IEnumerable<ServiceRequest> SortById()
            => _bstById.InOrder().Select(p => p.Value);

        public IEnumerable<ServiceRequest> SortByCreated()
            => _avlByCreated.InOrder().Select(p => p.Value);

        public IEnumerable<ServiceRequest> SortByPriority()
            => _rbtByPriority.InOrder().Select(p => p.Value);

        // “Next Jobs” list from heap
        public IEnumerable<ServiceRequest> GetNextJobs(int take = 5)
            => _heap.Unordered().OrderBy(x => x).Take(take).Select(x => x.Request);

        // --------------- DEPENDENCY HANDLING ---------------

        // When a main issue is resolved, all dependents also close
        public void ResolveRequest(string id)
        {
            if (!_byId.TryGetValue(id, out var mainIssue)) return;

            mainIssue.Status = RequestStatus.Closed;
            ResolveDependents(mainIssue);
        }

        private void ResolveDependents(ServiceRequest parent)
        {
            foreach (var child in parent.Dependents)
            {
                child.Status = RequestStatus.Closed;
                if (child.Dependents.Count > 0)
                    ResolveDependents(child);
            }
        }

        // --------------- GRAPH TOOLS (Locations) ---------------

        public string RunBfs(string startLocation)
        {
            int startIndex = FindLocationIndex(startLocation);
            if (startIndex < 0) return "Location not found.";

            var order = _graph.Bfs(startIndex).Select(i => _graph.GetNode(i));
            return string.Join(" → ", order);
        }

        public string RunDfs(string startLocation)
        {
            int startIndex = FindLocationIndex(startLocation);
            if (startIndex < 0) return "Location not found.";

            var order = _graph.Dfs(startIndex).Select(i => _graph.GetNode(i));
            return string.Join(" → ", order);
        }

        public string GetMstSummary()
        {
            var (mst, total) = _graph.Kruskal();
            var lines = mst.Select(e => $"{_graph.GetNode(e.From)} — {_graph.GetNode(e.To)} : {e.Weight:0.0} km");
            return string.Join(Environment.NewLine, lines) + $"\nTotal Distance: {total:0.0} km";
        }


        // Move status Open -> InProgress -> Closed
        public void UpdateStatus(string id)
        {
            var request = GetById(id);
            if (request == null) return;

            switch (request.Status)
            {
                case RequestStatus.Open:
                    request.Status = RequestStatus.InProgress;
                    break;
                case RequestStatus.InProgress:
                    request.Status = RequestStatus.Closed;
                    break;
                default:
                    break;
            }

            request.UpdatedAt = DateTime.UtcNow;

            // Optional: If this is a main issue and it closes, close dependents too
            if (request.Status == RequestStatus.Closed && request.Dependents.Count > 0)
                ResolveDependents(request);
        }



        // --------------- INTERNAL HELPERS ---------------

        private void SeedData()
        {
            // === Parent Issue 1 ===
            var mainWaterIssue = new ServiceRequest
            {
                Id = "M001",
                Title = "Main Water Line Burst – Durban North",
                Category = "Water",
                Location = "Durban North",
                Priority = RequestPriority.Critical,
                Status = RequestStatus.InProgress,
                EtaUtc = DateTime.UtcNow.AddHours(1)
            };

            // Dependent child issues (Water)
            var house21 = new ServiceRequest
            {
                Id = "R101",
                Title = "No Water – House 21",
                Category = "Water",
                Location = "Durban North",
                ParentId = mainWaterIssue.Id,
                Priority = RequestPriority.High,
                EtaUtc = DateTime.UtcNow.AddHours(2)
            };

            var house45 = new ServiceRequest
            {
                Id = "R102",
                Title = "No Water – House 45",
                Category = "Water",
                Location = "Durban North",
                ParentId = mainWaterIssue.Id,
                Priority = RequestPriority.High,
                EtaUtc = DateTime.UtcNow.AddHours(2)
            };

            var house63 = new ServiceRequest
            {
                Id = "R103",
                Title = "Low Pressure – House 63",
                Category = "Water",
                Location = "La Lucia", // ✅ Updated from Durban North → La Lucia
                ParentId = mainWaterIssue.Id,
                Priority = RequestPriority.Medium,
                EtaUtc = DateTime.UtcNow.AddHours(3)
            };

            mainWaterIssue.Dependents.AddRange(new[] { house21, house45, house63 });

            // === Parent Issue 2 ===
            var powerOutageMain = new ServiceRequest
            {
                Id = "M002",
                Title = "Power Outage – Springfield Substation Failure",
                Category = "Electrical",
                Location = "Springfield",
                Priority = RequestPriority.Critical,
                Status = RequestStatus.Open,
                EtaUtc = DateTime.UtcNow.AddHours(2)
            };

            // Dependent child issues (Electricity)
            var glenwoodOutage = new ServiceRequest
            {
                Id = "R210",
                Title = "Power Outage – Glenwood Area",
                Category = "Electrical",
                Location = "Glenwood",
                ParentId = powerOutageMain.Id,
                Priority = RequestPriority.High,
                EtaUtc = DateTime.UtcNow.AddHours(4)
            };

            var morningsideOutage = new ServiceRequest
            {
                Id = "R211",
                Title = "No Power – Morningside Flats",
                Category = "Electrical",
                Location = "Morningside",
                ParentId = powerOutageMain.Id,
                Priority = RequestPriority.High,
                EtaUtc = DateTime.UtcNow.AddHours(5)
            };

            powerOutageMain.Dependents.AddRange(new[] { glenwoodOutage, morningsideOutage });

            // === Independent Issues (no dependents) ===
            var streetlight = new ServiceRequest
            {
                Id = "R201",
                Title = "Streetlight Out – Oak Ave",
                Category = "Electrical",
                Location = "Musgrave", // ✅ Updated from Glenwood → Musgrave
                Priority = RequestPriority.Medium,
                EtaUtc = DateTime.UtcNow.AddHours(5)
            };

            var garbageDelay = new ServiceRequest
            {
                Id = "R305",
                Title = "Missed Garbage Collection – Hillcrest",
                Category = "Sanitation",
                Location = "Hillcrest",
                Priority = RequestPriority.Medium,
                EtaUtc = DateTime.UtcNow.AddHours(10)
            };

            var pothole = new ServiceRequest
            {
                Id = "R202",
                Title = "Pothole – Pine Rd",
                Category = "Roads",
                Location = "Umhlanga",
                Priority = RequestPriority.Low,
                EtaUtc = DateTime.UtcNow.AddHours(12)
            };

            var parkMaintenance = new ServiceRequest
            {
                Id = "R109",
                Title = "Park Maintenance – Umhlanga Rocks",
                Category = "Public Works",
                Location = "Bluff", // ✅ Updated from Umhlanga → Bluff
                Priority = RequestPriority.Low,
                EtaUtc = DateTime.UtcNow.AddHours(8)
            };

            var flooding = new ServiceRequest
            {
                Id = "R150",
                Title = "Flooded Intersection – Ridge Road",
                Category = "Roads",
                Location = "Morningside",
                Priority = RequestPriority.High,
                EtaUtc = DateTime.UtcNow.AddHours(6)
            };

            // === Add everything (in random order for testing sorting) ===
            foreach (var r in new[]
            {
        garbageDelay, house21, powerOutageMain, pothole, mainWaterIssue,
        house45, streetlight, house63, parkMaintenance, glenwoodOutage, morningsideOutage, flooding
    })
                _byId[r.Id] = r;
        }



        private void RebuildIndexes()
        {
            _bstById = new BinarySearchTree<string, ServiceRequest>();
            _avlByCreated = new AVLTree<DateTime, ServiceRequest>();
            _rbtByPriority = new RedBlackTree<(int, long), ServiceRequest>();
            _heap = new MinHeap<JobItem>();

            foreach (var req in _byId.Values)
            {
                _bstById.Insert(req.Id, req);
                _avlByCreated.Insert(req.CreatedAt, req);
                _rbtByPriority.Put((-(int)req.Priority, _sequence++), req);
                _heap.Push(new JobItem(req));
            }
        }

        private void BuildLocationGraph()
        {
            int dn = _graph.AddNode("Durban North");
            int gl = _graph.AddNode("Glenwood");
            int um = _graph.AddNode("Umhlanga");
            int mw = _graph.AddNode("Morningside");
            int sp = _graph.AddNode("Springfield");

            _graph.AddUndirectedEdge(dn, gl, 5.0);
            _graph.AddUndirectedEdge(dn, um, 8.0);
            _graph.AddUndirectedEdge(mw, gl, 4.2);
            _graph.AddUndirectedEdge(um, sp, 7.5);
            _graph.AddUndirectedEdge(sp, gl, 5.3);
        }

        private int FindLocationIndex(string name)
        {
            // Access private _nodes list via reflection (simplified)
            var field = typeof(Graph<string>).GetField("_nodes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodes = (List<string>)field.GetValue(_graph);
            return nodes.FindIndex(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
        }

        // Helper class for heap comparison
        private class JobItem : IComparable<JobItem>
        {
            public ServiceRequest Request { get; }
            public JobItem(ServiceRequest r) { Request = r; }

            public int CompareTo(JobItem other)
            {
                int etaCompare = DateTime.Compare(Request.EtaUtc, other.Request.EtaUtc);
                if (etaCompare != 0)
                    return etaCompare;

                // If ETA is the same, compare by priority (higher = earlier)
                return -Request.Priority.CompareTo(other.Request.Priority);
            }
        }
    }
}
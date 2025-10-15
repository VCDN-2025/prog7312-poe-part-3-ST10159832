using Microsoft.AspNetCore.Mvc;
using ST10159832KeenanPROGpart1.Models;

namespace ST10159832KeenanPROGpart1.Controllers
{
    public class IssuesController : Controller
    {
        
        private static LinkedList issueList = new LinkedList();
        private readonly IWebHostEnvironment _env;

        // https://www.youtube.com/watch?v=-ErugC3CcAY&t=301s -- video used for stacks and retrieval 
        // https://www.youtube.com/watch?v=4miX1JH6luo -- video used for queues and enqueues 
        // https://www.youtube.com/watch?v=ikbp6vph-6s -- video used for priority queues 
        // https://www.youtube.com/watch?v=9y0DKw_H9zw -- video used for sorted dictionary and dictionary 
        // https://www.youtube.com/watch?v=wHKlP8cov9w -- videos used for hashsets 
        private static Queue<LocalEvent> upcomingQueue = new Queue<LocalEvent>();                       
        private static Stack<string> recentSearchStack = new Stack<string>();                          
        private static PriorityQueue<LocalEvent, int> urgentPriorityQueue = new PriorityQueue<LocalEvent, int>(); 
        private static SortedDictionary<DateTime, List<LocalEvent>> eventsByDate = new SortedDictionary<DateTime, List<LocalEvent>>(); 
        private static HashSet<string> uniqueCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase); 
        private static Dictionary<string, int> searchFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); 

        
        public IssuesController(IWebHostEnvironment env)
        {
            _env = env;
        }

       
        static IssuesController()
        {
           
            SeedEvent(new LocalEvent("Beach Cleanup", "Environment", DateTime.Parse("2025-10-15"), "Community beach cleanup - bring gloves and bags", priority: 3));
            SeedEvent(new LocalEvent("Music Festival", "Entertainment", DateTime.Parse("2025-10-20"), "Live music, food stalls, family friendly.", priority: 5));
            SeedEvent(new LocalEvent("Community Meeting", "Government", DateTime.Parse("2025-10-18"), "Town hall meeting about upcoming projects.", priority: 2));
            SeedEvent(new LocalEvent("Art Exhibition", "Culture", DateTime.Parse("2025-10-25"), "Local artists exhibition opening night.", priority: 5));
            SeedEvent(new LocalEvent("Water Service Notice", "Alert", DateTime.Parse("2025-10-12"), "Planned water outage for maintenance.", priority: 1));
            SeedEvent(new LocalEvent("Small Business Workshop", "Business", DateTime.Parse("2025-10-30"), "Free entrepreneurship training.", priority: 4));
            SeedEvent(new LocalEvent("Road Safety Campaign", "Environment", DateTime.Parse("2025-10-22"), "Road safety awareness and free checks.", priority: 3));
        }

        private static void SeedEvent(LocalEvent ev)
        {
           
            upcomingQueue.Enqueue(ev);

           
            var key = ev.Date.Date;
            if (!eventsByDate.ContainsKey(key))
                eventsByDate[key] = new List<LocalEvent>();
            eventsByDate[key].Add(ev);

            
            if (!string.IsNullOrWhiteSpace(ev.Category))
                uniqueCategories.Add(ev.Category);

           
            urgentPriorityQueue.Enqueue(ev, ev.Priority);
        }

        
        [HttpGet]
        public IActionResult LocalEvents(string q = "", string category = "", string date = "")
        {
            
            ViewBag.Categories = uniqueCategories;
            ViewBag.RecentSearches = recentSearchStack.Take(10).ToList(); 

           
            var allEvents = eventsByDate.Values.SelectMany(list => list).OrderBy(ev => ev.Date).ToList();

           
            IEnumerable<LocalEvent> filtered = allEvents;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLower = q.Trim().ToLower();
                filtered = filtered.Where(e => (e.Title != null && e.Title.ToLower().Contains(qLower)) ||
                                               (e.Description != null && e.Description.ToLower().Contains(qLower)) ||
                                               (e.Category != null && e.Category.ToLower().Contains(qLower)));
                TrackSearch(qLower);
            }
            else if (!string.IsNullOrWhiteSpace(category))
            {
                var cat = category.Trim();
                filtered = filtered.Where(e => string.Equals(e.Category, cat, StringComparison.OrdinalIgnoreCase));
                TrackSearch(cat);
            }

           
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime parsed))
            {
                filtered = filtered.Where(e => e.Date.Date == parsed.Date);
                TrackSearch(parsed.ToShortDateString());
            }

           
            var recommendations = GetRecommendations(5);

           
            var urgent = PeekPriorityQueue(3);

            ViewBag.Recommendations = recommendations;
            ViewBag.Urgent = urgent;

            return View(filtered);
        }

       
        private static void TrackSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return;

            
            recentSearchStack.Push(term);
            if (recentSearchStack.Count > 50)
            {
               
                var temp = recentSearchStack.Take(50).ToList();
                recentSearchStack.Clear();
                foreach (var t in temp) recentSearchStack.Push(t);
            }

            
            if (searchFrequency.ContainsKey(term))
                searchFrequency[term]++;
            else
                searchFrequency[term] = 1;
        }

        
        private static List<LocalEvent> GetRecommendations(int max)
        {
            var recs = new List<LocalEvent>();

            if (searchFrequency.Count == 0)
                return recs;

            
            var topTerms = searchFrequency.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).Take(3).ToList();

            
            var allEvents = eventsByDate.Values.SelectMany(x => x).ToList();
            foreach (var t in topTerms)
            {
                var matches = allEvents.Where(e =>
                    (e.Category != null && e.Category.Equals(t, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Title != null && e.Title.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.Description != null && e.Description.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
                );

                foreach (var m in matches)
                {
                    if (!recs.Any(r => r.Id == m.Id))
                    {
                        recs.Add(m);
                        if (recs.Count >= max) return recs;
                    }
                }
            }

            return recs;
        }

       
        private static List<LocalEvent> PeekPriorityQueue(int n)
        {
            var list = new List<LocalEvent>();
            if (urgentPriorityQueue == null) return list;

            
            var temp = new PriorityQueue<LocalEvent, int>();
            while (urgentPriorityQueue.Count > 0)
            {
                urgentPriorityQueue.TryDequeue(out var ev, out var pr);
                list.Add(ev);
                temp.Enqueue(ev, pr);
                if (list.Count >= n) break;
            }

           
            while (temp.Count > 0)
            {
                temp.TryDequeue(out var e, out var p);
                urgentPriorityQueue.Enqueue(e, p);
            }

            return list;
        }

        

        [HttpGet]
        public IActionResult ReportIssue()
        {
            return View(new IssueViewModel());
        }

        [HttpPost]
        public IActionResult ReportIssue([FromForm] IssueViewModel model, [FromForm] IFormFile Attachment)
        {
            try
            {
                if (Attachment != null && Attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    var filePath = Path.Combine(uploads, Attachment.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Attachment.CopyTo(stream);
                    }
                    model.FileName = Attachment.FileName;
                }

                issueList.Add(model);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        [HttpGet]
        public IActionResult AllReports()
        {
            var issues = issueList.GetAll();
            return View(issues);
        }

        
        [HttpGet]
        public IActionResult ServiceRequest()
        {
            return View();
        }
    }
}

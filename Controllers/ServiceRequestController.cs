using Microsoft.AspNetCore.Mvc;
using ST10159832KeenanPROGpart1.Models;
using ST10159832KeenanPROGpart1.Services;

namespace ST10159832KeenanPROGpart1.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ServiceRequestService _service = ServiceRequestService.Instance;

        // Shows list of service requests
        public IActionResult Index(string sortBy)
        {
            IEnumerable<ServiceRequest> data = sortBy switch
            {
                "priority" => _service.SortByPriority(),
                "created" => _service.SortByCreated(),
                _ => _service.SortById(), // default
            };

            ViewBag.SortBy = sortBy;
            return View(data);
        }

        // Shows one specific request
        public IActionResult Details(string id)
        {
            var request = _service.GetById(id);
            if (request == null)
                return NotFound();

            return View(request);
        }

        // Mark a main issue as resolved (auto closes dependents)
       /* [HttpPost]
        public IActionResult Resolve(string id)
        {
            _service.ResolveRequest(id);
            TempData["Message"] = $"Main issue {id} and dependents resolved.";
            return RedirectToAction("Index");
        } */

        // Shows BFS, DFS, MST pages for location graph
        public IActionResult GraphTools(string type, string location)
        {
            string output = type switch
            {
                "bfs" => _service.RunBfs(location),
                "dfs" => _service.RunDfs(location),
                "mst" => _service.GetMstSummary(),
                _ => "Invalid option"
            };

            ViewBag.Type = type;
            ViewBag.Location = location;
            ViewBag.Output = output;
            return View();
        }


        [HttpPost]
        public IActionResult UpdateStatus(string id)
        {
            _service.UpdateStatus(id);
            TempData["Message"] = $"Request {id} status updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /ServiceRequest/Track?id=R101
        [HttpGet]
        public IActionResult Track(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Message"] = "Please enter a Request ID.";
                return RedirectToAction(nameof(Index));
            }

            var req = _service.GetById(id);
            if (req == null)
            {
                TempData["Message"] = $"No request found with ID: {id}";
                return RedirectToAction(nameof(Index));
            }

            return View(req); // Views/ServiceRequest/Track.cshtml
        }


    }
}


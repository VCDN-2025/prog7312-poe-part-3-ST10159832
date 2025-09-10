using Microsoft.AspNetCore.Mvc;
using ST10159832KeenanPROGpart1.Models;

namespace ST10159832KeenanPROGpart1.Controllers
{
    public class IssuesController : Controller
    {
        private static LinkedList issueList = new LinkedList();
        private readonly IWebHostEnvironment _env;

        public IssuesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult LocalEvents()
        {
            
            return View();
        }

        
        [HttpGet]
        public IActionResult ServiceRequest()
        {
            
            return View();
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
    }
}

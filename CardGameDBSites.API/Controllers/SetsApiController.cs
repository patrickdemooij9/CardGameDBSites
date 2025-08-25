using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/sets")]
    public class SetsApiController : Controller
    {
        /*[HttpGet("getAll")]
        public IActionResult GetSets()
        {

        }*/
    }
}

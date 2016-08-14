using System.Collections.Generic;
using System.Web.Http;

namespace MyWebApp.Controllers
{
    [RoutePrefix("api/v1/ideas")]
    public class IdeasController : ApiController
    {
        [HttpGet]
        [Route("getall")]
        public IEnumerable<string> GetAllIdeas()
        {
            return new string[] { "Drinkable Book", "Space Elevator" };
        }
    }
}

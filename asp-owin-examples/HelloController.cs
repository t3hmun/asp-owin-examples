using System.Web.Http;

namespace asp_owin_examples
{
    public class HelloController : ApiController
    {
        public string Get(int id = -1)
        {
            return $"Hi {id}";
        }
    }
}
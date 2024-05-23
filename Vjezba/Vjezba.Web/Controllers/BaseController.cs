using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Vjezba.Web.Controllers
{
    public class BaseController : Controller
    {
        public string UserId 
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }
    }
}

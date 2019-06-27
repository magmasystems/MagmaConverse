using log4net;
using Microsoft.AspNetCore.Mvc;

namespace MagmaConverse.Controllers
{
	[ControllerLogger]
	public abstract class BaseController : Controller
    {
        protected ILog Logger;

        public BaseController()
        {
            Logger = LogManager.GetLogger(this.GetType());
        }
    }
}

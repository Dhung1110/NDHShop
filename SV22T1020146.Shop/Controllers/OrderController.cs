using Microsoft.AspNetCore.Mvc;

namespace SV22T1020146.Shop.Controllers
{
    public class OrderController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Checkout() => View(); 
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult History() => View(); 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult Tracking(int orderId) => View(); 
    }
}

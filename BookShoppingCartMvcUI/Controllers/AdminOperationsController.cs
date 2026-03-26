using BookShoppingCartMvcUI.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class AdminOperationsController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepository;

        public AdminOperationsController(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository = userOrderRepository;
        }

        public async Task<IActionResult> AllOrders()
        {
            var orders = await _userOrderRepository.UserOrders(true);
            return View(orders);
        }

        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            try
            {
                await _userOrderRepository.TogglePaymentStatus(orderId);
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction(nameof(AllOrders));

        }

        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var order = await _userOrderRepository.GetOrderById(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order With id:{orderId} dose not found");
            }

            var orderStatusList = (await _userOrderRepository.GetOrderStatuses())
            .Select(orderStatus =>
            {
                return new SelectListItem
                {
                    Value = orderStatus.StatusId.ToString(),
                    Text = orderStatus.StatusName,
                    Selected = orderStatus.Id == order.OrderStatusId
                };
            });

            var data = new UpdateOrderStatusModel
            {
                OrderId = orderId,
                OrderStatusId = order.OrderStatusId,
                OrderStatusList = orderStatusList
            };
            return View(data);

        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    data.OrderStatusList = (await _userOrderRepository.GetOrderStatuses())
                    .Select(orderStatus =>
                        {
                            return new SelectListItem
                            {
                                Value = orderStatus.StatusId.ToString(),
                                Text = orderStatus.StatusName,
                                Selected = orderStatus.Id == data.OrderStatusId
                            };
                        }
                    );
                    return View(data);
                }

                await _userOrderRepository.ChangeOrderStatus(data);
                TempData["msg"] = "Updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Something get wrong";
            }
            return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });
        }

        public IActionResult DashBoard()
        {
            return View();
        }

    }
}

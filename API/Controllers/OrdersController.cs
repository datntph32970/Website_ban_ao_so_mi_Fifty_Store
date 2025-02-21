using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        // Sử dụng static list để lưu đơn hàng tạm thời
        private static List<Order> orders = new List<Order>();
        private static int currentOrderId = 1;

        // GET /orders
        [HttpGet]
        public IActionResult GetOrders()
        {
            return Ok(orders);
        }

        // GET /orders/{id}
        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Đơn hàng không tồn tại" });
            return Ok(order);
        }

        // POST /orders
        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            if (order == null || string.IsNullOrEmpty(order.UserId) || order.Items.Count == 0)
            {
                return BadRequest(new { message = "Thông tin đơn hàng không hợp lệ" });
            }
            order.Id = currentOrderId++;
            orders.Add(order);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // PUT /orders/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, [FromBody] Order updatedOrder)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Đơn hàng không tồn tại" });

            // Cập nhật các thông tin của đơn hàng
            if (updatedOrder.Items != null && updatedOrder.Items.Count > 0)
                order.Items = updatedOrder.Items;
            if (updatedOrder.Total > 0)
                order.Total = updatedOrder.Total;
            if (!string.IsNullOrEmpty(updatedOrder.Status))
                order.Status = updatedOrder.Status;

            return Ok(order);
        }

        // DELETE /orders/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Đơn hàng không tồn tại" });
            orders.Remove(order);
            return Ok(new { message = "Đơn hàng đã được xóa" });
        }
    }
}

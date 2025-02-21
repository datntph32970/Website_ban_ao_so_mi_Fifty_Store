using API.Models;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        // Sử dụng dictionary để lưu giỏ hàng theo userId (key: userId, value: list of CartItem)
        private static Dictionary<string, List<CartItem>> carts = new Dictionary<string, List<CartItem>>();

        // GET /cart/{userId}
        [HttpGet("{userId}")]
        public IActionResult GetCart(string userId)
        {
            if (carts.ContainsKey(userId))
                return Ok(carts[userId]);
            return Ok(new List<CartItem>());
        }

        // POST /cart/{userId}
        [HttpPost("{userId}")]
        public IActionResult AddToCart(string userId, [FromBody] CartItem item)
        {
            if (item == null || item.ProductId == 0)
            {
                return BadRequest(new { message = "Thông tin sản phẩm không hợp lệ" });
            }

            if (!carts.ContainsKey(userId))
            {
                carts[userId] = new List<CartItem>();
            }
            carts[userId].Add(item);
            return CreatedAtAction(nameof(GetCart), new { userId = userId }, carts[userId]);
        }

        // PUT /cart/{userId}/{index}
        [HttpPut("{userId}/{index}")]
        public IActionResult UpdateCartItem(string userId, int index, [FromBody] CartItem updatedItem)
        {
            if (!carts.ContainsKey(userId) || index < 0 || index >= carts[userId].Count)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }

            var item = carts[userId][index];
            if (updatedItem.ProductId != 0)
                item.ProductId = updatedItem.ProductId;
            if (!string.IsNullOrEmpty(updatedItem.Name))
                item.Name = updatedItem.Name;
            if (updatedItem.Quantity > 0)
                item.Quantity = updatedItem.Quantity;
            if (updatedItem.Price > 0)
                item.Price = updatedItem.Price;

            return Ok(carts[userId]);
        }

        // DELETE /cart/{userId}/{index}
        [HttpDelete("{userId}/{index}")]
        public IActionResult DeleteCartItem(string userId, int index)
        {
            if (!carts.ContainsKey(userId) || index < 0 || index >= carts[userId].Count)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }
            carts[userId].RemoveAt(index);
            return Ok(carts[userId]);
        }
    }
}

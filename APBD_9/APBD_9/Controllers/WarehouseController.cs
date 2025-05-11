using APBD_9.Models;
using APBD_9.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly iWarehouseService _warehouseService;

        public WarehouseController(iWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> PutProductInWarehouseAsync([FromBody] ProductInWarehouseDTO body)
        {
            if (! await _warehouseService.DoesProductExist(body.IdProduct))
            {
                return NotFound("Product not found");
            }
            if (! await _warehouseService.DoesOrderExist(body.IdProduct, body.Amount))
            {
                return NotFound("Order not found");
            }
            if (!await _warehouseService.DoesWarehouseExist(body.IdWarehouse))
            {
                return NotFound("Warehouse not found");
            }

            var orderId = await _warehouseService.GetOrderId(body.IdProduct, body.Amount);
            if(! await _warehouseService.OrderNotCompleted(orderId))
            {
                return BadRequest("Order already completed");
            }
            var createdId = await _warehouseService.putInProductWarehouse(body);
            return Created(Request.Path.Value ?? "api/product_warehouse", createdId);

        }

        [HttpPost("procedure")]
        public async Task<IActionResult> PutProductInWarehouseAsyncByProcedure([FromBody] ProductInWarehouseDTO body)
        {
            var createdId = await _warehouseService.putInProductWarehouseProcedure(body);
            return Created(Request.Path.Value ?? "api/product_warehouse", createdId);
        }
        
    }
}
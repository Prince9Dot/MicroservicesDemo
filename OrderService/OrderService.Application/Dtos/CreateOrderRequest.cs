using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Dtos
{
    public class CreateOrderRequest
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}

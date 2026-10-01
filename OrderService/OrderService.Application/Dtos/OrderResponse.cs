using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Dtos
{
    public class OrderResponse
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

    }
}

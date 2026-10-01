using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Events
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

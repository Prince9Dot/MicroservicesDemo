using Shared.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IOrderEventPublisher
    {
        Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent);
    }
}

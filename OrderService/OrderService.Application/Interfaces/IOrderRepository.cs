using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetOrdersAsync();
        Task<Order?> GetOrderAsync(int id);
        Task AddAsync(Order order);
    }
}

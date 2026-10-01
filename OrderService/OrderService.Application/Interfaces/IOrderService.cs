using OrderService.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetOrdersAsync();
        Task<OrderResponse?> GetOrderAsync(int id);
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    }
}

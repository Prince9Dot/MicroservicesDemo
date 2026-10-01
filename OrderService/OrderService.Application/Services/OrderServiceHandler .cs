using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Shared.Contracts.Events;

namespace OrderService.Application.Services
{
    public class OrderServiceHandler : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IOrderEventPublisher _eventPublisher;

        public OrderServiceHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _productServiceClient = productServiceClient;
            _eventPublisher = eventPublisher;
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();

            return orders.Select(MapToResponse);
        }

        public async Task<OrderResponse?> GetOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderAsync(id);

            if (order == null)
                return null;

            return MapToResponse(order);
        }

        public async Task<OrderResponse> CreateOrderAsync(
            CreateOrderRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var product = await _productServiceClient
                .GetProductAsync(request.ProductId);

            if (product == null)
            {
                throw new KeyNotFoundException(
                    $"Product {request.ProductId} was not found.");
            }

            if (product.Stock < request.Quantity)
            {
                throw new InvalidOperationException(
                    "Insufficient product stock.");
            }

            var totalAmount =
                product.Price * request.Quantity;

            var order = new Order
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                TotalAmount = totalAmount
            };

            await _orderRepository.AddAsync(order);

            await _eventPublisher.PublishOrderCreatedAsync(new OrderCreatedEvent
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow
            });

            return MapToResponse(order);
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount
            };
        }
    }
}

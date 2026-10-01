using OrderService.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IProductServiceClient
    {
        Task<ProductResponse?> GetProductAsync(int productId);
    }
}

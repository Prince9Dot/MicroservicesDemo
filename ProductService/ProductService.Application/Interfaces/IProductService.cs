using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetProductsAsync();
        Task<ProductResponse?> GetProductAsync(int id);
        Task<ProductResponse> CreateProductAsync(
            CreateProductRequest request);
    }
}

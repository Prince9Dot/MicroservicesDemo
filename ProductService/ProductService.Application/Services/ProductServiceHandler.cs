using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductService.Application.Services
{
    public class ProductServiceHandler : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductServiceHandler(
            IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductResponse>>
            GetProductsAsync()
        {
            var products =
                await _repository.GetProductsAsync();

            return products.Select(x => new ProductResponse
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Stock = x.Stock
            });
        }

        public async Task<ProductResponse?> GetProductAsync(int id)
        {
            var product =
                await _repository.GetProductAsync(id);

            if (product == null)
                return null;

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock
            };
        }

        public async Task<ProductResponse>
            CreateProductAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                Stock = request.Stock
            };

            await _repository.AddAsync(product);

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock
            };
        }
    }
}

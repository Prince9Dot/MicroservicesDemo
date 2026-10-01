using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace OrderService.Infrastructure.ExternalServices
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProductServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductResponse?> GetProductAsync(
            int productId)
        {
            var response = await _httpClient.GetAsync(
                $"api/Product/{productId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<ProductResponse>();
        }
    }
}

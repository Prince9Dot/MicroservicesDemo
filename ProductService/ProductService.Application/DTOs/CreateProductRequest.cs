using System;
using System.Collections.Generic;
using System.Text;

namespace ProductService.Application.DTOs
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}

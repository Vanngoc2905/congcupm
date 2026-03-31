using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using vnfood.Models;

namespace vnfood.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá.")]
        [Range(0, 1000000000, ErrorMessage = "Giá không hợp lệ.")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0, 1000000, ErrorMessage = "Số lượng không hợp lệ.")]
        [Display(Name = "Số lượng tồn kho")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }
        public IFormFile? ImageFile2 { get; set; }
        public IFormFile? ImageFile3 { get; set; }

        // For display only
        public List<Category>? Categories { get; set; }
    }
}

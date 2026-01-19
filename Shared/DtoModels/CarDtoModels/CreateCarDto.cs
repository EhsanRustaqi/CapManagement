using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.CarDtoModels
{
    public class CreateCarDto
    {
        [JsonPropertyName("carId")]
        public Guid CarId { get; set; }

        [Required(ErrorMessage = "Brand is required.")]
        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters.")]
        [JsonPropertyName("brand")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters.")]
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "License plate is required.")]
        [StringLength(10, ErrorMessage = "License plate cannot exceed 10 characters.")]
        [JsonPropertyName("numberPlate")]
        public string NumberPlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required.")]
        [Range(1980, 2025, ErrorMessage = "Year must be between 1980 and 2025.")]
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [JsonPropertyName("status")]
        public CarStatus Status { get; set; } = CarStatus.Active;

        [Required(ErrorMessage = "Company ID is required.")]
        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }


        // ✅ Include RDW properties too
        public string? Color { get; set; }
        public string? VehicleType { get; set; }
        public string? FuelType { get; set; }
        public string? NumberOfDoors { get; set; }
        public string? NumberOfSeats { get; set; }
        public string? EngineCapacity { get; set; }
        public string? EmptyWeight { get; set; }
        public string? ApkExpirationDate { get; set; }
    }

   

}


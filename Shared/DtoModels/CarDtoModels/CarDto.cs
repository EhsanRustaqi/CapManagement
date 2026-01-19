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
    public class CarDto
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


        // ✅ RDW-related extra properties (string because RDW returns text)
        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("vehicleType")]
        public string? VehicleType { get; set; }

        [JsonPropertyName("fuelType")]
        public string? FuelType { get; set; }

        [JsonPropertyName("numberOfDoors")]
        public string? NumberOfDoors { get; set; }

        [JsonPropertyName("numberOfSeats")]
        public string? NumberOfSeats { get; set; }

        [JsonPropertyName("engineCapacity")]
        public string? EngineCapacity { get; set; }

        [JsonPropertyName("emptyWeight")]
        public string? EmptyWeight { get; set; }

        [JsonPropertyName("apkExpirationDate")]
        public string? ApkExpirationDate { get; set; }



        public override bool Equals(object? obj)
        {
            return obj is CarDto other && CarId.Equals(other.CarId);
        }

        public override string ToString()
        {
            return NumberPlate ?? string.Empty; // Filters/searches against NumberPlate
        }

        public override int GetHashCode()
        {
            return CarId.GetHashCode();
        }
    }

    //public enum CarStatus
    //{
    //    Active,
    //    Maintenance,
    //    Inactive
    //}


}


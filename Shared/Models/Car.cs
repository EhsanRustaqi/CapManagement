    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    namespace CapManagement.Shared.Models
    {
        public class Car: BaseEntity
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
            [JsonPropertyName("NumberPlate")]
            public string NumberPlate { get; set; } = string.Empty;

            [Required(ErrorMessage = "Year is required.")]
            [Range(1980, 2025, ErrorMessage = "Year must be between 1980 and 2025.")]
            [JsonPropertyName("year")]
            public int Year { get; set; }

            [Required(ErrorMessage = "Status is required.")]
            [JsonPropertyName("status")]
            public CarStatus Status { get; set; } = CarStatus.Active; // Enum defined below

            [Required(ErrorMessage = "Company ID is required for multi-company isolation.")]
            [JsonPropertyName("companyId")]
            public Guid CompanyId { get; set; } // FK to Company

            public ICollection<Contract> Contracts { get; set; } = new List<Contract>();


        // New RDW API properties
        // -------------------------------

            [JsonPropertyName("firstRegistrationDate")]
            public DateTime? FirstRegistrationDate { get; set; }

            [JsonPropertyName("apkExpirationDate")]
           public DateTime? ApkExpirationDate { get; set; }

           [JsonPropertyName("color")]
            public string? Color { get; set; }

           [JsonPropertyName("vehicleType")]
           public string? VehicleType { get; set; }

           [JsonPropertyName("fuelType")]
           public string? FuelType { get; set; }

          [JsonPropertyName("numberOfDoors")]
           public int? NumberOfDoors { get; set; }

           [JsonPropertyName("numberOfSeats")]
            public int? NumberOfSeats { get; set; }

            [JsonPropertyName("engineCapacity")]
            public int? EngineCapacity { get; set; }

           [JsonPropertyName("emptyWeight")]
            public int? EmptyWeight { get; set; }
    }

        // Enum for CarStatus
        public enum CarStatus
        {
            Active,
            Maintenance,
            Inactive
        }
    }


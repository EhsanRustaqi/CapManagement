using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.Models
{
    public class BaseEntity
    {
        [Required]
        [DataType(DataType.DateTime)]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; } // Nullable, as updates are optional

        [DataType(DataType.DateTime)]
        [JsonPropertyName("deletedAt")]
        public DateTime? DeletedAt { get; set; } // Nullable for soft deletes

        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        [JsonPropertyName("createdByUserId")]
        public Guid? CreatedByUserId { get; set; } // User who created the entity

        [JsonPropertyName("updatedByUserId")]
        public Guid? UpdatedByUserId { get; set; } // User who last updated the entity

        [JsonPropertyName("deletedByUserId")]
        public Guid? DeletedByUserId { get; set; } // User who soft-deleted the entity


        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true; // Added for soft-delete

    }
}

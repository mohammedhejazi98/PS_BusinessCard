using System.ComponentModel.DataAnnotations;

namespace PS_BusinessCard.Models
{
    public class BusinessCard
    {
        #region Public Properties

        [MaxLength(350)]
        public string? Address { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [MaxLength(10)]
        public required string Gender { get; set; }
        
        public int Id { get; set; }
        
        [MaxLength(20)]
        public required string Name { get; set; }
        
        [MaxLength(20)]
        [Phone]
        public required string Phone { get; set; }
        
        [MaxLength(1000)]
        public string? PhotoBase64 { get; set; }

        #endregion Public Properties
    }
}
using System.ComponentModel.DataAnnotations;

namespace PS_BusinessCard.Dtos
{
    public class CreateBusinessCardDto
    {
        public string? Address { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }

        public required string Gender { get; set; }

        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Phone { get; set; }

        public string? PhotoBase64 { get; set; }

        public IFormFile? FileUpload { get; set; }
    }
}

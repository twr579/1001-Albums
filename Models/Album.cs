using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _1001Albums.Models
{
    public class Album
    {
        public int Id { get; set; }

        [DisplayName("Created At")]
        public DateTime? CreatedAt { get; set; }
        
        public required string Artist { get; set; }
        
        public required string Title { get; set; }

        [DisplayName("Release Date")]
        [DataType(DataType.Date)]
        public required DateTime ReleaseDate { get; set; }

        public required Genre Genre { get; set; }

        [Range(1, 10)]
        public required int Rating { get; set; }

        public ICollection<UserRating> UserRatings { get; } = new List<UserRating>();

        [DisplayName("Cover")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }

        public Album() { 
            CreatedAt = DateTime.Now;
        }
    }
}

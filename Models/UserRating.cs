using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _1001Albums.Models
{
    [PrimaryKey(nameof(AlbumId), nameof(UserId))]
    public class UserRating
    {
        public int AlbumId { get; set; }

        public Album? Album { get; set; }

        public string UserId { get; set; }

        public IdentityUser? User { get; set; }

        [Range(0, 10)]
        public required int Rating { get; set; }
    }
}

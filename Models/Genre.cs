using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace _1001Albums.Models
{
    public enum Genre
    {
        Rock,
        Pop,
        Jazz,
        Funk,
        [Display(Name = "R&B")]
        RhythmAndBlues,
        Folk,
    }
}

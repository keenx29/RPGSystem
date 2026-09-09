using System.ComponentModel.DataAnnotations;

namespace RPGSystem.ViewModels
{
    public class AddCharacterSenseViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = "";

        [Range(0, 1000)]
        public int? RangeFeet { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
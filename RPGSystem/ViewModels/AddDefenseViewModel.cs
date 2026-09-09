using System.ComponentModel.DataAnnotations;
using RPGSystem.Models.Characters;

namespace RPGSystem.ViewModels
{
    public class AddDefenseViewModel
    {
        public DefenseType Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = "";
    }
}
using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models;
using RPGSystem.Services;

namespace RPGSystem.Controllers
{
    public class CharacterController : Controller
    {
        private readonly DiceService _diceService;

        public CharacterController(DiceService diceService)
        {
            _diceService = diceService;
        }

        public IActionResult Sheet()
        {
            Character character = new Character
            {
                Name = "Thorin",

                Strength = 16,
                Dexterity = 12,
                Constitution = 14,

                Intelligence = 10,
                Wisdom = 8,
                Charisma = 13
            };

            return View(character);
        }
    }
}

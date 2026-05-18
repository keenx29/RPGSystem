using RPGSystem.Models;

public class Armor : Item
{
    public int BaseArmorClass { get; set; }

    public ArmorType ArmorType { get; set; }
}
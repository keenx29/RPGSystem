namespace RPGSystem.Models.Rolls
{
    public interface ICombatModifier
    {
        RollModification Apply(RollContext context);
    }
}

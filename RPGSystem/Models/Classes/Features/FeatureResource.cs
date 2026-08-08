namespace RPGSystem.Models.Classes.Features
{
    public class FeatureResource
    {
        public string Name { get; set; } = "";

        public int Current { get; set; }

        public int Max { get; set; }
        public FeatureResetType ResetType { get; set; } = FeatureResetType.None;
    }
}

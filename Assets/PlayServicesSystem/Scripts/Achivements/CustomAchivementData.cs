namespace TheKnights.PlayServicesSystem.Achievements
{
    /// <summary>
    /// Define an achievement's data
    /// </summary>
    public struct CustomAchivementData
    {
        public readonly string Title;
        public readonly string Description;
        public readonly bool Completed;
        public readonly string Reward;
        public readonly string ID;

        public CustomAchivementData(string title, string description, bool completed, string reward, string id)
        {
            Title = title;
            Description = description;
            Completed = completed;
            Reward = reward;
            ID = id;
        }
    }
}
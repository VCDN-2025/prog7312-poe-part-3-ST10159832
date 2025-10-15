namespace ST10159832KeenanPROGpart1.Models
{
    public class LocalEvent
    {
        public string Id { get; set; }              
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }           

        public LocalEvent()
        {
            Id = Guid.NewGuid().ToString();
        }

        public LocalEvent(string title, string category, DateTime date, string description, int priority = 5)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Category = category;
            Date = date;
            Description = description;
            Priority = priority;
        }
    }
}

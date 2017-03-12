namespace APIJSON
{
    public class APIJSON
    {
        public string userId { get; set; }
        public int buildId { get; set; }
        public Event[] events { get; set; }
    }

    public class Event
    {
        public string eventType { get; set; }
        public string itemId { get; set; }
        public string timestamp { get; set; }
        public int count { get; set; }
        public float unitPrice { get; set; }
    }
}
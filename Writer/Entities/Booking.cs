namespace Writer.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public string Owner { get; set; }
    }
}
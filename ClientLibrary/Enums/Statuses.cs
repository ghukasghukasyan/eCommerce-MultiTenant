namespace ClientLibrary.Enums
{
    public class Statuses
    {
        public enum OrderStatus
        {
            Pending = 1,
            Paid = 2,
            Shipped = 3,
            Completed = 4,
            Cancelled = 5,
            Failed = 6
        }
        public enum PaymentStatus
        {
            Pending = 1,
            Paid = 2,
            Failed = 3
        }
        public enum ActivityStatus
        {
            Active = 1,
            Paused = 2,
        }

        public enum InfluencerStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2,
            Suspended = 3
        }

        public enum DiscountType
        {
            Percentage = 1,
            FixedAmount = 2
        }
    }
}

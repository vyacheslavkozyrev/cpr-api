namespace CPR.Domain.Entities
{
    public enum ReviewCycleStatus
    {
        Draft = 0,
        Open = 1,
        InProgress = 2,
        Closed = 3
    }

    public static class ReviewCycleStatusExtensions
    {
        public static bool IsValidTransition(ReviewCycleStatus from, ReviewCycleStatus to)
        {
            return (from, to) switch
            {
                (ReviewCycleStatus.Draft, ReviewCycleStatus.Open) => true,
                (ReviewCycleStatus.Open, ReviewCycleStatus.InProgress) => true,
                (ReviewCycleStatus.InProgress, ReviewCycleStatus.Closed) => true,
                _ => false
            };
        }
    }
}

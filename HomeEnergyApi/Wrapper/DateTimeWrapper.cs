namespace HomeEnergyApi.Wrapper
{
    public class DateTimeWrapper : IDateTimeWrapper
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}

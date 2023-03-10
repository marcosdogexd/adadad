using System;
using Faker.Random;

namespace Faker
{
    public class Date : FakerBase
    {
        public Date()
        { }
        public Date(LocaleType type) : base(type) { }

        // default static interface
        public static Address Default { get { return defaultValue; } }
        private static Address defaultValue = new Address();

        public DateTime GetPast()
        {
            return this.GetPast(1);
        }

        public DateTime GetPast(int yearSpan)
        {
            DateTime now = DateTime.Now;
            long timespan = RandomProxy.Next(1000, (long)yearSpan * 365 * 24 * 3600 * 1000);
            TimeSpan span = new TimeSpan(-timespan);
            return now.Add(span);
        }

        public DateTime GetFuture()
        {
            return this.GetFuture(1);
        }

        public DateTime GetFuture(int yearSpan)
        {
            DateTime now = DateTime.Now;
            long timespan = RandomProxy.Next(1000, (long)yearSpan * 365 * 24 * 3600 * 1000);
            TimeSpan span = new TimeSpan(timespan);
            return now.Add(span);
        }

        public DateTime GetRecent(int daySpan)
        {
            DateTime now = DateTime.Now;
            long timespan = RandomProxy.Next(1000, (long)daySpan * 24 * 3600 * 1000);
            TimeSpan span = new TimeSpan(timespan);
            return now.Add(span);
        }

        public DateTime GetDateBetween(DateTime from, DateTime to)
        {
            var timeSpan = to - from;
            return from.Add(new TimeSpan((long)RandomProxy.NextDouble(timeSpan.TotalMilliseconds)));
        }
    }
}

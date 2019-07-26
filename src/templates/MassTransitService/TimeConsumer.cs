using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransitService.Messages;

namespace MassTransitService
{
    public class TimeConsumer :
        IConsumer<IsItTime>
    {
        public Task Consume(ConsumeContext<IsItTime> context)
        {
            var now = DateTimeOffset.Now;
            if (now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 17)
            {
                return context.RespondAsync<YesItIs>(new { });
            }

            return context.RespondAsync<NoNotYet>(new { });
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

namespace NServiceBus.ContextCop
{
    public class PipelineTracker : Behavior<ITransportReceiveContext>
    {
        public static readonly AsyncLocal<bool> InsidePipeline = new AsyncLocal<bool>();
        public override Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            InsidePipeline.Value = true;
            return next();
        }
    }
}
using System;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transports;

namespace NServiceBus.ContextCop
{
    public class MessageSessionDetection : Behavior<IOutgoingLogicalMessageContext>
    {
        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            IncomingMessage incomingMessage;
            if (!context.Extensions.TryGet(out incomingMessage))
            {
                // this is a send outside the pipeline because no incoming message is present.
                // check whether this is not a accidental invocation of IMessageSession inside the pipeline:
                if (PipelineTracker.InsidePipeline.Value)
                {
                    throw new Exception("you're using the wrong context!");
                }
            }

            return next();
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Features;
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

    public class PipelineTracker : Behavior<ITransportReceiveContext>
    {
        public static readonly AsyncLocal<bool> InsidePipeline = new AsyncLocal<bool>();
        public override Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            InsidePipeline.Value = true;
            return next();
        }
    }

    public class MessageSessionDetector : Feature
    {
        public MessageSessionDetector()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register("MessageSessionDetection", typeof(MessageSessionDetection), "Detects usages of IMessageSession from inside a message processing pipeline.");
            context.Pipeline.Register("PipelineTracker", typeof(PipelineTracker), "Tracks whether the currently executed code is executed within a pipeline");
        }
    }
}
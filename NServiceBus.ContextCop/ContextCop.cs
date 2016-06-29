using NServiceBus.Features;

namespace NServiceBus.ContextCop
{
    public class ContextCop : Feature
    {
        public ContextCop()
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
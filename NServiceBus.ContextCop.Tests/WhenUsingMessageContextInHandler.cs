using System;
using System.Threading.Tasks;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTesting.Customization;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Features;
using Xunit;

namespace NServiceBus.ContextCop.Tests
{
    public class WhenUsingMessageContextInHandler
    {
        static IMessageSession messageSession = null;

        [Fact]
        public async Task ThrowsOnSend()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>(e => e
                    .When(c =>
                    {
                        messageSession = c;
                        return c.SendLocal(new IncomingMessage());
                    }))
                .Done(c => c.MessageReceived)
                .Run();

            Assert.NotNull(context.SendException);
        }

        public class Context : ScenarioContext
        {
            public bool MessageReceived { get; set; }
            public Exception SendException { get; set; }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class IncomingMessageHandler : IHandleMessages<IncomingMessage>
            {
                private Context testContext;

                public IncomingMessageHandler(ScenarioContext testContext)
                {
                    this.testContext = (Context) testContext;
                }

                public async Task Handle(IncomingMessage message, IMessageHandlerContext context)
                {
                    testContext.MessageReceived = true;

                    try
                    {
                        await messageSession.SendLocal(new OutgoingMessage());
                    }
                    catch (Exception ex)
                    {
                        testContext.SendException = ex;
                    }
                }
            }

            public class OutgoingMessageHandler : IHandleMessages<OutgoingMessage>
            {
                public async Task Handle(OutgoingMessage message, IMessageHandlerContext context)
                {
                }
            }
        }

        public class IncomingMessage : ICommand
        {
        }

        public class OutgoingMessage : ICommand
        {
        }
    }

    class DefaultServer : IEndpointSetupTemplate
    {
        public Task<EndpointConfiguration> GetConfiguration(RunDescriptor runDescriptor, EndpointCustomizationConfiguration endpointConfiguration,
            IConfigurationSource configSource, Action<EndpointConfiguration> configurationBuilderCustomization)
        {
            var config = new EndpointConfiguration(endpointConfiguration.EndpointName);

            config.UseTransport<MsmqTransport>();
            config.PurgeOnStartup(true);
            config.UsePersistence<InMemoryPersistence>();
            config.SendFailedMessagesTo("error");
            config.EnableInstallers();
            config.RegisterComponents(r => r.ConfigureComponent<ScenarioContext>(() => runDescriptor.ScenarioContext, DependencyLifecycle.SingleInstance));
            config.TypesToIncludeInScan(new Type[]
            {
                typeof(ContextCop), // why is that needed?
                typeof(WhenUsingMessageContextInHandler.IncomingMessage),
                typeof(WhenUsingMessageContextInHandler.OutgoingMessage),
                typeof(WhenUsingMessageContextInHandler.Endpoint.OutgoingMessageHandler),
                typeof(WhenUsingMessageContextInHandler.Endpoint.IncomingMessageHandler)
            });
            config.DisableFeature<FirstLevelRetries>();
            config.DisableFeature<Features.SecondLevelRetries>();

            return Task.FromResult(config);
        }
    }
}
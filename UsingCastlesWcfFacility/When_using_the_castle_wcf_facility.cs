using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using Castle.Core;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace UsingCastlesWcfFacility
{
    /// <summary>
    /// Please wnsure that Visual Studio is running as Admin for these tests.
    /// If it's not, then Win7 won't allow services to open listening sockets.
    /// </summary>
    public class When_using_castle_wcf_facility
    {
        private const string BaseAddress = "http://localhost:8000/Operations.svc";
        private const string RelativeAddress = "Operations";
        private const int Expected = 42;


        [SetUp]
        public void Setup()
        {
            TestInterceptor.mostRecentMethodCallIntercepoted = string.Empty;
        }


        /// <summary>
        /// This test has been taken from the Castle WCF 
        /// facility unit tests.
        /// </summary>
        [Test]
        public void should_be_able_to_host_a_wcf_service_in_the_container()
        {

            // The container is used in a using block
            // so that all resources are cleaned up when it's disposed.
            using (
                new WindsorContainer()
                    .AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero) // Notice how it's possible to configure the facilityt here.  Usually, we don't add any extra config.
                    .Register(
                        Component
                            .For<IOperations>()
                            .ImplementedBy<Operations>()
                            .DependsOn(new { value = Expected }) // Notice how this constant is injected in to the service.
                            .AsWcfService(
                                new DefaultServiceModel()
                //.OpenEagerly()           // This is useful to force immediate creation
                // of services and to see any errors that may result.
                                    .AddBaseAddresses(BaseAddress)
                                    .AddEndpoints(
                                        WcfEndpoint.ForContract<IOperations>()
                                            .BoundTo(new WSHttpBinding())
                                            .At(RelativeAddress)
                                    )
                             )

                    )
                )
            {
                // This sleep is useful if/when you'd like to browse to http://localhost:8000/Operations.svc
                // to verify that no meta-data is being published.

                // Thread.Sleep(100000);

                // We're using the standard WCF channel factor here,
                // rather than the Castle proxies [Just to show that 
                // we're dealing with a standard WCF service.].
                IOperations client = ChannelFactory<IOperations>.CreateChannel(
                    new WSHttpBinding(),
                    new EndpointAddress(BaseAddress + "/" + RelativeAddress)
                    );

                int result = client.GetValueFromConstructor();

                Assert.AreEqual(Expected, result);
            }
        }


        [Test]
        public void should_be_able_to_publish_meta_data_on_wshttp_service()
        {
            // See the IIS sample in the source code for the
            // Castle WCF facility.
            ServiceDebugBehavior returnFaults = new ServiceDebugBehavior
                                                    {
                                                        IncludeExceptionDetailInFaults = true,
                                                        HttpHelpPageEnabled = true
                                                    };

            // Only this behaviour is required to get this service 
            // to publish meta-data.
            ServiceMetadataBehavior metadata = new ServiceMetadataBehavior
                                                   {
                                                       HttpGetEnabled = true
                                                   };
            using (
                new WindsorContainer()
                    .AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
                    .Register(

                        // Both of these will be added as behaviours 
                // to any services registered.
                        Component
                            .For<IServiceBehavior>()
                            .Instance(returnFaults),

                        Component
                            .For<IServiceBehavior>()
                            .Instance(metadata),

                        Component
                            .For<IOperations>()
                            .ImplementedBy<Operations>()
                            .DependsOn(new { value = Expected }) // Notice how this constant is injected in to the service.
                            .AsWcfService(
                                new DefaultServiceModel()
                                    //.OpenEagerly() // This is useful to force immediate creation of services and to see any errors that may result.
                                    .AddBaseAddresses(BaseAddress)
                                    .AddEndpoints(
                                        WcfEndpoint.ForContract<IOperations>()
                                            .BoundTo(new WSHttpBinding())
                                            .At(RelativeAddress)
                                    )
                             )
                    )
                )
            {
                // This sleep is useful if/when you'd like to browse to http://localhost:8000/Operations.svc
                // to verify that meta-data is being published for this service.

                //Thread.Sleep(100000);

                // Notice that we're using the standard WCF channel factory here,
                // rather than the castle proxies.
                IOperations client = ChannelFactory<IOperations>.CreateChannel(
                    new WSHttpBinding(),
                    new EndpointAddress(BaseAddress + "/" + RelativeAddress)
                    );

                int result = client.GetValueFromConstructor();

                Assert.AreEqual(Expected, result);
            }
        }


        [Test]
        public void should_be_able_to_show_that_interceptor_was_called_when_wcf_service_is_invoked()
        {
            ServiceDebugBehavior returnFaults = new ServiceDebugBehavior();
            returnFaults.IncludeExceptionDetailInFaults = true;
            returnFaults.HttpHelpPageEnabled = true;

            ServiceMetadataBehavior metadata = new ServiceMetadataBehavior();
            metadata.HttpGetEnabled = true;

            using (
                new WindsorContainer()
                    .AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
                    .Register(
                        Component.For<TestInterceptor>(), // The interceptor must be registered like this.

                        Component.For<IServiceBehavior>().Instance(returnFaults),
                        Component.For<IServiceBehavior>().Instance(metadata),

                        Component
                            .For<IOperations>()
                            .ImplementedBy<Operations>()
                            .Interceptors(InterceptorReference.ForType<TestInterceptor>()).Anywhere  // Don't care what order this is called in (and there is only one interceptor on this any case).
                            .DependsOn(new { value = Expected })                                    // Notice how this constant is injected in to the service.
                            .AsWcfService(
                                new DefaultServiceModel()
                                    //.OpenEagerly() // This is useful to force immediate creation of services and to see any errors that may result.
                                    .AddBaseAddresses(BaseAddress)
                                    .AddEndpoints(
                                        WcfEndpoint
                                          .ForContract<IOperations>()
                                          .BoundTo(new WSHttpBinding())
                                          .At(RelativeAddress)
                                    )
                             )))
            {
                // This sleep is useful if/when you'd like to browse to http://localhost:8000/Operations.svc
                // to verify that meta-data is being published for this service.

                //Thread.Sleep(100000);

                // Notice that we're using the standard WCF channel factory here,
                // rather than the castle proxies.
                IOperations client = ChannelFactory<IOperations>.CreateChannel(
                    new WSHttpBinding(),
                    new EndpointAddress(BaseAddress + "/" + RelativeAddress)
                    );

                int result = client.GetValueFromConstructor();

                Assert.AreEqual(Expected, result);

                Assert.That(TestInterceptor.mostRecentMethodCallIntercepoted, Is.EqualTo("GetValueFromConstructor"));
            }
        }
    }
}

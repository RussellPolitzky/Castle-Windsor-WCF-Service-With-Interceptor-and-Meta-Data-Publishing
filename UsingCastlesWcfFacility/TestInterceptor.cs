using System;
using Castle.DynamicProxy;

namespace UsingCastlesWcfFacility
{
    public class TestInterceptor : IInterceptor
    {
        public static string mostRecentMethodCallIntercepoted;

        static TestInterceptor()
        {
            mostRecentMethodCallIntercepoted = string.Empty;
        }

        public void Intercept(IInvocation invocation)
        {
            // Trace log the name of the method that's about to be called.
            string nameOfMethodToBeCalled = invocation.MethodInvocationTarget.Name;

            mostRecentMethodCallIntercepoted = nameOfMethodToBeCalled;

            // I'm wiring to the console here but you could be calling into 
            // any logging framework, for example.
            Console.WriteLine("Method {0} is about to be called.", nameOfMethodToBeCalled);

            invocation.Proceed(); //
        }
    }
}
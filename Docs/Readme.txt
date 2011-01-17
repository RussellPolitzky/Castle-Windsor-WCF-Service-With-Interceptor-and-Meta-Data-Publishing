
This sample shows how to use Castle Windsor to host a WCF service and
how to add a simple interceptor to that service.

The sample doesn't show how to use castle proxies though.  Instead,
Channel Factory is used in the sample tests.  Nevertheless, I recommend
using the castle proxies since unlike WCF proxiesm they don't violate the
Liskov Substitution Principle.


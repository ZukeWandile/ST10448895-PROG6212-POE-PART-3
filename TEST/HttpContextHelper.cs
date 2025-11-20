using Microsoft.AspNetCore.Http;

namespace ST10448895_CMCS_PROG.Tests
{
    public static class HttpContextHelper
    {
        public static DefaultHttpContext CreateHttpContextWithSession()
        {
            var context = new DefaultHttpContext();
            var session = new FakeSession();

            // Attach both features and direct assignment
            context.Features.Set<ISession>(session);
            context.Session = session;

            return context;
        }
    }
}

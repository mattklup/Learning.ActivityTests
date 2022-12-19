using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Learning.ActivityTests
{
    public class ActivitySourceTests
    {
        ActivitySource source;

        public ActivitySourceTests()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        }

        [Fact]
        public void ExecutionContextTest()
        {
            var copy = ExecutionContext.Capture();

            Assert.NotNull(copy);
        }

        private static void PrintActivityInfo()
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId, 3}] Activity.Current = {Activity.Current.Id} -> {Activity.Current.ParentId}");
        }
    }
}

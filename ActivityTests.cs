using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Learning.ActivityTests
{
    public class ActivityTests
    {
        public ActivityTests()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        }

        [Fact]
        public void ExecutionContextTest()
        {
            var copy = ExecutionContext.Capture();

            Assert.NotNull(copy);
        }

        [Fact]
        public void CurrentNullTest()
        {
            Assert.Null(Activity.Current);
        }

        [Fact]
        public void StartSetsCurrentTest()
        {
            Assert.Null(Activity.Current);
            var activity = new Activity("TestActivity");
            activity.Start();

            Assert.NotNull(Activity.Current);
            Assert.Equal(activity.Id, Activity.Current.Id);

            activity.Stop();

            Assert.Null(Activity.Current);
        }

        [Fact]
        public void StartSetsCurrentAndParentTest()
        {
            Assert.Null(Activity.Current);
            var parent = new Activity("TestParentActivity");
            parent.Start();

            Assert.NotNull(Activity.Current);
            Assert.Equal(parent.Id, Activity.Current.Id);

            //
            var child = new Activity("TestChildActivity");
            child.Start();

            Assert.NotNull(Activity.Current);
            Assert.Equal(child.Id, Activity.Current.Id);
            Assert.Equal(parent, child.Parent);
            Assert.Equal(parent.Id, child.Parent.Id);

            child.Stop();
            //

            parent.Stop();

            Assert.Null(Activity.Current);
        }

        [Fact]
        public async Task ParallelChildrenTestAsync()
        {
            Assert.Null(Activity.Current);
            var parent = new Activity("TestParentActivity");
            parent.Start();

            Assert.NotNull(Activity.Current);
            Assert.Equal(parent.Id, Activity.Current.Id);

            PrintActivityInfo();

            List<Task> tasks = new List<Task>();
            var ec = ExecutionContext.Capture();
            Assert.NotNull(ec);

            for (int i = 0; i < 5; ++i )
            {
                Assert.Equal(parent.Id, Activity.Current.Id);

                var task = Task.Run(async () =>
                {
                    var ec2 = ExecutionContext.Capture();

                    Assert.Same(ec, ec2);
                    Assert.NotNull(Activity.Current);
                    Assert.Equal(parent.Id, Activity.Current.Id);

                    var child = new Activity("TestChildActivity");

                    child.Start();

                    PrintActivityInfo();

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    PrintActivityInfo();

                    Assert.NotNull(Activity.Current);
                    Assert.Equal(child.Id, Activity.Current.Id);
                    Assert.Equal(parent, child.Parent);
                    Assert.Equal(parent.Id, child.Parent.Id);

                    child.Stop();

                    Assert.NotNull(Activity.Current);
                    Assert.Equal(parent.Id, Activity.Current.Id);

                    var ec3 = ExecutionContext.Capture();
                    Assert.NotSame(ec, ec3); // Copy-on write

                    Console.WriteLine("No exception");
                    Activity.Current = null;
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            tasks.ForEach((t) => t.Wait());

            parent.Stop();

            Assert.Null(Activity.Current);
        }

        private static void PrintActivityInfo()
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId, 3}] Activity.Current = {Activity.Current.Id} -> {Activity.Current.ParentId}");
        }
    }
}

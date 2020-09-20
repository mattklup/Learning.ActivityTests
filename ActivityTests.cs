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

            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId, 3}] Activity.Current = {Activity.Current.Id} -> {Activity.Current.ParentId}");

            List<Task> tasks = new List<Task>();
            var ec = ExecutionContext.Capture();

            for (int i = 0; i < 5; ++i )
            {

                var task = Task.Run(async () =>
                {
                    var ec2 = ExecutionContext.Capture();

                    Assert.Same(ec, ec2);
                    Assert.NotNull(Activity.Current);
                    Assert.Equal(parent.Id, Activity.Current.Id);

                    var child = new Activity("TestChildActivity");

                    child.Start();

                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId, 3}] Activity.Current = {Activity.Current.Id} -> {Activity.Current.ParentId}");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    Assert.NotNull(Activity.Current);
                    Assert.Equal(child.Id, Activity.Current.Id);
                    Assert.Equal(parent, child.Parent);
                    Assert.Equal(parent.Id, child.Parent.Id);

                    child.Stop();
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            tasks.ForEach((t) => t.Wait());

            parent.Stop();

            Assert.Null(Activity.Current);
        }
    }
}

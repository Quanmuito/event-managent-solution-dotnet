namespace Ems.Common.Tests.Helpers;

using System.Diagnostics;

public static class ActivityTestHelper
{
    public static IDisposable CreateActivityWithTraceId(string activityName = "TestActivity")
    {
        var activitySource = new ActivitySource("Test");
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activity = activitySource.StartActivity(activityName);
        activity?.SetIdFormat(ActivityIdFormat.W3C);
        activity?.Start();

        return new ActivityDisposable(activitySource, listener, activity);
    }

    private class ActivityDisposable : IDisposable
    {
        private readonly ActivitySource _activitySource;
        private readonly ActivityListener _listener;
        private readonly Activity? _activity;

        public ActivityDisposable(ActivitySource activitySource, ActivityListener listener, Activity? activity)
        {
            _activitySource = activitySource;
            _listener = listener;
            _activity = activity;
        }

        public void Dispose()
        {
            _activity?.Stop();
            _activity?.Dispose();
            _listener.Dispose();
            _activitySource.Dispose();
        }
    }
}

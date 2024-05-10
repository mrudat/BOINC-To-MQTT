namespace BOINC_To_MQTT.Throttle;

internal interface IThrottleable
{
    /// <summary>
    /// Sets the throttle during start-up.
    /// </summary>
    /// <param name="throttle">The starting value for the throttle.</param>
    internal void SetThrottle(double throttle);

    /// <summary>
    /// Sets the throttle to a new value.
    /// </summary>
    /// <param name="throttle">The new value for the throttle.</param>
    internal Task UpdateThrottleAsync(double throttle, CancellationToken cancellationToken = default);
}
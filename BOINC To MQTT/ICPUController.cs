
namespace BOINC_To_MQTT;

internal interface ICPUController : IController
{
    void SetCPUUsageLimit(double cpuUsageLimit);
    Task UpdateThrottle(double throttle, CancellationToken cancellationToken = default);
}
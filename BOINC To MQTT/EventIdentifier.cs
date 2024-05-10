namespace BOINC_To_MQTT;

public enum EventIdentifier : int
{
    ConnectedToMQTTServer,
    DisconnectedFromMQTTServer,
    IncorrectAuthentication,
    NewCPUThrottleSetting,
    PausedGPUWorkload,
    ReadCPUUsageLimit,
    UnhandledTopic,
    FullThrottle,
    ErrorFailedToConnectoToBoinc,
}
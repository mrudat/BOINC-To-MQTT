namespace BOINC_To_MQTT.Boinc;

internal interface IBoincContext
{
    internal CommonBoincOptions Options { get; }

    internal string GetUserReadableDescription();
}
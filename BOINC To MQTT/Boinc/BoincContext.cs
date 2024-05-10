namespace BOINC_To_MQTT.Boinc;

internal record BoincContext : IBoincContext
{
    internal CommonBoincOptions? BoincOptions { get; set; }

    CommonBoincOptions IBoincContext.Options => BoincOptions!;

    string IBoincContext.GetUserReadableDescription()
    {
        return BoincOptions!.GetUserReadableDescription();
    }
}
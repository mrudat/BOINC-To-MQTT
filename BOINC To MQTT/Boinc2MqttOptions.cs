// Ignore Spelling: BOINC MQTT homeassistant RPC

using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Mqtt;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BOINC_To_MQTT;

public sealed partial class Boinc2MqttOptions
{
    public const string ConfigurationSectionName = "BOINC2MQTT";

    [ValidateEnumeratedItems]
    public LocalBoincOptions[] Local { get; set; } = [];

    [ValidateEnumeratedItems]
    public RemoteBoincOptions[] Remote { get; set; } = [];

    [Required]
    [ValidateObjectMembers]
    public required MqttOptions MQTT { get; set; }
}

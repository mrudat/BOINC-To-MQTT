// Ignore Spelling: Mqtt

using System.ComponentModel.DataAnnotations;

namespace BOINC_To_MQTT.Mqtt;

/// <summary>
/// Specifies that a data field must be a valid MQTT identifier.<br />
/// 
/// Must consist of characters from the character class [a-zA-Z0-9_-] (alphanumerics, underscore and hyphen).
/// </summary>
public partial class MqttIdentifierAttribute() : RegularExpressionAttribute(@"^[a-zA-Z0-9_-]+$")
{
#if false
    // TODO https://github.com/dotnet/runtime/issues/101965
    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex MqttIdentifierRegex();
#endif
}

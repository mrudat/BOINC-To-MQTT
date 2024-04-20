using System.ComponentModel.DataAnnotations;

namespace BOINC_To_MQTT;

/// <summary>
/// Specifies that a data field must be a valid MQTT identifier.<br />
/// 
/// Must consist of characters from the character class [a-zA-Z0-9_-] (alphanumerics, underscore and hyphen).
/// </summary>
public class MQTTIdentifierAttribute : RegularExpressionAttribute
{
    public MQTTIdentifierAttribute() : base(@"^[a-zA-Z0-9_-]+$")
    {
    }
}
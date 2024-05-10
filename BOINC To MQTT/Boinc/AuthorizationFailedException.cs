// Ignore Spelling: Rpc Gpu BOINC

namespace BOINC_To_MQTT.Boinc;
[Serializable]
internal class AuthorisationFailedException(IBoincContext boincContext) : Exception($"Failed to authenticate to BOINC client {boincContext.GetUserReadableDescription()}")
{
}
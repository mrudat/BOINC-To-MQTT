// Ignore Spelling: BOINC

using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT.Tests;

public class OptionsWrapper(BOINC2MQTTWorkerOptions theOptions) : IOptions<BOINC2MQTTWorkerOptions>
{
    public BOINC2MQTTWorkerOptions Value => theOptions;
}

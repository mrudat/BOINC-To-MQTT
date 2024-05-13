// Ignore Spelling: BOINC MQTT homeassistant frontend username offline
using Testcontainers;

namespace BOINC_To_MQTT.Tests;

public interface IHomeAssistantFixture
{
    IMqttContainer MqttContainer { get; }
}

// Ignore Spelling: BOINC MQTT
using Testcontainers;

namespace BOINC_To_MQTT.Tests;

public interface IMqttFixture
{
    ICommonMqttContainer MqttContainer { get; }
}
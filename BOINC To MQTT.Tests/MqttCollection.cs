// Ignore Spelling: BOINC MQTT
using Testcontainers.EMQX;
using Testcontainers.HiveMQ;
using Testcontainers.Mosquitto;

namespace BOINC_To_MQTT.Tests;

[CollectionDefinition(nameof(MqttCollection))]
public class MqttCollection : ICollectionFixture<MqttFixture<MosquittoContainer, MosquittoBuilder>>, ICollectionFixture<MqttFixture<EmqxContainer, EmqxBuilder>>, ICollectionFixture<MqttFixture<HiveMQContainer, HiveMQBuilder>>
{
    //class MosquittoFixture : MqttFixture<MosquittoContainer, MosquittoBuilder> { }

    //class EmqxFixture : MqttFixture<EmqxContainer, EmqxBuilder> { }

    //class HiveMQFixture : MqttFixture<HiveMQContainer, HiveMQBuilder> { }
}
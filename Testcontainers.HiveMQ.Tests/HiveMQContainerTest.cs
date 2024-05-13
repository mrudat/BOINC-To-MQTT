// Ignore Spelling: HiveMQ MQTT TLS

using Testcontainers.Tests;
using Xunit.Abstractions;

namespace Testcontainers.HiveMQ.Tests;

[Collection("Container")]
public class CommonHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : CommonMqttContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class MqttHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }

//[Collection("Container")]
//public class MqttTlsHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttTlsContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class WebSocketsHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttWebSocketsContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }

//[Collection("Container")]
//public class WebSocketsTlsHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttWebSocketsTlsContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }

//[Collection("Container")]
//public class AddUserHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : AddUserContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }

[CollectionDefinition("Container")]
public class ContainerCollection : ICollectionFixture<HiveMQFixture> { }

public class HiveMQFixture : ContainerFixture<HiveMQContainer, HiveMQBuilder> { }

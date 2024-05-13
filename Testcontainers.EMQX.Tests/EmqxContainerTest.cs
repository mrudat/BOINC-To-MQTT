// Ignore Spelling: EMQX MQTT TLS

using Testcontainers.Tests;
using Xunit.Abstractions;

namespace Testcontainers.EMQX.Tests;

[Collection("Container")]
public class CommonEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : CommonMqttContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class MqttEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class MqttTlsEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttTlsContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class WebSocketsEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttWebSocketsContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class WebSocketsTlsEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttWebSocketsTlsContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }

//[Collection("Container")]
//public class AddUserEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : AddUserContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }

[CollectionDefinition("Container")]
public class ContainerCollection : ICollectionFixture<EmqxFixture> { }

public class EmqxFixture : ContainerFixture<EmqxContainer, EmqxBuilder> { }

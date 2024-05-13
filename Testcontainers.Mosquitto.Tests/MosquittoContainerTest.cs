// Ignore Spelling: Mosquitto MQTT TLS

using Testcontainers.Tests;
using Xunit.Abstractions;

namespace Testcontainers.Mosquitto.Tests;

[Collection("Container")]
public class CommonMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : CommonMqttContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class MqttMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }

//[Collection("Container")]
//public class MqttTlsMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttTlsContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class WebSocketsMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttWebSocketsContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }

//[Collection("Container")]
//public class WebSocketsTlsMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttWebSocketsTlsContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }

[Collection("Container")]
public class AddUserMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : AddUserContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }

[CollectionDefinition("Container")]
public class ContainerCollection : ICollectionFixture<MosquittoFixture> { }

public class MosquittoFixture : ContainerFixture<MosquittoContainer, MosquittoBuilder> { }

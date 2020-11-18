# Multicast Message Broker
Single .NET class encapsulating UDP IP multicasting functionality and exposing it via observer pattern.

Original article:
https://www.codeproject.com/Tips/797036/Multicast-Message-Broker

## Introduction
Component (single class) which encapsulates network interactions and exposes multicast functionality to upper layers through publish/subscribe implementation of the observer pattern.

Requirements for message broker component:

* distribute string messages to anyone interested (observer pattern) * using udp multicasting
* single component encapsulating both send & receive logic
* no machine specific configuration
* basic error detection
* thread-safe interaction
* detection of duplicate messages
* ability to handle fragmented data
* ability to recover from data received in unexpected format (e.g.  network errors, interrupted transmission, etc.)

## Background
Some resources about IP multicasting:

* http://en.wikipedia.org/wiki/IP_multicast
* http://www.cisco.com/c/en/us/td/docs/ios/solutions_docs/ip_multicast/White_papers/mcst_ovr.html

## Using the code
Message broker consists of one class - **MulticastMessageBroker**, just drop it where you need it.

In the following snippet is demonstrated basic interaction with message broker: 
```csharp
    // create message broker
    var multicastBroker = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000);

    // connect to network
    multicastBroker.Connect();

    // subscribe to feed
    var subscriptionId = Guid.NewGuid();
    multicastBroker.Subscribe(subscriptionId,
        (m) =>
        {
            Console.WriteLine("Message received: {0}", m);
        });

    // ...

    // send notification
    multicastBroker.SendMessage(subscriptionId, "hello!");

    // ...

    // clean-up when no more needed
    multicastBroker.ClearAllSubscribers();
    multicastBroker.Disconnect();
```

In the *bin* folder is a very simple console chat application built on top of message broker. With this you can quickly test multicasting functionality inside your network. The application connects to 239.255.255.20:6000 and allows you to send/receive multicast messages. It should work on the same machine (when you run multiple instances you can send messages between them) as well across multiple machines inside multicast-enabled network.

## Implementation
Message broker implementation is based on **UdpClient** from **System.Net.Sockets**.

Following message format is used internally:

```csharp
    Guid:myself(sender) + Guid:uniqueMessageId + Guid:subscriptionId + int:messageLenght + byte[2]:CRC16 + string:message
```

To support detection & recovery from data received in an unexpected format, CRC check of header is included in the message. If this check doesn't pass after message header bytes are read from the incoming data stream, receiving pipeline is reset and broker continues to search for start of the message. CRC algo is not my own implementation, I've included first one that I've found which had required interface.

One of requirements was that the broker should work straight out of the box on any machine without the need of configuration. Because of this, by default, broker transmits/receives on all network interfaces it finds on the machine. If you want to change this behavior you can adjust the interface filter in JoinMulticastGroup method. Currently the filter looks like this:
```csharp
    Dns.GetHostAddresses(Dns.GetHostName()).Where(i => i.AddressFamily == AddressFamily.InterNetwork)
```

There is one constructor:
```csharp
    public MulticastMessageBroker(
        IPAddress multicastGroup,
        int port,
        int multicastTTL = 255,
        int processedMessageHistoryCount = 30,
        int maxSendByteCount = -1)
```

Each message carries unique id. To detect duplicate messages, message broker stores unique id of each received message into the list. If the received message unique id is in this list, the message is discarded. The length of this list can be controlled by processedMessageHistoryCount. By setting value to -1, you can turn off duplicate message detection.

With maxSendByteCount you can specify how many bytes will be sent at once (= one call to UdpClient.Send method). This was used mainly for testing, but there was no reason to remove it, maybe it will be useful in some scenarios.

No time intensive work is done in the constructor. To connect to network and start receiving/sending messages you need to call Connect method. Current connection status is indicated by IsConnected property. Connect / Disconnect methods can be called independently from Subscribe / Unsubscribe methods.

To subscribe for specific feed you can use:

```csharp
    public void Subscribe(Guid subscriptionId, Action<string> callback)
```

When calling this method you specify callback which will be called when a message with given subscription id is received. Each callback is invoked on new thread taken from the thread-pool, so there is no delay in case one of callbacks takes longer time to finish. If you would like to change or adjust this you can do it in FinalizeMessage method.

All of the public methods are thread-safe.

## Side note
There is one problem I came across when testing this through wifi connection on the battery powered laptop. I was able to send notifications, but reception seemed to be broken - sometimes I did not get messages sent from another computer in network. After some investigation I've found out that the problem was in the power profile for wifi adapter, after disabling power savings everything worked as expected. 

## Dependencies
.NET Framework 4
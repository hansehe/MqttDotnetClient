# MqttDotnetClient

Demo application with dotnet core using [MqttNet](https://github.com/chkr1011/MQTTnet) to implement mqtt with [RabbitMq](https://www.rabbitmq.com/mqtt.html).

## Get Started
1. Start RabbitMq with mqtt enabled in Docker:
   1. `pip install DockerBuildManagement`
   2. `dbm -swarm -start`
2. Open solution, restore, build and run it:
   1. [MqttDotnetClient/MqttDotnetClient.sln](MqttDotnetClient/MqttDotnetClient.sln)

## Useful links:
- MqttNet source code: https://github.com/chkr1011/MQTTnet
- Mqtt details: https://mosquitto.org/man/mqtt-7.html
- Mqtt qos security details: http://www.steves-internet-guide.com/understanding-mqtt-qos-levels-part-1/
- Note! RabbitMq does not suppert QoS 2 (Exactly once), and will downgrade the security to QoS 1 (At least once).
  - See: https://www.rabbitmq.com/mqtt.html


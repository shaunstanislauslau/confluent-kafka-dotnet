﻿// Copyright 2018 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.

using Confluent.Kafka;
using System;


namespace ConfluentCloudExample
{
    /// <summary>
    ///     This is a simple example demonstrating how to produce a message to
    ///     Confluent Cloud then read it back again.
    ///     
    ///     https://www.confluent.io/confluent-cloud/
    /// 
    ///     Confluent Cloud does not auto-create topics. You will need to use the ccloud
    ///     cli to create the dotnet-test-topic topic before running this example. The
    ///     <ccloud bootstrap servers>, <ccloud key> and <ccloud secret> parameters are
    ///     available via the confluent cloud web interface. For more information,
    ///     refer to the quick-start:
    ///
    ///     https://docs.confluent.io/current/cloud-quickstart.html
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var pConfig = new ProducerConfig
            {
                BootstrapServers = "<ccloud bootstrap servers>",
                BrokerVersionFallback = "0.10.0.0",
                ApiVersionFallbackMs = 0,
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                // Note: If your root CA certificates are in an unusual location you
                // may need to specify this using the SslCaLocation property.
                SaslUsername = "<ccloud key>",
                SaslPassword = "<ccloud secret>"
            };

            using (var producer = new ProducerBuilder<Null, string>(pConfig).Build())
            {
                producer.ProduceAsync("dotnet-test-topic", new Message<Null, string> { Value = "test value" })
                    .ContinueWith(task => task.IsFaulted
                        ? $"error producing message: {task.Exception.Message}"
                        : $"produced to: {task.Result.TopicPartitionOffset}");
                
                // block until all in-flight produce requests have completed (successfully
                // or otherwise) or 10s has elapsed.
                producer.Flush(TimeSpan.FromSeconds(10));
            }

            var cConfig = new ConsumerConfig
            {
                BootstrapServers = "<confluent cloud bootstrap servers>",
                BrokerVersionFallback = "0.10.0.0",
                ApiVersionFallbackMs = 0,
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = "<confluent cloud key>",
                SaslPassword = "<confluent cloud secret>",
                GroupId = Guid.NewGuid().ToString(),
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<Null, string>(cConfig).Build())
            {
                consumer.Subscribe("dotnet-test-topic");

                try
                {
                    var consumeResult = consumer.Consume();
                    Console.WriteLine($"consumed: {consumeResult.Value}");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"consume error: {e.Error.Reason}");
                }

                consumer.Close();
            }
        }
    }
}

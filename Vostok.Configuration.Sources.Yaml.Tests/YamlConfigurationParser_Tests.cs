using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Yaml.Tests
{
    internal class YamlConfigurationParser_Tests
    {
        [Test]
        public void Should_parse_lists()
        {
            var yaml = @"
fruits:
  - Apple
  - Orange
  - Strawberry
  - Mango

vegetables:
  - Potato
  - Eggplant
  - Cabbage";

            var settingsNode = YamlConfigurationParser.Parse(yaml);

            var expected = new ObjectNode(
                new[]
                {
                    new ArrayNode(
                        "fruits",
                        new[]
                        {
                            new ValueNode("0", "Apple"),
                            new ValueNode("1", "Orange"),
                            new ValueNode("2", "Strawberry"),
                            new ValueNode("3", "Mango")
                        }),
                    new ArrayNode(
                        "vegetables",
                        new[]
                        {
                            new ValueNode("0", "Potato"),
                            new ValueNode("1", "Eggplant"),
                            new ValueNode("2", "Cabbage")
                        })
                });

            settingsNode.Should().Be(expected);
        }

        [Test]
        public void Should_parse_list_in_root()
        {
            var yaml = @"
- Apple
- Orange
- Strawberry
- Mango";

            var settingsNode = YamlConfigurationParser.Parse(yaml);

            var expected = new ArrayNode(
                new[]
                {
                    new ValueNode("0", "Apple"),
                    new ValueNode("1", "Orange"),
                    new ValueNode("2", "Strawberry"),
                    new ValueNode("3", "Mango")
                });

            settingsNode.Should().Be(expected);
        }

        [Test]
        public void Should_parse_yaml_numbers()
        {
            var yaml = @"
number: .inf";

            var settingsNode = YamlConfigurationParser.Parse(yaml);

            var expected = new ObjectNode(new[] {new ValueNode("number", "Infinity")});

            settingsNode.Should().Be(expected);
        }

        [Test]
        public void Should_parse_yaml_datetime()
        {
            var yaml = @"
date: 2001-12-14
datetime: 2001-12-14T21:59:43.1000000-05:00
utc: 2001-12-14T21:59:43.1000000Z
";

            var settingsNode = YamlConfigurationParser.Parse(yaml);

            var date = new DateTime(2001, 12, 14).ToString("O");

            var datetime = new DateTimeOffset(
                    new DateTime(2001, 12, 14, 21, 59, 43, 100),
                    TimeSpan.FromHours(-5))
                .LocalDateTime
                .ToString("O");

            var utc = new DateTime(2001, 12, 14, 21, 59, 43, 100, DateTimeKind.Utc)
                .ToLocalTime()
                .ToString("O");

            settingsNode["date"].Value.Should().Be(date);
            settingsNode["datetime"].Value.Should().Be(datetime);
            settingsNode["utc"].Value.Should().Be(utc);
        }
    }
}
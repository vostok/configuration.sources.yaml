using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Yaml.Tests
{
    internal class YamlConfigurationParser_Tests
    {
        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        public void Should_Accept_Different_LineBreak(string lineBreak)
        {
            var yaml = @"
a: 123
b: qqq
c: dhjdfjdfj";

            yaml = string.Join(lineBreak, yaml.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries));

            var settingsNode = YamlConfigurationParser.Parse(yaml);

            settingsNode["a"].Value.Should().Be("123");
            settingsNode["b"].Value.Should().Be("qqq");
            settingsNode["c"].Value.Should().Be("dhjdfjdfj");
        }

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
                            new ValueNode("Apple"),
                            new ValueNode("Orange"),
                            new ValueNode("Strawberry"),
                            new ValueNode("Mango")
                        }),
                    new ArrayNode(
                        "vegetables",
                        new[]
                        {
                            new ValueNode("Potato"),
                            new ValueNode("Eggplant"),
                            new ValueNode("Cabbage")
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
                    new ValueNode("Apple"),
                    new ValueNode("Orange"),
                    new ValueNode("Strawberry"),
                    new ValueNode("Mango")
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
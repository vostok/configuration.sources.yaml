using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SharpYaml.Schemas;
using SharpYaml.Serialization;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Yaml
{
    [PublicAPI]
    public class YamlConfigurationParser
    {
        private static readonly Serializer Serializer = new Serializer(new SerializerSettings(new ExtendedSchema()));

        public static ISettingsNode Parse(string content)
            => Parse(content, null);

        public static ISettingsNode Parse(string content, string rootName)
            => string.IsNullOrWhiteSpace(content) ? null : ParseYaml(content, rootName);

        private static ISettingsNode ParseYaml(string content, string rootName)
        {
            using (var reader = new StringReader(content))
            {
                var deserialize = Serializer.Deserialize(reader);
                return ConvertToNode(deserialize, rootName);
            }
        }

        private static ISettingsNode ConvertToNode(object parsedYaml, string name = null)
        {
            switch (parsedYaml)
            {
                case Dictionary<object, object> mapping:
                    return ConvertToNode(mapping, name);
                case List<object> list:
                    return ConvertToNode(list, name);
                case object scalar:
                    return new ValueNode(name, FormatAsString(scalar));
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsedYaml), parsedYaml.GetType().Name, "Unknown type of node.");
            }
        }

        private static string FormatAsString(object obj)
        {
            if (obj == null)
                return null;

            if (obj is DateTime dateTime)
                return dateTime.ToString("O");

            if (obj is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return obj.ToString();
        }

        private static ISettingsNode ConvertToNode(Dictionary<object, object> parsedYaml, string name)
        {
            // (epeshk): skip ambiguous complex mapping key until we come up with something better

            var children = from kvp in parsedYaml
                           let key = kvp.Key as string
                           where key != null
                           select ConvertToNode(kvp.Value, key);

            return new ObjectNode(name, children);
        }

        private static ISettingsNode ConvertToNode(List<object> parsedYaml, string name)
        {
            var children = new ISettingsNode[parsedYaml.Count];

            for (var i = 0; i < children.Length; i++)
                children[i] = ConvertToNode(parsedYaml[i], i.ToString());

            return new ArrayNode(name, children);
        }
    }
}
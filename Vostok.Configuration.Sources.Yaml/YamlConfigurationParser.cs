using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Schemas;
using SharpYaml.Serialization;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Yaml
{
    [PublicAPI]
    public static class YamlConfigurationParser
    {
        private static readonly ExtendedSchema Schema = new ExtendedSchema();

        public static ISettingsNode Parse(string content)
            => Parse(content, null);

        public static ISettingsNode Parse(string content, string rootName)
            => string.IsNullOrWhiteSpace(content) ? null : ParseYaml(content, rootName);

        private static ISettingsNode ParseYaml(string content, string rootName)
        {
            using (var reader = new StringReader(content))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                return ConvertDocumentsToNode(yamlStream.Documents, rootName);
            }
        }

        private static ISettingsNode ConvertDocumentsToNode(IList<YamlDocument> documents, string rootName)
        {
            if (documents.Count == 0)
                return new ObjectNode(rootName);

            if (documents.Count == 1)
                return ConvertDocumentToNode(documents[0], rootName);

            return new ArrayNode(rootName, documents.Select(x => ConvertDocumentToNode(x)).ToArray());
        }

        private static ISettingsNode ConvertDocumentToNode(YamlDocument document, string name = null)
        {
            var root = document.RootNode;
            return ConvertNode(root, name);
        }

        private static ISettingsNode ConvertNode(YamlNode node, string name = null)
        {
            switch (node)
            {
                case YamlMappingNode mappingNode:
                    return ConvertNode(mappingNode, name);
                case YamlSequenceNode sequenceNode:
                    return ConvertNode(sequenceNode, name);
                case YamlScalarNode scalarNode:
                    return ConvertNode(scalarNode, name);
            }

            throw new ArgumentOutOfRangeException(nameof(node), node.GetType().Name, "Unknown type of YamlNode.");
        }

        private static ISettingsNode ConvertNode(YamlMappingNode node, string name)
        {
            // (epeshk): skip ambiguous complex mapping keys until we come up with something better
            
            var children = from kvp in node.Children
                           let key = (kvp.Key as YamlScalarNode)?.Value
                           where key != null
                           select ConvertNode(kvp.Value, key);

            return new ObjectNode(name, children);
        }

        private static ISettingsNode ConvertNode(YamlSequenceNode node, string name)
        {
            return new ArrayNode(name, node.Children.Select(x => ConvertNode(x)).ToArray());
        }

        private static ISettingsNode ConvertNode(YamlScalarNode node, string name)
        {
            var scalar = ConvertToScalar(node);

            if (!Schema.TryParse(scalar, true, out _, out var value))
                throw new YamlException($"Can't parse scalar '{scalar}'");

            return new ValueNode(name, FormatAsString(value));
        }

        private static Scalar ConvertToScalar(YamlScalarNode node)
        {
            var scalar = new Scalar(node.Anchor, node.Tag, node.Value, node.Style, false, false);
            return scalar;
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
    }
}
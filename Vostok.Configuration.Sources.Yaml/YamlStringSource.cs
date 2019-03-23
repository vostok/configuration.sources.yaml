using JetBrains.Annotations;
using Vostok.Configuration.Sources.Manual;

namespace Vostok.Configuration.Sources.Yaml
{
    /// <summary>
    /// A source that works by parsing in-memory YAML strings.
    /// </summary>
    [PublicAPI]
    public class YamlStringSource : ManualFeedSource<string>
    {
        public YamlStringSource()
            : base(YamlConfigurationParser.Parse)
        {
        }

        public YamlStringSource(string json)
            : this()
        {
            Push(json);
        }
    }
}
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Yaml
{
    /// <summary>
    /// A source that parses settings from local files in YAML format.
    /// </summary>
    [PublicAPI]
    public class YamlFileSource : FileSource
    {
        public YamlFileSource([NotNull] string filePath)
            : this(new FileSourceSettings(filePath))
        {
        }

        public YamlFileSource([NotNull] FileSourceSettings settings)
            : base(settings, YamlConfigurationParser.Parse)
        {
        }
    }
}
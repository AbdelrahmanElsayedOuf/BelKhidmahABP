using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace BelKhidmah.Localization
{
    public static class BelKhidmahLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(BelKhidmahConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(BelKhidmahLocalizationConfigurer).GetAssembly(),
                        "BelKhidmah.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}

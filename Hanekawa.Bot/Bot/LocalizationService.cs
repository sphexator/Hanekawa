using System.Globalization;
using Disqord.Bot.Commands.Application.Default;
using Disqord.Serialization.Json;
using Microsoft.Extensions.Options;

namespace Hanekawa.Bot.Bot;

public class LocalizationService : DefaultApplicationCommandLocalizer
{
    public LocalizationService(IOptions<DefaultApplicationCommandLocalizerConfiguration> options, 
        ILogger<DefaultApplicationCommandLocalizer> logger, IJsonSerializer serializer) 
        : base(options, logger, serializer)
    { }

    protected override StoreInformation CreateStoreInformation(CultureInfo locale, string localeFilePath, 
        bool localeExists, MemoryStream memoryStream, LocalizationStoreJsonModel model)
    {
        
        return base.CreateStoreInformation(locale, localeFilePath, localeExists, memoryStream, model);
    }
}
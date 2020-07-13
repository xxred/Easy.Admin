using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace Easy.Admin.Localization
{
    public class DbStringLocalizerFactory : IStringLocalizerFactory
    {
        private static readonly ConcurrentDictionary<string, IStringLocalizer> _resourceLocalizations = new ConcurrentDictionary<string, IStringLocalizer>();

        public IStringLocalizer Create(string baseName, string location)
        {
            var resourceKey = baseName + location;

            return CreateLocalizer(resourceKey);
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return CreateLocalizer(resourceSource.Name);
        }

        private IStringLocalizer CreateLocalizer(string resourceKey)
        {
            if (_resourceLocalizations.Keys.Contains(resourceKey))
            {
                return _resourceLocalizations[resourceKey];
            }

            var stringLocalizer = new DbStringLocalizer(resourceKey);
            return _resourceLocalizations.GetOrAdd(resourceKey, stringLocalizer);
        }
    }
}

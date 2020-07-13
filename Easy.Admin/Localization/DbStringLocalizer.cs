using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Easy.Admin.Localization.Models;
using Microsoft.Extensions.Localization;

namespace Easy.Admin.Localization
{
    public class DbStringLocalizer : IStringLocalizer
    {
        private readonly string _resourceKey;

        public DbStringLocalizer(string resourceKey)
        {
            _resourceKey = resourceKey;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public LocalizedString this[string name]
        {
            get
            {
                var culture = CultureInfo.CurrentCulture.ToString();

                var localization = LocalizationRecords.FindByKeyAndResourceKeyAndLocalizationCulture(name, _resourceKey, culture);

                if (localization != null) return new LocalizedString(name, localization.Text);
                // 不存在创建一个，方便到时候翻译
                localization = new LocalizationRecords
                {
                    Key = name,
                    LocalizationCulture = culture,
                    ResourceKey = _resourceKey,
                    Text = name
                };

                localization.Save();

                return new LocalizedString(name, localization.Text);
            }
        }

        public LocalizedString this[string name, params object[] arguments] => this[name];
    }
}

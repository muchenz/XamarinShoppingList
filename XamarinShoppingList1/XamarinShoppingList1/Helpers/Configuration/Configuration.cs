using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace XamarinShoppingList1.Helpers.Configuration
{
    class Configuration : IConfiguration

    {
        IConfiguration _configuration;
        public Configuration()
        {
            Stream resourceStream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("XamarinShoppingList1.appsettings.json");

            _configuration = new ConfigurationBuilder().AddJsonStream(resourceStream).Build();


        }
        public string this[string key]
        {
            get
            {
                return _configuration[key];
            }
            set
            {
                _configuration[key] = value;
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _configuration.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key);
        }
    }
}

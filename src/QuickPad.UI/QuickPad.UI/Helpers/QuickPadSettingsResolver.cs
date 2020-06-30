using System;
using Newtonsoft.Json.Serialization;

namespace QuickPad.UI.Helpers
{
    public class QuickPadSettingsResolver : DefaultContractResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public QuickPadSettingsResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override JsonObjectContract CreateObjectContract(Type type)
        {
            if (_serviceProvider.GetService(type) == null) return base.CreateObjectContract(type);

            var contract = base.CreateObjectContract(type);
            
            contract.DefaultCreator = () => _serviceProvider.GetService(type);

            return contract;

        }
    }
}
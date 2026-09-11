using System;

namespace Pinball.Client.Services
{
    public static class ClientServices
    {
        private static IClientDataService _data;

        public static IClientDataService Data
        {
            get
            {
                if (_data == null)
                    throw new InvalidOperationException("Client services have not been initialized.");
                return _data;
            }
        }

        public static void InitializeForDevelopment()
        {
            if (_data == null)
                _data = new LocalClientDataService();
        }

        public static void ReplaceDataService(IClientDataService dataService)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));
            _data = dataService;
        }
    }
}

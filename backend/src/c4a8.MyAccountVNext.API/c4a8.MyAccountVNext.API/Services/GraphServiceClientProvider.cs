using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c4a8.MyAccountVNext.API.Services
{
    public class GraphServiceClientProvider
    {
        private readonly GraphServiceClient _graphClient;

        public GraphServiceClientProvider()
        {
            var graphCred = new ClientSecretCredential("asdf", "asdf","asdf");
            _graphClient = new GraphServiceClient(graphCred);
        }

        public GraphServiceClient GetGraphClient()
        {
            return _graphClient;
        }
    }
}

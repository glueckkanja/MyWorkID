namespace c4a8.MyAccountVNext.Server.HttpClients.MsGraph
{
    public class LogGraphRequestHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LogGraphRequestHandler(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            IEnumerable<string> headerValues = httpRequest.Headers.GetValues("Authorization");
            var authHeader = headerValues.FirstOrDefault();
            _logger.LogInformation("Sending Graph request with auth header {0}", authHeader);// log the request before it goes out.
            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);
            return response;
        }
    }
}

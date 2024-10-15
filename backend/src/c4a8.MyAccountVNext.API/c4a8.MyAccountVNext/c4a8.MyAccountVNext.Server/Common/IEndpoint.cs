namespace c4a8.MyAccountVNext.Server.Common
{
    public interface IEndpoint
    {
        static abstract void MapEndpoint(IEndpointRouteBuilder endpoints);
    }
}

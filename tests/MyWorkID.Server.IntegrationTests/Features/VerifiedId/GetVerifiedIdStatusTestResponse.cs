namespace MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    public class GetVerifiedIdStatusTestResponse
    {
        public bool Required { get; set; }
        public int ValidityInMinutes { get; set; }
        public bool RequiresVerificationPerAction { get; set; }
        public bool IsVerified { get; set; }
        public DateTimeOffset? LastVerifiedAt { get; set; }
        public string? VerifiedForAction { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
    }
}

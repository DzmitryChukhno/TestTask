namespace GQL.GraphQL.Payloads
{
    public record LoginPayload(string Token, DateTime ExpiresAt);
}

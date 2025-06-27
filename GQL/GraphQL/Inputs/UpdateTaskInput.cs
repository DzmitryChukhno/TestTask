namespace GQL.GraphQL.Inputs
{
    public class UpdateTaskInput
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int? CreatedById { get; set; }
    }
}

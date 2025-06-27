namespace GQL.GraphQL.Inputs
{
    public class AddTaskInput
    {
        public string Title { get; set; } = default!;
        public string Status { get; set; } = "New";
        public string? Description { get; set; }
        public int CreatedById { get; set; }
    }
}

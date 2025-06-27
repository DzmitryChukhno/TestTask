using GQL.Data;
using GQL.Models;

namespace GQL.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<TaskItem> GetTasks([Service] AppDbContext context)
        => context.Tasks;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context)
       => context.Users;
}

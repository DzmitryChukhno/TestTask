using GQL.Data;
using GQL.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GQL.GraphQL;

public class Query
{

    [Authorize]
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<TaskItem> GetTasks([Service] AppDbContext context)
    {
        return context.Tasks.Include(t => t.CreatedBy);
    }


    [Authorize]
    public async Task<TaskItem?> GetTaskById(int id, ClaimsPrincipal user, [Service] AppDbContext context)
    {
        var task = await context.Tasks
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return null;

        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Admin" || task.CreatedById == userId)
            return task;

        throw new GraphQLException(ErrorBuilder.New()
            .SetMessage("Access denied.")
            .SetCode("FORBIDDEN")
            .Build());
    }


    [UsePaging(IncludeTotalCount = true)]
    [Authorize(Roles = ["Admin" ])]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context)
    {
        return context.Users;
    }

}

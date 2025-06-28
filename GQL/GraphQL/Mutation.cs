using GQL.Data;
using GQL.GraphQL.Inputs;
using GQL.GraphQL.Payloads;
using GQL.Models;
using GQL.Services;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HotChocolate.Authorization;

namespace GQL.GraphQL
{
    public class Mutation
    {
        [Authorize]
        public async Task<TaskItem> CreateTaskAsync(AddTaskInput input, [Service] AppDbContext context, ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Пользователь не авторизован")
                    .Build());
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var createdById = (role == "Admin" && input.CreatedById != 0)
                ? input.CreatedById
                : userId;

            var task = new TaskItem
            {
                Title = input.Title,
                Description = input.Description,
                Status = input.Status,
                CreatedById = createdById
            };

            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            await context.Entry(task)
                .Reference(t => t.CreatedBy)
                .LoadAsync();

            return task;
        }


        [Authorize]
        public async Task<TaskItem> UpdateTaskAsync(UpdateTaskInput input, [Service] AppDbContext context, ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);


            Console.WriteLine(user.Identity?.IsAuthenticated); // должно быть true
            Console.WriteLine(user.FindFirst(ClaimTypes.Role)?.Value); // должно быть Admin

            if (userIdClaim is null || roleClaim is null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Не удалось получить данные пользователя")
                    .Build());
            }

            var userId = int.Parse(userIdClaim.Value);
            var userRole = roleClaim.Value;

            var task = await context.Tasks.FindAsync(input.Id);
            if (task is null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Задача не найдена")
                    .Build());
            }

            var isOwner = task.CreatedById == userId;
            var isAdmin = userRole == "Admin";

            if (!isOwner && !isAdmin)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Недостаточно прав для редактирования этой задачи")
                    .Build());
            }

            task.Title = input.Title ?? task.Title;
            task.Description = input.Description ?? task.Description;
            task.Status = input.Status ?? task.Status;

            if (isAdmin && input.CreatedById.HasValue)
                task.CreatedById = input.CreatedById.Value;

            await context.SaveChangesAsync();
            return task;
        }


        [Authorize(Roles = ["Admin"])]
        public async Task<bool> DeleteTaskAsync(int id, [Service] IDbContextFactory<AppDbContext> contextFactory)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var task = await context.Tasks.FindAsync(id);
            if (task is null)
                return false;

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
            return true;
        }

        [GraphQLName("login")]
        public LoginPayload Login(AuthInput input, [Service] AuthService auth, [Service] IConfiguration config)
        {
            var token = auth.Authenticate(input.Username, input.Password);
            if (token is null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Неверный логин или пароль")
                    .Build());
            }

            return new LoginPayload(
                token,
                DateTime.UtcNow.AddMinutes(
                    double.Parse(config["Jwt:ExpiresInMinutes"]!))
            );
        }
    }
}

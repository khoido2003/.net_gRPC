using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services
{
  public class TodoService : ToDoit.ToDoitBase
  {

    private readonly AppDbContext _dbContext;

    public TodoService(AppDbContext dbContext)
    {
      _dbContext = dbContext;
    }


    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {

      if (request.Title == string.Empty || request.Description == string.Empty)
      {
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Title and Description are required"));
      }

      var todoItem = new TodoItem { Title = request.Title, Description = request.Description };

      await _dbContext.AddAsync(todoItem);
      await _dbContext.SaveChangesAsync();

      return await Task.FromResult(
        new CreateToDoResponse
        {
          Id = todoItem.Id
        }
      );
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {

      if (request.Id <= 0)
      {
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID"));
      }

      var todoItem = await _dbContext.TodoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

      if (todoItem == null)
      {
        throw new RpcException(new Status(StatusCode.NotFound, "Todo item not found"));
      }

      return await Task.FromResult(
        new ReadToDoResponse
        {
          Id = todoItem.Id,
          Title = todoItem.Title,
          Description = todoItem.Description,
          ToDoStatus = todoItem.ToDoStatus
        }
      );

      throw new RpcException(new Status(StatusCode.NotFound, "Todo item not found"));
    }


    public override async Task<GetAllResponse> ReadToDoList(GetAllRequest request, ServerCallContext context)
    {
      var response = new GetAllResponse();
      var todoItems = await _dbContext.TodoItems.ToListAsync();

      foreach (var toDo in todoItems)
      {
        response.ToDo.Add(new ReadToDoResponse
        {
          Id = toDo.Id,
          Title = toDo.Title,
          Description = toDo.Description,
          ToDoStatus = toDo.ToDoStatus
        });
      }

      return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
      if (request.Title == string.Empty || request.Description == string.Empty)
      {
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Title and Description are required"));
      }

      var toDoItem = await _dbContext.TodoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

      if (toDoItem == null)
      {
        throw new RpcException(new Status(StatusCode.NotFound, $"No task with Id {request.Id}"));
      }

      toDoItem.Title = request.Title;
      toDoItem.Description = request.Description;
      toDoItem.ToDoStatus = request.ToDoStatus;

      await _dbContext.SaveChangesAsync();

      return await Task.FromResult(new UpdateToDoResponse
      {
        Id = toDoItem.Id
      });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
      if (request.Id <= 0)
      {
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));
      }

      var toDoItem = await _dbContext.TodoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

      if (toDoItem == null)
      {
        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id ${request.Id}"));
      }

      _dbContext.Remove(toDoItem);

      await _dbContext.SaveChangesAsync();

      return await Task.FromResult(new DeleteToDoResponse
      {
        Id = toDoItem.Id
      });
    }


  }
}
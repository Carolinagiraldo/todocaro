using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using todocaro.common.Models;
using todocaro.common.Responses;
using todocaro.Functions.Entities;

namespace todocaro.Functions.functions
{
    public static class TodoApi
    {
        // metodo para crear tareas
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo",Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new todo.");

            

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            todo todo = JsonConvert.DeserializeObject<todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response    
                { 
                IsSucess = false,
                Message = "The request must have a TaskDescription."
                });
            }

            TodoEntity todoEntity = new TodoEntity
            {
                createdTime =DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = "New todo stored in table";
            log.LogInformation(message);

           

            return new OkObjectResult(new Response 
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });
        }

        // metodo para actualizar tareas
        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
              [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
              [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
              string id,
              ILogger log)
        {
            log.LogInformation($"Update for todo: {id}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            todo todo = JsonConvert.DeserializeObject<todo>(requestBody);

            // Validate todo id

            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found."
                });
            }

            // Update todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.IsCompleted = todo.IsCompleted;
            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;

            }


            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = $"Todo: {id}, update in table.";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });
        }

        // metodo para recuperar tareas

        [FunctionName(nameof(GetAllTodos))]
        public static async Task<IActionResult> GetAllTodos(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
             [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
             ILogger log)
        {
            log.LogInformation("Get all todos received.");
            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            
            string message = "Restrieved all todos.";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todos
            });
        }

        [FunctionName(nameof(GetTodoById))]
        public static IActionResult GetTodoById(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
             [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
            string id,
             ILogger log)
        {
            log.LogInformation($"Get todo by id: {id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found."
                });
            }

            string message = $"Todo: {todoEntity.RowKey}, retrieved.";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });
        }


        [FunctionName(nameof(DeleteTodo))]
        public static  async Task<IActionResult> DeleteTodo(
                 [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
                 [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
                  [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
                  string id,
                 ILogger log)
        {
            log.LogInformation($"Delete todo: {id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found."
                });
            }
            await todoTable.ExecuteAsync(TableOperation.Delete(todoEntity));
            string message = $"Todo: {todoEntity.RowKey}, delete.";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
}

using NorthOneChallenge.DBconnections;
using NorthOneChallenge.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NorthOneChallenge.Services
{
    public class TaskService
    {
        public Tuple<List<Task>, List<string>> Create(IEnumerable<Task> tasks)
        {
            var result = MakeResultTuple(tasks);        

            //validate before connecting to DB
            bool allTasksOk = result.Item2.Count == 0 && ValidateTasks(tasks, result.Item2);

            //create only if all are good
            if (allTasksOk)
            {
                using (var db = new ToDoListContext())
                {
                    foreach (var task in tasks)
                    {
                        var createdTask = db.Tasks.Add(task);
                        result.Item1.Add(createdTask);
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        result.Item1.Clear();
                        result.Item2.Add("Failed to create tasks" + ex.Message);
                    }
                }
            }

            return result;
        }

        public Tuple<List<Task>, List<string>> Update(IEnumerable<Task> tasks)
        {
            var result = MakeResultTuple(tasks);

            //validate before connecting to DB
            bool allTasksOk = result.Item2.Count == 0 && ValidateTasks(tasks, result.Item2);
            if (!allTasksOk)
            {
                return result;
            }
            
            using (var db = new ToDoListContext())
            {
                //get all tasks in one trip
                int[] taskIds = tasks.Select(t => t.Id).ToArray();
                List<Task> dbTasks = null;

                try
                {
                    //order by Id so we can use binary search
                    dbTasks = db.Tasks
                        .Where(t => taskIds.Contains(t.Id))
                        .OrderBy(t => t.Id)
                        .ToList();
                }
                catch(Exception ex)
                {
                    result.Item2.Add("Failed to retrieve tasks from DB. Error: " + ex.Message);
                    allTasksOk = false;
                }

                if (allTasksOk && dbTasks != null)
                {
                    TaskComparer comparer = new TaskComparer();
                    foreach (var task in tasks)
                    {
                        //ensure all passed in tasks exist in DB
                        int index = dbTasks.BinarySearch(new Task() { Id = task.Id }, comparer);
                        if (index >= 0)
                        {
                            var dbTask = dbTasks[index];
                            dbTask.Description = task.Description;
                            dbTask.Title = task.Title;
                            dbTask.Status = task.Status;
                            dbTask.DueDate = task.DueDate;

                            result.Item1.Add(dbTask);
                        }
                        else
                        {
                            result.Item2.Add(String.Format("Unable to update task: {0}. Task does not exist in DB", task.Id));
                            allTasksOk = false;
                        }
                    }

                    if (allTasksOk)
                    {
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            result.Item1.Clear();
                            result.Item2.Add("Failed to update tasks" + ex.Message);
                        }
                    }
                }
            }                       

            return result;
        }

        public List<string> Delete(int[] taskIds)
        {
            List<string> errors = new List<string>();

            using (var db = new ToDoListContext())
            {               
                var dbTasks = db.Tasks.Where(t => taskIds.Contains(t.Id));
                if(taskIds.Length == dbTasks.Count())
                {
                    db.Tasks.RemoveRange(dbTasks);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        errors.Add("An error occured while trying to delete tasks. Error: " + ex.Message);
                    }
                }
                else
                {
                    errors.Add("Unable to find all requested tasks to delete");
                }
            }

            return errors;
        }

        public Tuple<List<Task>, List<string>> GetTasksById(int[] ids)
        {
            var result = MakeResultTuple();
            using (var db = new ToDoListContext())
            {               
                var query = db.Tasks.Where(t => ids.Contains(t.Id));
                var tasks = RunQueryable(query, result.Item2);
                result.Item1.AddRange(tasks);                
            }

            if(result.Item1.Count != ids.Length)
            {
                result.Item2.Add("Not all the requested ids were returned");
            }

            return result;
        }

        public Tuple<List<Task>, List<string>> GetTasksByStatus(Task.TaskStatus status)
        {
            var result = MakeResultTuple();
            using (var db = new ToDoListContext())
            {
                var query = db.Tasks.Where(t => t.Status == status);
                var tasks = RunQueryable(query, result.Item2);
                result.Item1.AddRange(tasks);
            }
            return result;
        }

        /// <summary>
        /// returns a list of tasks with due date greater than the current time and staus = PENDING ordered by due date in ascending order
        /// </summary>
        /// <returns></returns>
        public Tuple<List<Task>, List<string>> GetUpcomingTasks()
        {
            var result = MakeResultTuple();

            using (var db = new ToDoListContext())
            {
                var query = db.Tasks
                    .Where(t => DbFunctions.DiffDays(DateTime.Now, t.DueDate) >= 0
                            && t.Status == Task.TaskStatus.PENDING)
                    .OrderBy(t => t.DueDate);

                var tasks = RunQueryable(query, result.Item2);
                result.Item1.AddRange(tasks);
            }

            return result;
        }

        /// <summary>
        /// return a list of tasks with due date before current and status = PENDING in descending order
        /// </summary>
        /// <returns></returns>
        public Tuple<List<Task>, List<string>> GetOverdueTasks()
        {
            var result = MakeResultTuple();

            using (var db = new ToDoListContext())
            {
                var query = db.Tasks
                    .Where(t => DbFunctions.DiffDays(DateTime.Now, t.DueDate) <= 0 && t.Status == Task.TaskStatus.PENDING)
                    .OrderByDescending(t => t.DueDate);

                var tasks = RunQueryable(query, result.Item2);
                result.Item1.AddRange(tasks);
            }

            return result;
        }

        private List<Task> RunQueryable(IQueryable<Task> query, List<string> errors)
        {
            List<Task> result = null;
            try
            {
                result = query.ToList();
            }
            catch(Exception ex)
            {
                errors.Add("An error occured during DB operation. Error: " + ex.Message);
            }

            return result ?? new List<Task>();
        }

        private bool ValidateTasks(IEnumerable<Task> tasks, List<string> errors)
        {
            bool allTasksOk = true;
            foreach (var task in tasks)
            {
                allTasksOk = allTasksOk && ValidateTask(task, errors);
            }
            return allTasksOk;
        }

        private bool ValidateTask(Task task, List<string> errors)
        {
            bool ok = true;
            if(task.DueDate < DateTime.Now)
            {
                errors.Add(String.Format("Task: {0} has due date in the past", task.Id));
                ok = false;
            }

            if (String.IsNullOrEmpty(task.Title))
            {
                errors.Add(String.Format("Task: {0} title is empty or missing", task.Id));
                ok = false;
            }

            if (!String.IsNullOrEmpty(task.Title) && task.Title.Length > 100)
            {
                errors.Add(String.Format("Task: {0} title exceeds maximum length", task.Id));
                ok = false;
            }

            if (!String.IsNullOrEmpty(task.Description) && task.Description.Length > 500)
            {
                errors.Add(String.Format("Task: {0} description exceeds maximum length", task.Id));
                ok = false;
            }

            return ok;
        }

        private Tuple<List<Task>, List<string>> MakeResultTuple(IEnumerable<Task> tasks)
        {
            var result = MakeResultTuple();
            if(tasks == null)
            {
                result.Item2.Add("Passed in tasks are null");
            }
            return result;
        }

        private Tuple<List<Task>, List<string>> MakeResultTuple()
        {
            List<Task> newTasks = new List<Task>();
            List<string> errors = new List<string>();            
            Tuple<List<Task>, List<string>> tuple = new Tuple<List<Task>, List<string>>(newTasks, errors);
            return tuple;
        }

        private class TaskComparer : IComparer<Task>
        {
            public int Compare(Task x, Task y)
            {
                if(x != null && y == null)
                {
                    return 1;
                }
                else if(x == null && y != null)
                {
                    return -1;
                }
                else if(x == null && y == null)
                {
                    return 0;
                }
                else
                {
                    return x.Id - y.Id;
                }
            }
        }
    }
}
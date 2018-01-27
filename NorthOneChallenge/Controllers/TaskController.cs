using NorthOneChallenge.Models;
using NorthOneChallenge.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NorthOneChallenge.Controllers
{
    public class TaskController : Controller
    {
        private TaskService _service;

        public TaskController()
        {
            _service = new TaskService();
        }

        // GET: Task
        public ActionResult Index()
        {
            return RedirectToAction("ShowUpcoming");
        }

        public ActionResult ShowUpcoming()
        {
            var tasks = _service.GetUpcomingTasks();
            if(tasks.Item2.Count == 0)
            {
                return View(tasks);
            }
            else
            {
                return View("ListErrors", tasks.Item2);
            }                        
        }

        public ActionResult ShowOverdue()
        {
            var tasks = _service.GetOverdueTasks();
            if(tasks.Item2.Count == 0)
            {
                return View(tasks);
            }
            else
            {
                return View("ListErrors", tasks.Item2);
            }
        }

        public ActionResult ShowCompleted()
        {
            var tasks = _service.GetTasksByStatus(Task.TaskStatus.DONE);
            if (tasks.Item2.Count == 0)
            {
                return View(tasks);
            }
            else
            {
                return View("ListErrors", tasks.Item2);
            }
        }

        [HttpGet]
        public ActionResult CreateTask()
        {
            var task = new Task();
            task.DueDate = DateTime.Now.AddDays(1);
            task.Status = Task.TaskStatus.PENDING;

            return View(task);
        }

        [HttpPost]
        public ActionResult CreateTask(FormCollection form)
        {
            Task model = new Task();
            if (TryUpdateModel(model))
            {
                var result = _service.Create(new Task[] { model });
                if (result.Item2.Count == 0)
                {
                    RedirectToAction("ShowUpcoming");
                }
                else
                {
                    return View("ListErrors", result.Item2);
                }
            }

            return View("ListErrors", GetModelErrors());
        }

        [HttpGet]
        public ActionResult EditTask(int Id)
        {
            var result = _service.GetTasksById(new int[] { Id });
            if(result.Item2.Count == 0)
            {
                return View(result.Item1[0]);
            }
            else
            {
                return View("ListErrors", result.Item2);
            }
        }

        [HttpPost]
        public ActionResult EditTask(FormCollection form)
        {
            Task model = new Task();
            if (TryUpdateModel(model))
            {
                var result = _service.GetTasksById(new int[] { model.Id });
                if (result.Item2.Count == 0)
                {
                    result = _service.Update(new Task[] { model });
                    if(result.Item2.Count == 0)
                    {
                        return RedirectToAction("ShowUpcoming");
                    }
                }

                return View("ListErrors", result.Item2);
            }            

            return View("ListErrors", GetModelErrors());
        }

        [HttpGet]
        public ActionResult DeleteTask(int Id)
        {
            var result = _service.Delete(new int[] { Id });
            if(result.Count == 0)
            {
                return RedirectToAction("ShowUpcoming");
            }
            else
            {
                return View("ListErrors", result);
            }            
        }

        private List<string> GetModelErrors()
        {
            List<string> errors = new List<string>();
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            return errors;
        }
    }
}
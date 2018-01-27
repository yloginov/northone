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
            return View(tasks);            
        }

        public ActionResult ShowOverdue()
        {
            var tasks = _service.GetOverdueTasks();
            return View(tasks);
        }

        public ActionResult ShowCompleted()
        {
            var tasks = _service.GetTasksByStatus(Task.TaskStatus.DONE);
            return View(tasks);
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
            } 

            return RedirectToAction("ShowUpcoming");
        }

        [HttpGet]
        public ActionResult EditTask(int Id)
        {
            var result = _service.GetTasksById(new int[] { Id });
            if(result.Item2.Count == 0)
            {
                return View(result.Item1[0]);
            }

            return View();
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
                }                
            }

            return RedirectToAction("ShowUpcoming");
        }

        [HttpGet]
        public ActionResult DeleteTask(int Id)
        {
            var result = _service.Delete(new int[] { Id });
            return RedirectToAction("ShowUpcoming");
        }
    }
}
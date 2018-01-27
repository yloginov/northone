using NorthOneChallenge.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NorthOneChallenge.DBconnections
{
    public class ToDoListContext : DbContext
    {
        public ToDoListContext() : base("name=ToDoListContext")
        {

        }

        public DbSet<Task> Tasks { get; set; }
    }
}
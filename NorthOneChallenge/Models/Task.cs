using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthOneChallenge.Models
{
    [Table("Task")]
    public class Task
    {
        public enum TaskStatus
        {
            PENDING = 1,
            DONE = 2
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public TaskStatus Status { get; set; }

        public DateTime DueDate { get; set; }
    }
}
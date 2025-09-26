using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TodoApp
{ 
   public enum PriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public string Category { get; set; } = "";
    public DateTime? DueDate { get; set; } = null;
    public bool IsDone { get; set; } = false;
}
    public class TaskRepository
    {
        private readonly string _filePath;
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        public TaskRepository(string filePath)
        {
            _filePath = filePath;
            Load();
        }

        public void Add(TaskItem item)
        {
            Tasks.Add(item);
            Save();
        }

        public void Remove(TaskItem item)
        {
            Tasks.RemoveAll(t => t.Id == item.Id);
            Save();
        }

        public void Update(TaskItem item)
        {
            var idx = Tasks.FindIndex(t => t.Id == item.Id);
            if (idx >= 0) Tasks[idx] = item;
            Save();
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Tasks, options);
            File.WriteAllText(_filePath, json);
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Tasks = new List<TaskItem>();
                    return;
                }
                var json = File.ReadAllText(_filePath);
                Tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            catch
            {
                Tasks = new List<TaskItem>();
            }
        }
    }
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private ListView lvTasks;

        public MainForm()
        {
            this.Text = "Todo List";
            this.Width = 900;
            this.Height = 600;

            lvTasks = new ListView
            {
                Left = 10,
                Top = 10,
                Width = 860,
                Height = 500,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvTasks.Columns.Add("Название", 300);
            lvTasks.Columns.Add("Приоритет", 100);
            lvTasks.Columns.Add("Категория", 150);
            lvTasks.Columns.Add("Срок", 120);
            lvTasks.Columns.Add("Выполнена", 100);

            this.Controls.Add(lvTasks);
        }
    }
}
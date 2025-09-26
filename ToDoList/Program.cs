using System;
using System.Windows.Forms;

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
        public MainForm()
        {
            this.Text = "TodoApp";
            this.Width = 800;
            this.Height = 600;
        }
    }
}
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TodoApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

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

    public class TaskForm : Form
    {
        private TextBox txtTitle;
        private ComboBox cbPriority;
        private TextBox txtCategory;
        private DateTimePicker dtpDueDate;
        private CheckBox chkNoDate;
        private CheckBox chkDone;
        private Button btnOk, btnCancel;

        public TaskItem Task { get; private set; }

        public TaskForm(TaskItem? task = null)
        {
            Task = task != null ? new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Priority = task.Priority,
                Category = task.Category,
                DueDate = task.DueDate,
                IsDone = task.IsDone
            } : new TaskItem();

            InitializeComponent();

            txtTitle.Text = Task.Title;
            cbPriority.SelectedItem = Task.Priority.ToString();
            txtCategory.Text = Task.Category;
            if (Task.DueDate.HasValue) dtpDueDate.Value = Task.DueDate.Value;
            chkNoDate.Checked = !Task.DueDate.HasValue;
            chkDone.Checked = Task.IsDone;
        }

        private void InitializeComponent()
        {
            this.Text = "������";
            this.Width = 400;
            this.Height = 250;

            Label lblTitle = new Label { Left = 10, Top = 10, Text = "��������:" };
            txtTitle = new TextBox { Left = 100, Top = 10, Width = 250 };

            Label lblPriority = new Label { Left = 10, Top = 40, Text = "���������:" };
            cbPriority = new ComboBox { Left = 100, Top = 40, Width = 150 };
            cbPriority.Items.AddRange(Enum.GetNames(typeof(PriorityLevel)));

            Label lblCategory = new Label { Left = 10, Top = 70, Text = "���������:" };
            txtCategory = new TextBox { Left = 100, Top = 70, Width = 250 };

            Label lblDueDate = new Label { Left = 10, Top = 100, Text = "����:" };
            dtpDueDate = new DateTimePicker { Left = 100, Top = 100, Width = 200 };
            chkNoDate = new CheckBox { Left = 310, Top = 100, Text = "��� ����" };

            chkDone = new CheckBox { Left = 100, Top = 130, Text = "���������" };

            btnOk = new Button { Text = "��", Left = 200, Top = 170, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "������", Left = 280, Top = 170, DialogResult = DialogResult.Cancel };

            btnOk.Click += (s, e) =>
            {
                Task.Title = txtTitle.Text;
                Task.Priority = (PriorityLevel)Enum.Parse(typeof(PriorityLevel), cbPriority.SelectedItem.ToString());
                Task.Category = txtCategory.Text;
                Task.DueDate = chkNoDate.Checked ? null : dtpDueDate.Value.Date;
                Task.IsDone = chkDone.Checked;
            };

            this.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblPriority, cbPriority, lblCategory, txtCategory, lblDueDate, dtpDueDate, chkNoDate, chkDone, btnOk, btnCancel });
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
            lvTasks.Columns.Add("��������", 300);
            lvTasks.Columns.Add("���������", 100);
            lvTasks.Columns.Add("���������", 150);
            lvTasks.Columns.Add("����", 120);
            lvTasks.Columns.Add("���������", 100);

            this.Controls.Add(lvTasks);
        }
    }
}
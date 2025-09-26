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
            this.Text = "Задача";
            this.Width = 400;
            this.Height = 250;

            Label lblTitle = new Label { Left = 10, Top = 10, Text = "Название:" };
            txtTitle = new TextBox { Left = 100, Top = 10, Width = 250 };

            Label lblPriority = new Label { Left = 10, Top = 40, Text = "Приоритет:" };
            cbPriority = new ComboBox { Left = 100, Top = 40, Width = 150 };
            cbPriority.Items.AddRange(Enum.GetNames(typeof(PriorityLevel)));

            Label lblCategory = new Label { Left = 10, Top = 70, Text = "Категория:" };
            txtCategory = new TextBox { Left = 100, Top = 70, Width = 250 };

            Label lblDueDate = new Label { Left = 10, Top = 100, Text = "Срок:" };
            dtpDueDate = new DateTimePicker { Left = 100, Top = 100, Width = 200 };
            chkNoDate = new CheckBox { Left = 310, Top = 100, Text = "Без даты" };

            chkDone = new CheckBox { Left = 100, Top = 130, Text = "Выполнена" };

            btnOk = new Button { Text = "ОК", Left = 200, Top = 170, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Отмена", Left = 280, Top = 170, DialogResult = DialogResult.Cancel };

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
        private Button btnAdd, btnEdit, btnDelete, btnToggleDone;
        private TaskRepository repo;

        public MainForm()
        {
            this.Text = "Todo List";
            this.Width = 900;
            this.Height = 600;

            repo = new TaskRepository("tasks.json");

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

            btnAdd = new Button { Text = "Добавить", Left = 10, Top = 520 };
            btnEdit = new Button { Text = "Редактировать", Left = 110, Top = 520 };
            btnDelete = new Button { Text = "Удалить", Left = 230, Top = 520 };
            btnToggleDone = new Button { Text = "Отметить/Снять", Left = 330, Top = 520 };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnToggleDone.Click += BtnToggleDone_Click;

            this.Controls.Add(lvTasks);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnToggleDone);

            RefreshTasks();
        }

        private void RefreshTasks()
        {
            lvTasks.Items.Clear();
            foreach (var task in repo.Tasks)
            {
                var lvi = new ListViewItem(task.Title);
                lvi.SubItems.Add(task.Priority.ToString());
                lvi.SubItems.Add(task.Category);
                lvi.SubItems.Add(task.DueDate?.ToShortDateString() ?? "-");
                lvi.SubItems.Add(task.IsDone ? "Да" : "Нет");
                lvi.Tag = task;
                lvTasks.Items.Add(lvi);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new TaskForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                repo.Add(form.Task);
                RefreshTasks();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0) return;
            var task = (TaskItem)lvTasks.SelectedItems[0].Tag;
            var form = new TaskForm(task);
            if (form.ShowDialog() == DialogResult.OK)
            {
                repo.Update(form.Task);
                RefreshTasks();
            }
        }
            private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0) return;
            var task = (TaskItem)lvTasks.SelectedItems[0].Tag;
            repo.Remove(task);
            RefreshTasks();
        }

        private void BtnToggleDone_Click(object sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0) return;
            var task = (TaskItem)lvTasks.SelectedItems[0].Tag;
            task.IsDone = !task.IsDone;
            repo.Update(task);
            RefreshTasks();
        }
    }
}
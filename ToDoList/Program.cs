using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        public override string ToString()
        {
            return $"{Title} ({Category}) - {Priority} - {(DueDate.HasValue ? DueDate.Value.ToString("yyyy-MM-dd") : "—")} - {(IsDone ? "?" : " ")}";
        }
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
        private Button btnOk;
        private Button btnCancel;

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
            LoadTaskToForm();
        }

        private void InitializeComponent()
        {
            this.Text = "Задача";
            this.Width = 420;
            this.Height = 260;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblTitle = new Label { Text = "Название:", Left = 10, Top = 15, Width = 80 };
            txtTitle = new TextBox { Left = 100, Top = 12, Width = 290 };

            var lblPriority = new Label { Text = "Приоритет:", Left = 10, Top = 50, Width = 80 };
            cbPriority = new ComboBox { Left = 100, Top = 47, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbPriority.

            Items.AddRange(Enum.GetNames(typeof(PriorityLevel)));

            var lblCategory = new Label { Text = "Категория:", Left = 10, Top = 85, Width = 80 };
            txtCategory = new TextBox { Left = 100, Top = 82, Width = 290 };

            var lblDue = new Label { Text = "Срок (дата):", Left = 10, Top = 120, Width = 80 };
            dtpDueDate = new DateTimePicker { Left = 100, Top = 117, Width = 200, Format = DateTimePickerFormat.Short };
            chkNoDate = new CheckBox { Left = 310, Top = 119, Width = 80, Text = "Без даты" };
            chkNoDate.CheckedChanged += (s, e) => dtpDueDate.Enabled = !chkNoDate.Checked;

            chkDone = new CheckBox { Left = 100, Top = 150, Width = 120, Text = "Выполнена" };

            btnOk = new Button { Text = "ОК", Left = 210, Width = 80, Top = 180, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Отмена", Left = 300, Width = 80, Top = 180, DialogResult = DialogResult.Cancel };

            btnOk.Click += BtnOk_Click;

            this.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblPriority, cbPriority, lblCategory, txtCategory, lblDue, dtpDueDate, chkNoDate, chkDone, btnOk, btnCancel });
        }

        private void LoadTaskToForm()
        {
            txtTitle.Text = Task.Title;
            cbPriority.SelectedIndex = (int)Task.Priority;
            txtCategory.Text = Task.Category;
            if (Task.DueDate.HasValue)
            {
                dtpDueDate.Value = Task.DueDate.Value;
                chkNoDate.Checked = false;
                dtpDueDate.Enabled = true;
            }
            else
            {
                chkNoDate.Checked = true;
                dtpDueDate.Enabled = false;
            }
            chkDone.Checked = Task.IsDone;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            Task.Title = txtTitle.Text.Trim();
            Task.Priority = (PriorityLevel)cbPriority.SelectedIndex;
            Task.Category = txtCategory.Text.Trim();
            Task.DueDate = chkNoDate.Checked ? null : dtpDueDate.Value.Date;
            Task.IsDone = chkDone.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public class MainForm : Form
    {
        private ListView lvTasks;
        private Button btnAdd, btnEdit, btnDelete, btnToggleDone;
        private ComboBox cbSortBy;
        private TaskRepository repo;
        private string dataFile = "tasks.json";

        public MainForm()
        {
            repo = new TaskRepository(dataFile);
            InitializeComponent();
            RefreshList();
        }

        private void InitializeComponent()
        {
            this.Text = "Todo List";
            this.Width = 900;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            lvTasks = new ListView
            {
                Left = 10,
                Top = 10,
                Width = 860,
                Height = 460,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvTasks.Columns.Add("Название", 300);
            lvTasks.Columns.Add("Приоритет", 90);
            lvTasks.Columns.Add("Категория", 150);
            lvTasks.Columns.Add("Срок", 120);
            lvTasks.Columns.Add("Выполнена", 80);

            btnAdd = new Button { Text = "Добавить", Left = 10, Top = 480, Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Left = 120, Top = 480, Width = 120 };
            btnDelete = new Button { Text = "Удалить", Left = 250, Top = 480, Width = 100 };

            btnToggleDone = new Button { Text = "Отметить", Left = 360, Top = 480, Width = 130 };

            var lblSort = new Label { Text = "Сортировать по:", Left = 510, Top = 485, Width = 100 };
            cbSortBy = new ComboBox { Left = 620, Top = 482, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbSortBy.Items.AddRange(new string[] { "Название", "Приоритет", "Категория", "Срок", "Выполнена" });
            cbSortBy.SelectedIndexChanged += (s, e) => { SortAndRefresh(); };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnToggleDone.Click += BtnToggleDone_Click;

            this.Controls.AddRange(new Control[] { lvTasks, btnAdd, btnEdit, btnDelete, btnToggleDone, lblSort, cbSortBy });
        }

        private void RefreshList()
        {
            lvTasks.Items.Clear();
            foreach (var t in repo.Tasks)
            {
                var lvi = new ListViewItem(t.Title);
                lvi.SubItems.Add(t.Priority.ToString());
                lvi.SubItems.Add(t.Category);
                lvi.SubItems.Add(t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd") : "");
                lvi.SubItems.Add(t.IsDone ? "Да" : "Нет");
                lvi.Tag = t;
                lvTasks.Items.Add(lvi);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var f = new TaskForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                repo.Add(f.Task);
                RefreshList();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0) { MessageBox.Show("Выберите задачу."); return; }
            var selected = (TaskItem)lvTasks.SelectedItems[0].Tag;
            using var f = new TaskForm(selected);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                repo.Update(f.Task);
                RefreshList();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0) { MessageBox.Show("Выберите задачу."); return; }
            var selected = (TaskItem)lvTasks.SelectedItems[0].Tag;
            var res = MessageBox.Show($"Удалить задачу: \"{selected.Title}\"?", "Подтвердите", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                repo.Remove(selected);
                RefreshList();
            }
        }

        private void BtnToggleDone_Click(object? sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count == 0) { MessageBox.Show("Выберите задачу."); return; }
            var selected = (TaskItem)lvTasks.SelectedItems[0].Tag;
            selected.IsDone = !selected.IsDone;
            repo.Update(selected);
            RefreshList();
        }

        private void SortAndRefresh()
        {
            var key = cbSortBy.SelectedIndex;
            if (key < 0) return;

            IEnumerable<TaskItem> sorted = repo.Tasks;
            switch (key)
            {
                case 0: sorted = repo.Tasks.OrderBy(t => t.Title, StringComparer.OrdinalIgnoreCase); break;
                case 1: sorted = repo.Tasks.OrderBy(t => (int)t.Priority); break;
                case 2: sorted = repo.Tasks.OrderBy(t => t.Category ?? "", StringComparer.OrdinalIgnoreCase); break;
                case 3: sorted = repo.Tasks.OrderBy(t => t.DueDate.HasValue ? 0 : 1).ThenBy(t => t.DueDate ?? DateTime.MaxValue); break;
                case 4: sorted = repo.Tasks.OrderBy(t => t.IsDone); break;
            }

            repo.Tasks = sorted.ToList();
            repo.Save();
            RefreshList();
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
}
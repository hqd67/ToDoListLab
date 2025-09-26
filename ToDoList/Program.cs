using System;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace migrarAcc
{
    internal class Prompt : IDisposable
    {
        private Form prompt { get; set; }
        public string Result { get; }

        public Prompt(string caption)
        {
            Result = ShowDialog(caption);
        }

        private string ShowDialog(string caption)
        {
            prompt = new Form()
            {
                Width = 350,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                TopMost= true
            };
            TextBox textBox = new TextBox() { Left = 40, Top = 30, Width = 250, PasswordChar = '*', MaxLength = 15 };
            Button confirmation = new Button() { Text = "Aceptar", Left = 40, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancelar", Left =190, Width = 100, Top = 80, DialogResult = DialogResult.Cancel };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        public void Dispose()
        {
            if (prompt != null) { prompt.Dispose(); }
        }
    }
}

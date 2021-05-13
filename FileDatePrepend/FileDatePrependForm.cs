using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileDatePrepend
{
    public partial class FileDatePrependForm : Form
    {
        private readonly string stateFile;

        public FileDatePrependForm()
        {
            InitializeComponent();
            string dir = new FileInfo(GetType().Assembly.Location).DirectoryName;
            stateFile = Path.Combine(dir, "filedate.state");
            groupBox1.AllowDrop = true;
            groupBox2.AllowDrop = true;
            groupBox1.DragEnter += GroupBox2_DragEnter;
            groupBox1.DragDrop += GroupBox2_DragDrop;
            groupBox2.DragEnter += GroupBox2_DragEnter;
            groupBox2.DragDrop += GroupBox2_DragDrop;
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            FormClosing += FileDatePrependForm_FormClosing;
            if (File.Exists(stateFile))
            {
                string[] l = File.ReadAllLines(stateFile);
                l.OrderBy(i => i).ToList().ForEach(i => listBox1.Items.Add(i));
            }
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                textBox1.Text = listBox1.SelectedItem as string;
            }
        }

        private void FileDatePrependForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StringBuilder b = new StringBuilder();
            foreach (var i in listBox1.Items)
            {
                b.AppendLine(i.ToString());
            }
            File.WriteAllText(stateFile, b.ToString());
        }

        private void GroupBox2_DragDrop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.FileDrop);
            if (data != null)
            {
                DateTime now = DateTime.Now;
                if ((sender as GroupBox).Text == "yesterday")
                {
                    now = now.AddDays(-1);
                }
                string add = now.ToString("yyyy MM dd");
                string highlight = ishighlight.Checked ? "highlight " : "";


                string[] files = (string[])data;
                Regex guidRemove = new Regex("([a-z0-9-])+");
                foreach (string f in files)
                {
                    FileInfo fi = new FileInfo(f);
                    string fname = fi.Name.Replace(fi.Extension, "");
                    Match col = guidRemove.Match(fname);
                    fname = fname.Replace(col.Value, "");


                    string destFileName = Path.Combine(fi.DirectoryName, $"{add} {highlight}{listBox1.SelectedItem} {fname}{fi.Extension}".Replace("  ", " ").Replace(" .", "."));
                    int part = 1;
                    while (File.Exists(destFileName))
                    {
                        destFileName = Path.Combine(fi.DirectoryName, $"{add} {listBox1.SelectedItem} part{part} {fname}{fi.Extension}".Replace("  ", " "));
                        part++;
                    }
                    File.Move(f, destFileName, false);
                }

            }
        }

        private void GroupBox2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void add_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox1.Text);
        }

        private void delete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void replace_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                List<string> temp = new List<string>();
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    string insert = listBox1.Items[i] as string;
                    if (i == listBox1.SelectedIndex)
                    {
                        insert = textBox1.Text;
                    }
                    temp.Add(insert);
                }
                listBox1.Items.Clear();
                temp.ForEach(i => listBox1.Items.Add(i));
            }
        }
    }
}

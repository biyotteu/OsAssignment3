using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OsAssignment3
{
    public partial class Form2 : Form
    {
        public int threadCount = 0;
        public List<String> files = new List<String>();
        public List<String> malwares = new List<String>();

        private int minThreadCount = 0;

        public Form2()
        {
            InitializeComponent();
            openFileDialog1.Multiselect = true;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            threadCount = Int32.Parse(textBox1.Text);
            if(minThreadCount > 0 && threadCount >= minThreadCount)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("쓰레드 수를 확인해주세요!");
            }
        }

        private void malwareListLoadButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                malwares.Clear();
                foreach (String file in openFileDialog1.FileNames)
                {
                    malwares.Add(file);
                }
                render();
            }
        }

        private void fileLoadButton_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                files.Clear();
                foreach (String file in openFileDialog1.FileNames)
                {
                    files.Add(file);
                }
                render();
                calcThreadCount();
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void render()
        {
            fileListView.Columns[0].Width = fileListView.Width - 10;
            malwareListView.Columns[0].Width = malwareListView.Width - 10;
            fileListView.Items.Clear();
            malwareListView.Items.Clear();

            foreach (String file in files)
            {
                var tmp = file.Split('\\');
                ListViewItem item = new ListViewItem(tmp[tmp.Length-1]);
                fileListView.Items.Add(item);
            }

            foreach (String malware in malwares)
            {
                var tmp = malware.Split('\\');
                ListViewItem item = new ListViewItem(tmp[tmp.Length - 1].Replace(".txt",""));
                item.ForeColor = Color.Red;
                malwareListView.Items.Add(item);
            }
        }

        private void calcThreadCount()
        {
            long sum = 0;
            foreach(String file in files)
            {
                /*if(new System.IO.FileInfo(file).Length != File.ReadAllText(file).Length)
                {
                    Debug.WriteLine(file);
                }*/
                sum += new System.IO.FileInfo(file).Length;
                /*sum += File.ReadAllText(file).Length;*/
            }
            this.minThreadCount = (int)((sum+99)/100);
            totalStringLabel.Text = sum.ToString();
            minThreadCountLabel.Text = ((sum + 99) / 100).ToString();
        }
    }
}

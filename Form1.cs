using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace OsAssignment3
{
    public partial class Form1 : Form
    {
        private static List<String> files = new List<String>();
        private static List<String> malwares = new List<String>();
        private int threadCount = 0;
        private static List<Label> threadLabels = new List<Label>();
        private static List<ProgressBar> progressBars = new List<ProgressBar>();
        private static List<List<String>> isMalwareFile = new List<List<String>>();
        private static List<object> fileObjs = new List<object>();
        private static List<object> malwareObjs = new List<object>();
        private static List<bool> isComplete = new List<bool>();
        private static List<bool> isLock = new List<bool>();
        private List<Thread> threads = new List<Thread>();
        private static int count;
        private static object countObj = new object();
        private static Queue<int> queue = new Queue<int>();
        private static List<int> tmp =  new List<int>();
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            benignListView.Columns.Add("");
            benignListView.Columns.Add("");
            benignListView.Columns.Add("");
        }

        class ThreadLabel:Label
        {
            public ThreadLabel()
            {
                this.BackColor = Color.Yellow;
                this.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                this.TextAlign = ContentAlignment.MiddleCenter;
                this.Height = 30;
            }
            public ThreadLabel(String text):this()
            {
                this.Text = text;
            }
        }
        
        private void settingButton_Click(object sender, EventArgs e)
        {
            Form2 settingForm = new Form2();
            if(settingForm.ShowDialog() == DialogResult.OK)
            {
                files = settingForm.files;
                malwares = settingForm.malwares;
                threadCount = settingForm.threadCount;
                render();
            }
        }

        private void render()
        {
            fileListView.Columns[0].Width = fileListView.Width - 6;
            malwareListView.Columns[0].Width = malwareListView.Width - 6;
            fileListView.Items.Clear();
            malwareListView.Items.Clear();

            foreach (String file in files)
            {
                var tmp = file.Split('\\');
                ListViewItem item = new ListViewItem(tmp[tmp.Length - 1]);
                Debug.WriteLine(File.ReadAllText(file).Length.ToString());
                fileListView.Items.Add(item);
            }

            foreach (String malware in malwares)
            {
                var tmp = malware.Split('\\');
                ListViewItem item = new ListViewItem(tmp[tmp.Length - 1].Replace(".txt", ""));
                item.ForeColor = Color.Red;
                malwareListView.Items.Add(item);
            }
        }

        private void initThreadStatusListView()
        {
            threadLabels.Clear();
            threadStatusPanel.Controls.Clear();
            for (int i = 1; i <= (threadCount + 1) / 2; i++)
            {
                TableLayoutPanel container = new TableLayoutPanel();
                container.Width = threadStatusPanel.Width - 6;
                container.RowCount = 1;
                container.ColumnCount = 2;
                container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                container.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
                container.Height = (int)container.RowStyles[0].Height;
                ThreadLabel lbl = new ThreadLabel((i * 2 - 1).ToString());
                container.Controls.Add(lbl);
                threadLabels.Add(lbl);
                if (i != (threadCount + 1) / 2 || threadCount % 2 != 1)
                {
                    ThreadLabel lbl2 = new ThreadLabel((i * 2).ToString());
                    container.Controls.Add(lbl2);
                    threadLabels.Add((lbl2));
                }
                threadStatusPanel.Controls.Add(container);
            }
        }

        private void initProgressListView()
        {
            progressBars.Clear();
            progressListView.Controls.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                TableLayoutPanel container = new TableLayoutPanel();
                container.Width = progressListView.Width - 6;
                container.RowCount = 1;
                container.ColumnCount = 2;
                container.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                container.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
                container.Height = (int)container.RowStyles[0].Height;
                var tmp = files[i].Split('\\');
                ThreadLabel lbl = new ThreadLabel(tmp[tmp.Length - 1]);
                lbl.BackColor = Color.White;
                container.Controls.Add(lbl);
                ProgressBar progress = new ProgressBar();
                progress.Value = 0;
                progress.Minimum = 0;
                Debug.WriteLine(new FileInfo(files[i]).Length.ToString()+"~~~~~~~");
                progress.Maximum = (int)new FileInfo(files[i]).Length;
                progress.Dock = DockStyle.Fill;
                progress.Step = 1;
                progressBars.Add(progress);
                container.Controls.Add(progress);
                progressListView.Controls.Add(container);
            }
        }

        private static void process(int threadID)
        {
            int i = 0,j, n = files.Count;
            int limit = 100;
            do
            {
                while (true)
                {
                    lock (countObj)
                    {
                        if (count == 0) break;
                    }
                    try {
                        i = queue.Dequeue();
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }
                if (count == 0) break;

                //critical section
                lock (fileObjs[i])
                {
                    threadLabels[threadID].BackColor = Color.Green;
                    int startIndex = progressBars[i].Value;
                    int endIndex = progressBars[i].Maximum < startIndex + limit ? progressBars[i].Maximum : startIndex + limit;
                    string text = File.ReadAllText(files[i]);
                    for (j = 0; j < malwares.Count; j++)
                    {
                        string v = malwares[j];
                        string mal;
                        bool isMalware = false;
                        lock (malwareObjs[j])
                        {
                            mal = File.ReadAllText(malwares[j]);
                        }
                        for (int p = startIndex; p < endIndex; p++)
                        {
                            bool cmp = true;
                            for (int q = 0; q < mal.Length; q++)
                            {
                                if (p + q >= text.Length || text[p + q] != mal[q])
                                {
                                    cmp = false;
                                    break;
                                }
                            }
                            if (cmp)
                            {
                                isMalware = true;
                                break;
                            }
                        }
                        if (isMalware)
                        {
                            isMalwareFile[i].Add(malwares[j]);
                        }
                    }
                    tmp[threadID] -= (endIndex - startIndex);
                    limit -= (endIndex - startIndex);
                    progressBars[i].Value = endIndex;
                    Thread.Sleep(100);
                    Debug.WriteLine(i.ToString()+"      "+startIndex .ToString() + "      " + progressBars[i].Value.ToString() + "      " + progressBars[i].Maximum.ToString() +"        "+ (progressBars[i].Value - startIndex).ToString()+ "@@@@@@\n");
                    if (progressBars[i].Value < progressBars[i].Maximum)
                    {
                        queue.Enqueue(i);
                        break;
                    }
                    else
                    {
                        lock (countObj)
                        {
                            count--;
                            if (count == 0) break;
                        }
                    }
                }
            } while (limit > 0);
            threadLabels[threadID].BackColor = Color.Gray;
        }

        private void initThreads()
        {
            threads.Clear();
            for(int i = 0; i < threadCount; i++)
            {
                int idx = i;
                threads.Add(new Thread(() => process(idx)));
                tmp.Add(100);
            }
        }
        private void readyButton_Click(object sender, EventArgs e)
        {
            initThreadStatusListView();
            initProgressListView();
            initThreads();
            isLock.Clear();
            isComplete.Clear();
            isMalwareFile.Clear();
            fileObjs.Clear();

            count = files.Count;
            while (queue.Count > 0) queue.Dequeue();

            for(int i = 0; i < files.Count; i++)
            {
                queue.Enqueue(i);
                isLock.Add(false);
                isComplete.Add(false);
                isMalwareFile.Add(new List<string>());
            }
            
            for(int i=0;i<files.Count;i++) fileObjs.Add(new object());
            malwareObjs.Clear();
            for(int i=0;i<malwares.Count;i++) malwareObjs.Add(new object());
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            foreach(Thread t in threads)
                t.Start();
            foreach(Thread t in threads)
                t.Join();

            for(int i = 0; i < tmp.Count; i++)
            {
                Debug.WriteLine(tmp[i].ToString() + "   " + i.ToString() + "!!!!!");
            }
            Debug.WriteLine(queue.Count.ToString() + "   " +count.ToString() + "#@%$!%$##@$");

            for (int i = 0; i < 3; i++)
            {
                benignListView.Columns[i].Width = (benignListView.Width - 10) / 3;
            }
            benignListView.Items.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                if (isMalwareFile[i].Count > 0) continue;
                ListViewItem item = new ListViewItem();
                var tmp = files[i].Split('\\');
                item.SubItems[0].Text = tmp[tmp.Length - 1];
                item.SubItems.Add("→");
                item.SubItems.Add("safe");
                item.SubItems[2].ForeColor = Color.Green;
                benignListView.Items.Add(item);
                benignListView.Items[benignListView.Items.Count - 1].UseItemStyleForSubItems = false;
            }

            isMalwareView.Controls.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                if (isMalwareFile[i].Count == 0) continue;
                FlowLayoutPanel container = new FlowLayoutPanel();
                container.FlowDirection = FlowDirection.LeftToRight;
                container.AutoSize = true;
                container.WrapContents = true;
                
                var tmp = files[i].Split('\\');
                Label fileLabel = new Label();
                fileLabel.Text = tmp[tmp.Length - 1];
                fileLabel.TextAlign = ContentAlignment.MiddleCenter;
                fileLabel.Width = 50;
                container.Controls.Add(fileLabel);

                foreach(String mal in isMalwareFile[i])
                {
                    var tmp2 = mal.Split('\\');
                    Label malLabel = new Label();
                    malLabel.Text = tmp2[tmp2.Length - 1].Replace(".txt", "");
                    malLabel.BackColor = Color.Red;
                    malLabel.TextAlign = ContentAlignment.MiddleCenter;
                    malLabel.Width = 50;
                    container.Controls.Add(malLabel);
                }
                isMalwareView.Controls.Add(container);
            }
        }
    }
}

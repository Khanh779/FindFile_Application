using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace FindFile_Application
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Enum.GetNames(typeof(View)).ToList().ForEach(x => comboBox1.Items.Add(x));
            comboBox1.Text = listView1.View.ToString();
            listView1.ItemChecked += ListView1_ItemChecked; ;
        }

        private void ListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            label4.Text = "Number of Item: " + listView1.Items.Count.ToString() + "/"+d+", Checked: " + listView1.CheckedItems.Count.ToString();
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
          
            foreach (ColumnHeader item in listView1.Columns)
            {
                item.Width = listView1.Width / listView1.Columns.Count - 2;
            };
            base.OnClientSizeChanged(e);
        }

        string[] columns = new string[] { "Name", "Path", "Type", "Size", "Creation Time" };

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderBrowserDialog1.SelectedPath);
                    textBox3.Text = directoryInfo.FullName;
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            d = countFiles(new DirectoryInfo(textBox3.Text+"\\").FullName);
            MessageBox.Show("The number of files is " + d.ToString());
            button2.Enabled = true;
            findFiles(textBox3.Text, textBox2.Text);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            label5.Invoke((Action)delegate { label5.Text = "Status: Comppleted (" + progressBar1.Value + "%)"; });
            label4.Text = "Number of files: " + listView1.Items.Count.ToString()+"/ "+ d;
            MessageBox.Show("The operation is completed");
        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int a = (int)numericUpDown1.Value;
                imageList1.ImageSize = new Size(a, a);
                if (backgroundWorker1.IsBusy == false)
                {
                    listView1.Items.Clear();
                    imageList1.Images.Clear();
                    progressBar1.Value = 0;
                    string d = "\nYou can use the following extensions: \n*.txt, *.docx, *.pdf, *.jpg, ...\nOr for all files: *.* or * ";

                    if(textBox2.Text.Contains(","))
                    {
                        foreach (string x in textBox2.Text.Split(','))
                        {
                            string y = x.Trim();
                            if( !y.Contains(".") || y==String.Empty || y==" " || y=="" ) 
                            {
                                MessageBox.Show("The path is not valid"+d, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }    
                    else
                    {
                        if ( !textBox2.Text.Contains("*.*") || !textBox2.Text.Contains("*"))
                        {
                            MessageBox.Show("The extension is not valid" + d, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    MessageBox.Show("The operation is started");
                    button2.Enabled = false;
                    backgroundWorker1.RunWorkerAsync();
                    return;
                }
                if (backgroundWorker1.IsBusy == true)
                {
                    MessageBox.Show("The operation is stopped");
           
                    backgroundWorker1.CancelAsync();
                    return;
                }

                if(backgroundWorker2.IsBusy==true)
                {
                    MessageBox.Show("The operation is stopped");
                    button2.Enabled = false;
                    backgroundWorker2.CancelAsync();
                    return;
                }

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.View = (View)Enum.Parse(typeof(View), comboBox1.SelectedItem.ToString());
            if (listView1.View == View.Details)
            {
                listView1.Columns.Clear();
                columns.ToList().ForEach(x => listView1.Columns.Add(x));
                foreach (ColumnHeader item in listView1.Columns)
                {
                    item.Width = listView1.Width / listView1.Columns.Count - 2;
                };
            }
        }

        int d = 0;

        int countFiles(string path)
        {
            int a = 0;
            try
            {
                foreach(var file in System.IO.Directory.GetFiles(path))
                {
                    try
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(file);
                        if(textBox2.Text.Contains(","))
                        {
                            textBox2.Text.Split(',').ToList().ForEach(x =>
                            {
                                var y = x.Split('.');
                                string b = fi.Extension.Replace(".", "");
                                if ((y[0].Contains(fi.Name) || y[0].Contains("*") || textBox2.Text == " " || textBox2.Text == String.Empty) && (y[1].Contains(b.Replace(".", ""))))
                                    a++;
                            });
                        }    
                        else
                        if (textBox2.Text == "*.*" || textBox2.Text == "*" || textBox2.Text == String.Empty)
                            a++;
                       
                   
                    }
                    catch { }

                    if (backgroundWorker1.CancellationPending == true) break;
                }    
                foreach (string dir in System.IO.Directory.GetDirectories(path))
                {
                   
                    try
                    {
                        a += countFiles(dir);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("ErrorLog.txt", "count: " + ex.ToString());
            }
            return a;
        }

        void findFiles(string path, string pattern)
        {
            try
            {
                string[] files = System.IO.Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (backgroundWorker1.CancellationPending == true) break;
                    if (progressBar1.Value <= 98)
                        backgroundWorker1.ReportProgress((int)(listView1.Items.Count * 98 / d));
                    label5.Invoke((Action)delegate { label5.Text = "Status: Scanning... ("+listView1.Items.Count+"/"+d+" - " + progressBar1.Value + "%)"; });
                    try
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(file);
                        if(textBox2.Text.Contains(",")&& textBox2.Text.Contains("."))
                        {
                            foreach (string x in textBox2.Text.Split(','))
                            {
                                var y = x.Split('.');
                                string b = fi.Extension;
                                if ((y[0].Contains(fi.Name) || y[0].Contains("*") || textBox2.Text == " ") && (y[1].Contains(b.Replace(".", ""))))
                                {
                                    addFile(fi);

                                }
                            }
                        }    
                        else if (textBox2.Text == "*.*" || textBox2.Text == "*" || textBox2.Text == String.Empty)
                        {
                            //addFile(fi);
                        }
                       
                    
                    }
                    catch { }
                  

                }

                foreach (string dir in System.IO.Directory.GetDirectories(path))
                {
                    if (backgroundWorker1.CancellationPending == true) break;
                    try
                    {
                        findFiles(dir, pattern);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("ErrorLog.txt", "file: " + ex.ToString());
            }
        }

        void addFile(System.IO.FileInfo fi)
        {
            listView1.Invoke((Action)delegate
            {
                ListViewItem lvi = new ListViewItem(fi.Name, fi.FullName);
                Bitmap bitImage = null;
                try
                {
                    bitImage = new Bitmap(fi.FullName);
                }
                catch
                {
                    bitImage = Icon.ExtractAssociatedIcon(fi.FullName).ToBitmap();
                }

                imageList1.Images.Add(fi.FullName, bitImage);


                lvi.SubItems.Add(fi.DirectoryName);
                lvi.SubItems.Add(fi.Extension);
                lvi.SubItems.Add(fi.Length.ToString());
                lvi.SubItems.Add(fi.CreationTime.ToString());
                if(listView1.Items.Contains(lvi)==false)
                    listView1.Items.Add(lvi);
            });
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem sel in listView1.SelectedItems)
            {
                string x = sel.SubItems[1].Text + "\\" + sel.Text;
                System.Diagnostics.Process.Start(x);
            }
        }

        private void openLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.CheckedItems.Count > 0)
                foreach (ListViewItem sel in listView1.CheckedItems)
                {

                    string x = sel.SubItems[1].Text + "\\";
                    System.Diagnostics.Process.Start(x);
                }
        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            listView1.LargeImageList = listView1.SmallImageList = imageList1;
            int a = (int)numericUpDown1.Value;
            foreach (ListViewItem item in listView1.Items)
            {
                item.ImageList.ImageSize = new Size(a, a);
                item.ImageKey = item.SubItems[1] + "\\" + item.Text;
            }
        }

        private void moveToDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.CheckedItems.Count > 0 && MessageBox.Show("Do you want to move " + listView1.CheckedItems.Count +
             " file(s)?", "Move", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
                {
                    backgroundWorker2.RunWorkerAsync();
                }
        } 

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < listView1.CheckedItems.Count; i++)
            {
                label5.Invoke((Action)delegate { label5.Text = "Status: Moving... (" + progressBar1.Value + "%)"; });
                ListViewItem sel = listView1.CheckedItems[i];
                try
                {
                    string x = new FileInfo(sel.SubItems[1].Text + "\\" + sel.Text).FullName;
                    System.IO.File.Move(x, folderBrowserDialog2.SelectedPath + "\\" + sel.Text);
                 
                    listView1.Items.Remove(sel);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText("ErrorMoveLog.txt", "file: " + ex.ToString());
                }
                backgroundWorker2.ReportProgress(i * 98 / listView1.CheckedItems.Count);
                if (backgroundWorker2.CancellationPending == true) break;
           
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value=e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100 ;
            label5.Invoke((Action)delegate { label5.Text = "Status: Compeleted (" + progressBar1.Value + "%)"; });
        }

        
        
    }
}

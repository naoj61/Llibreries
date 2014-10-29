using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public partial class FileBrowser : UserControl
    {
        // Counters for statusBar1 control
        private int iFiles = 0;
        private int iDirectories = 0;

        // Windows Designer variables

        private System.Windows.Forms.StatusBar statusBar1;

        public FileBrowser()
        {
            InitializeComponent();
        }


        // Methods
        public void AddDirectories(TreeNode tnSubNode)
        {
            // This method is used to get directories (from disks, or from other directories)

            treeView1.BeginUpdate();
            iDirectories = 0;

            try
            {
                DirectoryInfo diRoot;

                // If drive, get directories from drives
                if (tnSubNode.SelectedImageIndex < 11)
                {
                    diRoot = new DirectoryInfo(tnSubNode.FullPath + "\\");
                }

                //  Else, get directories from directories
                else
                {
                    diRoot = new DirectoryInfo(tnSubNode.FullPath);
                }
                DirectoryInfo[] dirs = diRoot.GetDirectories();

                // Must clear this first, else the directories will get duplicated in treeview
                tnSubNode.Nodes.Clear();

                // Add the sub directories to the treeView1
                foreach (DirectoryInfo dir in dirs)
                {
                    iDirectories++;
                    TreeNode subNode = new TreeNode(dir.Name);
                    subNode.ImageIndex = 11;
                    subNode.SelectedImageIndex = 12;
                    tnSubNode.Nodes.Add(subNode);
                }

            }
            // Throw Exception when accessing directory: C:\System Volume Information	 // do nothing
            catch { ;	}

            treeView1.EndUpdate();
        }

        public void AddFiles(string strPath)
        {
            listView1.BeginUpdate();

            listView1.Items.Clear();
            iFiles = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(strPath + "\\");
                FileInfo[] theFiles = di.GetFiles();
                foreach (FileInfo theFile in theFiles)
                {
                    iFiles++;
                    ListViewItem lvItem = new ListViewItem(theFile.Name);
                    lvItem.SubItems.Add(theFile.Length.ToString());
                    lvItem.SubItems.Add(theFile.LastWriteTime.ToShortDateString());
                    lvItem.SubItems.Add(theFile.LastWriteTime.ToShortTimeString());
                    listView1.Items.Add(lvItem);
                }
            }
            catch (Exception Exc) { statusBar1.Text = Exc.ToString(); }

            listView1.EndUpdate();
        }

        public void Form1_Load()
        {
            // This routine adds all computer drives to the root nodes of treeView1 control

            string[] aDrives = Environment.GetLogicalDrives();

            treeView1.BeginUpdate();

            foreach (string strDrive in aDrives)
            {
                TreeNode dnMyDrives = new TreeNode(strDrive.Remove(2, 1));

                switch (strDrive)
                {
                    case "A:\\":
                        dnMyDrives.SelectedImageIndex = 0;
                        dnMyDrives.ImageIndex = 0;
                        break;
                    case "C:\\":

                        // The next statement causes the treeView1_AfterSelect Event to fire once on startup.
                        // This effect can be seen just after intial program load. C:\ node is selected
                        // Automatically on program load, expanding the C:\ treeView1 node.
                        treeView1.SelectedNode = dnMyDrives;
                        //dnMyDrives.SelectedImageIndex = 1;
                        //dnMyDrives.ImageIndex = 1;

                        break;
                    case "D:\\":
                        dnMyDrives.SelectedImageIndex = 2;
                        dnMyDrives.ImageIndex = 2;
                        break;
                    default:
                        dnMyDrives.SelectedImageIndex = 3;
                        dnMyDrives.ImageIndex = 3;
                        break;
                }

                treeView1.Nodes.Add(dnMyDrives);
            }
            treeView1.EndUpdate();
        }

        private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            // Get subdirectories from disk, add to treeView1 control
            AddDirectories(e.Node);

            // if node is collapsed, expand it. This allows single click to open folders.
            treeView1.SelectedNode.Expand();

            // Get files from disk, add to listView1 control
            AddFiles(e.Node.FullPath.ToString());
            statusBar1.Text = iDirectories.ToString() + " Folder(s)  " + iFiles.ToString() + " File(s)";
        }


        private void listView1_ItemActivate(object sender, System.EventArgs e)
        {
            try
            {
                string sPath = treeView1.SelectedNode.FullPath;
                string sFileName = listView1.FocusedItem.Text;

                Process.Start(sPath + "\\" + sFileName);
            }
            catch (Exception Exc) { MessageBox.Show(Exc.ToString()); }
        }

    }
}

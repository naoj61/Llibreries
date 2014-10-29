using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public partial class FileBrowser2 : UserControl
    {

        public event EventHandler FitxerClick;
        public event EventHandler FitxerDobleClick;

        public FileBrowser2()
        {
            InitializeComponent();
            PopulateTreeView();
        }

        private DirectoryInfo vDirectoriArrel = new DirectoryInfo(@"C:\");

        public DirectoryInfo _DirectoriArrel
        {
            get { return vDirectoriArrel; }
            set { vDirectoriArrel = value; }
        }

        private FileInfo vFitxeriSeleccionat;

        public FileInfo _FitxerSeleccionat
        {
            get { return vFitxeriSeleccionat; }
        }

        public void directoriArrel(DirectoryInfo directori, bool expand)
        {
            _DirectoriArrel = directori;
            PopulateTreeView(expand);
        }

        private void PopulateTreeView(bool expand = false)
        {
            if (_DirectoriArrel.Exists)
            {
                TreeNode rootNode = new TreeNode(_DirectoriArrel.Name);
                rootNode.Tag = _DirectoriArrel;
                GetDirectories(_DirectoriArrel.GetDirectories(), rootNode);
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(rootNode);
                if (expand)
                {
                    seleccionaNode(rootNode, true);
                }
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                try
                {
                    aNode = new TreeNode(subDir.Name, 0, 0);
                    aNode.Tag = subDir;
                    aNode.ImageKey = "folder";
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0 && subDirs[0].Parent != null && subDirs[0].Parent.Parent != null)
                    {
                        GetDirectories(subSubDirs, aNode);
                    }
                    nodeToAddTo.Nodes.Add(aNode);
                }
                catch (System.UnauthorizedAccessException)
                {
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            seleccionaNode(e.Node);
        }

        private void seleccionaNode(TreeNode treeNode, bool expand = false)
        {
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo) treeNode.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new[] {new ListViewItem.ListViewSubItem(item, "Directory"), new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new[] {new ListViewItem.ListViewSubItem(item, "File"), new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            if (expand)
                treeNode.Expand();

            vFitxeriSeleccionat = null;
            if (FitxerClick != null)
            {
                FitxerClick(vFitxeriSeleccionat, EventArgs.Empty);
            }
        }



        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView) sender;

            if (lv.SelectedItems.Count > 0)
            {
                vFitxeriSeleccionat = new FileInfo(((DirectoryInfo) treeView1.SelectedNode.Tag).FullName + "\\" + listView1.SelectedItems[0].Text);
                if (!vFitxeriSeleccionat.Exists)
                    vFitxeriSeleccionat = null;
                if (FitxerClick != null)
                {
                    FitxerClick(vFitxeriSeleccionat, EventArgs.Empty);
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].ImageIndex == 0)
                {
                    // Doble click en un directori
                    foreach (TreeNode node in treeView1.SelectedNode.Nodes)
                    {
                        if (node.Text == listView1.SelectedItems[0].Text)
                        {
                            treeView1.SelectedNode = node;
                            seleccionaNode(node);
                            break;
                        }
                    }
                }
                else
                {
                    // Doble click en un fitxer
                    if (FitxerDobleClick != null)
                    {
                        FitxerDobleClick(vFitxeriSeleccionat, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
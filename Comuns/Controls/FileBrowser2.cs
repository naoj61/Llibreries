using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            _Filter = "*.*";
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

        /// <summary>
        /// *.txt|*.*
        /// </summary>
        public string _Filter { get; set; }

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
                tvDirectoris.Nodes.Clear();
                tvDirectoris.Nodes.Add(rootNode);
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

        private void tvDirectoris_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            seleccionaNode(e.Node);
        }

        private void seleccionaNode(TreeNode treeNode, bool expand = false)
        {
            lvFitxers.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo) treeNode.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new[] {new ListViewItem.ListViewSubItem(item, "Directory"), new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                lvFitxers.Items.Add(item);
            }
            foreach (FileInfo file in FitxersFiltre(nodeDirInfo, _Filter))
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new[] {new ListViewItem.ListViewSubItem(item, "File"), new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                lvFitxers.Items.Add(item);
            }

            lvFitxers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            if (expand)
                treeNode.Expand();

            vFitxeriSeleccionat = null;
            if (FitxerClick != null)
            {
                FitxerClick(vFitxeriSeleccionat, EventArgs.Empty);
            }
        }



        private void lvFitxers_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView) sender;

            if (lv.SelectedItems.Count > 0)
            {
                vFitxeriSeleccionat = new FileInfo(((DirectoryInfo) tvDirectoris.SelectedNode.Tag).FullName + "\\" + lvFitxers.SelectedItems[0].Text);
                if (!vFitxeriSeleccionat.Exists)
                    vFitxeriSeleccionat = null;
                if (FitxerClick != null)
                {
                    FitxerClick(vFitxeriSeleccionat, EventArgs.Empty);
                }
            }
        }

        private void lvFitxers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvFitxers.SelectedItems.Count > 0)
            {
                if (lvFitxers.SelectedItems[0].ImageIndex == 0)
                {
                    // Doble click en un directori
                    foreach (TreeNode node in tvDirectoris.SelectedNode.Nodes)
                    {
                        if (node.Text == lvFitxers.SelectedItems[0].Text)
                        {
                            tvDirectoris.SelectedNode = node;
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

        public void canviaIdioma(string texts)
        {
            var t = texts.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

            if(t.Count()<3)
                return;

            columnHeaderName.Text = t[0];
            columnHeaderType.Text = t[1];
            columnHeaderLastModified.Text = t[2];

        }


        /// <summary>
        /// Torna una llista amb els fitxers que compleixen la máscara.
        /// </summary>
        /// <param name="nodeDirInfo"></param>
        /// <param name="mascaraFiltre"></param>
        /// <returns></returns>
        private static IEnumerable<FileInfo> FitxersFiltre(DirectoryInfo nodeDirInfo, string mascaraFiltre)
        {
            if (string.IsNullOrEmpty(mascaraFiltre))
                mascaraFiltre = "*.*";

            List<FileInfo> fitxers = new List<FileInfo>();

            foreach (var filtre in mascaraFiltre.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                fitxers.AddRange(nodeDirInfo.GetFiles(filtre));
            }
            return fitxers;
        }
    }
}
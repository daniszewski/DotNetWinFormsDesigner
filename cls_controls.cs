﻿
using System.ComponentModel;

namespace SWD4CS
{
    internal class cls_controls
    {
        internal Control? ctrl;
        internal string? className;
        internal Component? nonCtrl = new();
        private cls_userform? form; FlowLayoutPanel? otherCtrlPanel; cls_selectbox? selectBox;
        private bool selectFlag = false; bool changeFlag = true;
        private Point memPos; int grid = 4;
        internal List<string> decHandler = new();
        internal List<string> decFunc = new();

        public cls_controls(cls_userform form, FlowLayoutPanel? otherCtrlPanel, string className, Control parent, int X, int Y, bool fileFlag = false)
        {
            if ((className == "TabPage" && parent == form) || (parent is StatusStrip)) { return; }

            this.form = form;
            this.otherCtrlPanel = otherCtrlPanel;

            if (AddCtrl_Init(className))
            {
                this.className = className;
                this.ctrl!.Location = new System.Drawing.Point(X, Y);
                this.form.CtrlItems!.Add(this);

                if (this.nonCtrl.GetType() == typeof(Component))
                {
                    parent.Controls.Add(this.ctrl);
                    CreateSelectBox(parent);
                }
                else { otherCtrlPanel!.Controls.Add(this.ctrl); Selected = true; }

                if (this.ctrl is TabControl && !fileFlag)
                {
                    _ = new cls_controls(form, otherCtrlPanel, "TabPage", this.ctrl!, 0, 0);
                    _ = new cls_controls(form, otherCtrlPanel, "TabPage", this.ctrl!, 0, 0);
                }

                if (this.ctrl is TabPage) { CreateSelectBox(this.ctrl); }

                ctrl!.Click += new System.EventHandler(Ctrl_Click);
                ctrl.MouseMove += new System.Windows.Forms.MouseEventHandler(ControlMouseMove);
                ctrl.MouseDown += new System.Windows.Forms.MouseEventHandler(ControlMouseDown);
            }
        }

        // ********************************************************************************************
        // events Function 
        // ********************************************************************************************
        private void ControlMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Selected)
            {
                memPos.X = (int)(e.X / grid) * grid;
                memPos.Y = (int)(e.Y / grid) * grid;
            }
        }

        private void ControlMouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Selected)
            {
                Point pos = new((int)(e.X / grid) * grid, (int)(e.Y / grid) * grid);
                Point newPos = new(pos.X - memPos.X + ctrl!.Location.X, pos.Y - memPos.Y + ctrl.Location.Y);
                ctrl.Location = newPos;
                Selected = true;
                changeFlag = false;
            }
            else { changeFlag = true; }
        }

        private void Ctrl_Click(object? sender, EventArgs e)
        {
            if (e.ToString() != "System.EventArgs")
            {
                MouseEventArgs me = (MouseEventArgs)e;
                if (form!.mainForm!.toolLstBox!.Text == "")
                {
                    SetSelected(me);
                    foreach (TreeNode n in form.mainForm.ctrlTree!.Nodes)
                    {
                        TreeNode ret = FindNode(n, this.ctrl!.Name);
                        if (ret != null) { form!.mainForm.ctrlTree.SelectedNode = ret; break; }
                    }
                }
                else
                {
                    if (this.nonCtrl!.GetType() == typeof(Component)) { AddControls(me); }
                }
            }
        }

        private void SplitContainerPanelClick(object? sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            SplitterPanel? panel = sender as SplitterPanel;

            if (e.ToString() == "System.EventArgs") { return; }
            if (form!.mainForm!.toolLstBox!.Text == "") { SetSelected(me); }
            else { AddControls(me, panel); }
        }

        // ********************************************************************************************
        // internal Function 
        // ********************************************************************************************
        internal bool Selected
        {
            set
            {
                selectFlag = value;
                if (selectBox != null) { selectBox!.SetSelectBoxPos(value); }
                else
                {
                    if (value) { this.ctrl!.BackColor = System.Drawing.SystemColors.ActiveCaption; }
                    else { this.ctrl!.BackColor = System.Drawing.Color.Transparent; }
                }
                ShowProperty(value);
                form!.mainForm!.eventView!.ShowEventList(value, this);
            }
            get { return selectFlag; }
        }

        internal void Delete()
        {
            Selected = false;
            ctrl!.Parent!.Controls.Remove(ctrl);
        }

        internal static string SetFormProperty(Control? cls_ctrl, string? propertyName, string? propertyValue)
        {
            Component? ctrl = cls_ctrl;
            string ret = cls_properties.SetProperty(ctrl!, propertyName!, propertyValue!);
            return ret;
        }

        internal static string SetCtrlProperty(cls_controls? cls_ctrl, string? propertyName, string? propertyValue)
        {
            Component? ctrl = new();
            if (cls_ctrl!.nonCtrl!.GetType() == typeof(Component)) { ctrl = cls_ctrl!.ctrl; }
            else { ctrl = cls_ctrl!.nonCtrl; }
            string ret = cls_properties.SetProperty(ctrl!, propertyName!, propertyValue!);
            return ret;
        }

        // ********************************************************************************************
        // private Function 
        // ********************************************************************************************
        private TreeNode FindNode(TreeNode treeNode, string ctrlName)
        {
            if (ctrlName == treeNode.Text) { return treeNode; }

            TreeNode ret;
            foreach (TreeNode tn in treeNode.Nodes)
            {
                ret = FindNode(tn, ctrlName);
                if (ret != null) { return ret; }
            }
            return null!;
        }

        private void CreateSelectBox(Control parent)
        {
            bool select_flag = true;
            Control ctl = this.ctrl!;
            if (this.ctrl is TabPage) { select_flag = false; }
            else if (!(parent is FlowLayoutPanel) && !(parent is TableLayoutPanel)) { ctl = parent; }
            selectBox = new cls_selectbox(this, ctl);
            if (this.ctrl is TabPage) { return; }
            Selected = select_flag;
        }

        private void SetSelected(MouseEventArgs me)
        {
            if (me.Button != MouseButtons.Left) { return; }
            if (Selected && changeFlag) { Selected = false; }
            else
            {
                if ((Control.ModifierKeys & Keys.Control) != Keys.Control) { form!.SelectAllClear(); }
                Selected = true;
            }
        }

        private void ShowProperty(bool flag)
        {
            if (flag)
            {
                Component Comp;
                if (this.nonCtrl!.GetType() == typeof(Component)) { Comp = this.ctrl!; }
                else { Comp = this.nonCtrl; }
                form!.mainForm!.propertyGrid!.SelectedObject = Comp;
                form.mainForm.propertyCtrlName!.Text = this.ctrl!.Name;
            }
            else
            {
                form!.mainForm!.propertyGrid!.SelectedObject = null;
                form.mainForm!.propertyCtrlName!.Text = "";
            }
        }
        private void CreateTrancePanel(Control ctrl)
        {
            cls_transparent_panel trancepanel = new();
            trancepanel.Dock = DockStyle.Fill;
            trancepanel.BackColor = Color.FromArgb(0, 0, 0, 0);
            trancepanel.Click += new System.EventHandler(Ctrl_Click);
            trancepanel.MouseMove += new System.Windows.Forms.MouseEventHandler(ControlMouseMove);
            trancepanel.MouseDown += new System.Windows.Forms.MouseEventHandler(ControlMouseDown);
            ctrl.Controls.Add(trancepanel);
            trancepanel.BringToFront();
            trancepanel.Invalidate();
        }

        private void CreatePickBox(Control ctrl)
        {
            Button pickbox = new();
            pickbox.Size = new System.Drawing.Size(24, 24);
            pickbox.Text = "▼";
            pickbox.Click += new System.EventHandler(Ctrl_Click);
            pickbox.MouseMove += new System.Windows.Forms.MouseEventHandler(ControlMouseMove);
            pickbox.MouseDown += new System.Windows.Forms.MouseEventHandler(ControlMouseDown);
            ctrl.Controls.Add(pickbox);
        }

        private void AddControls(MouseEventArgs me, SplitterPanel? splitpanel = null)
        {
            int X = (int)(me.X / grid) * grid;
            int Y = (int)(me.Y / grid) * grid;
            form!.SelectAllClear();

            if ((this.ctrl is TabControl && form!.mainForm!.toolLstBox!.Text == "TabPage") ||
                (this.ctrl is TabControl == false && form!.mainForm!.toolLstBox!.Text != "TabPage"))
            {
                if (splitpanel == null) { _ = new cls_controls(form, otherCtrlPanel, form!.mainForm!.toolLstBox!.Text, this.ctrl!, X, Y); }
                else { _ = new cls_controls(form, otherCtrlPanel, form!.mainForm!.toolLstBox!.Text, splitpanel!, X, Y); }
            }
            form!.mainForm!.toolLstBox!.SelectedIndex = 0;
        }

        // ********************************************************************************************
        // コントロール追加時に編集する箇所
        // ********************************************************************************************
        private bool AddCtrl_Init(string className)
        {
            switch (className)
            {
                case "Button":
                    this.ctrl = new Button();
                    this.ctrl.Name = className + form!.cnt_Control;
                    break;
                case "Label":
                    this.ctrl = new Label();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    this.ctrl!.AutoSize = true;
                    break;
                case "GroupBox":
                    this.ctrl = new GroupBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "TextBox":
                    this.ctrl = new TextBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "ListBox":
                    this.ctrl = new ListBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    ListBox? listbox = this.ctrl as ListBox;
                    listbox!.Items.Add("ListBox");
                    break;
                case "TabControl":
                    this.ctrl = new TabControl();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "TabPage":
                    this.ctrl = new TabPage();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "CheckBox":
                    this.ctrl = new CheckBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    this.ctrl!.AutoSize = true;
                    break;
                case "ComboBox":
                    this.ctrl = new ComboBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "SplitContainer":
                    this.ctrl = new SplitContainer();
                    this.ctrl.Size = new System.Drawing.Size(120, 32);
                    this.ctrl!.Name = className + form!.cnt_Control;
                    this.ctrl.Size = new System.Drawing.Size(250, 125);
                    SplitContainer? splitcontainer = this.ctrl as SplitContainer;
                    splitcontainer!.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    splitcontainer.Panel1.Name = this.ctrl.Name + ".Panel1";
                    splitcontainer.Panel1.Click += new System.EventHandler(this.SplitContainerPanelClick);
                    splitcontainer.Panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(ControlMouseMove);
                    splitcontainer.Panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(ControlMouseDown);
                    splitcontainer.Panel2.Name = this.ctrl.Name + ".Panel2";
                    splitcontainer.Panel2.Click += new System.EventHandler(this.SplitContainerPanelClick);
                    splitcontainer.Panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(ControlMouseMove);
                    splitcontainer.Panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(ControlMouseDown);
                    break;
                case "DataGridView":
                    this.ctrl = new DataGridView();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "Panel":
                    this.ctrl = new Panel();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    Panel? panel = this.ctrl as Panel;
                    panel!.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    break;
                case "CheckedListBox":
                    this.ctrl = new CheckedListBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CheckedListBox? checkedlistbox = this.ctrl as CheckedListBox;
                    checkedlistbox!.Items.Add("CheckedListBox");
                    break;
                case "LinkLabel":
                    this.ctrl = new LinkLabel();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    this.ctrl!.AutoSize = true;
                    break;
                case "PictureBox":
                    this.ctrl = new PictureBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    PictureBox? picbox = this.ctrl as PictureBox;
                    picbox!.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    break;
                case "ProgressBar":
                    this.ctrl = new ProgressBar();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    ProgressBar? prgressbar = this.ctrl as ProgressBar;
                    prgressbar!.Value = 50;
                    break;
                case "RadioButton":
                    this.ctrl = new RadioButton();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    this.ctrl!.AutoSize = true;
                    break;
                case "RichTextBox":
                    this.ctrl = new RichTextBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "StatusStrip":
                    this.ctrl = new StatusStrip();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "HScrollBar":
                    this.ctrl = new HScrollBar();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreateTrancePanel(this.ctrl);
                    break;
                case "VScrollBar":
                    this.ctrl = new VScrollBar();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreateTrancePanel(this.ctrl);
                    break;
                case "MonthCalendar":
                    this.ctrl = new MonthCalendar();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreateTrancePanel(this.ctrl);
                    break;
                case "ListView":
                    this.ctrl = new ListView();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreateTrancePanel(this.ctrl);
                    break;
                case "TreeView":
                    this.ctrl = new TreeView();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreateTrancePanel(this.ctrl);
                    break;
                case "MaskedTextBox":
                    this.ctrl = new MaskedTextBox();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "PropertyGrid":
                    this.ctrl = new PropertyGrid();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreateTrancePanel(this.ctrl);
                    break;
                case "DateTimePicker":
                    this.ctrl = new DateTimePicker();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    CreatePickBox(this.ctrl);
                    break;
                case "DomainUpDown":
                    this.ctrl = new DomainUpDown();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "FlowLayoutPanel":
                    this.ctrl = new FlowLayoutPanel();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    FlowLayoutPanel? flwlayoutpnl = this.ctrl as FlowLayoutPanel;
                    flwlayoutpnl!.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    break;
                case "Splitter":
                    this.ctrl = new Splitter();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    Splitter? splitter = this.ctrl as Splitter;
                    splitter!.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    break;
                case "TableLayoutPanel":
                    this.ctrl = new TableLayoutPanel();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    TableLayoutPanel? tbllaypnl = this.ctrl as TableLayoutPanel;
                    tbllaypnl!.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
                    break;
                case "TrackBar":
                    this.ctrl = new TrackBar();
                    this.ctrl!.Name = className + form!.cnt_Control;
                    break;
                case "Timer":
                    this.nonCtrl = new System.Windows.Forms.Timer();
                    break;
                case "ColorDialog":
                    this.nonCtrl = new System.Windows.Forms.ColorDialog();
                    break;
                case "OpenFileDialog":
                    this.nonCtrl = new System.Windows.Forms.OpenFileDialog();
                    break;
                case "FontDialog":
                    this.nonCtrl = new System.Windows.Forms.FontDialog();
                    break;
                case "FolderBrowserDialog":
                    this.nonCtrl = new System.Windows.Forms.FolderBrowserDialog();
                    break;
                case "SaveFileDialog":
                    this.nonCtrl = new System.Windows.Forms.SaveFileDialog();
                    break;
                case "ImageList":
                    this.nonCtrl = new System.Windows.Forms.ImageList();
                    break;
                default:
                    return false;
            }

            if (this.ctrl == null)
            {
                Label lbl = new Label();
                lbl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.ctrl = lbl;
                this.ctrl.AutoSize = true;
                this.ctrl!.Name = className + form!.cnt_Control;
            }
            if (className != "DateTimePicker") { this.ctrl!.Text = this.ctrl!.Name; }
            this.ctrl!.TabIndex = form!.cnt_Control;
            form.cnt_Control++;
            return true;
        }

        internal static void AddToolList(ListBox ctrlLstBox)
        {
            ctrlLstBox.Items.Add("");
            ctrlLstBox.Items.Add("Button");
            ctrlLstBox.Items.Add("CheckBox");
            ctrlLstBox.Items.Add("CheckedListBox");
            ctrlLstBox.Items.Add("ComboBox");
            ctrlLstBox.Items.Add("DataGridView");
            ctrlLstBox.Items.Add("DateTimePicker");
            ctrlLstBox.Items.Add("DomainUpDown");
            ctrlLstBox.Items.Add("FlowLayoutPanel");
            ctrlLstBox.Items.Add("GroupBox");
            ctrlLstBox.Items.Add("HScrollBar");
            ctrlLstBox.Items.Add("Label");
            ctrlLstBox.Items.Add("LinkLabel");
            ctrlLstBox.Items.Add("ListBox");
            ctrlLstBox.Items.Add("ListView");
            ctrlLstBox.Items.Add("MaskedTextBox");
            ctrlLstBox.Items.Add("MonthCalendar");
            ctrlLstBox.Items.Add("Panel");
            ctrlLstBox.Items.Add("PictureBox");
            ctrlLstBox.Items.Add("ProgressBar");
            ctrlLstBox.Items.Add("PropertyGrid");
            ctrlLstBox.Items.Add("RadioButton");
            ctrlLstBox.Items.Add("RichTextBox");
            ctrlLstBox.Items.Add("SplitContainer");
            ctrlLstBox.Items.Add("Splitter");
            ctrlLstBox.Items.Add("StatusStrip");
            ctrlLstBox.Items.Add("TabControl");
            ctrlLstBox.Items.Add("TableLayoutPanel");
            ctrlLstBox.Items.Add("TabPage");
            ctrlLstBox.Items.Add("TextBox");
            ctrlLstBox.Items.Add("TrackBar");
            ctrlLstBox.Items.Add("TreeView");
            ctrlLstBox.Items.Add("VScrollBar");
            ctrlLstBox.Items.Add("");
            ctrlLstBox.Items.Add("ColorDialog");
            ctrlLstBox.Items.Add("FolderBrowserDialog");
            ctrlLstBox.Items.Add("FontDialog");
            ctrlLstBox.Items.Add("ImageList");
            ctrlLstBox.Items.Add("OpenFileDialog");
            ctrlLstBox.Items.Add("SaveFileDialog");
            ctrlLstBox.Items.Add("Timer");

            ctrlLstBox.SelectedIndex = 0;
        }
    }
}

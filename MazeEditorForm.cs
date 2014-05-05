/*************************************************************************
 *                                                                       *
 * This file is part of RoBOSS Simulation System,                        *
 * Copyright (C) 2004,2005 Dariusz Czyrnek, Wojciech Turek               *
 * All rights reserved.  Email: soofka@icslab.agh.edu.pl                 *
 *                                                                       *
 * RoBOSS Simulation System is free software; you can redistribute it    *
 * and/or modify it under the terms of The GNU General Public License    *
 * version 2.0 as published by the Free Software Foundation;             *
 * The text of the GNU General Public License is included with this      *
 * program in the file LICENSE.TXT.                                      *
 *                                                                       *
 * This program is distributed in the hope that it will be useful,       *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the file     *
 * LICENSE.TXT for more details.                                         *
 *                                                                       *
 *************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using Geometry;
using RoomsGraph;
using EMK.Cartography;
using EMK.LightGeometry;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace MazeEditor
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class MazeEditorForm : System.Windows.Forms.Form
    {


        private enum EditMode
        {
            Idle,
            CreateWall,
            CreateRobot,
            CreateVictim,
            MoveWall,
            DeleteWall,
            MoveRobot,
            MoveVictim,
            DeleteRobot,
            DeleteVictim,
            CreateNode,
            MoveNode,
            SelectRoom
        };

        private const float zoomStep = 2.0f;

        private DoubleBufferedPanel mazePanel;
        private System.Windows.Forms.Panel viewPanel;
        private System.Windows.Forms.Splitter topHorizontalSplitter;
        private System.Windows.Forms.ToolBarButton newToolBarButton;
        private System.Windows.Forms.ToolBarButton loadToolBarButton;
        private System.Windows.Forms.ToolBarButton saveToolBarButton;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolBar mainToolBar;
        private System.Windows.Forms.ToolBarButton zoomPlusToolBarButton;
        private System.Windows.Forms.ToolBarButton deleteToolBarButton;
        private System.Windows.Forms.ToolBarButton zoomMinusToolBarButton;
        private System.Windows.Forms.ToolBarButton widthPlusToolBarButton;
        private System.Windows.Forms.ToolBarButton heightPlusToolBarButton;
        private System.Windows.Forms.ColorDialog wallColorDialog;
        private System.Windows.Forms.Label wallColorLabel;
        private System.Windows.Forms.Panel wallColorPanel;
        private System.Windows.Forms.Label label123;
        private System.Windows.Forms.Label label423423;
        private System.Windows.Forms.Label robotHeightLabel;
        private System.Windows.Forms.Label robotTypeLabel;
        private System.Windows.Forms.TextBox robotTypeTextBox;
        private System.Windows.Forms.Label robotNameLabel;
        private System.Windows.Forms.TextBox robotNameTextBox;
        private System.Windows.Forms.ImageList mainMenuImageList;
        private System.Windows.Forms.Label mazeNameLabel;
        private System.Windows.Forms.Label gravityLabel;
        private System.Windows.Forms.TextBox worldNameTextBox;
        private System.Windows.Forms.Label snapToGridLabel;
        private System.Windows.Forms.Label snapToAngleLabel;


        private ArrayList mazeWalls = null;
        private ArrayList mazeRobots = null;
        private ArrayList mazeVictims = null;

        private ArrayList mazeRooms = null;
        private MazeGraph mazeGraph = null;

        private Graphics mazeBitmapGraphics = null;
        private float zoom = 1.0f;
        private Matrix invertedMazeMatrix = null;

        private Graphics mazeBitmapStaticGraphics = null;

        private Bitmap mazePanelBitmap = null;
        private Graphics mazeGraphics = null;

        private Pen mazeStaticPen;
        private Brush mazeStaticGridBrush;
        private Brush scaleBrush;
        private Font scaleFont;

        private Color mazeBackColor = Color.DimGray;

        private EditMode editMode;
        private bool createWallInProgress;
        private PointF[] tempPoints;
        private PointF firstPoint;

        private PointF[] coordPoints;

        private bool moveInProgress;
        private MazeWall selectedWall = null;
        private MazeRobot selectedRobot = null;
        private MazeVictim selectedVictim = null;

        private int movedWallPointIndex;

        private Pen notCreatedWallPen;

        private MazeNode selectedNode = null;
        //		private RoomsGraphBuilder roomsGraphBuilder;
        private Pen graphPen;
        private Pen selectedGraphPen;
        private Brush graphBrush;

        private Pen roomPen;
        private Pen doorPen;
        private Pen doorPenWithArrow;
        private Pen passagePenWithArrow;
        private Pen selectedDoorPen;
        private Pen selectedWallPen;

        private Brush robotBrush;
        private Brush victimBrush;

        private bool snapToWall;
        private bool SnapToWall
        {
            get
            {
                return snapToWall;
            }
            set
            {
                snapToWall = value;
                if (snapToWall)
                {
                    snapToWallLabel.ForeColor = System.Drawing.SystemColors.ControlText;
                    SnapToAngle = false;
                    SnapToGrid = false;
                }
                else
                {
                    snapToWallLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
                }
                mazePanel_Paint(this, null);
            }
        }


        private bool snapToAngle;
        private bool SnapToAngle
        {
            get
            {
                return snapToAngle;
            }
            set
            {
                snapToAngle = value;
                if (snapToAngle)
                {
                    snapToAngleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
                    SnapToWall = false;
                }
                else
                {
                    snapToAngleLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
                }
                mazePanel_Paint(this, null);
            }
        }

        private bool snapToGrid;
        private bool SnapToGrid
        {
            get
            {
                return snapToGrid;
            }
            set
            {
                snapToGrid = value;
                if (snapToGrid)
                {
                    snapToGridLabel.ForeColor = System.Drawing.SystemColors.ControlText;
                    SnapToWall = false;
                }
                else
                {
                    snapToGridLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
                }
                mazePanel_Paint(this, null);
            }
        }

        private Color wallColor;

        private int robotNameIndex;
        private System.Windows.Forms.Label coordXLabel;
        private System.Windows.Forms.Label coordYLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown heightNumericUpDown;
        private System.Windows.Forms.NumericUpDown widthNumericUpDown;
        private System.Windows.Forms.NumericUpDown gravityNumericUpDown;
        private System.Windows.Forms.NumericUpDown wallWidthNumericUpDown;
        private System.Windows.Forms.NumericUpDown wallHeightNumericUpDown;
        private System.Windows.Forms.NumericUpDown robotHeightNumericUpDown;
        private System.Windows.Forms.TabPage wallsTabPage;
        private System.Windows.Forms.TabPage robotsTabPage;
        private System.Windows.Forms.TabPage mazeTabPage;
        private System.Windows.Forms.ToolBarButton createToolBarButton;
        private System.Windows.Forms.ToolBarButton moveToolBarButton;
        private System.Windows.Forms.TabControl objectSelectorTabControl;
        private System.Windows.Forms.TabPage graphTabPage;
        private System.Windows.Forms.Button recreateRoomsButton;
        private System.Windows.Forms.ComboBox typeSelectComboBox;
        private System.Windows.Forms.TreeView roomsTreeView;
        private System.Windows.Forms.Panel leftMenuPanel;
        private System.Windows.Forms.Splitter verticalSplitter;
        private TabPage roomsTabPage;
        private ToolBarButton separator4;
        private ToolBarButton exportRoBossToolBarButton;
        private ToolBarButton importRoBossToolBarButton;
        private ToolBarButton separator1ToolBarButton;
        private ToolBarButton separator3BarButton;
        private TabPage victimsTabPage;
        private TreeView graphTreeView;


        public static System.Globalization.NumberFormatInfo numberFormatInfo;
        private CheckBox joinWithRadioButton;
        private Button removeRoomButton;
        private Panel roomPropertiesPanel;
        private Label roomLengthLabel;
        private Label roomAreaLabel;
        private Panel doorPropertiesPanel;
        private NumericUpDown gateBlockedNumericUpDown;
        private Label label4;
        private NumericUpDown roomExpPersonCountNumericUpDown;
        private TextBox roomFunctionTextBox;
        private Label label6;
        private Label label5;
        private Button removeInaccessibleRoomsButton;
        private TextBox spaceNameTextBox;
        private Label label3;
        private ListBox mazeWallsListBox;
        private Label wallLengthLabel;
        private Label wallWidthLabel;
        private Label wallHeightLabel;
        private Label wallAngleLabel;
        private Button removeDoorDoorEdgesButton;
        private TrackBar sizeTrackBar;
        private CheckBox ConvexOnlyCheckBox;
        private Label snapToWallLabel;
        private CheckBox createDoorWallCheckBox;
        private Button FlopAllWallButton;
        private Button translateWallsToZeroZeoButton;
        private TabPage PFTabPage;
        private Button butPause;
        private Button butStop;
        private NumericUpDown dNumberRobots;
        private Label label7;
        private Button butStart;
        private GroupBox groupBox1;
        private TextBox txtPort;
        private TextBox txtIP;
        private Label label9;
        private Label label8;
        private MazeSpace previousSelectedRoom;

        public MazeEditorForm()
        {
            InitializeComponent();

            mazeStaticPen = new Pen(Color.Coral, 1);
            notCreatedWallPen = new Pen(Color.FromArgb(128, 64, 255, 64), 1);
            scaleFont = new Font("sans", 8);
            scaleBrush = mazeStaticPen.Brush;
            mazeStaticGridBrush = (new Pen(Color.FromArgb(192, 100, 240, 100), 1)).Brush;

            SetEditMode(EditMode.MoveWall);
            tempPoints = new PointF[2];
            coordPoints = new PointF[1];

            robotBrush = (new Pen(Color.White, 1)).Brush;
            victimBrush = (new Pen(Color.Orange, 1)).Brush;





            graphPen = new Pen(Color.FromArgb(255, 200, 44, 44), 1);
            selectedGraphPen = new Pen(Color.FromArgb(192, 255, 255, 55), 2);
            graphBrush = new Pen(Color.FromArgb(128, 200, 200, 44), 1).Brush;

            roomPen = new Pen(Color.FromArgb(32, 0, 222, 0), 2);
            doorPen = new Pen(Color.FromArgb(64, 55, 255, 55), 3);
            selectedDoorPen = new Pen(Color.FromArgb(192, 55, 255, 55), 4);

            selectedWallPen = new Pen(Color.FromArgb(192, 55, 55, 255), 4);


            doorPenWithArrow = new Pen(Color.FromArgb(64, 55, 255, 55), 3);
            doorPenWithArrow.CustomEndCap = new AdjustableArrowCap(3, 3);
            passagePenWithArrow = new Pen(Color.FromArgb(64, 55, 255, 55), 3);
            passagePenWithArrow.DashStyle = DashStyle.Dot;
            passagePenWithArrow.CustomEndCap = new AdjustableArrowCap(3, 3);


            CreateNewMaze();

            wallColor = wallColorPanel.BackColor;

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MazeEditorForm));
            this.viewPanel = new System.Windows.Forms.Panel();
            this.mazePanel = new MazeEditor.DoubleBufferedPanel();
            this.verticalSplitter = new System.Windows.Forms.Splitter();
            this.topHorizontalSplitter = new System.Windows.Forms.Splitter();
            this.mainToolBar = new System.Windows.Forms.ToolBar();
            this.newToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.loadToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.saveToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.separator1ToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.createToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.moveToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.deleteToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.separator3BarButton = new System.Windows.Forms.ToolBarButton();
            this.zoomPlusToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.zoomMinusToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.widthPlusToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.heightPlusToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.separator4 = new System.Windows.Forms.ToolBarButton();
            this.importRoBossToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.exportRoBossToolBarButton = new System.Windows.Forms.ToolBarButton();
            this.mainMenuImageList = new System.Windows.Forms.ImageList(this.components);
            this.wallColorDialog = new System.Windows.Forms.ColorDialog();
            this.leftMenuPanel = new System.Windows.Forms.Panel();
            this.coordXLabel = new System.Windows.Forms.Label();
            this.coordYLabel = new System.Windows.Forms.Label();
            this.snapToAngleLabel = new System.Windows.Forms.Label();
            this.snapToWallLabel = new System.Windows.Forms.Label();
            this.snapToGridLabel = new System.Windows.Forms.Label();
            this.objectSelectorTabControl = new System.Windows.Forms.TabControl();
            this.mazeTabPage = new System.Windows.Forms.TabPage();
            this.mazeNameLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.widthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.gravityNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.gravityLabel = new System.Windows.Forms.Label();
            this.heightNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.worldNameTextBox = new System.Windows.Forms.TextBox();
            this.wallsTabPage = new System.Windows.Forms.TabPage();
            this.translateWallsToZeroZeoButton = new System.Windows.Forms.Button();
            this.FlopAllWallButton = new System.Windows.Forms.Button();
            this.createDoorWallCheckBox = new System.Windows.Forms.CheckBox();
            this.wallWidthLabel = new System.Windows.Forms.Label();
            this.wallHeightLabel = new System.Windows.Forms.Label();
            this.wallAngleLabel = new System.Windows.Forms.Label();
            this.wallLengthLabel = new System.Windows.Forms.Label();
            this.mazeWallsListBox = new System.Windows.Forms.ListBox();
            this.wallHeightNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.wallColorLabel = new System.Windows.Forms.Label();
            this.label423423 = new System.Windows.Forms.Label();
            this.wallColorPanel = new System.Windows.Forms.Panel();
            this.label123 = new System.Windows.Forms.Label();
            this.wallWidthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.robotsTabPage = new System.Windows.Forms.TabPage();
            this.robotNameLabel = new System.Windows.Forms.Label();
            this.robotTypeTextBox = new System.Windows.Forms.TextBox();
            this.robotTypeLabel = new System.Windows.Forms.Label();
            this.robotHeightLabel = new System.Windows.Forms.Label();
            this.robotHeightNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.robotNameTextBox = new System.Windows.Forms.TextBox();
            this.victimsTabPage = new System.Windows.Forms.TabPage();
            this.roomsTabPage = new System.Windows.Forms.TabPage();
            this.ConvexOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.removeInaccessibleRoomsButton = new System.Windows.Forms.Button();
            this.roomPropertiesPanel = new System.Windows.Forms.Panel();
            this.spaceNameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.roomExpPersonCountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.roomFunctionTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.roomLengthLabel = new System.Windows.Forms.Label();
            this.roomAreaLabel = new System.Windows.Forms.Label();
            this.joinWithRadioButton = new System.Windows.Forms.CheckBox();
            this.removeRoomButton = new System.Windows.Forms.Button();
            this.doorPropertiesPanel = new System.Windows.Forms.Panel();
            this.gateBlockedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.typeSelectComboBox = new System.Windows.Forms.ComboBox();
            this.roomsTreeView = new System.Windows.Forms.TreeView();
            this.recreateRoomsButton = new System.Windows.Forms.Button();
            this.graphTabPage = new System.Windows.Forms.TabPage();
            this.graphTreeView = new System.Windows.Forms.TreeView();
            this.sizeTrackBar = new System.Windows.Forms.TrackBar();
            this.removeDoorDoorEdgesButton = new System.Windows.Forms.Button();
            this.PFTabPage = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.butPause = new System.Windows.Forms.Button();
            this.butStop = new System.Windows.Forms.Button();
            this.dNumberRobots = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.butStart = new System.Windows.Forms.Button();
            this.viewPanel.SuspendLayout();
            this.leftMenuPanel.SuspendLayout();
            this.objectSelectorTabControl.SuspendLayout();
            this.mazeTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.widthNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gravityNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightNumericUpDown)).BeginInit();
            this.wallsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wallHeightNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wallWidthNumericUpDown)).BeginInit();
            this.robotsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.robotHeightNumericUpDown)).BeginInit();
            this.roomsTabPage.SuspendLayout();
            this.roomPropertiesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.roomExpPersonCountNumericUpDown)).BeginInit();
            this.doorPropertiesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gateBlockedNumericUpDown)).BeginInit();
            this.graphTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sizeTrackBar)).BeginInit();
            this.PFTabPage.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dNumberRobots)).BeginInit();
            this.SuspendLayout();
            // 
            // viewPanel
            // 
            this.viewPanel.AutoScroll = true;
            this.viewPanel.BackColor = System.Drawing.Color.Black;
            this.viewPanel.Controls.Add(this.mazePanel);
            this.viewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewPanel.Location = new System.Drawing.Point(375, 27);
            this.viewPanel.Name = "viewPanel";
            this.viewPanel.Size = new System.Drawing.Size(559, 608);
            this.viewPanel.TabIndex = 0;
            // 
            // mazePanel
            // 
            this.mazePanel.BackColor = System.Drawing.Color.DimGray;
            this.mazePanel.Location = new System.Drawing.Point(0, 0);
            this.mazePanel.Name = "mazePanel";
            this.mazePanel.Size = new System.Drawing.Size(0, 0);
            this.mazePanel.TabIndex = 0;
            this.mazePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.mazePanel_Paint);
            this.mazePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mazePanel_MouseMove);
            this.mazePanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mazePanel_MouseUp);
            // 
            // verticalSplitter
            // 
            this.verticalSplitter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.verticalSplitter.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.verticalSplitter.Location = new System.Drawing.Point(371, 27);
            this.verticalSplitter.MinExtra = 50;
            this.verticalSplitter.MinSize = 50;
            this.verticalSplitter.Name = "verticalSplitter";
            this.verticalSplitter.Size = new System.Drawing.Size(4, 608);
            this.verticalSplitter.TabIndex = 1;
            this.verticalSplitter.TabStop = false;
            // 
            // topHorizontalSplitter
            // 
            this.topHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
            this.topHorizontalSplitter.Location = new System.Drawing.Point(371, 26);
            this.topHorizontalSplitter.Name = "topHorizontalSplitter";
            this.topHorizontalSplitter.Size = new System.Drawing.Size(563, 1);
            this.topHorizontalSplitter.TabIndex = 2;
            this.topHorizontalSplitter.TabStop = false;
            // 
            // mainToolBar
            // 
            this.mainToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.newToolBarButton,
            this.loadToolBarButton,
            this.saveToolBarButton,
            this.separator1ToolBarButton,
            this.createToolBarButton,
            this.moveToolBarButton,
            this.deleteToolBarButton,
            this.separator3BarButton,
            this.zoomPlusToolBarButton,
            this.zoomMinusToolBarButton,
            this.widthPlusToolBarButton,
            this.heightPlusToolBarButton,
            this.separator4,
            this.importRoBossToolBarButton,
            this.exportRoBossToolBarButton});
            this.mainToolBar.ButtonSize = new System.Drawing.Size(26, 26);
            this.mainToolBar.Divider = false;
            this.mainToolBar.DropDownArrows = true;
            this.mainToolBar.ImageList = this.mainMenuImageList;
            this.mainToolBar.Location = new System.Drawing.Point(0, 0);
            this.mainToolBar.Name = "mainToolBar";
            this.mainToolBar.ShowToolTips = true;
            this.mainToolBar.Size = new System.Drawing.Size(934, 26);
            this.mainToolBar.TabIndex = 3;
            this.mainToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.mainToolBar_ButtonClick);
            // 
            // newToolBarButton
            // 
            this.newToolBarButton.ImageIndex = 0;
            this.newToolBarButton.Name = "newToolBarButton";
            // 
            // loadToolBarButton
            // 
            this.loadToolBarButton.ImageIndex = 1;
            this.loadToolBarButton.Name = "loadToolBarButton";
            // 
            // saveToolBarButton
            // 
            this.saveToolBarButton.ImageIndex = 2;
            this.saveToolBarButton.Name = "saveToolBarButton";
            // 
            // separator1ToolBarButton
            // 
            this.separator1ToolBarButton.Name = "separator1ToolBarButton";
            this.separator1ToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // createToolBarButton
            // 
            this.createToolBarButton.ImageIndex = 11;
            this.createToolBarButton.Name = "createToolBarButton";
            this.createToolBarButton.Pushed = true;
            this.createToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // moveToolBarButton
            // 
            this.moveToolBarButton.ImageIndex = 4;
            this.moveToolBarButton.Name = "moveToolBarButton";
            this.moveToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // deleteToolBarButton
            // 
            this.deleteToolBarButton.ImageIndex = 3;
            this.deleteToolBarButton.Name = "deleteToolBarButton";
            this.deleteToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            // 
            // separator3BarButton
            // 
            this.separator3BarButton.Name = "separator3BarButton";
            this.separator3BarButton.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // zoomPlusToolBarButton
            // 
            this.zoomPlusToolBarButton.ImageIndex = 10;
            this.zoomPlusToolBarButton.Name = "zoomPlusToolBarButton";
            // 
            // zoomMinusToolBarButton
            // 
            this.zoomMinusToolBarButton.ImageIndex = 9;
            this.zoomMinusToolBarButton.Name = "zoomMinusToolBarButton";
            // 
            // widthPlusToolBarButton
            // 
            this.widthPlusToolBarButton.ImageIndex = 7;
            this.widthPlusToolBarButton.Name = "widthPlusToolBarButton";
            this.widthPlusToolBarButton.Visible = false;
            // 
            // heightPlusToolBarButton
            // 
            this.heightPlusToolBarButton.ImageIndex = 8;
            this.heightPlusToolBarButton.Name = "heightPlusToolBarButton";
            this.heightPlusToolBarButton.Visible = false;
            // 
            // separator4
            // 
            this.separator4.Name = "separator4";
            this.separator4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // importRoBossToolBarButton
            // 
            this.importRoBossToolBarButton.ImageIndex = 16;
            this.importRoBossToolBarButton.Name = "importRoBossToolBarButton";
            this.importRoBossToolBarButton.ToolTipText = "import from RoBOSS";
            // 
            // exportRoBossToolBarButton
            // 
            this.exportRoBossToolBarButton.ImageIndex = 15;
            this.exportRoBossToolBarButton.Name = "exportRoBossToolBarButton";
            this.exportRoBossToolBarButton.ToolTipText = "Export for RoBOSS";
            // 
            // mainMenuImageList
            // 
            this.mainMenuImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mainMenuImageList.ImageStream")));
            this.mainMenuImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.mainMenuImageList.Images.SetKeyName(0, "");
            this.mainMenuImageList.Images.SetKeyName(1, "");
            this.mainMenuImageList.Images.SetKeyName(2, "");
            this.mainMenuImageList.Images.SetKeyName(3, "");
            this.mainMenuImageList.Images.SetKeyName(4, "");
            this.mainMenuImageList.Images.SetKeyName(5, "");
            this.mainMenuImageList.Images.SetKeyName(6, "");
            this.mainMenuImageList.Images.SetKeyName(7, "");
            this.mainMenuImageList.Images.SetKeyName(8, "");
            this.mainMenuImageList.Images.SetKeyName(9, "");
            this.mainMenuImageList.Images.SetKeyName(10, "");
            this.mainMenuImageList.Images.SetKeyName(11, "");
            this.mainMenuImageList.Images.SetKeyName(12, "");
            this.mainMenuImageList.Images.SetKeyName(13, "rooms.png");
            this.mainMenuImageList.Images.SetKeyName(14, "victims.png");
            this.mainMenuImageList.Images.SetKeyName(15, "exportRoBOSS.png");
            this.mainMenuImageList.Images.SetKeyName(16, "importRoBOSS.png");
            // 
            // leftMenuPanel
            // 
            this.leftMenuPanel.Controls.Add(this.coordXLabel);
            this.leftMenuPanel.Controls.Add(this.coordYLabel);
            this.leftMenuPanel.Controls.Add(this.snapToAngleLabel);
            this.leftMenuPanel.Controls.Add(this.snapToWallLabel);
            this.leftMenuPanel.Controls.Add(this.snapToGridLabel);
            this.leftMenuPanel.Controls.Add(this.objectSelectorTabControl);
            this.leftMenuPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftMenuPanel.Location = new System.Drawing.Point(0, 26);
            this.leftMenuPanel.Name = "leftMenuPanel";
            this.leftMenuPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 60);
            this.leftMenuPanel.Size = new System.Drawing.Size(371, 609);
            this.leftMenuPanel.TabIndex = 4;
            // 
            // coordXLabel
            // 
            this.coordXLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.coordXLabel.Location = new System.Drawing.Point(116, 580);
            this.coordXLabel.Name = "coordXLabel";
            this.coordXLabel.Size = new System.Drawing.Size(80, 20);
            this.coordXLabel.TabIndex = 17;
            this.coordXLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // coordYLabel
            // 
            this.coordYLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.coordYLabel.Location = new System.Drawing.Point(204, 580);
            this.coordYLabel.Name = "coordYLabel";
            this.coordYLabel.Size = new System.Drawing.Size(80, 20);
            this.coordYLabel.TabIndex = 17;
            this.coordYLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // snapToAngleLabel
            // 
            this.snapToAngleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.snapToAngleLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.snapToAngleLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.snapToAngleLabel.Location = new System.Drawing.Point(12, 580);
            this.snapToAngleLabel.Name = "snapToAngleLabel";
            this.snapToAngleLabel.Size = new System.Drawing.Size(80, 20);
            this.snapToAngleLabel.TabIndex = 16;
            this.snapToAngleLabel.Text = "snap to angle";
            this.snapToAngleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.snapToAngleLabel.Click += new System.EventHandler(this.snapToAngleLabel_Click);
            // 
            // snapToWallLabel
            // 
            this.snapToWallLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.snapToWallLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.snapToWallLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.snapToWallLabel.Location = new System.Drawing.Point(98, 552);
            this.snapToWallLabel.Name = "snapToWallLabel";
            this.snapToWallLabel.Size = new System.Drawing.Size(80, 20);
            this.snapToWallLabel.TabIndex = 16;
            this.snapToWallLabel.Text = "snap to wall";
            this.snapToWallLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.snapToWallLabel.Click += new System.EventHandler(this.snapToWallLabel_Click);
            // 
            // snapToGridLabel
            // 
            this.snapToGridLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.snapToGridLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.snapToGridLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.snapToGridLabel.Location = new System.Drawing.Point(12, 552);
            this.snapToGridLabel.Name = "snapToGridLabel";
            this.snapToGridLabel.Size = new System.Drawing.Size(80, 20);
            this.snapToGridLabel.TabIndex = 16;
            this.snapToGridLabel.Text = "snap to grid";
            this.snapToGridLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.snapToGridLabel.Click += new System.EventHandler(this.snapToGridLabel_Click);
            // 
            // objectSelectorTabControl
            // 
            this.objectSelectorTabControl.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.objectSelectorTabControl.Controls.Add(this.mazeTabPage);
            this.objectSelectorTabControl.Controls.Add(this.wallsTabPage);
            this.objectSelectorTabControl.Controls.Add(this.robotsTabPage);
            this.objectSelectorTabControl.Controls.Add(this.victimsTabPage);
            this.objectSelectorTabControl.Controls.Add(this.roomsTabPage);
            this.objectSelectorTabControl.Controls.Add(this.graphTabPage);
            this.objectSelectorTabControl.Controls.Add(this.PFTabPage);
            this.objectSelectorTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectSelectorTabControl.ImageList = this.mainMenuImageList;
            this.objectSelectorTabControl.ItemSize = new System.Drawing.Size(42, 30);
            this.objectSelectorTabControl.Location = new System.Drawing.Point(0, 0);
            this.objectSelectorTabControl.Multiline = true;
            this.objectSelectorTabControl.Name = "objectSelectorTabControl";
            this.objectSelectorTabControl.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.objectSelectorTabControl.SelectedIndex = 0;
            this.objectSelectorTabControl.Size = new System.Drawing.Size(371, 549);
            this.objectSelectorTabControl.TabIndex = 2;
            this.objectSelectorTabControl.SelectedIndexChanged += new System.EventHandler(this.objectSelectorTabControl_SelectedIndexChanged);
            // 
            // mazeTabPage
            // 
            this.mazeTabPage.Controls.Add(this.mazeNameLabel);
            this.mazeTabPage.Controls.Add(this.label1);
            this.mazeTabPage.Controls.Add(this.label2);
            this.mazeTabPage.Controls.Add(this.widthNumericUpDown);
            this.mazeTabPage.Controls.Add(this.gravityNumericUpDown);
            this.mazeTabPage.Controls.Add(this.gravityLabel);
            this.mazeTabPage.Controls.Add(this.heightNumericUpDown);
            this.mazeTabPage.Controls.Add(this.worldNameTextBox);
            this.mazeTabPage.Location = new System.Drawing.Point(4, 67);
            this.mazeTabPage.Name = "mazeTabPage";
            this.mazeTabPage.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mazeTabPage.Size = new System.Drawing.Size(363, 478);
            this.mazeTabPage.TabIndex = 3;
            this.mazeTabPage.Text = "maze";
            // 
            // mazeNameLabel
            // 
            this.mazeNameLabel.Location = new System.Drawing.Point(8, 8);
            this.mazeNameLabel.Name = "mazeNameLabel";
            this.mazeNameLabel.Size = new System.Drawing.Size(64, 24);
            this.mazeNameLabel.TabIndex = 9;
            this.mazeNameLabel.Text = "world name";
            this.mazeNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 24);
            this.label1.TabIndex = 13;
            this.label1.Text = "width";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 24);
            this.label2.TabIndex = 15;
            this.label2.Text = "height";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // widthNumericUpDown
            // 
            this.widthNumericUpDown.DecimalPlaces = 2;
            this.widthNumericUpDown.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.widthNumericUpDown.Location = new System.Drawing.Point(80, 96);
            this.widthNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.widthNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.widthNumericUpDown.Name = "widthNumericUpDown";
            this.widthNumericUpDown.Size = new System.Drawing.Size(112, 20);
            this.widthNumericUpDown.TabIndex = 16;
            this.widthNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.widthNumericUpDown.ValueChanged += new System.EventHandler(this.widthHeightNumericUpDown_ValueChanged);
            // 
            // gravityNumericUpDown
            // 
            this.gravityNumericUpDown.DecimalPlaces = 2;
            this.gravityNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.gravityNumericUpDown.Location = new System.Drawing.Point(80, 64);
            this.gravityNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.gravityNumericUpDown.Name = "gravityNumericUpDown";
            this.gravityNumericUpDown.Size = new System.Drawing.Size(112, 20);
            this.gravityNumericUpDown.TabIndex = 17;
            this.gravityNumericUpDown.Value = new decimal(new int[] {
            981,
            0,
            0,
            131072});
            // 
            // gravityLabel
            // 
            this.gravityLabel.Location = new System.Drawing.Point(8, 64);
            this.gravityLabel.Name = "gravityLabel";
            this.gravityLabel.Size = new System.Drawing.Size(64, 24);
            this.gravityLabel.TabIndex = 11;
            this.gravityLabel.Text = "gravity";
            this.gravityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // heightNumericUpDown
            // 
            this.heightNumericUpDown.DecimalPlaces = 2;
            this.heightNumericUpDown.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.heightNumericUpDown.Location = new System.Drawing.Point(80, 120);
            this.heightNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.heightNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.heightNumericUpDown.Name = "heightNumericUpDown";
            this.heightNumericUpDown.Size = new System.Drawing.Size(112, 20);
            this.heightNumericUpDown.TabIndex = 16;
            this.heightNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.heightNumericUpDown.ValueChanged += new System.EventHandler(this.widthHeightNumericUpDown_ValueChanged);
            // 
            // worldNameTextBox
            // 
            this.worldNameTextBox.Location = new System.Drawing.Point(8, 32);
            this.worldNameTextBox.Name = "worldNameTextBox";
            this.worldNameTextBox.Size = new System.Drawing.Size(184, 20);
            this.worldNameTextBox.TabIndex = 8;
            this.worldNameTextBox.Text = "MyMaze";
            // 
            // wallsTabPage
            // 
            this.wallsTabPage.Controls.Add(this.translateWallsToZeroZeoButton);
            this.wallsTabPage.Controls.Add(this.FlopAllWallButton);
            this.wallsTabPage.Controls.Add(this.createDoorWallCheckBox);
            this.wallsTabPage.Controls.Add(this.wallWidthLabel);
            this.wallsTabPage.Controls.Add(this.wallHeightLabel);
            this.wallsTabPage.Controls.Add(this.wallAngleLabel);
            this.wallsTabPage.Controls.Add(this.wallLengthLabel);
            this.wallsTabPage.Controls.Add(this.mazeWallsListBox);
            this.wallsTabPage.Controls.Add(this.wallHeightNumericUpDown);
            this.wallsTabPage.Controls.Add(this.wallColorLabel);
            this.wallsTabPage.Controls.Add(this.label423423);
            this.wallsTabPage.Controls.Add(this.wallColorPanel);
            this.wallsTabPage.Controls.Add(this.label123);
            this.wallsTabPage.Controls.Add(this.wallWidthNumericUpDown);
            this.wallsTabPage.ImageIndex = 5;
            this.wallsTabPage.Location = new System.Drawing.Point(4, 67);
            this.wallsTabPage.Name = "wallsTabPage";
            this.wallsTabPage.Size = new System.Drawing.Size(363, 478);
            this.wallsTabPage.TabIndex = 0;
            this.wallsTabPage.Text = "walls";
            // 
            // translateWallsToZeroZeoButton
            // 
            this.translateWallsToZeroZeoButton.Location = new System.Drawing.Point(228, 33);
            this.translateWallsToZeroZeoButton.Name = "translateWallsToZeroZeoButton";
            this.translateWallsToZeroZeoButton.Size = new System.Drawing.Size(52, 23);
            this.translateWallsToZeroZeoButton.TabIndex = 26;
            this.translateWallsToZeroZeoButton.Text = "0, 0";
            this.translateWallsToZeroZeoButton.UseVisualStyleBackColor = true;
            this.translateWallsToZeroZeoButton.Click += new System.EventHandler(this.translateWallsToZeroZeoButton_Click);
            // 
            // FlopAllWallButton
            // 
            this.FlopAllWallButton.Location = new System.Drawing.Point(228, 9);
            this.FlopAllWallButton.Name = "FlopAllWallButton";
            this.FlopAllWallButton.Size = new System.Drawing.Size(52, 23);
            this.FlopAllWallButton.TabIndex = 26;
            this.FlopAllWallButton.Text = "flip";
            this.FlopAllWallButton.UseVisualStyleBackColor = true;
            this.FlopAllWallButton.Click += new System.EventHandler(this.FlopAllWallButton_Click);
            // 
            // createDoorWallCheckBox
            // 
            this.createDoorWallCheckBox.AutoSize = true;
            this.createDoorWallCheckBox.Location = new System.Drawing.Point(88, 87);
            this.createDoorWallCheckBox.Name = "createDoorWallCheckBox";
            this.createDoorWallCheckBox.Size = new System.Drawing.Size(47, 17);
            this.createDoorWallCheckBox.TabIndex = 25;
            this.createDoorWallCheckBox.Text = "door";
            this.createDoorWallCheckBox.UseVisualStyleBackColor = true;
            // 
            // wallWidthLabel
            // 
            this.wallWidthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wallWidthLabel.Location = new System.Drawing.Point(167, 402);
            this.wallWidthLabel.Name = "wallWidthLabel";
            this.wallWidthLabel.Size = new System.Drawing.Size(123, 24);
            this.wallWidthLabel.TabIndex = 23;
            this.wallWidthLabel.Text = "width";
            this.wallWidthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wallHeightLabel
            // 
            this.wallHeightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wallHeightLabel.Location = new System.Drawing.Point(167, 426);
            this.wallHeightLabel.Name = "wallHeightLabel";
            this.wallHeightLabel.Size = new System.Drawing.Size(113, 24);
            this.wallHeightLabel.TabIndex = 24;
            this.wallHeightLabel.Text = "height";
            this.wallHeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wallAngleLabel
            // 
            this.wallAngleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wallAngleLabel.Location = new System.Drawing.Point(8, 426);
            this.wallAngleLabel.Name = "wallAngleLabel";
            this.wallAngleLabel.Size = new System.Drawing.Size(120, 24);
            this.wallAngleLabel.TabIndex = 22;
            this.wallAngleLabel.Text = "angle";
            this.wallAngleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wallLengthLabel
            // 
            this.wallLengthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wallLengthLabel.Location = new System.Drawing.Point(8, 402);
            this.wallLengthLabel.Name = "wallLengthLabel";
            this.wallLengthLabel.Size = new System.Drawing.Size(120, 24);
            this.wallLengthLabel.TabIndex = 21;
            this.wallLengthLabel.Text = "length";
            this.wallLengthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mazeWallsListBox
            // 
            this.mazeWallsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mazeWallsListBox.FormattingEnabled = true;
            this.mazeWallsListBox.Location = new System.Drawing.Point(11, 116);
            this.mazeWallsListBox.Name = "mazeWallsListBox";
            this.mazeWallsListBox.Size = new System.Drawing.Size(349, 277);
            this.mazeWallsListBox.TabIndex = 20;
            this.mazeWallsListBox.SelectedIndexChanged += new System.EventHandler(this.mazeWallsListBox_SelectedIndexChanged);
            // 
            // wallHeightNumericUpDown
            // 
            this.wallHeightNumericUpDown.DecimalPlaces = 2;
            this.wallHeightNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.wallHeightNumericUpDown.Location = new System.Drawing.Point(88, 32);
            this.wallHeightNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.wallHeightNumericUpDown.Name = "wallHeightNumericUpDown";
            this.wallHeightNumericUpDown.Size = new System.Drawing.Size(112, 20);
            this.wallHeightNumericUpDown.TabIndex = 19;
            this.wallHeightNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // wallColorLabel
            // 
            this.wallColorLabel.Location = new System.Drawing.Point(8, 56);
            this.wallColorLabel.Name = "wallColorLabel";
            this.wallColorLabel.Size = new System.Drawing.Size(64, 24);
            this.wallColorLabel.TabIndex = 13;
            this.wallColorLabel.Text = "wall color";
            this.wallColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label423423
            // 
            this.label423423.Location = new System.Drawing.Point(8, 8);
            this.label423423.Name = "label423423";
            this.label423423.Size = new System.Drawing.Size(64, 24);
            this.label423423.TabIndex = 9;
            this.label423423.Text = "wall width";
            this.label423423.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wallColorPanel
            // 
            this.wallColorPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.wallColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wallColorPanel.Location = new System.Drawing.Point(88, 56);
            this.wallColorPanel.Name = "wallColorPanel";
            this.wallColorPanel.Size = new System.Drawing.Size(112, 24);
            this.wallColorPanel.TabIndex = 12;
            this.wallColorPanel.Click += new System.EventHandler(this.wallColorPanel_Click);
            // 
            // label123
            // 
            this.label123.Location = new System.Drawing.Point(8, 32);
            this.label123.Name = "label123";
            this.label123.Size = new System.Drawing.Size(64, 24);
            this.label123.TabIndex = 11;
            this.label123.Text = "wall height";
            this.label123.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wallWidthNumericUpDown
            // 
            this.wallWidthNumericUpDown.DecimalPlaces = 2;
            this.wallWidthNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.wallWidthNumericUpDown.Location = new System.Drawing.Point(88, 8);
            this.wallWidthNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.wallWidthNumericUpDown.Name = "wallWidthNumericUpDown";
            this.wallWidthNumericUpDown.Size = new System.Drawing.Size(112, 20);
            this.wallWidthNumericUpDown.TabIndex = 18;
            this.wallWidthNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // robotsTabPage
            // 
            this.robotsTabPage.Controls.Add(this.robotNameLabel);
            this.robotsTabPage.Controls.Add(this.robotTypeTextBox);
            this.robotsTabPage.Controls.Add(this.robotTypeLabel);
            this.robotsTabPage.Controls.Add(this.robotHeightLabel);
            this.robotsTabPage.Controls.Add(this.robotHeightNumericUpDown);
            this.robotsTabPage.Controls.Add(this.robotNameTextBox);
            this.robotsTabPage.ImageIndex = 6;
            this.robotsTabPage.Location = new System.Drawing.Point(4, 67);
            this.robotsTabPage.Name = "robotsTabPage";
            this.robotsTabPage.Size = new System.Drawing.Size(363, 478);
            this.robotsTabPage.TabIndex = 1;
            this.robotsTabPage.Text = "robots";
            // 
            // robotNameLabel
            // 
            this.robotNameLabel.Location = new System.Drawing.Point(8, 64);
            this.robotNameLabel.Name = "robotNameLabel";
            this.robotNameLabel.Size = new System.Drawing.Size(64, 24);
            this.robotNameLabel.TabIndex = 18;
            this.robotNameLabel.Text = "robot name";
            this.robotNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // robotTypeTextBox
            // 
            this.robotTypeTextBox.Location = new System.Drawing.Point(16, 40);
            this.robotTypeTextBox.Name = "robotTypeTextBox";
            this.robotTypeTextBox.Size = new System.Drawing.Size(176, 20);
            this.robotTypeTextBox.TabIndex = 17;
            this.robotTypeTextBox.Text = "SomeType";
            this.robotTypeTextBox.TextChanged += new System.EventHandler(this.robotTypeTextBox_TextChanged);
            // 
            // robotTypeLabel
            // 
            this.robotTypeLabel.Location = new System.Drawing.Point(8, 16);
            this.robotTypeLabel.Name = "robotTypeLabel";
            this.robotTypeLabel.Size = new System.Drawing.Size(64, 24);
            this.robotTypeLabel.TabIndex = 16;
            this.robotTypeLabel.Text = "robot type";
            this.robotTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // robotHeightLabel
            // 
            this.robotHeightLabel.Location = new System.Drawing.Point(8, 120);
            this.robotHeightLabel.Name = "robotHeightLabel";
            this.robotHeightLabel.Size = new System.Drawing.Size(64, 24);
            this.robotHeightLabel.TabIndex = 15;
            this.robotHeightLabel.Text = "robot height";
            this.robotHeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // robotHeightNumericUpDown
            // 
            this.robotHeightNumericUpDown.DecimalPlaces = 2;
            this.robotHeightNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.robotHeightNumericUpDown.Location = new System.Drawing.Point(88, 120);
            this.robotHeightNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.robotHeightNumericUpDown.Name = "robotHeightNumericUpDown";
            this.robotHeightNumericUpDown.Size = new System.Drawing.Size(104, 20);
            this.robotHeightNumericUpDown.TabIndex = 20;
            this.robotHeightNumericUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // robotNameTextBox
            // 
            this.robotNameTextBox.Location = new System.Drawing.Point(16, 88);
            this.robotNameTextBox.Name = "robotNameTextBox";
            this.robotNameTextBox.Size = new System.Drawing.Size(176, 20);
            this.robotNameTextBox.TabIndex = 19;
            this.robotNameTextBox.Text = "SomeName";
            // 
            // victimsTabPage
            // 
            this.victimsTabPage.ImageIndex = 14;
            this.victimsTabPage.Location = new System.Drawing.Point(4, 67);
            this.victimsTabPage.Name = "victimsTabPage";
            this.victimsTabPage.Size = new System.Drawing.Size(363, 478);
            this.victimsTabPage.TabIndex = 5;
            this.victimsTabPage.Text = "victims";
            this.victimsTabPage.UseVisualStyleBackColor = true;
            // 
            // roomsTabPage
            // 
            this.roomsTabPage.Controls.Add(this.ConvexOnlyCheckBox);
            this.roomsTabPage.Controls.Add(this.removeInaccessibleRoomsButton);
            this.roomsTabPage.Controls.Add(this.roomPropertiesPanel);
            this.roomsTabPage.Controls.Add(this.doorPropertiesPanel);
            this.roomsTabPage.Controls.Add(this.typeSelectComboBox);
            this.roomsTabPage.Controls.Add(this.roomsTreeView);
            this.roomsTabPage.Controls.Add(this.recreateRoomsButton);
            this.roomsTabPage.ImageIndex = 13;
            this.roomsTabPage.Location = new System.Drawing.Point(4, 67);
            this.roomsTabPage.Name = "roomsTabPage";
            this.roomsTabPage.Size = new System.Drawing.Size(363, 478);
            this.roomsTabPage.TabIndex = 4;
            this.roomsTabPage.Text = "rooms";
            this.roomsTabPage.UseVisualStyleBackColor = true;
            // 
            // ConvexOnlyCheckBox
            // 
            this.ConvexOnlyCheckBox.AutoSize = true;
            this.ConvexOnlyCheckBox.Checked = true;
            this.ConvexOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConvexOnlyCheckBox.Location = new System.Drawing.Point(272, 20);
            this.ConvexOnlyCheckBox.Name = "ConvexOnlyCheckBox";
            this.ConvexOnlyCheckBox.Size = new System.Drawing.Size(83, 17);
            this.ConvexOnlyCheckBox.TabIndex = 14;
            this.ConvexOnlyCheckBox.Text = "convex only";
            this.ConvexOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // removeInaccessibleRoomsButton
            // 
            this.removeInaccessibleRoomsButton.Location = new System.Drawing.Point(15, 47);
            this.removeInaccessibleRoomsButton.Name = "removeInaccessibleRoomsButton";
            this.removeInaccessibleRoomsButton.Size = new System.Drawing.Size(140, 23);
            this.removeInaccessibleRoomsButton.TabIndex = 13;
            this.removeInaccessibleRoomsButton.Text = "remove inaccesible rooms";
            this.removeInaccessibleRoomsButton.UseVisualStyleBackColor = true;
            this.removeInaccessibleRoomsButton.Click += new System.EventHandler(this.removeInaccessibleRoomsButton_Click);
            // 
            // roomPropertiesPanel
            // 
            this.roomPropertiesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.roomPropertiesPanel.Controls.Add(this.spaceNameTextBox);
            this.roomPropertiesPanel.Controls.Add(this.label3);
            this.roomPropertiesPanel.Controls.Add(this.roomExpPersonCountNumericUpDown);
            this.roomPropertiesPanel.Controls.Add(this.roomFunctionTextBox);
            this.roomPropertiesPanel.Controls.Add(this.label6);
            this.roomPropertiesPanel.Controls.Add(this.label5);
            this.roomPropertiesPanel.Controls.Add(this.roomLengthLabel);
            this.roomPropertiesPanel.Controls.Add(this.roomAreaLabel);
            this.roomPropertiesPanel.Controls.Add(this.joinWithRadioButton);
            this.roomPropertiesPanel.Controls.Add(this.removeRoomButton);
            this.roomPropertiesPanel.Location = new System.Drawing.Point(50, 373);
            this.roomPropertiesPanel.Name = "roomPropertiesPanel";
            this.roomPropertiesPanel.Size = new System.Drawing.Size(300, 102);
            this.roomPropertiesPanel.TabIndex = 11;
            this.roomPropertiesPanel.Visible = false;
            // 
            // spaceNameTextBox
            // 
            this.spaceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spaceNameTextBox.Location = new System.Drawing.Point(142, 39);
            this.spaceNameTextBox.Name = "spaceNameTextBox";
            this.spaceNameTextBox.Size = new System.Drawing.Size(158, 20);
            this.spaceNameTextBox.TabIndex = 17;
            this.spaceNameTextBox.TextChanged += new System.EventHandler(this.spaceNameTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "name:";
            // 
            // roomExpPersonCountNumericUpDown
            // 
            this.roomExpPersonCountNumericUpDown.Location = new System.Drawing.Point(142, 81);
            this.roomExpPersonCountNumericUpDown.Name = "roomExpPersonCountNumericUpDown";
            this.roomExpPersonCountNumericUpDown.Size = new System.Drawing.Size(82, 20);
            this.roomExpPersonCountNumericUpDown.TabIndex = 15;
            this.roomExpPersonCountNumericUpDown.ValueChanged += new System.EventHandler(this.roomExpPersonCountNumericUpDown_ValueChanged);
            // 
            // roomFunctionTextBox
            // 
            this.roomFunctionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.roomFunctionTextBox.Location = new System.Drawing.Point(142, 60);
            this.roomFunctionTextBox.Name = "roomFunctionTextBox";
            this.roomFunctionTextBox.Size = new System.Drawing.Size(158, 20);
            this.roomFunctionTextBox.TabIndex = 14;
            this.roomFunctionTextBox.TextChanged += new System.EventHandler(this.roomFunctionTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "expected person count:  ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "function:  ";
            // 
            // roomLengthLabel
            // 
            this.roomLengthLabel.AutoSize = true;
            this.roomLengthLabel.Location = new System.Drawing.Point(3, 22);
            this.roomLengthLabel.Name = "roomLengthLabel";
            this.roomLengthLabel.Size = new System.Drawing.Size(77, 13);
            this.roomLengthLabel.TabIndex = 11;
            this.roomLengthLabel.Text = "space length:  ";
            // 
            // roomAreaLabel
            // 
            this.roomAreaLabel.AutoSize = true;
            this.roomAreaLabel.Location = new System.Drawing.Point(3, 4);
            this.roomAreaLabel.Name = "roomAreaLabel";
            this.roomAreaLabel.Size = new System.Drawing.Size(69, 13);
            this.roomAreaLabel.TabIndex = 10;
            this.roomAreaLabel.Text = "space area:  ";
            // 
            // joinWithRadioButton
            // 
            this.joinWithRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.joinWithRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.joinWithRadioButton.AutoSize = true;
            this.joinWithRadioButton.Location = new System.Drawing.Point(233, 2);
            this.joinWithRadioButton.Name = "joinWithRadioButton";
            this.joinWithRadioButton.Size = new System.Drawing.Size(67, 23);
            this.joinWithRadioButton.TabIndex = 8;
            this.joinWithRadioButton.Text = "join with ...";
            this.joinWithRadioButton.UseVisualStyleBackColor = true;
            this.joinWithRadioButton.CheckedChanged += new System.EventHandler(this.joinWithRadioButton_CheckedChanged);
            // 
            // removeRoomButton
            // 
            this.removeRoomButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeRoomButton.Location = new System.Drawing.Point(142, 2);
            this.removeRoomButton.Name = "removeRoomButton";
            this.removeRoomButton.Size = new System.Drawing.Size(88, 23);
            this.removeRoomButton.TabIndex = 9;
            this.removeRoomButton.Text = "remove space";
            this.removeRoomButton.UseVisualStyleBackColor = true;
            this.removeRoomButton.Click += new System.EventHandler(this.removeRoomButton_Click);
            // 
            // doorPropertiesPanel
            // 
            this.doorPropertiesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.doorPropertiesPanel.Controls.Add(this.gateBlockedNumericUpDown);
            this.doorPropertiesPanel.Controls.Add(this.label4);
            this.doorPropertiesPanel.Location = new System.Drawing.Point(15, 373);
            this.doorPropertiesPanel.Name = "doorPropertiesPanel";
            this.doorPropertiesPanel.Size = new System.Drawing.Size(288, 39);
            this.doorPropertiesPanel.TabIndex = 12;
            // 
            // gateBlockedNumericUpDown
            // 
            this.gateBlockedNumericUpDown.DecimalPlaces = 2;
            this.gateBlockedNumericUpDown.Location = new System.Drawing.Point(129, 1);
            this.gateBlockedNumericUpDown.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.gateBlockedNumericUpDown.Name = "gateBlockedNumericUpDown";
            this.gateBlockedNumericUpDown.Size = new System.Drawing.Size(84, 20);
            this.gateBlockedNumericUpDown.TabIndex = 1;
            this.gateBlockedNumericUpDown.ValueChanged += new System.EventHandler(this.gateBlockedNumericUpDown_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "gate blocked: ";
            // 
            // typeSelectComboBox
            // 
            this.typeSelectComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typeSelectComboBox.Location = new System.Drawing.Point(15, 345);
            this.typeSelectComboBox.Name = "typeSelectComboBox";
            this.typeSelectComboBox.Size = new System.Drawing.Size(337, 21);
            this.typeSelectComboBox.TabIndex = 2;
            this.typeSelectComboBox.SelectedIndexChanged += new System.EventHandler(this.typeSelectComboBox_SelectedIndexChanged);
            // 
            // roomsTreeView
            // 
            this.roomsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.roomsTreeView.FullRowSelect = true;
            this.roomsTreeView.HideSelection = false;
            this.roomsTreeView.Location = new System.Drawing.Point(15, 80);
            this.roomsTreeView.Name = "roomsTreeView";
            this.roomsTreeView.Size = new System.Drawing.Size(337, 259);
            this.roomsTreeView.TabIndex = 6;
            this.roomsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.roomsTreeView_AfterSelect);
            // 
            // recreateRoomsButton
            // 
            this.recreateRoomsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recreateRoomsButton.Location = new System.Drawing.Point(15, 17);
            this.recreateRoomsButton.Name = "recreateRoomsButton";
            this.recreateRoomsButton.Size = new System.Drawing.Size(248, 24);
            this.recreateRoomsButton.TabIndex = 0;
            this.recreateRoomsButton.Text = "recreate all spaces and graph";
            this.recreateRoomsButton.Click += new System.EventHandler(this.recreateRoomsButton_Click);
            // 
            // graphTabPage
            // 
            this.graphTabPage.Controls.Add(this.graphTreeView);
            this.graphTabPage.Controls.Add(this.sizeTrackBar);
            this.graphTabPage.Controls.Add(this.removeDoorDoorEdgesButton);
            this.graphTabPage.ImageIndex = 12;
            this.graphTabPage.Location = new System.Drawing.Point(4, 67);
            this.graphTabPage.Name = "graphTabPage";
            this.graphTabPage.Size = new System.Drawing.Size(363, 478);
            this.graphTabPage.TabIndex = 2;
            this.graphTabPage.Text = "graph";
            // 
            // graphTreeView
            // 
            this.graphTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphTreeView.FullRowSelect = true;
            this.graphTreeView.HideSelection = false;
            this.graphTreeView.Location = new System.Drawing.Point(15, 78);
            this.graphTreeView.Name = "graphTreeView";
            this.graphTreeView.Size = new System.Drawing.Size(337, 366);
            this.graphTreeView.TabIndex = 9;
            this.graphTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.graphTreeView_AfterSelect);
            // 
            // sizeTrackBar
            // 
            this.sizeTrackBar.LargeChange = 10;
            this.sizeTrackBar.Location = new System.Drawing.Point(248, 14);
            this.sizeTrackBar.Maximum = 100;
            this.sizeTrackBar.Minimum = 2;
            this.sizeTrackBar.Name = "sizeTrackBar";
            this.sizeTrackBar.Size = new System.Drawing.Size(104, 45);
            this.sizeTrackBar.TabIndex = 11;
            this.sizeTrackBar.TickFrequency = 10;
            this.sizeTrackBar.Value = 10;
            this.sizeTrackBar.Scroll += new System.EventHandler(this.sizeTrackBar_Scroll);
            // 
            // removeDoorDoorEdgesButton
            // 
            this.removeDoorDoorEdgesButton.Location = new System.Drawing.Point(15, 14);
            this.removeDoorDoorEdgesButton.Name = "removeDoorDoorEdgesButton";
            this.removeDoorDoorEdgesButton.Size = new System.Drawing.Size(135, 23);
            this.removeDoorDoorEdgesButton.TabIndex = 10;
            this.removeDoorDoorEdgesButton.Text = "Remove door-door edges";
            this.removeDoorDoorEdgesButton.UseVisualStyleBackColor = true;
            this.removeDoorDoorEdgesButton.Click += new System.EventHandler(this.removeDoorDoorEdgesButton_Click);
            // 
            // PFTabPage
            // 
            this.PFTabPage.Controls.Add(this.groupBox1);
            this.PFTabPage.Controls.Add(this.butPause);
            this.PFTabPage.Controls.Add(this.butStop);
            this.PFTabPage.Controls.Add(this.dNumberRobots);
            this.PFTabPage.Controls.Add(this.label7);
            this.PFTabPage.Controls.Add(this.butStart);
            this.PFTabPage.Location = new System.Drawing.Point(4, 67);
            this.PFTabPage.Name = "PFTabPage";
            this.PFTabPage.Size = new System.Drawing.Size(363, 478);
            this.PFTabPage.TabIndex = 6;
            this.PFTabPage.Text = "PF";
            this.PFTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtPort);
            this.groupBox1.Controls.Add(this.txtIP);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(11, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(339, 73);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "UDP";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(63, 40);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 20);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "1234";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(63, 12);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(100, 20);
            this.txtIP.TabIndex = 2;
            this.txtIP.Text = "127.0.0.1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(22, 45);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Port:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "IP:";
            // 
            // butPause
            // 
            this.butPause.Enabled = false;
            this.butPause.Location = new System.Drawing.Point(145, 108);
            this.butPause.Name = "butPause";
            this.butPause.Size = new System.Drawing.Size(75, 23);
            this.butPause.TabIndex = 4;
            this.butPause.Text = "Pause";
            this.butPause.UseVisualStyleBackColor = true;
            this.butPause.Visible = false;
            this.butPause.Click += new System.EventHandler(this.butPause_Click);
            // 
            // butStop
            // 
            this.butStop.Location = new System.Drawing.Point(238, 108);
            this.butStop.Name = "butStop";
            this.butStop.Size = new System.Drawing.Size(75, 23);
            this.butStop.TabIndex = 3;
            this.butStop.Text = "Stop";
            this.butStop.UseVisualStyleBackColor = true;
            this.butStop.Click += new System.EventHandler(this.butStop_Click);
            // 
            // dNumberRobots
            // 
            this.dNumberRobots.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.dNumberRobots.Location = new System.Drawing.Point(105, 7);
            this.dNumberRobots.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.dNumberRobots.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dNumberRobots.Name = "dNumberRobots";
            this.dNumberRobots.Size = new System.Drawing.Size(93, 20);
            this.dNumberRobots.TabIndex = 2;
            this.dNumberRobots.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Number of robots:";
            // 
            // butStart
            // 
            this.butStart.Location = new System.Drawing.Point(52, 108);
            this.butStart.Name = "butStart";
            this.butStart.Size = new System.Drawing.Size(75, 23);
            this.butStart.TabIndex = 0;
            this.butStart.Text = "Start";
            this.butStart.UseVisualStyleBackColor = true;
            this.butStart.Click += new System.EventHandler(this.butStart_Click);
            // 
            // MazeEditorForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(934, 635);
            this.Controls.Add(this.viewPanel);
            this.Controls.Add(this.verticalSplitter);
            this.Controls.Add(this.topHorizontalSplitter);
            this.Controls.Add(this.leftMenuPanel);
            this.Controls.Add(this.mainToolBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(472, 424);
            this.Name = "MazeEditorForm";
            this.Text = "Maze Editor";
            this.viewPanel.ResumeLayout(false);
            this.leftMenuPanel.ResumeLayout(false);
            this.objectSelectorTabControl.ResumeLayout(false);
            this.mazeTabPage.ResumeLayout(false);
            this.mazeTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.widthNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gravityNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightNumericUpDown)).EndInit();
            this.wallsTabPage.ResumeLayout(false);
            this.wallsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wallHeightNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wallWidthNumericUpDown)).EndInit();
            this.robotsTabPage.ResumeLayout(false);
            this.robotsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.robotHeightNumericUpDown)).EndInit();
            this.roomsTabPage.ResumeLayout(false);
            this.roomsTabPage.PerformLayout();
            this.roomPropertiesPanel.ResumeLayout(false);
            this.roomPropertiesPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.roomExpPersonCountNumericUpDown)).EndInit();
            this.doorPropertiesPanel.ResumeLayout(false);
            this.doorPropertiesPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gateBlockedNumericUpDown)).EndInit();
            this.graphTabPage.ResumeLayout(false);
            this.graphTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sizeTrackBar)).EndInit();
            this.PFTabPage.ResumeLayout(false);
            this.PFTabPage.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dNumberRobots)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MazeEditorForm.numberFormatInfo = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.CurrentCulture.NumberFormat.Clone();
            MazeEditorForm.numberFormatInfo.NumberDecimalSeparator = ".";
            Application.Run(new MazeEditorForm());
        }

        private void CreateMazeGraphics()
        {
            if (mazePanelBitmap != null)
            {
                mazePanelBitmap.Dispose();
            }
            mazePanelBitmap = new Bitmap(mazePanel.Width, mazePanel.Height);
            if (mazeBitmapGraphics != null)
            {
                mazeBitmapGraphics.Dispose();
            }
            mazeBitmapGraphics = Graphics.FromImage(mazePanelBitmap);
            //mazeBitmapGraphics.TranslateTransform((float)mazePanel.Width / 2, (float)mazePanel.Height / 2);
            mazeBitmapGraphics.ScaleTransform(zoom, zoom);

            invertedMazeMatrix = mazeBitmapGraphics.Transform;
            invertedMazeMatrix.Invert();

            if (mazeBitmapStaticGraphics != null)
            {
                mazeBitmapStaticGraphics.Dispose();
            }
            mazeBitmapStaticGraphics = Graphics.FromImage(mazePanelBitmap);

            if (mazeGraphics != null)
            {
                mazeGraphics.Dispose();
            }
            mazeGraphics = mazePanel.CreateGraphics();
        }

        private void CreateNewMaze()
        {
            if (mazeWalls != null || mazeRobots != null)
            {
                if (MessageBox.Show("Current Maze will be lost. Do you want to proceed?", "Create new Maze", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    != DialogResult.Yes)
                {
                    return;
                }
            }
            mazeWalls = new ArrayList();
            mazeRobots = new ArrayList();
            mazeVictims = new ArrayList();
            mazeRooms = new ArrayList();
            mazeGraph = new MazeGraph();
            MazeIdentifiable.ClearBusyIdsCache();

            recreateRoomsTreeView();
            recreateGraphTreeView();
            RefreshMazeWallsTreeView();


            mazePanel.Size = new Size((int)((float)widthNumericUpDown.Value * 100 * zoom), (int)((float)heightNumericUpDown.Value * 100 * zoom));
            CreateMazeGraphics();

            createWallInProgress = false;
            moveInProgress = false;
            snapToAngle = false;
            snapToGrid = false;

            robotNameIndex = 0;

            viewPanel.AutoScrollPosition = new Point(mazePanel.Width / -2, mazePanel.Width / -2);
        }


        private void LoadMaze()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Robolog Json file (*.roson)|*.roson";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            MazeIdentifiable.ClearBusyIdsCache();

            JsonHelper jsonHelper = new JsonHelper();
            jsonHelper.LoadAll(fileDialog.FileName);

            mazeWalls = jsonHelper.mazeWalls;
            mazeRobots = jsonHelper.mazeRobots;
            mazeVictims = jsonHelper.mazeVictims;
            mazeGraph = jsonHelper.mazeGraph;
            mazeRooms = jsonHelper.mazeSpaces;


            double maxX = 500;
            double maxY = 500;
            foreach (MazeWall mazeWall in mazeWalls)
            {
                maxX = Math.Max(maxX, Math.Max(mazeWall.points[0].X, mazeWall.points[1].X));
                maxY = Math.Max(maxY, Math.Max(mazeWall.points[0].Y, mazeWall.points[1].Y));
            }
            widthNumericUpDown.Value = (decimal)(maxX + 10) / 100;
            heightNumericUpDown.Value = (decimal)(maxY + 10) / 100;


            recreateRoomsTreeView();
            recreateGraphTreeView();
            RefreshMazeWallsTreeView();


            mazePanel_Paint(this, null);

        }

        private void SaveMaze()
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Robolog Json file (*.roson)|*.roson";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            JsonHelper.SaveAll(fileDialog.FileName, mazeWalls, mazeRobots, mazeVictims, mazeRooms, mazeGraph);




        }




        private void LoadMazeFromRoBoss()
        {
            if (mazeWalls != null || mazeRobots != null)
            {
                if (MessageBox.Show("Current Maze will be lost. Do you want to proceed?", "Load Maze", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    != DialogResult.Yes)
                {
                    return;
                }
            }

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "XML file (*.xml)|*.xml";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            mazeWalls = new ArrayList();
            mazeRobots = new ArrayList();
            mazeVictims = new ArrayList();
            mazeRooms = new ArrayList();
            mazeGraph = new MazeGraph();
            MazeIdentifiable.ClearBusyIdsCache();

            recreateRoomsTreeView();
            recreateGraphTreeView();
            RefreshMazeWallsTreeView();

            try
            {

                XmlTextReader reader = new XmlTextReader(fileDialog.FileName);
                reader.WhitespaceHandling = WhitespaceHandling.None;

                XmlDocument worldDocumnet = new XmlDocument();
                worldDocumnet.Load(reader);
                reader.Close();
                reader = null;

                XmlNode worldNode = worldDocumnet.SelectSingleNode("/World");		//whole settings document
                if (worldNode == null)
                {
                    MessageBox.Show("World node missing - probaly not a World Definition file.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                worldNameTextBox.Text = XmlHelper.GetStringAttributeFromNode(worldNode, "name", "");

                XmlNodeList robotsNodeList = worldDocumnet.SelectNodes("/World/Robots/Robot");
                XmlNodeList environmentNodeList = worldDocumnet.SelectNodes("/World/Environment/Geoms/Geom");

                XmlNode floorNode = environmentNodeList.Item(0);
                widthNumericUpDown.Value = (decimal)XmlHelper.GetDoubleAttributeFromNode(floorNode, "size_x");
                heightNumericUpDown.Value = (decimal)XmlHelper.GetDoubleAttributeFromNode(floorNode, "size_y");
                //mazePanel.Size = new Size(
                //    (int)(XmlHelper.GetDoubleAttributeFromNode(floorNode, "size_x") * zoom * 100),
                //    (int)(XmlHelper.GetDoubleAttributeFromNode(floorNode, "size_y") * zoom * 100));
                //CreateMazeGraphics();
                //widthNumericUpDown.Value = (decimal)mazePanel.Size.Width / 100;
                //heightNumericUpDown.Value = (decimal)mazePanel.Size.Height / 100;

                for (int i = 1; i < environmentNodeList.Count; i++)
                {
                    mazeWalls.Add(MazeWall.BuildFromXmlNode(environmentNodeList.Item(i)));
                }

                for (int i = 0; i < robotsNodeList.Count; i++)
                {
                    mazeRobots.Add(MazeRobot.BuildFromXmlNode(robotsNodeList.Item(i)));
                }





                worldDocumnet = null;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }





            createWallInProgress = false;
            moveInProgress = false;
            snapToAngle = false;
            snapToGrid = false;


            robotNameIndex = 0;

            viewPanel.AutoScrollPosition = new Point(0, 0);
            mazePanel_Paint(this, null);
        }

        private void mainToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            if (e.Button == newToolBarButton)
            {
                CreateNewMaze();
            }
            else if (e.Button == loadToolBarButton)
            {
                LoadMaze();
            }
            else if (e.Button == saveToolBarButton)
            {
                SaveMaze();
            }

            else if (e.Button == createToolBarButton)
            {
                moveToolBarButton.Pushed = false;
                deleteToolBarButton.Pushed = false;
                selectEditMode();
            }
            else if (e.Button == moveToolBarButton)
            {
                createToolBarButton.Pushed = false;
                deleteToolBarButton.Pushed = false;
                selectEditMode();
            }
            else if (e.Button == deleteToolBarButton)
            {
                moveToolBarButton.Pushed = false;
                createToolBarButton.Pushed = false;
                selectEditMode();
            }

            else if (e.Button == zoomPlusToolBarButton)
            {
                changeZoom(1);
            }
            else if (e.Button == zoomMinusToolBarButton)
            {
                changeZoom(-1);
            }
            else if (e.Button == widthPlusToolBarButton)
            {
                mazePanel.Width += mazePanel.Width / 5;
                CreateMazeGraphics();
                mazePanel_Paint(this, null);
            }
            else if (e.Button == heightPlusToolBarButton)
            {
                mazePanel.Height += mazePanel.Height / 5;
                CreateMazeGraphics();
                mazePanel_Paint(this, null);
            }
            else if (e.Button == importRoBossToolBarButton)
            {
                LoadMazeFromRoBoss();
            }
            else if (e.Button == exportRoBossToolBarButton)
            {
                SaveMazeForRoBOSS();
            }


        }

        private void changeZoom(int zoomDirection)
        {
            float oldZoom = zoom;

            if (zoomDirection > 0)
                zoom *= zoomStep;
            else if (zoomDirection < 0)
                zoom /= zoomStep;

            float maxZoom = 20000 / (float)(Math.Max(widthNumericUpDown.Value, heightNumericUpDown.Value) * 100);
            float minZoom = 1 / zoomStep * (float)Math.Min((decimal)viewPanel.Width / (widthNumericUpDown.Value * 100), (decimal)viewPanel.Height / (heightNumericUpDown.Value * 100));

            if ((zoom > maxZoom && oldZoom < zoom) || (zoom < minZoom && oldZoom > zoom))
                zoom = oldZoom;


            mazePanel.Size = new Size((int)((float)widthNumericUpDown.Value * 100 * zoom), (int)((float)heightNumericUpDown.Value * 100 * zoom));
            CreateMazeGraphics();
            mazePanel_Paint(this, null);

        }

        private void selectEditMode()
        {
            if (createToolBarButton.Pushed)
            {
                if (objectSelectorTabControl.SelectedTab == wallsTabPage)
                    SetEditMode(EditMode.CreateWall);
                else if (objectSelectorTabControl.SelectedTab == robotsTabPage)
                    SetEditMode(EditMode.CreateRobot);
                else if (objectSelectorTabControl.SelectedTab == victimsTabPage)
                    SetEditMode(EditMode.CreateVictim);
                else if (objectSelectorTabControl.SelectedTab == graphTabPage)
                    SetEditMode(EditMode.CreateNode);
                else
                    SetEditMode(EditMode.Idle);
            }
            else if (moveToolBarButton.Pushed)
            {
                if (objectSelectorTabControl.SelectedTab == wallsTabPage)
                    SetEditMode(EditMode.MoveWall);
                else if (objectSelectorTabControl.SelectedTab == robotsTabPage)
                    SetEditMode(EditMode.MoveRobot);
                else if (objectSelectorTabControl.SelectedTab == victimsTabPage)
                    SetEditMode(EditMode.MoveVictim);
                else if (objectSelectorTabControl.SelectedTab == graphTabPage)
                    SetEditMode(EditMode.MoveNode);
                else
                    SetEditMode(EditMode.Idle);
            }
            else if (deleteToolBarButton.Pushed)
            {
                if (objectSelectorTabControl.SelectedTab == wallsTabPage)
                    SetEditMode(EditMode.DeleteWall);
                else if (objectSelectorTabControl.SelectedTab == robotsTabPage)
                    SetEditMode(EditMode.DeleteRobot);
                else if (objectSelectorTabControl.SelectedTab == victimsTabPage)
                    SetEditMode(EditMode.DeleteVictim);
                else
                    SetEditMode(EditMode.Idle);
            }

            if (objectSelectorTabControl.SelectedTab == roomsTabPage)
                SetEditMode(EditMode.SelectRoom);

        }


        private void mazePanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            mazeBitmapStaticGraphics.Clear(mazeBackColor);
            foreach (MazeWall wall in mazeWalls)
            {
                Pen wallPen = new Pen(wall.Color, wall.Width);
                mazeBitmapGraphics.DrawLine(wallPen, wall.points[0], wall.points[1]);
                wallPen.Dispose();
            }
            if (selectedWall != null)
            {
                mazeBitmapGraphics.DrawLine(selectedWallPen, selectedWall.points[0], selectedWall.points[1]);
            }

            int size = sizeTrackBar.Value;
            int sizeOffset = sizeTrackBar.Value / 2;

            foreach (MazeRobot robot in mazeRobots)
            {
                mazeBitmapGraphics.FillEllipse(robotBrush, robot.position.X - sizeOffset, robot.position.Y - sizeOffset, size, size);
            }
            foreach (MazeVictim victim in mazeVictims)
            {
                mazeBitmapGraphics.FillEllipse(victimBrush, victim.position.X - sizeOffset / 2, victim.position.Y - sizeOffset, size / 2, size);
            }
            if (objectSelectorTabControl.SelectedTab == graphTabPage && mazeRooms != null)
            {
                foreach (MazeSpace room in mazeRooms)
                    DrawRoom(room, false);
                foreach (MazeNode node in mazeGraph.MazeGraphNodes)
                {
                    if (node.Door != null)
                        mazeBitmapGraphics.FillEllipse(graphBrush, (float)node.position.x - sizeOffset, (float)node.position.y - sizeOffset, size, size);
                    else
                        mazeBitmapGraphics.FillEllipse(graphBrush, (float)node.position.x - sizeOffset * 2, (float)node.position.y - sizeOffset * 2, size * 2, size * 2);
                }
                if (graphTreeView.SelectedNode != null && graphTreeView.SelectedNode.Tag as MazeArc != null)
                {
                    MazeArc arc = graphTreeView.SelectedNode.Tag as MazeArc;
                    mazeBitmapGraphics.DrawLine(selectedGraphPen, (float)arc.from.position.x, (float)arc.from.position.y, (float)arc.to.position.x, (float)arc.to.position.y);
                    MazeNode node = graphTreeView.SelectedNode.Parent.Tag as MazeNode;
                    mazeBitmapGraphics.DrawEllipse(selectedGraphPen, (float)node.position.x - sizeOffset, (float)node.position.y - sizeOffset, size, size);
                }
                if (graphTreeView.SelectedNode != null && graphTreeView.SelectedNode.Tag as MazeNode != null)
                {
                    MazeNode node = graphTreeView.SelectedNode.Tag as MazeNode;
                    mazeBitmapGraphics.DrawEllipse(selectedGraphPen, (float)node.position.x - sizeOffset, (float)node.position.y - sizeOffset, size, size);
                }

                foreach (MazeArc arc in mazeGraph.MazeGraphArcs)
                {
                    graphPen.CustomEndCap = new AdjustableArrowCap(size / 2, size / 2);
                    graphPen.CustomStartCap = new AdjustableArrowCap(1, size / 2);
                    Vector2D arcDir = new Vector2D(arc.from.position, arc.to.position);
                    arcDir.Normalize();
                    Point2D from = arc.from.position.GetTranslatedPoint(arcDir * (arc.from.Door == null ? size : size / 2));
                    arcDir.Inverse();
                    Point2D to = arc.to.position.GetTranslatedPoint(arcDir * (arc.to.Door == null ? size : size / 2));

                    mazeBitmapGraphics.DrawLine(graphPen, from, to);
                }
            }
            if (objectSelectorTabControl.SelectedTab == roomsTabPage && mazeRooms != null)
            {
                foreach (MazeSpace room in mazeRooms)
                {
                    DrawRoom(room, false);
                }
                if (roomsTreeView.SelectedNode != null)
                {
                    if (roomsTreeView.SelectedNode.Tag as MazeSpace != null)
                    {
                        DrawRoom(roomsTreeView.SelectedNode.Tag as MazeSpace, true);
                    }
                    else if (roomsTreeView.SelectedNode.Tag as MazeWall != null)
                    {
                        DrawRoom(roomsTreeView.SelectedNode.Parent.Tag as MazeSpace, true);
                        MazeWall door = roomsTreeView.SelectedNode.Tag as MazeWall;
                        mazeBitmapGraphics.DrawLine(selectedDoorPen, door.points[0], door.points[1]);
                    }

                    foreach (MazeWall door in mazeWalls)
                    {
                        if (door.MazeWallType != MazeWallType.gate)
                            continue;

                        Vector2D v = new Vector2D(door.points[0], door.points[1]);
                        v.MakePerpendicular();
                        v.Length = 20;
                        if (door.MazeDoorType == MazeGateType.door)
                        {
                            mazeBitmapGraphics.DrawLine(doorPenWithArrow, door.Center, door.Center.GetTranslatedPoint(v));
                            v.Inverse();
                            mazeBitmapGraphics.DrawLine(doorPenWithArrow, door.Center, door.Center.GetTranslatedPoint(v));
                        }
                        else if (door.MazeDoorType == MazeGateType.passage)
                        {
                            mazeBitmapGraphics.DrawLine(passagePenWithArrow, door.Center, door.Center.GetTranslatedPoint(v));
                            v.Inverse();
                            mazeBitmapGraphics.DrawLine(passagePenWithArrow, door.Center, door.Center.GetTranslatedPoint(v));
                        }
                        else if (door.MazeDoorType == MazeGateType.doorOneWayFromTo || door.MazeDoorType == MazeGateType.doorOneWayToFrom)      //draw direction
                        {
                            bool pointsIn = Math.Abs(v.AngleBetween(new Vector2D(door.Center, door.RoomFrom.CenterPoint))) < Math.PI / 2;
                            if ((!pointsIn && door.MazeDoorType == MazeGateType.doorOneWayToFrom) ||
                                (pointsIn && door.MazeDoorType == MazeGateType.doorOneWayFromTo))
                                v.Inverse();
                            mazeBitmapGraphics.DrawLine(doorPenWithArrow, door.Center, door.Center.GetTranslatedPoint(v));
                        }
                    }
                }

            }

            DrawStatic();

            mazeGraphics.DrawImage(mazePanelBitmap, 0, 0);
        }

        private void DrawRoom(MazeSpace room, bool fill)
        {
            PointF[] points = new PointF[room.Walls.Count];
            for (int i = 0; i < room.Points.Length; i++)
                points[i] = room.Points[i];

            mazeBitmapGraphics.DrawPolygon(roomPen, points);


            foreach (MazeWall door in room.Walls)
            {
                if (door.MazeWallType == MazeWallType.gate)
                {
                    mazeBitmapGraphics.DrawLine(doorPen, door.points[0], door.points[1]);
                }
            }

            if (fill)
            {
                mazeBitmapGraphics.FillPolygon(roomPen.Brush, points, System.Drawing.Drawing2D.FillMode.Winding);
            }

        }


        private void DrawStatic()
        {
            int deltaX = -viewPanel.AutoScrollPosition.X;
            int deltaY = -viewPanel.AutoScrollPosition.Y;

            if (snapToGrid)
            {
                for (int x = 10; x < mazePanelBitmap.Width; x += 10)
                {
                    for (int y = 10; y < mazePanelBitmap.Height; y += 10)
                    {
                        mazeBitmapStaticGraphics.FillRectangle(mazeStaticGridBrush, x, y, 1, 1);
                    }
                }
            }
            mazeBitmapStaticGraphics.DrawLine(mazeStaticPen, 10 + deltaX, 10 + deltaY, 10 + deltaX, 13 + deltaY);
            mazeBitmapStaticGraphics.DrawLine(mazeStaticPen, 10 + deltaX, 13 + deltaY, 110 + deltaX, 13 + deltaY);
            mazeBitmapStaticGraphics.DrawLine(mazeStaticPen, 110 + deltaX, 13 + deltaY, 110 + deltaX, 10 + deltaY);
            mazeBitmapStaticGraphics.DrawString((1 / zoom).ToString("f2") + "m", scaleFont, scaleBrush, 40 + deltaX, 20 + deltaY);
        }

        private void SetEditMode(EditMode newEditMode)
        {

            editMode = newEditMode;
            createWallInProgress = false;
            moveInProgress = false;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CancelAction();
            }

            if ((keyData & Keys.Control) != 0)
            {
                SnapToAngle = !SnapToAngle;
            }

            if ((keyData & Keys.Shift) != 0)
            {
                SnapToGrid = !SnapToGrid;
            }
            return false;
        }

        private void CancelAction()
        {
            if (moveInProgress)
            {
                tempPoints[0] = firstPoint;
                invertedMazeMatrix.TransformPoints(tempPoints);

                if (editMode == EditMode.MoveWall)
                {
                    selectedWall.points[movedWallPointIndex].X = tempPoints[0].X;
                    selectedWall.points[movedWallPointIndex].Y = tempPoints[0].Y;
                    updateWallDisplayedInfo();
                }
                else if (editMode == EditMode.MoveRobot)
                {
                    selectedRobot.position = tempPoints[0];
                }
                else if (editMode == EditMode.MoveNode)
                {
                    selectedNode.position.x = tempPoints[0].X;
                    selectedNode.position.y = tempPoints[0].Y;
                }
            }

            selectEditMode();
            mazePanel_Paint(this, null);
        }



        private void mazePanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CancelAction();
            }
            else if (editMode == EditMode.CreateWall)
            {
                AddWallPoint(e.X, e.Y);
            }
            else if (editMode == EditMode.CreateRobot)
            {
                CreateRobot(e.X, e.Y);
            }
            else if (editMode == EditMode.CreateVictim)
            {
                CreateVictim(e.X, e.Y);
            }
            else if (editMode == EditMode.CreateNode)
            {
                //CreateNode(e.X, e.Y);
            }
            else if (editMode == EditMode.MoveWall)
            {
                MoveWallPoint(e.X, e.Y);
            }
            else if (editMode == EditMode.MoveRobot)
            {
                MoveRobot(e.X, e.Y);
            }
            else if (editMode == EditMode.MoveVictim)
            {
                MoveVictim(e.X, e.Y);
            }
            else if (editMode == EditMode.MoveNode)
            {
                MoveNode(e.X, e.Y);
            }
            else if (editMode == EditMode.DeleteWall)
            {
                DeletWall(e.X, e.Y);
            }
            else if (editMode == EditMode.DeleteRobot)
            {
                DeleteRobot(e.X, e.Y);
            }
            else if (editMode == EditMode.DeleteVictim)
            {
                DeleteVictim(e.X, e.Y);
            }
            else if (editMode == EditMode.SelectRoom)
            {
                SelectRoomByClick(e.X, e.Y);
            }
        }

        private void SelectRoomByClick(int x, int y)
        {
            if (mazeRooms == null)
                return;
            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);

            foreach (MazeSpace room in mazeRooms)
                if (room.ContainsPoint(tempPoints[0]))
                    foreach (TreeNode roomNode in roomsTreeView.Nodes)
                        if (roomNode.Tag == room)
                        {
                            roomsTreeView.SelectedNode = roomNode;
                            return;
                        }
        }

        private void mazePanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            coordPoints[0].X = e.X;
            coordPoints[0].Y = e.Y;
            ProcessSnaps(ref coordPoints[0], coordPoints[0]);
            invertedMazeMatrix.TransformPoints(coordPoints);
            coordXLabel.Text = (coordPoints[0].X / 100).ToString("f3");
            coordYLabel.Text = (coordPoints[0].Y / 100).ToString("f3");


            if (editMode == EditMode.CreateWall)
            {
                if (createWallInProgress)
                {
                    tempPoints[1].X = e.X;
                    tempPoints[1].Y = e.Y;
                    ProcessSnaps(ref tempPoints[1], firstPoint);
                    mazePanel_Paint(this, null);
                    mazeGraphics.DrawLine(notCreatedWallPen, firstPoint, tempPoints[1]);
                }
            }
            else if (editMode == EditMode.MoveWall)
            {
                if (moveInProgress)
                {
                    tempPoints[1].X = e.X;
                    tempPoints[1].Y = e.Y;
                    ProcessSnaps(ref tempPoints[1], firstPoint);
                    invertedMazeMatrix.TransformPoints(tempPoints);
                    selectedWall.points[movedWallPointIndex].X = tempPoints[1].X;
                    selectedWall.points[movedWallPointIndex].Y = tempPoints[1].Y;
                    updateWallDisplayedInfo();
                    mazePanel_Paint(this, null);
                }
            }
            else if (editMode == EditMode.MoveRobot)
            {
                if (moveInProgress)
                {
                    tempPoints[1].X = e.X;
                    tempPoints[1].Y = e.Y;
                    ProcessSnaps(ref tempPoints[1], firstPoint);
                    invertedMazeMatrix.TransformPoints(tempPoints);
                    selectedRobot.position.X = tempPoints[1].X;
                    selectedRobot.position.Y = tempPoints[1].Y;
                    mazePanel_Paint(this, null);
                }
            }
            else if (editMode == EditMode.MoveVictim)
            {
                if (moveInProgress)
                {
                    tempPoints[1].X = e.X;
                    tempPoints[1].Y = e.Y;
                    ProcessSnaps(ref tempPoints[1], firstPoint);
                    invertedMazeMatrix.TransformPoints(tempPoints);
                    selectedVictim.position.X = tempPoints[1].X;
                    selectedVictim.position.Y = tempPoints[1].Y;
                    mazePanel_Paint(this, null);
                }
            }
            else if (editMode == EditMode.MoveNode)
            {
                if (moveInProgress)
                {
                    tempPoints[1].X = e.X;
                    tempPoints[1].Y = e.Y;
                    ProcessSnaps(ref tempPoints[1], firstPoint);
                    invertedMazeMatrix.TransformPoints(tempPoints);
                    if (selectedNode.Room.ContainsPoint(tempPoints[1]))  /// czy punkt w pokoju...
                    {
                        selectedNode.position.x = tempPoints[1].X;
                        selectedNode.position.y = tempPoints[1].Y;
                        mazePanel_Paint(this, null);
                    }
                }
            }


        }



        private void mazePanel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
                changeZoom(1);
            else if (e.Delta < 0)
                changeZoom(-1);


        }

        private void ProcessSnaps(ref PointF snappedPoint, PointF referencePoint)
        {
            if (snapToAngle)
            {
                if (Math.Abs(snappedPoint.X - referencePoint.X) < Math.Abs(snappedPoint.Y - referencePoint.Y))
                {
                    snappedPoint.X = referencePoint.X;
                }
                else
                {
                    snappedPoint.Y = referencePoint.Y;
                }
            }
            if (snapToGrid)
            {
                snappedPoint.X = (float)Math.Round(snappedPoint.X / 10) * 10;
                snappedPoint.Y = (float)Math.Round(snappedPoint.Y / 10) * 10;
            }
            if (snapToWall)
            {
                tempPoints[0] = snappedPoint;
                invertedMazeMatrix.TransformPoints(tempPoints);
                if (mazeWalls == null || mazeWalls.Count == 0)
                    return;
                double distance, minDistance = double.MaxValue;
                Point2D mindistancePoint = new Point2D(0, 0);
                foreach (MazeWall wall in mazeWalls)
                {
                    if (wall != selectedWall)
                    {
                        Point2D p = GeometryHelper.GetClosestPointOnSegment(wall.Segment, tempPoints[0], out distance);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            mindistancePoint = p;
                        }
                    }
                }
                tempPoints[0] = mindistancePoint;
                mazeBitmapGraphics.Transform.TransformPoints(tempPoints);
                snappedPoint = tempPoints[0];
            }
        }

        private void CreateRobot(int x, int y)
        {
            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);
            mazeRobots.Add(new MazeRobot(robotTypeTextBox.Text, robotNameTextBox.Text + robotNameIndex.ToString(), tempPoints[0], (float)robotHeightNumericUpDown.Value * 100));
            robotNameIndex++;
            mazePanel_Paint(this, null);
        }

        private void CreateVictim(int x, int y)
        {
            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);
            mazeVictims.Add(new MazeVictim(tempPoints[0]));
            mazePanel_Paint(this, null);
        }

        private void CreateNode(int x, int y)
        {
            throw new NotImplementedException();
            /*           if (mazeGraph == null)
                           return;
                       tempPoints[0].X = x;
                       tempPoints[0].Y = y;
                       invertedMazeMatrix.TransformPoints(tempPoints);
                       MazeGraphNode newNode = new MazeGraphNode(new Point2D(tempPoints[0].X, tempPoints[0].Y), MazeGraphNodeType.SpaceNode);
                       mazeGraph.AddNode(newNode);

                       foreach (MazeGraphNode oldNode in mazeGraph.MazeGraphNodes)
                       {
                           if (oldNode == newNode)
                               continue;
                           Segment2D edgeSegment = new Segment2D(oldNode.position.x, oldNode.position.y, newNode.position.x, newNode.position.y);
                           foreach (MazeWall wall in mazeWalls)
                           {
                               Segment2D wallSegment = new Segment2D(wall.points[0], wall.points[1]);
                               if (!double.IsNaN(wallSegment.GetIntersectionPoint(edgeSegment).x))
                               {
                                   edgeSegment = null;
                                   break;
                               }
                           }
                           if (edgeSegment != null)
                           {
                               mazeGraph.AddArc(oldNode, newNode);
                               mazeGraph.AddArc(newNode, oldNode);
                           }
                       }
                       mazePanel_Paint(this,null);
           */
        }

        private void AddWallPoint(int x, int y)
        {

            if (createWallInProgress)
            {
                //				wallPoints[1].X = x;
                //				wallPoints[1].Y = y;
                tempPoints[0] = firstPoint;
                firstPoint = tempPoints[1];
                invertedMazeMatrix.TransformPoints(tempPoints);
                MazeWall newWall = new MazeWall(tempPoints[0], tempPoints[1], (float)wallWidthNumericUpDown.Value * 100, (float)wallHeightNumericUpDown.Value * 100, wallColor);
                if (createDoorWallCheckBox.Checked)
                {
                    newWall.MazeWallType = MazeWallType.gate;
                    newWall.Width = 0;
                    newWall.Height = 0;
                    newWall.Color = Color.Black;
                }
                mazeWalls.Add(newWall);
                RefreshMazeWallsTreeView();
                mazeWallsListBox.SelectedItem = newWall;
            }
            else
            {
                createWallInProgress = true;
                firstPoint.X = x;
                firstPoint.Y = y;
                if (snapToGrid)
                {
                    firstPoint.X = (float)Math.Round(firstPoint.X / 10) * 10;
                    firstPoint.Y = (float)Math.Round(firstPoint.Y / 10) * 10;
                }

            }
            mazePanel_Paint(this, null);
        }

        private void MoveWallPoint(int x, int y)
        {

            if (moveInProgress)
            {
                moveInProgress = false;
            }
            else
            {
                tempPoints[0].X = x;
                tempPoints[0].Y = y;
                invertedMazeMatrix.TransformPoints(tempPoints);
                float minDist = float.MaxValue;
                float dist = 0.0f;
                selectedWall = null;
                foreach (MazeWall wall in mazeWalls)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        dist = GetLength(wall.points[i], tempPoints[0]);
                        if (dist < minDist)
                        {
                            selectedWall = wall;
                            minDist = dist;
                            movedWallPointIndex = i;
                        }
                    }
                }
                if (selectedWall != null)
                {
                    moveInProgress = true;
                    tempPoints[0] = selectedWall.points[movedWallPointIndex];
                    mazeBitmapGraphics.Transform.TransformPoints(tempPoints);
                    firstPoint = tempPoints[0];
                }
                mazeWallsListBox.SelectedItem = selectedWall;
            }
        }

        private void MoveRobot(int x, int y)
        {

            if (moveInProgress)
            {
                moveInProgress = false;
            }
            else
            {
                tempPoints[0].X = x;
                tempPoints[0].Y = y;
                invertedMazeMatrix.TransformPoints(tempPoints);
                float minDist = float.MaxValue;
                float dist = 0.0f;
                selectedRobot = null;
                foreach (MazeRobot robot in mazeRobots)
                {
                    dist = GetLength(robot.position, tempPoints[0]);
                    if (dist < minDist)
                    {
                        selectedRobot = robot;
                        minDist = dist;
                    }
                }
                if (selectedRobot != null)
                {
                    moveInProgress = true;
                    tempPoints[0] = selectedRobot.position;
                    firstPoint = tempPoints[0];
                    mazeBitmapGraphics.Transform.TransformPoints(tempPoints);
                }
            }
        }
        private void MoveVictim(int x, int y)
        {

            if (moveInProgress)
            {
                moveInProgress = false;
            }
            else
            {
                tempPoints[0].X = x;
                tempPoints[0].Y = y;
                invertedMazeMatrix.TransformPoints(tempPoints);
                float minDist = float.MaxValue;
                float dist = 0.0f;
                selectedVictim = null;
                foreach (MazeVictim victim in mazeVictims)
                {
                    dist = GetLength(victim.position, tempPoints[0]);
                    if (dist < minDist)
                    {
                        selectedVictim = victim;
                        minDist = dist;
                    }
                }
                if (selectedVictim != null)
                {
                    moveInProgress = true;
                    tempPoints[0] = selectedVictim.position;
                    firstPoint = tempPoints[0];
                    mazeBitmapGraphics.Transform.TransformPoints(tempPoints);
                }
            }
        }
        private void MoveNode(int x, int y)
        {

            if (moveInProgress)
            {
                moveInProgress = false;
            }
            else
            {
                tempPoints[0].X = x;
                tempPoints[0].Y = y;
                invertedMazeMatrix.TransformPoints(tempPoints);
                float minDist = float.MaxValue;
                float dist = 0.0f;
                selectedNode = null;
                if (mazeGraph != null)
                {
                    foreach (MazeNode n in mazeGraph.MazeGraphNodes)
                    {
                        dist = (float)Vector2D.GetLength(n.position.x - tempPoints[0].X, n.position.y - tempPoints[0].Y);
                        if (dist < minDist)
                        {
                            selectedNode = n;
                            minDist = dist;
                        }
                    }
                }
                if (selectedNode != null)
                {
                    moveInProgress = true;
                    tempPoints[0] = new PointF((float)selectedNode.position.x, (float)selectedNode.position.y);
                    firstPoint = tempPoints[0];
                    mazeBitmapGraphics.Transform.TransformPoints(tempPoints);
                }
            }
        }

        private void DeletWall(int x, int y)
        {
            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);
            float minDist = float.MaxValue;
            float dist = 0.0f;
            selectedWall = null;
            foreach (MazeWall wall in mazeWalls)
            {
                dist = Math.Min(GetLength(wall.points[0], tempPoints[0]), GetLength(wall.points[1], tempPoints[0]));
                if (dist < minDist)
                {
                    selectedWall = wall;
                    minDist = dist;
                }
            }
            if (selectedWall != null)
            {
                mazeWalls.Remove(selectedWall);
                RefreshMazeWallsTreeView();
            }
            mazePanel_Paint(this, null);
        }


        private void DeleteRobot(int x, int y)
        {
            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);
            float minDist = float.MaxValue;
            float dist = 0.0f;
            selectedRobot = null;
            foreach (MazeRobot robot in mazeRobots)
            {
                dist = GetLength(robot.position, tempPoints[0]);
                if (dist < minDist)
                {
                    selectedRobot = robot;
                    minDist = dist;
                }
            }
            if (selectedRobot != null)
            {
                mazeRobots.Remove(selectedRobot);
            }

            mazePanel_Paint(this, null);
        }
        private void DeleteVictim(int x, int y)
        {
            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);
            float minDist = float.MaxValue;
            float dist = 0.0f;
            selectedVictim = null;
            foreach (MazeVictim victim in mazeVictims)
            {
                dist = GetLength(victim.position, tempPoints[0]);
                if (dist < minDist)
                {
                    selectedVictim = victim;
                    minDist = dist;
                }
            }
            if (selectedVictim != null)
            {
                mazeVictims.Remove(selectedVictim);
            }

            mazePanel_Paint(this, null);
        }

        private float GetLength(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }


        private void SaveMazeForRoBOSS()
        {
            if (mazeRobots.Count == 0)
            {
                if (MessageBox.Show("There are no robots in the maze. Generated file will not be accepted by RoBOSSController.\nAre you sure you want to save the maze?", "Incomplete definition", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "XML file (*.xml)|*.xml";
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            XmlTextWriter writer;
            try
            {
                writer = new XmlTextWriter(fileDialog.FileName, null);
            }
            catch
            {

                return;
            }
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("World");
            writer.WriteAttributeString("name", worldNameTextBox.Text);
            writer.WriteAttributeString("gravity", gravityNumericUpDown.Value.ToString(MazeEditorForm.numberFormatInfo));
            writer.WriteStartElement("Robots");
            foreach (MazeRobot robot in mazeRobots)
            {
                robot.WriteXMLDefinitionNode(writer);
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Environment");
            writer.WriteStartElement("Geoms");
            writer.WriteStartElement("Geom");
            writer.WriteAttributeString("type", "box");
            writer.WriteAttributeString("position_z", "-0.5");
            writer.WriteAttributeString("size_x", (mazePanel.Width / (zoom * 100)).ToString(MazeEditorForm.numberFormatInfo));
            writer.WriteAttributeString("size_y", (mazePanel.Height / (zoom * 100)).ToString(MazeEditorForm.numberFormatInfo));
            writer.WriteAttributeString("size_z", "1.0");
            writer.WriteAttributeString("color", "00000000");
            writer.WriteEndElement();

            foreach (MazeWall wall in mazeWalls)
            {
                wall.WriteXMLDefinitionNode(writer);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Flush();
            writer.Close();

        }

        private void wallColorPanel_Click(object sender, System.EventArgs e)
        {
            if (wallColorDialog.ShowDialog() == DialogResult.OK)
            {
                wallColor = wallColorDialog.Color;
                wallColorPanel.BackColor = wallColor;
            }

        }



        private void robotTypeTextBox_TextChanged(object sender, System.EventArgs e)
        {
            robotNameIndex = 0;
        }


        private void snapToGridLabel_Click(object sender, System.EventArgs e)
        {
            SnapToGrid = !SnapToGrid;
        }

        private void snapToAngleLabel_Click(object sender, System.EventArgs e)
        {
            SnapToAngle = !SnapToAngle;
        }

        private void snapToWallLabel_Click(object sender, System.EventArgs e)
        {
            SnapToWall = !SnapToWall;
        }



        private void widthHeightNumericUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            mazePanel.Size = new Size((int)((float)widthNumericUpDown.Value * 100 * zoom), (int)((float)heightNumericUpDown.Value * 100 * zoom));
            CreateMazeGraphics();
            mazePanel_Paint(this, null);
        }

        private void objectSelectorTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            selectEditMode();
            mazePanel_Paint(this, null);
        }

        private void recreateRoomsButton_Click(object sender, EventArgs e)
        {
            if (mazeGraph != null && MessageBox.Show("This operation will remove the rooms and the graph. It cannot be undone.\n Are you sure?", "", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;
            mazeGraph = new MazeGraph();
            mazeRooms = new ArrayList();

            roomsTreeView.Nodes.Clear();

            RoomsGraphBuilder roomsGraphBuilder = new RoomsGraphBuilder();
            if (!ConvexOnlyCheckBox.Checked)
            {
                roomsGraphBuilder.GenerateConvexRoomsOnly = false;
                roomsGraphBuilder.SplitRoomsIntoConvex = false;
            }
            foreach (MazeWall wall in mazeWalls)
            {
                if (wall.MazeWallType == MazeWallType.wall)
                    roomsGraphBuilder.AddWall(new Point2D(wall.points[0].X, wall.points[0].Y), new Point2D(wall.points[1].X, wall.points[1].Y));
                else
                    roomsGraphBuilder.AddDoor(new Point2D(wall.points[0].X, wall.points[0].Y), new Point2D(wall.points[1].X, wall.points[1].Y));
            }
            roomsGraphBuilder.BuildRegion();

            Hashtable mazeNodeByRoomasGraphNodes = new Hashtable();
            Hashtable mazeWallsByRoomasGraphWalls = new Hashtable();

            ArrayList oldMazeWalls = mazeWalls;
            mazeWalls = new ArrayList();
            foreach (Room graphBuilderRoom in roomsGraphBuilder.Rooms)
            {
                ArrayList roomWalls = new ArrayList();
                foreach (Wall graphBuilderWall in graphBuilderRoom.Walls)
                {
                    bool oldWallFound = false;
                    foreach (MazeWall mazeWall in oldMazeWalls)     //find old 
                    {
                        if (mazeWall.Segment.ContainsSegment(graphBuilderWall.WallSegment))
                        {
                            oldWallFound = true;
                            MazeWall roomWall = new MazeWall(graphBuilderWall.From, graphBuilderWall.To, mazeWall.Width, mazeWall.Height, mazeWall.Color);
                            roomWall.MazeWallType = (graphBuilderWall.Type == Wall.WallType.Solid ? MazeWallType.wall : MazeWallType.gate);
                            int indexOf = mazeWalls.IndexOf(roomWall);
                            if (indexOf >= 0)
                            {
                                roomWall = (MazeWall)mazeWalls[indexOf];
                            }
                            else
                            {
                                mazeWalls.Add(roomWall);
                                mazeWallsByRoomasGraphWalls[graphBuilderWall] = roomWall;
                            }
                            roomWalls.Add(roomWall);
                            break;
                        }
                    }
                    if (!oldWallFound)
                    {
                        MazeWall roomWall = new MazeWall(graphBuilderWall.From, graphBuilderWall.To, 0, 0, Color.Black);
                        int indexOf = mazeWalls.IndexOf(roomWall);
                        if (indexOf >= 0)
                        {
                            roomWall = (MazeWall)mazeWalls[indexOf];
                        }
                        else
                        {
                            roomWall.MazeWallType = MazeWallType.gate;
                            mazeWalls.Add(roomWall);
                            mazeWallsByRoomasGraphWalls[graphBuilderWall] = roomWall;
                        }
                        roomWalls.Add(roomWall);
                    }
                }

                /*              ///reorder walls
                              ArrayList orderedRoomWalls = new ArrayList();
                              orderedRoomWalls.Add(roomWalls[0]);
                              roomWalls.RemoveAt(0);
                              Point2D nextPoint = (roomWalls[0] as Wall).To;
                              while (roomWalls.Count > 0)
                              {
                                  foreach (Wall wall in roomWalls)
                                      if (wall.From.GetDistance(nextPoint) < 0.01)
                                      {
                                          nextPoint = wall.From;
                                          roomWalls.Remove(wall);
                                          orderedRoomWalls.Add(wall);
                                          break;
                                      }
                                      else if (wall.To.GetDistance(nextPoint) < 0.01)
                                      {
                                          nextPoint = wall.To;
                                          roomWalls.Remove(wall);
                                          orderedRoomWalls.Add(wall);
                                          break;
                                      }
                              }
              */
                MazeSpace room = new MazeSpace(roomWalls);
                mazeRooms.Add(room);
                foreach (MazeWall roomWall in roomWalls)
                {
                    if (roomWall.RoomFrom == null)
                        roomWall.RoomFrom = room;
                    else
                        roomWall.RoomTo = room;
                }


                //////////////////////////////////////   nodes

                foreach (RoomsGraph.Node borderNode in graphBuilderRoom.BorderNodes)        //push door-nodes into rooms
                {
                    Vector2D v = new Vector2D(borderNode.DoorWall.From, borderNode.DoorWall.To);
                    v.MakePerpendicular();
                    v.Length = (double)wallWidthNumericUpDown.Value * 100;
                    if (Math.Abs(v.AngleBetween(new Vector2D(borderNode.DoorWall.Center, borderNode.Location))) > Math.PI / 2)
                        v.Inverse();
                    borderNode.Position.X += v.x;
                    borderNode.Position.Y += v.y;
                }

                room.CetralNode = new MazeNode(graphBuilderRoom.CentralNode.Location, room, null);
                mazeNodeByRoomasGraphNodes[graphBuilderRoom.CentralNode] = room.CetralNode;
                mazeGraph.AddNode(room.CetralNode);
                foreach (RoomsGraph.Node n in graphBuilderRoom.BorderNodes)
                {
                    MazeNode mazeGraphNode = new MazeNode(n.Location, room, (MazeWall)mazeWallsByRoomasGraphWalls[n.DoorWall]);
                    mazeNodeByRoomasGraphNodes[n] = mazeGraphNode;
                    room.BorderNodes.Add(mazeGraphNode);
                    mazeGraph.AddNode(mazeGraphNode);
                }
            }

            foreach (RoomsGraph.Arc arc in roomsGraphBuilder.Graph.Arcs)
            {
                if (mazeNodeByRoomasGraphNodes[arc.StartNode] != null && mazeNodeByRoomasGraphNodes[arc.EndNode] != null)
                    mazeGraph.AddArc((MazeNode)mazeNodeByRoomasGraphNodes[arc.StartNode], (MazeNode)mazeNodeByRoomasGraphNodes[arc.EndNode]);
            }

            recreateRoomsTreeView();
            recreateGraphTreeView();
            RefreshMazeWallsTreeView();

            mazePanel_Paint(this, null);
        }


        private void RefreshMazeWallsTreeView()
        {
            mazeWallsListBox.Items.Clear();
            if (mazeWalls == null)
                return;

            foreach (MazeWall wall in mazeWalls)
            {
                mazeWallsListBox.Items.Add(wall);
            }

        }

        private void recreateRoomsTreeView()
        {
            roomsTreeView.Nodes.Clear();
            if (mazeRooms == null)
                return;

            foreach (MazeSpace room in mazeRooms)
            {
                TreeNode roomTreeNode = new TreeNode();
                roomTreeNode.Tag = room;
                roomTreeNode.Text = room.ID + " with " + room.Walls.Count + " walls around " + room.CenterPoint.ToString();
                roomsTreeView.Nodes.Add(roomTreeNode);
                foreach (MazeWall roomWall in room.Walls)
                {
                    if (roomWall.MazeWallType == MazeWallType.gate)
                    {
                        TreeNode doorTreeNode = new TreeNode();
                        doorTreeNode.Tag = roomWall;
                        doorTreeNode.Text = roomWall.ToString();
                        roomTreeNode.Nodes.Add(doorTreeNode);

                    }
                }
            }

        }

        private void recreateGraphTreeView()
        {
            graphTreeView.Nodes.Clear();
            if (mazeGraph == null)
                return;

            foreach (MazeNode node in mazeGraph.MazeGraphNodes)
            {
                ArrayList accessibleTargetNodes = new ArrayList();

                TreeNode nodenode = new TreeNode();
                nodenode.Tag = node;
                nodenode.Text = node.MazeGraphNodeType + ": " + node.position.ToString();
                graphTreeView.Nodes.Add(nodenode);
                // wêz³y dostêpne 
                //foreach (MazeGraphArc arc in node.OutgoingGraphArcs)
                //{
                //    if (accessibleTargetNodes.Contains(arc.EndNode))
                //    {
                //        arcsToRemove.Add(arc);
                //    }
                //    else
                //    {
                //        ArcTreeNode newNode = new ArcTreeNode(arc);
                //        nodenode.Nodes.Add(newNode);
                //        accessibleTargetNodes.Add(arc.EndNode);
                //    }
                //}
            }
        }


        private void roomsTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (roomsTreeView.SelectedNode == null)
                return;


            if (roomsTreeView.SelectedNode.Tag as MazeSpace != null)
            {
                typeSelectComboBox.Items.Clear();
                foreach (MazeSpaceType roomType in Enum.GetValues(typeof(MazeSpaceType)))
                    typeSelectComboBox.Items.Add(roomType);
                typeSelectComboBox.SelectedItem = (roomsTreeView.SelectedNode.Tag as MazeSpace).MazeRoomType;
                doorPropertiesPanel.Visible = false;
                roomPropertiesPanel.Visible = true;
                roomLengthLabel.Text = "space length: " + (roomsTreeView.SelectedNode.Tag as MazeSpace).Diameter.ToString("f2");
                roomAreaLabel.Text = "space area:    " + (roomsTreeView.SelectedNode.Tag as MazeSpace).Area.ToString("f2");
                roomFunctionTextBox.Text = (roomsTreeView.SelectedNode.Tag as MazeSpace).Function;
                spaceNameTextBox.Text = (roomsTreeView.SelectedNode.Tag as MazeSpace).Name;
                roomExpPersonCountNumericUpDown.Value = Math.Min((roomsTreeView.SelectedNode.Tag as MazeSpace).ExpectedPersonCount, roomExpPersonCountNumericUpDown.Maximum);

                if (joinWithRadioButton.Checked)
                {
                    if (previousSelectedRoom != null)
                        TryJoinRooms(previousSelectedRoom, roomsTreeView.SelectedNode.Tag as MazeSpace);

                    joinWithRadioButton.Checked = false;
                }
                if (roomsTreeView.SelectedNode != null)
                    previousSelectedRoom = roomsTreeView.SelectedNode.Tag as MazeSpace;

            }
            else if (roomsTreeView.SelectedNode.Tag as MazeWall != null && (roomsTreeView.SelectedNode.Tag as MazeWall).MazeWallType == MazeWallType.gate)
            {
                typeSelectComboBox.Items.Clear();
                foreach (MazeGateType doorType in Enum.GetValues(typeof(MazeGateType)))
                    typeSelectComboBox.Items.Add(doorType);
                typeSelectComboBox.SelectedItem = (roomsTreeView.SelectedNode.Tag as MazeWall).MazeDoorType;

                doorPropertiesPanel.Visible = true;
                gateBlockedNumericUpDown.Value = (decimal)(roomsTreeView.SelectedNode.Tag as MazeWall).blocked;
                roomPropertiesPanel.Visible = false;

            }
            mazePanel_Paint(this, null);

        }

        /// <summary>
        /// 
        /// Common walls required 
        /// Common walls removed with nodes 
        /// 
        /// New Room from remaining walls 
        /// 
        /// All internal arcs removed 
        /// Central node recreated 
        /// Arcs recreated 
        /// 
        /// New Room added, old removed 
        /// 
        /// Trees recreated 
        /// New Room selected
        /// 
        /// </summary>
        /// <param name="firstRoom"></param>
        /// <param name="secondRoom"></param>
        private void TryJoinRooms(MazeSpace firstRoom, MazeSpace secondRoom)
        {
            ArrayList commonWalls = new ArrayList();
            foreach (MazeWall wall in firstRoom.Walls)
                if (secondRoom.Walls.Contains(wall))
                    commonWalls.Add(wall);

            if (commonWalls.Count == 0)
            {
                MessageBox.Show("Cannot join rooms -- common walls not found");
                return;
            }

            ArrayList newRoomWalls = new ArrayList();
            foreach (MazeWall wall in firstRoom.Walls)
                if (!commonWalls.Contains(wall))
                    newRoomWalls.Add(wall);
            foreach (MazeWall wall in secondRoom.Walls)
                if (!commonWalls.Contains(wall))
                    newRoomWalls.Add(wall);

            mazeGraph.RemoveNode(firstRoom.CetralNode);
            mazeGraph.RemoveNode(secondRoom.CetralNode);

            ArrayList nodesToRemove = new ArrayList();
            foreach (MazeWall wall in commonWalls)
            {
                mazeWalls.Remove(wall);
                foreach (MazeNode node in mazeGraph.MazeGraphNodes)
                    if (node.Door == wall)
                        nodesToRemove.Add(node);
                foreach (MazeNode node in nodesToRemove)
                    mazeGraph.RemoveNode(node);
            }

            foreach (MazeWall wall in commonWalls)
                mazeWalls.Remove(wall);

            MazeSpace newRoom = new MazeSpace(newRoomWalls);
            newRoom.CetralNode = new MazeNode(newRoom.CenterPoint, newRoom, null);
            mazeGraph.AddNode(newRoom.CetralNode);

            foreach (MazeWall wall in newRoomWalls)
            {
                foreach (MazeNode node in mazeGraph.MazeGraphNodes)
                    if (node.Door == wall && (node.Room == firstRoom || node.Room == secondRoom))
                    {
                        node.Room = newRoom;
                        mazeGraph.AddArc(node, newRoom.CetralNode);
                        mazeGraph.AddArc(newRoom.CetralNode, node);
                    }

            }

            mazeRooms.Remove(firstRoom);
            mazeRooms.Remove(secondRoom);
            mazeRooms.Add(newRoom);

            RefreshMazeWallsTreeView();
            recreateGraphTreeView();
            recreateRoomsTreeView();
        }


        private void removeDoorDoorEdgesButton_Click(object sender, EventArgs e)
        {
            ArrayList arcsToRemove = new ArrayList();
            foreach (MazeArc arc in mazeGraph.MazeGraphArcs)
            {
                if (arc.from.Door != null && arc.to.Door != null && arc.from.Room == arc.to.Room)
                    arcsToRemove.Add(arc);
            }
            foreach (MazeArc arc in arcsToRemove)
            {
                mazeGraph.RemoveArc(arc);
            }
            mazePanel_Paint(this, null);
        }

        private void removeRoomButton_Click(object sender, EventArgs e)
        {
            if (roomsTreeView.SelectedNode == null)
                return;
            if (roomsTreeView.SelectedNode.Tag as MazeSpace == null)
                return;

            RemoveRoom(roomsTreeView.SelectedNode.Tag as MazeSpace);

            recreateGraphTreeView();
            recreateRoomsTreeView();
        }

        protected void RemoveRoom(MazeSpace room)
        {
            mazeRooms.Remove(room);

            ArrayList nodesToRemove = new ArrayList();
            foreach (MazeNode node in mazeGraph.MazeGraphNodes)
                if (node.Room == room)
                    nodesToRemove.Add(node);
            foreach (MazeNode node in nodesToRemove)
                mazeGraph.RemoveNode(node);


        }


        private void typeSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (roomsTreeView.SelectedNode == null)
                return;

            if (roomsTreeView.SelectedNode.Tag as MazeSpace != null)
            {
                (roomsTreeView.SelectedNode.Tag as MazeSpace).MazeRoomType = (MazeSpaceType)typeSelectComboBox.SelectedItem;
            }
            else if (roomsTreeView.SelectedNode.Tag as MazeWall != null && (roomsTreeView.SelectedNode.Tag as MazeWall).MazeWallType == MazeWallType.gate)
            {
                (roomsTreeView.SelectedNode.Tag as MazeWall).MazeDoorType = (MazeGateType)typeSelectComboBox.SelectedItem;

                MazeWall door = roomsTreeView.SelectedNode.Tag as MazeWall;

                MazeNode nodeFrom = null, nodeTo = null;
                foreach (MazeNode node in mazeGraph.MazeGraphNodes)
                {
                    if (node.Door == door)
                    {
                        if (node.Room == door.RoomFrom)
                            nodeFrom = node;
                        else if (node.Room == door.RoomTo)
                            nodeTo = node;
                    }
                }

                if (nodeFrom != null && nodeTo != null)
                {
                    mazeGraph.RemoveArc(new MazeArc(nodeFrom, nodeTo));
                    mazeGraph.RemoveArc(new MazeArc(nodeTo, nodeFrom));

                    if (door.MazeDoorType == MazeGateType.door || door.MazeDoorType == MazeGateType.passage)
                    {
                        mazeGraph.AddArc(nodeFrom, nodeTo);
                        mazeGraph.AddArc(nodeTo, nodeFrom);
                    }
                    else if (door.MazeDoorType == MazeGateType.doorOneWayFromTo)
                    {
                        mazeGraph.AddArc(nodeFrom, nodeTo);
                    }
                    else if (door.MazeDoorType == MazeGateType.doorOneWayToFrom)
                    {
                        mazeGraph.AddArc(nodeTo, nodeFrom);
                    }

                }

                recreateGraphTreeView();

            }
            mazePanel_Paint(this, null);

        }


        private void gateBlockedNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (roomsTreeView.SelectedNode != null && roomsTreeView.SelectedNode.Tag as MazeWall != null)
                (roomsTreeView.SelectedNode.Tag as MazeWall).blocked = (double)gateBlockedNumericUpDown.Value;
        }

        private void roomFunctionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (roomsTreeView.SelectedNode != null && roomsTreeView.SelectedNode.Tag as MazeSpace != null)
                (roomsTreeView.SelectedNode.Tag as MazeSpace).Function = roomFunctionTextBox.Text;

        }
        private void spaceNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (roomsTreeView.SelectedNode != null && roomsTreeView.SelectedNode.Tag as MazeSpace != null)
                (roomsTreeView.SelectedNode.Tag as MazeSpace).Name = spaceNameTextBox.Text;
        }


        private void roomExpPersonCountNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (roomsTreeView.SelectedNode != null && roomsTreeView.SelectedNode.Tag as MazeSpace != null)
                (roomsTreeView.SelectedNode.Tag as MazeSpace).ExpectedPersonCount = (int)roomExpPersonCountNumericUpDown.Value;
        }

        private void removeInaccessibleRoomsButton_Click(object sender, EventArgs e)
        {
            if (mazeRooms == null || mazeRobots == null)
                return;

            if (mazeRobots.Count == 0)
            {
                MessageBox.Show("No robots in the maze -- all rooms would be deleted! ");
                return;
            }

            ArrayList accessibleRooms = new ArrayList();
            foreach (MazeRobot robot in mazeRobots)
                foreach (MazeSpace room in mazeRooms)
                    if (!accessibleRooms.Contains(room) && room.ContainsPoint(robot.position))
                        accessibleRooms.Add(room);

            for (int i = 0; i < accessibleRooms.Count; i++)
            {
                MazeSpace room = (MazeSpace)accessibleRooms[i];
                foreach (MazeWall door in room.Walls)
                {
                    if (door.MazeWallType == MazeWallType.gate)
                    {
                        if (!accessibleRooms.Contains(door.RoomTo))
                            accessibleRooms.Add(door.RoomTo);
                        if (!accessibleRooms.Contains(door.RoomFrom))
                            accessibleRooms.Add(door.RoomFrom);
                    }
                }
            }

            ArrayList inaccessibleRooms = new ArrayList();
            foreach (MazeSpace room in mazeRooms)
                if (!accessibleRooms.Contains(room))
                    inaccessibleRooms.Add(room);

            foreach (MazeSpace room in inaccessibleRooms)
                RemoveRoom(room);

            recreateGraphTreeView();
            recreateRoomsTreeView();

        }

        private void graphTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            mazePanel_Paint(this, null);
        }

        private void mazeWallsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mazeWallsListBox.SelectedItem != null)
            {
                MazeWall wall = mazeWallsListBox.SelectedItem as MazeWall;
                selectedWall = wall;
                wallLengthLabel.Text = "length: " + (wall.Segment.Length / 100).ToString("f2");
                wallAngleLabel.Text = "angle: " + (new Vector2D(wall.points[0], wall.points[1])).Angle.ToString("f2");
                wallHeightLabel.Text = "height: " + (wall.Height / 100).ToString("f2");
                wallWidthLabel.Text = "width: " + (wall.Width / 100).ToString("f2");
            }
            else
            {
                wallLengthLabel.Text = "length: ";
                wallAngleLabel.Text = "angle: ";
                wallHeightLabel.Text = "height: ";
                wallWidthLabel.Text = "width: ";
            }
            mazePanel_Paint(this, null);
        }

        private void updateWallDisplayedInfo()
        {
            if (selectedWall != null)
            {
                wallLengthLabel.Text = "length: " + (selectedWall.Segment.Length / 100).ToString("f2");
                wallAngleLabel.Text = "angle: " + (new Vector2D(selectedWall.points[0], selectedWall.points[1])).Angle.ToString("f2");
                wallHeightLabel.Text = "height: " + (selectedWall.Height / 100).ToString("f2");
                wallWidthLabel.Text = "width: " + (selectedWall.Width / 100).ToString("f2");
            }
            else
            {
                wallLengthLabel.Text = "length: ";
                wallAngleLabel.Text = "angle: ";
                wallHeightLabel.Text = "height: ";
                wallWidthLabel.Text = "width: ";
            }
        }

        private void sizeTrackBar_Scroll(object sender, EventArgs e)
        {
            mazePanel_Paint(this, null);
        }

        private void joinWithRadioButton_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void FlopAllWallButton_Click(object sender, EventArgs e)
        {
            foreach (MazeWall wall in mazeWalls)
            {
                wall.points[0].X = (float)widthNumericUpDown.Value * 100 - wall.points[0].X;
                wall.points[1].X = (float)widthNumericUpDown.Value * 100 - wall.points[1].X;
            }
            mazePanel_Paint(this, null);
        }

        private void translateWallsToZeroZeoButton_Click(object sender, EventArgs e)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxWidth = 0;
            foreach (MazeWall wall in mazeWalls)
            {
                if (minX > wall.points[0].X)
                    minX = wall.points[0].X;
                if (minX > wall.points[1].X)
                    minX = wall.points[1].X;
                if (minY > wall.points[0].Y)
                    minY = wall.points[0].Y;
                if (minY > wall.points[1].Y)
                    minY = wall.points[1].Y;

                if (maxWidth < wall.Width)
                    maxWidth = wall.Width;
            }

            foreach (MazeWall wall in mazeWalls)
            {
                wall.points[0].X -= minX - maxWidth * 2;
                wall.points[1].X -= minX - maxWidth * 2;
                wall.points[0].Y -= minY - maxWidth * 2;
                wall.points[1].Y -= minY - maxWidth * 2;
            }
            mazePanel_Paint(this, null);
        }

        #region :: PF implementation ::

        private void butStart_Click(object sender, EventArgs e)
        {
            initUDPandStart(txtIP.Text, int.Parse(txtPort.Text), Convert.ToInt32(dNumberRobots.Value));
            initUDP_BBandStart("127.0.0.1", 5555);
        }

        UdpClient udp;

        bool endTransmision = false;

        IPEndPoint endPoint;

        Thread reciveThread;

        MazeRobot[] RobotsPF = null;

        int maxRobots;

        private void initUDPandStart(string sIP, int port, int iRobotNumber)
        {
            mazeRobots.Clear();
            MazeIdentifiable.ClearBusyIdsCache();

            RobotsPF = new MazeRobot[iRobotNumber];
            maxRobots = iRobotNumber;
            string robotName;

            for (int i = 0; i < maxRobots; i++)
            {
                robotName = string.Format("robot{0}", i.ToString());

                RobotsPF[i] = new MazeRobot("Robot", robotName, new Point2D(i * 100, i * 100), (float)(1.0 * 100));
                RobotsPF[i].ID = robotName;

                mazeRobots.Add(RobotsPF[i]);
            }

            udp = new UdpClient(port);
            endPoint = new IPEndPoint(IPAddress.Parse(sIP), port);

            endTransmision = false;

            reciveThread = new Thread(new ThreadStart(getParticlePossition));
            reciveThread.Start();
        }

        private void getParticlePossition()
        {
            byte[] tablica;
            string temp;
            string[] tmp;
            int indexTable;
            double x;
            double y;
            double alfa;
            double probability;

            while (!endTransmision)
            {
                tablica = udp.Receive(ref endPoint);
                temp = System.Text.Encoding.Default.GetString(tablica);

                foreach (var item in temp.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    tmp = item.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    indexTable = int.Parse(tmp[0]); //aktaualny index;

                    if ((tmp.Length == 5) && (indexTable < maxRobots)) //zmienic gdy ilosc danych do przeslania sie zmienia;
                    {
                        x = double.Parse(tmp[1].Replace(".", ","));
                        y = double.Parse(tmp[2].Replace(".", ","));
                        //alfa = double.Parse(tmp[3].Replace(".", ","));
                        //probability = double.Parse(tmp[4].Replace(".", ","));

                        RobotsPF[indexTable].position = new Point2D(x * 100, y * 100);
                    }
                }
                this.Invoke((MethodInvoker)delegate()
                {
                    mazePanel_Paint(this, null);
                });
            }
        }

        private void threadStop()
        {
            endTransmision = true;
            udp.Close();

            reciveThread.Abort();

            reciveThread = null;

            endTransmision_BB = true;
            udp_BB.Close();

            reciveThread_BB.Abort();

            reciveThread_BB = null;
        }

        private void threadPause()
        {
            reciveThread.Suspend();
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            threadStop();
        }

        private void butPause_Click(object sender, EventArgs e)
        {
            threadPause();
        }

        #endregion

        #region ::    BoundingBox    ::

        UdpClient udp_BB;

        bool endTransmision_BB = false;

        IPEndPoint endPoint_BB;

        Thread reciveThread_BB;

        private void initUDP_BBandStart(string sIP, int port)
        {
            udp_BB = new UdpClient(port);
            endPoint_BB = new IPEndPoint(IPAddress.Parse(sIP), port);

            endTransmision_BB = false;

            reciveThread_BB = new Thread(new ThreadStart(getBoundingBoxPossition));
            reciveThread_BB.Start();
        }

        private void getBoundingBoxPossition()
        {
            int id = 999999;
            byte[] tablica;
            string temp;
            string[] tmp;

            while (!endTransmision_BB)
            {
                tablica = udp_BB.Receive(ref endPoint_BB);
                temp = System.Text.Encoding.Default.GetString(tablica);

                foreach (var item in temp.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    tmp = item.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);


                    MazeWall mazeWall = new MazeWall(
                            new Point2D(double.Parse(tmp[1].Replace(".", ",")) * 100, double.Parse(tmp[2].Replace(".", ",")) * 100),
                            new Point2D(double.Parse(tmp[3].Replace(".", ",")) * 100, double.Parse(tmp[4].Replace(".", ",")) * 100),
                            (float)0.05 * 100,
                            (float)0.05 * 100,
                            Color.Yellow);

                    mazeWall.ID = id.ToString();
                    mazeWalls.Add(mazeWall);

                    id++;



                    mazeWall = new MazeWall(
       new Point2D(double.Parse(tmp[5].Replace(".", ",")) * 100, double.Parse(tmp[6].Replace(".", ",")) * 100),
       new Point2D(double.Parse(tmp[7].Replace(".", ",")) * 100, double.Parse(tmp[8].Replace(".", ",")) * 100),
       (float)0.05 * 100,
       (float)0.05 * 100,
       Color.Yellow);

                    mazeWall.ID = id.ToString();
                    mazeWalls.Add(mazeWall);

                    id++;


                    mazeWall = new MazeWall(
new Point2D(double.Parse(tmp[5].Replace(".", ",")) * 100, double.Parse(tmp[6].Replace(".", ",")) * 100),
new Point2D(double.Parse(tmp[1].Replace(".", ",")) * 100, double.Parse(tmp[2].Replace(".", ",")) * 100),
(float)0.05 * 100,
(float)0.05 * 100,
Color.Yellow);

                    mazeWall.ID = id.ToString();
                    mazeWalls.Add(mazeWall);

                    id++;


                    mazeWall = new MazeWall(
        new Point2D(double.Parse(tmp[7].Replace(".", ",")) * 100, double.Parse(tmp[8].Replace(".", ",")) * 100),
        new Point2D(double.Parse(tmp[3].Replace(".", ",")) * 100, double.Parse(tmp[4].Replace(".", ",")) * 100),
        (float)0.05 * 100,
        (float)0.05 * 100,
        Color.Yellow);

                    mazeWall.ID = id.ToString();
                    mazeWalls.Add(mazeWall);

                    id++;
                }

                this.Invoke((MethodInvoker)delegate()
                {
                    mazePanel_Paint(this, null);
                });
            }
        }

        #endregion
    }
}

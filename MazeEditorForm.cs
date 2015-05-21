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
using System.Collections.Generic;
using System.Linq;


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

        private string worldName = string.Empty;
        private int iWorldTimeout;
        private ArrayList mazeWalls = null;
        private ArrayList mazeRobots = null;
        private ArrayList mazeVictims = null;

        private ArrayList mazeRooms = null;
        private MazeGraph mazeGraph = null;

        private ArrayList mazeTargets = null;

        private ArrayList mazeNodeNodes = null;
        public ArrayList mazeSpaceNode = null;
        public ArrayList mazeSpaceRobots = null;

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
        private Brush robotBrushSelected;
        private Pen robotPenSelected;
        private Pen robotPenArrow;
        private Font fontSelected;
        private Font fontGraph;

        private Pen[] taskPanDirection;

        private Brush victimBrush;

        private Brush taskPenDone;
        private Brush taskPenInProgress;

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
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn X;
        private DataGridViewTextBoxColumn Y;
        private DataGridViewTextBoxColumn Prop;
        private DataGridViewTextBoxColumn Alfa;
        private Button button1;
        private TabPage tabPage1;
        private Button button2;
        private DataGridView dataGridView1DTP;
        private GroupBox groupBox2;
        private TextBox textBox1;
        private TextBox txtIPDTP;
        private Label label10;
        private Label label11;
        private Button butPauseDTP;
        private Button butStopDTP;
        private Button bytStartDTP;
        private DataGridViewTextBoxColumn colID;
        private DataGridViewTextBoxColumn colX;
        private DataGridViewTextBoxColumn colY;
        private DataGridViewTextBoxColumn colAngle;
        private DataGridViewTextBoxColumn colTaskID;
        private DataGridViewTextBoxColumn colTask_X;
        private DataGridViewTextBoxColumn colTask_Y;
        private DataGridViewTextBoxColumn colTask_Name;
        private DataGridViewCheckBoxColumn colIsEnd;
        private Label label13;
        private Label label12;
        private TextBox TargetY;
        private TextBox TargetX;
        private NumericUpDown iTimeout;
        private Label label15;
        private TabPage tabPage2;
        private Button button3;
        private Button butSelectDirectory;
        private Label label14;
        private TextBox txtWorkingPath;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button butRun;
        private NumericUpDown dPassageFactor;
        private NumericUpDown dSearchFactor;
        private Label label17;
        private Label label16;
        private Label label21;
        private Label label20;
        private Label label19;
        private Label label18;
        private TextBox txtToX;
        private TextBox txtToY;
        private TextBox txtFromY;
        private TextBox txtFromX;
        private Button butWallUpdate;
        private Button butRemoveCurrentWall;
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
            robotBrushSelected = (new Pen(Color.Red, 1)).Brush;
            robotPenArrow = new Pen(Color.Red, 2);

            taskPanDirection = new Pen[] { new Pen(Color.Red, 2), new Pen(Color.Green, 2), new Pen(Color.Orange, 2) };

            victimBrush = (new Pen(Color.Orange, 1)).Brush;
            fontSelected = new System.Drawing.Font(this.Font, FontStyle.Bold);
            fontGraph = new Font(FontFamily.GenericSansSerif,3.0F, FontStyle.Bold);

            robotPenSelected = new Pen(Color.Red, 1);

            taskPenDone = (new Pen(Color.Green, 1)).Brush;
            taskPenInProgress = (new Pen(Color.White, 1)).Brush;

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
            this.iTimeout = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
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
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.TargetY = new System.Windows.Forms.TextBox();
            this.TargetX = new System.Windows.Forms.TextBox();
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
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Prop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Alfa = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridView1DTP = new System.Windows.Forms.DataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAngle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTaskID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTask_X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTask_Y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTask_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsEnd = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.txtIPDTP = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.butPauseDTP = new System.Windows.Forms.Button();
            this.butStopDTP = new System.Windows.Forms.Button();
            this.bytStartDTP = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.dPassageFactor = new System.Windows.Forms.NumericUpDown();
            this.dSearchFactor = new System.Windows.Forms.NumericUpDown();
            this.butRun = new System.Windows.Forms.Button();
            this.butSelectDirectory = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.txtWorkingPath = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.butWallUpdate = new System.Windows.Forms.Button();
            this.txtFromX = new System.Windows.Forms.TextBox();
            this.txtFromY = new System.Windows.Forms.TextBox();
            this.txtToY = new System.Windows.Forms.TextBox();
            this.txtToX = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.butRemoveCurrentWall = new System.Windows.Forms.Button();
            this.mazePanel = new MazeEditor.DoubleBufferedPanel();
            this.viewPanel.SuspendLayout();
            this.leftMenuPanel.SuspendLayout();
            this.objectSelectorTabControl.SuspendLayout();
            this.mazeTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iTimeout)).BeginInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dNumberRobots)).BeginInit();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1DTP)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dPassageFactor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dSearchFactor)).BeginInit();
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
            this.objectSelectorTabControl.Controls.Add(this.tabPage1);
            this.objectSelectorTabControl.Controls.Add(this.tabPage2);
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
            this.mazeTabPage.Controls.Add(this.iTimeout);
            this.mazeTabPage.Controls.Add(this.label15);
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
            // iTimeout
            // 
            this.iTimeout.Location = new System.Drawing.Point(80, 153);
            this.iTimeout.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.iTimeout.Name = "iTimeout";
            this.iTimeout.Size = new System.Drawing.Size(112, 20);
            this.iTimeout.TabIndex = 20;
            this.iTimeout.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.iTimeout.ValueChanged += new System.EventHandler(this.iTimeout_ValueChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 160);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(45, 13);
            this.label15.TabIndex = 19;
            this.label15.Text = "Timeout";
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
            this.worldNameTextBox.TextChanged += new System.EventHandler(this.worldNameTextBox_TextChanged);
            // 
            // wallsTabPage
            // 
            this.wallsTabPage.Controls.Add(this.butRemoveCurrentWall);
            this.wallsTabPage.Controls.Add(this.label21);
            this.wallsTabPage.Controls.Add(this.label20);
            this.wallsTabPage.Controls.Add(this.label19);
            this.wallsTabPage.Controls.Add(this.label18);
            this.wallsTabPage.Controls.Add(this.txtToX);
            this.wallsTabPage.Controls.Add(this.txtToY);
            this.wallsTabPage.Controls.Add(this.txtFromY);
            this.wallsTabPage.Controls.Add(this.txtFromX);
            this.wallsTabPage.Controls.Add(this.butWallUpdate);
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
            this.mazeWallsListBox.Size = new System.Drawing.Size(349, 186);
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
            this.robotsTabPage.Controls.Add(this.label13);
            this.robotsTabPage.Controls.Add(this.label12);
            this.robotsTabPage.Controls.Add(this.TargetY);
            this.robotsTabPage.Controls.Add(this.TargetX);
            this.robotsTabPage.Controls.Add(this.robotNameLabel);
            this.robotsTabPage.Controls.Add(this.robotTypeTextBox);
            this.robotsTabPage.Controls.Add(this.robotTypeLabel);
            this.robotsTabPage.Controls.Add(this.robotHeightLabel);
            this.robotsTabPage.Controls.Add(this.robotHeightNumericUpDown);
            this.robotsTabPage.Controls.Add(this.robotNameTextBox);
            this.robotsTabPage.ImageIndex = 6;
            this.robotsTabPage.Location = new System.Drawing.Point(4, 34);
            this.robotsTabPage.Name = "robotsTabPage";
            this.robotsTabPage.Size = new System.Drawing.Size(363, 511);
            this.robotsTabPage.TabIndex = 1;
            this.robotsTabPage.Text = "robots";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(19, 194);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(51, 13);
            this.label13.TabIndex = 24;
            this.label13.Text = "Target Y:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 161);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "Target X:";
            // 
            // TargetY
            // 
            this.TargetY.Location = new System.Drawing.Point(88, 188);
            this.TargetY.Name = "TargetY";
            this.TargetY.Size = new System.Drawing.Size(100, 20);
            this.TargetY.TabIndex = 22;
            this.TargetY.Text = "0";
            // 
            // TargetX
            // 
            this.TargetX.Location = new System.Drawing.Point(88, 161);
            this.TargetX.Name = "TargetX";
            this.TargetX.Size = new System.Drawing.Size(100, 20);
            this.TargetX.TabIndex = 21;
            this.TargetX.Text = "0";
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
            this.victimsTabPage.Location = new System.Drawing.Point(4, 34);
            this.victimsTabPage.Name = "victimsTabPage";
            this.victimsTabPage.Size = new System.Drawing.Size(363, 511);
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
            this.roomsTabPage.Location = new System.Drawing.Point(4, 34);
            this.roomsTabPage.Name = "roomsTabPage";
            this.roomsTabPage.Size = new System.Drawing.Size(363, 511);
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
            this.roomPropertiesPanel.Location = new System.Drawing.Point(50, 406);
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
            this.doorPropertiesPanel.Location = new System.Drawing.Point(15, 406);
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
            this.typeSelectComboBox.Location = new System.Drawing.Point(15, 378);
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
            this.roomsTreeView.Size = new System.Drawing.Size(337, 292);
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
            this.graphTabPage.Location = new System.Drawing.Point(4, 34);
            this.graphTabPage.Name = "graphTabPage";
            this.graphTabPage.Size = new System.Drawing.Size(363, 511);
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
            this.graphTreeView.Size = new System.Drawing.Size(337, 399);
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
            this.sizeTrackBar.Size = new System.Drawing.Size(104, 42);
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
            this.PFTabPage.Controls.Add(this.button1);
            this.PFTabPage.Controls.Add(this.dataGridView1);
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
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(260, 108);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "ShowSelected";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.X,
            this.Y,
            this.Prop,
            this.Alfa});
            this.dataGridView1.Location = new System.Drawing.Point(3, 137);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(347, 292);
            this.dataGridView1.TabIndex = 6;
            // 
            // ID
            // 
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.Width = 50;
            // 
            // X
            // 
            this.X.HeaderText = "X";
            this.X.Name = "X";
            this.X.Width = 50;
            // 
            // Y
            // 
            this.Y.HeaderText = "Y";
            this.Y.Name = "Y";
            this.Y.Width = 50;
            // 
            // Prop
            // 
            this.Prop.HeaderText = "Prop";
            this.Prop.Name = "Prop";
            this.Prop.Width = 50;
            // 
            // Alfa
            // 
            this.Alfa.HeaderText = "Alfa";
            this.Alfa.Name = "Alfa";
            this.Alfa.Width = 50;
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
            this.txtIP.Text = "192.168.2.100";
            this.txtIP.TextChanged += new System.EventHandler(this.txtIP_TextChanged);
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
            this.butPause.Location = new System.Drawing.Point(89, 108);
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
            this.butStop.Enabled = false;
            this.butStop.Location = new System.Drawing.Point(170, 108);
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
            104,
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
            this.butStart.Location = new System.Drawing.Point(8, 108);
            this.butStart.Name = "butStart";
            this.butStart.Size = new System.Drawing.Size(75, 23);
            this.butStart.TabIndex = 0;
            this.butStart.Text = "Start";
            this.butStart.UseVisualStyleBackColor = true;
            this.butStart.Click += new System.EventHandler(this.butStart_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.dataGridView1DTP);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.butPauseDTP);
            this.tabPage1.Controls.Add(this.butStopDTP);
            this.tabPage1.Controls.Add(this.bytStartDTP);
            this.tabPage1.Location = new System.Drawing.Point(4, 67);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(363, 478);
            this.tabPage1.TabIndex = 7;
            this.tabPage1.Text = "TC";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(265, 94);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "ShowSelected";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            // 
            // dataGridView1DTP
            // 
            this.dataGridView1DTP.AllowUserToAddRows = false;
            this.dataGridView1DTP.AllowUserToDeleteRows = false;
            this.dataGridView1DTP.AllowUserToResizeRows = false;
            this.dataGridView1DTP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1DTP.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1DTP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1DTP.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colX,
            this.colY,
            this.colAngle,
            this.colTaskID,
            this.colTask_X,
            this.colTask_Y,
            this.colTask_Name,
            this.colIsEnd});
            this.dataGridView1DTP.Location = new System.Drawing.Point(8, 147);
            this.dataGridView1DTP.Name = "dataGridView1DTP";
            this.dataGridView1DTP.ReadOnly = true;
            this.dataGridView1DTP.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1DTP.RowHeadersVisible = false;
            this.dataGridView1DTP.Size = new System.Drawing.Size(347, 325);
            this.dataGridView1DTP.TabIndex = 12;
            // 
            // colID
            // 
            this.colID.HeaderText = "ID";
            this.colID.Name = "colID";
            this.colID.ReadOnly = true;
            // 
            // colX
            // 
            this.colX.HeaderText = "X";
            this.colX.Name = "colX";
            this.colX.ReadOnly = true;
            this.colX.Visible = false;
            // 
            // colY
            // 
            this.colY.HeaderText = "Y";
            this.colY.Name = "colY";
            this.colY.ReadOnly = true;
            this.colY.Visible = false;
            // 
            // colAngle
            // 
            this.colAngle.HeaderText = "Angle";
            this.colAngle.Name = "colAngle";
            this.colAngle.ReadOnly = true;
            this.colAngle.Visible = false;
            // 
            // colTaskID
            // 
            this.colTaskID.HeaderText = "TaskID";
            this.colTaskID.Name = "colTaskID";
            this.colTaskID.ReadOnly = true;
            this.colTaskID.Visible = false;
            // 
            // colTask_X
            // 
            this.colTask_X.HeaderText = "Task_X";
            this.colTask_X.Name = "colTask_X";
            this.colTask_X.ReadOnly = true;
            // 
            // colTask_Y
            // 
            this.colTask_Y.HeaderText = "Task_Y";
            this.colTask_Y.Name = "colTask_Y";
            this.colTask_Y.ReadOnly = true;
            // 
            // colTask_Name
            // 
            this.colTask_Name.HeaderText = "Task_Name";
            this.colTask_Name.Name = "colTask_Name";
            this.colTask_Name.ReadOnly = true;
            // 
            // colIsEnd
            // 
            this.colIsEnd.HeaderText = "IsEnd";
            this.colIsEnd.Name = "colIsEnd";
            this.colIsEnd.ReadOnly = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.txtIPDTP);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Location = new System.Drawing.Point(16, 15);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(339, 73);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "UDP";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(63, 40);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "4321";
            // 
            // txtIPDTP
            // 
            this.txtIPDTP.Location = new System.Drawing.Point(63, 12);
            this.txtIPDTP.Name = "txtIPDTP";
            this.txtIPDTP.Size = new System.Drawing.Size(100, 20);
            this.txtIPDTP.TabIndex = 2;
            this.txtIPDTP.Text = "127.0.0.1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(22, 45);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 13);
            this.label10.TabIndex = 1;
            this.label10.Text = "Port:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(22, 16);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(20, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "IP:";
            // 
            // butPauseDTP
            // 
            this.butPauseDTP.Enabled = false;
            this.butPauseDTP.Location = new System.Drawing.Point(94, 94);
            this.butPauseDTP.Name = "butPauseDTP";
            this.butPauseDTP.Size = new System.Drawing.Size(75, 23);
            this.butPauseDTP.TabIndex = 10;
            this.butPauseDTP.Text = "Pause";
            this.butPauseDTP.UseVisualStyleBackColor = true;
            this.butPauseDTP.Visible = false;
            this.butPauseDTP.Click += new System.EventHandler(this.butPauseDTP_Click);
            // 
            // butStopDTP
            // 
            this.butStopDTP.Enabled = false;
            this.butStopDTP.Location = new System.Drawing.Point(175, 94);
            this.butStopDTP.Name = "butStopDTP";
            this.butStopDTP.Size = new System.Drawing.Size(75, 23);
            this.butStopDTP.TabIndex = 9;
            this.butStopDTP.Text = "Stop";
            this.butStopDTP.UseVisualStyleBackColor = true;
            this.butStopDTP.Click += new System.EventHandler(this.butStopDTP_Click);
            // 
            // bytStartDTP
            // 
            this.bytStartDTP.Location = new System.Drawing.Point(13, 94);
            this.bytStartDTP.Name = "bytStartDTP";
            this.bytStartDTP.Size = new System.Drawing.Size(75, 23);
            this.bytStartDTP.TabIndex = 8;
            this.bytStartDTP.Text = "Start";
            this.bytStartDTP.UseVisualStyleBackColor = true;
            this.bytStartDTP.Click += new System.EventHandler(this.butStartDTP_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label17);
            this.tabPage2.Controls.Add(this.label16);
            this.tabPage2.Controls.Add(this.dPassageFactor);
            this.tabPage2.Controls.Add(this.dSearchFactor);
            this.tabPage2.Controls.Add(this.butRun);
            this.tabPage2.Controls.Add(this.butSelectDirectory);
            this.tabPage2.Controls.Add(this.label14);
            this.tabPage2.Controls.Add(this.txtWorkingPath);
            this.tabPage2.Controls.Add(this.button3);
            this.tabPage2.Location = new System.Drawing.Point(4, 67);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(363, 478);
            this.tabPage2.TabIndex = 8;
            this.tabPage2.Text = "BFS";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(14, 36);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(84, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "Passage Factor:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(13, 10);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 13);
            this.label16.TabIndex = 7;
            this.label16.Text = "Search Factor:";
            // 
            // dPassageFactor
            // 
            this.dPassageFactor.DecimalPlaces = 2;
            this.dPassageFactor.Location = new System.Drawing.Point(104, 34);
            this.dPassageFactor.Name = "dPassageFactor";
            this.dPassageFactor.Size = new System.Drawing.Size(127, 20);
            this.dPassageFactor.TabIndex = 6;
            this.dPassageFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // dSearchFactor
            // 
            this.dSearchFactor.DecimalPlaces = 2;
            this.dSearchFactor.Location = new System.Drawing.Point(104, 8);
            this.dSearchFactor.Name = "dSearchFactor";
            this.dSearchFactor.Size = new System.Drawing.Size(126, 20);
            this.dSearchFactor.TabIndex = 5;
            this.dSearchFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // butRun
            // 
            this.butRun.Location = new System.Drawing.Point(271, 118);
            this.butRun.Name = "butRun";
            this.butRun.Size = new System.Drawing.Size(75, 23);
            this.butRun.TabIndex = 4;
            this.butRun.Text = "RunForAll";
            this.butRun.UseVisualStyleBackColor = true;
            this.butRun.Click += new System.EventHandler(this.butRun_Click);
            // 
            // butSelectDirectory
            // 
            this.butSelectDirectory.Location = new System.Drawing.Point(323, 92);
            this.butSelectDirectory.Name = "butSelectDirectory";
            this.butSelectDirectory.Size = new System.Drawing.Size(23, 23);
            this.butSelectDirectory.TabIndex = 3;
            this.butSelectDirectory.UseVisualStyleBackColor = true;
            this.butSelectDirectory.Click += new System.EventHandler(this.butSelectDirectory_Click);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(14, 76);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(74, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "Working path:";
            // 
            // txtWorkingPath
            // 
            this.txtWorkingPath.Location = new System.Drawing.Point(13, 92);
            this.txtWorkingPath.Name = "txtWorkingPath";
            this.txtWorkingPath.ReadOnly = true;
            this.txtWorkingPath.Size = new System.Drawing.Size(304, 20);
            this.txtWorkingPath.TabIndex = 1;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(271, 50);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 0;
            this.button3.Text = "Run Current";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // butWallUpdate
            // 
            this.butWallUpdate.Location = new System.Drawing.Point(170, 376);
            this.butWallUpdate.Name = "butWallUpdate";
            this.butWallUpdate.Size = new System.Drawing.Size(87, 23);
            this.butWallUpdate.TabIndex = 27;
            this.butWallUpdate.Text = "Update Wall";
            this.butWallUpdate.UseVisualStyleBackColor = true;
            this.butWallUpdate.Click += new System.EventHandler(this.butWallUpdate_Click);
            // 
            // txtFromX
            // 
            this.txtFromX.Location = new System.Drawing.Point(54, 317);
            this.txtFromX.Name = "txtFromX";
            this.txtFromX.Size = new System.Drawing.Size(100, 20);
            this.txtFromX.TabIndex = 28;
            // 
            // txtFromY
            // 
            this.txtFromY.Location = new System.Drawing.Point(55, 345);
            this.txtFromY.Name = "txtFromY";
            this.txtFromY.Size = new System.Drawing.Size(100, 20);
            this.txtFromY.TabIndex = 29;
            // 
            // txtToY
            // 
            this.txtToY.Location = new System.Drawing.Point(209, 345);
            this.txtToY.Name = "txtToY";
            this.txtToY.Size = new System.Drawing.Size(100, 20);
            this.txtToY.TabIndex = 30;
            // 
            // txtToX
            // 
            this.txtToX.Location = new System.Drawing.Point(209, 317);
            this.txtToX.Name = "txtToX";
            this.txtToX.Size = new System.Drawing.Size(100, 20);
            this.txtToX.TabIndex = 31;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(8, 320);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(43, 13);
            this.label18.TabIndex = 32;
            this.label18.Text = "From X:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(9, 349);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(43, 13);
            this.label19.TabIndex = 33;
            this.label19.Text = "From Y:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(162, 320);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(33, 13);
            this.label20.TabIndex = 34;
            this.label20.Text = "To X:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(165, 348);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(33, 13);
            this.label21.TabIndex = 35;
            this.label21.Text = "To Y:";
            // 
            // butRemoveCurrentWall
            // 
            this.butRemoveCurrentWall.Location = new System.Drawing.Point(264, 376);
            this.butRemoveCurrentWall.Name = "butRemoveCurrentWall";
            this.butRemoveCurrentWall.Size = new System.Drawing.Size(87, 23);
            this.butRemoveCurrentWall.TabIndex = 36;
            this.butRemoveCurrentWall.Text = "Remove Wall";
            this.butRemoveCurrentWall.UseVisualStyleBackColor = true;
            this.butRemoveCurrentWall.Click += new System.EventHandler(this.butRemoveCurrentWall_Click);
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
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MazeEditorForm_FormClosed);
            this.Load += new System.EventHandler(this.MazeEditorForm_Load);
            this.viewPanel.ResumeLayout(false);
            this.leftMenuPanel.ResumeLayout(false);
            this.objectSelectorTabControl.ResumeLayout(false);
            this.mazeTabPage.ResumeLayout(false);
            this.mazeTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iTimeout)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dNumberRobots)).EndInit();
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1DTP)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dPassageFactor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dSearchFactor)).EndInit();
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
            mazeTargets = new ArrayList();
            mazeNodeNodes = new ArrayList();
            mazeSpaceNode = new ArrayList();
            mazeSpaceRobots = new ArrayList();
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

            LoadMaze(fileDialog.FileName);         
        }

        private void LoadMaze(string sFileName)
        {
            MazeIdentifiable.ClearBusyIdsCache();

            JsonHelper jsonHelper = new JsonHelper();
            jsonHelper.LoadAll(sFileName);

            mazeWalls = jsonHelper.mazeWalls;
            mazeRobots = jsonHelper.mazeRobots;
            mazeVictims = jsonHelper.mazeVictims;
            mazeGraph = jsonHelper.mazeGraph;
            mazeRooms = jsonHelper.mazeSpaces;
            worldName = jsonHelper.worldName;
            mazeNodeNodes = jsonHelper.mazeNodeNodes;
            mazeSpaceNode = jsonHelper.mazeSpaceNode;
            mazeSpaceRobots = jsonHelper.mazeSpaceRobots;

            iWorldTimeout = jsonHelper.iWorldTimeout;


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

            JsonHelper.SaveAll(fileDialog.FileName, mazeWalls, mazeRobots, mazeVictims, mazeRooms, mazeGraph, worldName,iWorldTimeout);




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
                if (robot.Selected)
                {
                    mazeBitmapGraphics.FillEllipse(robotBrushSelected, robot.position.X - sizeOffset, robot.position.Y - sizeOffset, size, size);
                    mazeBitmapGraphics.DrawString(robot.name, fontSelected, robotBrushSelected, robot.position.X - sizeOffset, robot.position.Y - sizeOffset);
                    mazeBitmapGraphics.DrawLine(robotPenArrow, robot.position.X, robot.position.Y, robot.arrow.X, robot.arrow.Y);
                }
                else
                {
                    robotPenSelected.Color = getRobotColor(robot.Probability);
                    mazeBitmapGraphics.FillEllipse(robotPenSelected.Brush, robot.position.X - sizeOffset, robot.position.Y - sizeOffset, size, size);
                    mazeBitmapGraphics.DrawLine(robotPenArrow, robot.position.X, robot.position.Y, robot.arrow.X, robot.arrow.Y);
                }
            }

            RobotTask taskCurrent;
            RobotTask taskPrivious = null;

            for (int i = 0; i < robotsTaskList.Count(); i++)
            {
                taskCurrent = robotsTaskList[i];

                if (taskCurrent.IsEnd)
                    mazeBitmapGraphics.FillRectangle(taskPenDone, taskCurrent.GetTaskX - sizeOffset, taskCurrent.GetTaskY - sizeOffset, size, size);
                else
                    mazeBitmapGraphics.FillRectangle(taskPenInProgress, taskCurrent.GetTaskX - sizeOffset, taskCurrent.GetTaskY - sizeOffset, size, size);


                if (i == 0)
                    taskPrivious = taskCurrent;
                else
                {
                    if (taskPrivious.RobotID != taskCurrent.RobotID)
                        taskPrivious = taskCurrent;
                    else
                    {
                        Pen temp = taskPanDirection[taskPrivious.RobotID - 1];
                        temp.CustomEndCap = new AdjustableArrowCap(size / 2, size / 2);
                        Vector2D arcDir = new Vector2D(new Point2D(taskPrivious.GetTaskX, taskPrivious.GetTaskY), new Point2D(taskCurrent.GetTaskX, taskCurrent.GetTaskY));
                        arcDir.Normalize();

                        mazeBitmapGraphics.DrawLine(temp, taskPrivious.GetTaskX, taskPrivious.GetTaskY, taskCurrent.GetTaskX, taskCurrent.GetTaskY);

                        taskPrivious = taskCurrent;
                    }                    
                }
            }



            /*for(int i = 0; i < mazeTargets.Count; i++)
            {
                target = (Target) mazeTargets[i];

                if (target.TaskDone)
                    mazeBitmapGraphics.FillRectangle(taskPenDone, target.X - sizeOffset, target.Y - sizeOffset, size, size);
                else
                    mazeBitmapGraphics.FillRectangle(taskPenInProgress, target.X - sizeOffset, target.Y - sizeOffset, size, size);

                if ((i > 0) && (i != mazeTargets.Count))
                {
                    targetPrevious = (Target)mazeTargets[i - 1];
                    Pen temp = taskPanDirection[targetPrevious.ID - 1];
                    temp.CustomEndCap = new AdjustableArrowCap(size / 2, size / 2);
                    Vector2D arcDir = new Vector2D(new Point2D(targetPrevious.X, targetPrevious.Y), new Point2D(target.X, target.Y));
                    arcDir.Normalize();

                    mazeBitmapGraphics.DrawLine(temp, targetPrevious.X, targetPrevious.Y, target.X, target.Y);
                }
            }*/

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

                    if (true)
                        mazeBitmapGraphics.DrawString(arc.Weight.ToString(), fontGraph, robotBrushSelected, (float)((arc.from.position.x + arc.to.position.x) / 2), (float)((arc.from.position.y + arc.to.position.y) / 2));

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
            float tmpTargetX,tmpTargetY;
            tmpTargetX = tmpTargetY = 0;

            float.TryParse(TargetX.Text, out tmpTargetX);
            float.TryParse(TargetY.Text, out tmpTargetY);

            tempPoints[0].X = x;
            tempPoints[0].Y = y;
            invertedMazeMatrix.TransformPoints(tempPoints);
            mazeRobots.Add(new MazeRobot(robotTypeTextBox.Text, robotNameTextBox.Text, tempPoints[0], (float)robotHeightNumericUpDown.Value * 100, new PointF(tmpTargetX,tmpTargetY)));            
            robotNameIndex++;
            robotNameTextBox.Text = robotNameTextBox.Text + robotNameIndex.ToString();
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
                nodenode.Text = node.MazeGraphNodeType + ": " + node.position.ToString() + "[" + node.ID + "]";
                graphTreeView.Nodes.Add(nodenode);
                // węzły dostępne 
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
                //if(wall.MazeWallType == MazeWallType.gate) //wywalamy tylko drzwi pomieedzy pomiesczeniami
                mazeWalls.Remove(wall);

                foreach (MazeNode node in mazeGraph.MazeGraphNodes)
                    if (node.Door == wall)
                        nodesToRemove.Add(node);
                foreach (MazeNode node in nodesToRemove)
                    mazeGraph.RemoveNode(node);
            }

            foreach (MazeWall wall in commonWalls)
                mazeWalls.Remove(wall);

            /* foreach (MazeWall wall in commonWalls) //dodanie scian wspolnych
             {
                 if (wall.MazeWallType == MazeWallType.wall)
                 {
                     mazeWalls.Add(wall);
                     newRoomWalls.Add(wall);
                 }
             }*/

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

                txtFromX.Text = wall.points[0].X.ToString();
                txtFromY.Text = wall.points[0].Y.ToString();
                txtToX.Text = wall.points[1].X.ToString();
                txtToY.Text = wall.points[1].Y.ToString();
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
            // initUDPandStart(txtIP.Text, int.Parse(txtPort.Text), Convert.ToInt32(dNumberRobots.Value));
            // initUDP_BBandStart(txtIP.Text, 5555);
            initUDPandStart(txtIP.Text, 1234, Convert.ToInt32(dNumberRobots.Value));
            butStart.Enabled = false;
            butStop.Enabled = true;
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

            dataGridView1.Rows.Clear();


            for (int i = 0; i < maxRobots; i++)
            {
                robotName = string.Format("robot{0}", i.ToString());

                RobotsPF[i] = new MazeRobot("Robot", robotName, new Point2D(i * 100, i * 100), (float)(1.0 * 100),new PointF(0,0));
                RobotsPF[i].ID = robotName;

                mazeRobots.Add(RobotsPF[i]);

                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells["ID"].Value = i;
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
            int indexTable = 0;
            double x = 0;
            double y = 0;
            double alfa = 0;
            double probability = 0;

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
                        alfa = double.Parse(tmp[3].Replace(".", ","));

                        if (double.TryParse(tmp[4].Replace(".", ","), out probability))
                            //probability = double.Parse(tmp[4].Replace(".", ","));

                            RobotsPF[indexTable].position = new Point2D(x * 100, y * 100);
                        RobotsPF[indexTable].Probability = probability;
                        RobotsPF[indexTable].UpdateArrowPosiotion(alfa);
                    }

                    this.Invoke((MethodInvoker)delegate()
                    {
                        if (dataGridView1.Rows.Count > indexTable)
                        {
                            dataGridView1.Rows[indexTable].Cells["X"].Value = x;
                            dataGridView1.Rows[indexTable].Cells["Y"].Value = y;
                            dataGridView1.Rows[indexTable].Cells["Prop"].Value = probability;
                            dataGridView1.Rows[indexTable].Cells["Alfa"].Value = alfa;
                        }
                    });
                }
                this.Invoke((MethodInvoker)delegate()
                {
                    mazePanel_Paint(this, null);

                });
            }
        }

        private void threadStop()
        {
            if (reciveThread != null)
            {
                endTransmision = true;
                udp.Close();

                reciveThread.Abort();

                reciveThread = null;

                endTransmision_BB = true;

                if (udp_BB != null)
                    udp_BB.Close();

                if (reciveThread_BB != null)
                    reciveThread_BB.Abort();

                reciveThread_BB = null;
            }
        }

        private void threadPause()
        {
            reciveThread.Suspend();
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            threadStop();
            butStart.Enabled = true;
            butStop.Enabled = false;
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

        private void txtIP_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < RobotsPF.Length; i++)
                RobotsPF[i].Selected = false;

            if (dataGridView1.SelectedRows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    int index = int.Parse(dataGridView1.SelectedRows[i].Cells["ID"].Value.ToString());

                    RobotsPF[index].Selected = true;
                }
            }

            this.Invoke((MethodInvoker)delegate()
            {
                mazePanel_Paint(this, null);
            });
        }

        private Color getRobotColor(double propability)
        {
            /*if ((propability >= 0) && (propability < 0.1))
                return Color.White;
            else if ((propability >= 0.1) && (propability < 0.2))
                return Color.PaleVioletRed;
            else if ((propability >= 0.2) && (propability < 0.3))
                return Color.Violet;
            else if ((propability >= 0.3) && (propability < 0.4))
                return Color.BlueViolet;
            else if ((propability >= 0.4) && (propability < 0.5))
                return Color.Blue;
            else if ((propability >= 0.5) && (propability < 0.6))
                return Color.LightGreen;
            else if ((propability >= 0.6) && (propability < 0.7))
                return Color.Green;
            else if ((propability >= 0.7) && (propability < 0.8))
                return Color.GreenYellow;
            else if ((propability >= 0.8) && (propability < 0.9))
                return Color.Yellow;
            else if ((propability >= 0.9) && (propability <= 1))
                return Color.Orange;
            else
                return Color.Black;*/

            if ((propability >= 0) && (propability < 0.4))
                return Color.White;
            else if ((propability >= 0.4) && (propability < 0.6))
                return Color.Blue;
            else if ((propability >= 0.6) && (propability < 0.8))
                return Color.Green;
            else if ((propability >= 0.8) && (propability < 0.9))
                return Color.Yellow;
            else if ((propability >= 0.9) && (propability <= 1))
                return Color.Orange;
            else
                return Color.Black;
        }

        private string getIPAdress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        private void MazeEditorForm_Load(object sender, EventArgs e)
        {
           txtIPDTP.Text = txtIP.Text = getIPAdress();
        }

        #region Drive to point implementation

        private void butStartDTP_Click(object sender, EventArgs e)
        {
            // initUDPandStart(txtIP.Text, int.Parse(txtPort.Text), Convert.ToInt32(dNumberRobots.Value));
            // initUDP_BBandStart("127.0.0.1", 5555);
            initUDPandStartDTP(txtIPDTP.Text, 4321);
            bytStartDTP.Enabled = false;
            butStopDTP.Enabled = true;
        }

        UdpClient udpDTP;

        bool endTransmisionDTP = false;

        IPEndPoint endPointDTP;

        Thread reciveThreadDTP;

        MazeRobot[] RobotsPFDTP = null;

        //       int maxRobots;

        BindingSource source = new BindingSource();
        BindingList<RobotTask> bindingList;
       

        private void initUDPandStartDTP(string sIP, int port)
        {
            mazeRobots.Clear();
            MazeIdentifiable.ClearBusyIdsCache();

            //dataGridView1DTP.Rows.Clear();

            udpDTP = new UdpClient(port);
            endPointDTP = new IPEndPoint(IPAddress.Parse(sIP), port);

            endTransmisionDTP = false;

            reciveThreadDTP = new Thread(new ThreadStart(getParticlePossitionDTP));
            reciveThreadDTP.Start();

            //bindingList = new BindingList<RobotTask>(robotsTaskList);
            //source = new BindingSource(bindingList, null);
            //dataGridView1DTP.DataSource = source;

        }

        private void addItem(string[] item)
        {
            int indexFromTaskList = -1;
            string sID = item[1];
            double dX = double.Parse(item[2].Replace(".",","));
            double dY = double.Parse(item[3].Replace(".", ","));
            double dAngle = double.Parse(item[4].Replace(".", ","));
            int id = int.Parse(item[5]);

            for (int i = 6; i < item.Length; i += 5)
            {
                RobotTask tempRobot = new RobotTask();
                tempRobot.ID = sID;
                tempRobot.X = dX * 100;
                tempRobot.Y = dY * 100;
                tempRobot.Angle = dAngle;
                tempRobot.RobotID = id;

                tempRobot.TaskID = item[i];
                tempRobot.Task_X = double.Parse(item[i+ 1].Replace(".", ",")) * 100;
                tempRobot.Task_Y = double.Parse(item[i +2].Replace(".", ",")) * 100;
                tempRobot.Task_Name = item[i + 3];
                tempRobot.IsEnd = bool.Parse(item[i +4].Replace(".", ","));

                indexFromTaskList = getRobotTaskId(tempRobot);

                if (indexFromTaskList > -1) //aktualizacja zadania
                {
                    robotsTaskList[indexFromTaskList].IsEnd = tempRobot.IsEnd;
                    robotsTaskList[indexFromTaskList].X = tempRobot.X;
                    robotsTaskList[indexFromTaskList].Y = tempRobot.Y;
                    robotsTaskList[indexFromTaskList].Angle = tempRobot.Angle;
                }
                else
                    robotsTaskList.Add(tempRobot);
            }
        }

        private int getRobotTaskId(RobotTask tempRobot)
        {
            for (int i = 0; i < robotsTaskList.Count(); i++)
            {
                if (robotsTaskList[i].TaskID == tempRobot.TaskID)
                    return i;
            }

            return -1;
        }


       /* private void updateItem(string[] item)
        {
            string sID = item[1];
            double dX = double.Parse(item[2],System.Globalization.NumberStyles.Float);
            double dY = double.Parse(item[3]);
            double dAngle = double.Parse(item[4]);



            //  List<RobotTask> temp = robotsTaskList.Where(tt => tt.ID == sID);

        }*/

        private void getParticlePossitionDTP()
        {
            byte[] tablica;
            string temp;
            string[] tmp;

            while (!endTransmisionDTP)
            {
                tablica = udpDTP.Receive(ref endPointDTP);
                temp = System.Text.Encoding.Default.GetString(tablica);
                //mazeRobots.Clear();
                //robotsTaskList.Clear();

                foreach (var item in temp.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    tmp = item.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    if (tmp[0] == "insert")
                        addItem(tmp);
                    else if (tmp[0] == "reset")
                        removeAllRobotAndTask();
                   
                    /*else if (tmp[0] == "update")
                      updateItem(tmp);*/
                }

                RobotRefresh();
                
                
                //TaskRefresh();
                //    indexTable = int.Parse(tmp[0]); //aktaualny index;

                //    if ((tmp.Length == 5) && (indexTable < maxRobots)) //zmienic gdy ilosc danych do przeslania sie zmienia;
                //    {
                //        x = double.Parse(tmp[1].Replace(".", ","));
                //        y = double.Parse(tmp[2].Replace(".", ","));
                //        alfa = double.Parse(tmp[3].Replace(".", ","));

                //        if (double.TryParse(tmp[4].Replace(".", ","), out probability))
                //            //probability = double.Parse(tmp[4].Replace(".", ","));

                //            RobotsPF[indexTable].position = new Point2D(x * 100, y * 100);

                //    }

                //this.Invoke((MethodInvoker)delegate()
                //{
                //    if (dataGridView1.Rows.Count > indexTable)
                //    {
                //        dataGridView1.Rows[indexTable].Cells["X"].Value = x;
                //        dataGridView1.Rows[indexTable].Cells["Y"].Value = y;
                //        dataGridView1.Rows[indexTable].Cells["Prop"].Value = probability;
                //        dataGridView1.Rows[indexTable].Cells["Alfa"].Value = alfa;
                //    }
                //});
                //}
                this.Invoke((MethodInvoker)delegate()
                {
                    mazePanel_Paint(this, null);

                    refreshDGVList(robotsTaskList);
                });
            }
        }

        private void RobotRefresh()
        {
            mazeRobots.Clear();
            
            List<MazeRobot> tempRobot = new List<MazeRobot>();
            var temp = robotsTaskList.GroupBy(pp => pp.ID).Select(f => f.First()).ToList();

            foreach (var tmp in temp)
            {
                MazeRobot tt = new MazeRobot("Robot", tmp.ID, new Point2D(tmp.X, tmp.Y), (float)(1.0 * 100),new PointF(0,0));
                tt.UpdateArrowPosiotion(tmp.Angle);
                
                tempRobot.Add(tt);
                mazeRobots.Add(tt);
            }

            RobotsPF = tempRobot.ToArray();     
        }

        private void TaskRefresh()
        {
            List<Target> tempTarget = new List<Target>();

            foreach (var task in robotsTaskList)
            {
                Target targ = new Target(task.Task_X, task.Task_Y, task.IsEnd, task.ID, task.RobotID);
                mazeTargets.Add(targ);
            }
        }

        private void removeAllRobotAndTask()
        {
            robotsTaskList.Clear();
            mazeRobots.Clear();
            mazeTargets.Clear();
        }

        private void threadStopDTP()
        {
            if (reciveThreadDTP != null)
            {
                udpDTP.Close();
                endTransmisionDTP = true;

                udpDTP = null;

                reciveThreadDTP.Abort();
                reciveThreadDTP = null;
            }
        }

        private void threadPauseDTP()
        {
            reciveThread.Suspend();
        }

        private void butStopDTP_Click(object sender, EventArgs e)
        {
            threadStopDTP();
            bytStartDTP.Enabled = true;
            butStopDTP.Enabled = false;
        }

        private void butPauseDTP_Click(object sender, EventArgs e)
        {
            threadPauseDTP();
        }

        private class RobotTask
        {
            public string ID { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Angle { get; set; }
            public string TaskID { get; set; }

            private double dTask_X;
            public double Task_X 
            {
                get
                {
                    return dTask_X;
                }
                set
                {
                    dTask_X  = value;
                    iTask_X = Convert.ToInt32(value);
                }
            }

            private double dTask_Y;
            public double Task_Y 
            {
                get
                {
                    return dTask_Y;
                }
                set
                {
                    dTask_Y = value;
                    iTask_Y = Convert.ToInt32(value);
                }
            }
            public string Task_Name { get; set; }
            public bool IsEnd { get; set; }
            public int RobotID { get; set; }

            private int iTask_X;
            public int GetTaskX
            {
                get
                {
                    return iTask_X;
                }
            }

            private int iTask_Y;
            public int GetTaskY
            {
                get
                {
                    return iTask_Y;
                }
            }

            public RobotTask()
            {
            }
        }

        private List<RobotTask> robotsTaskList = new List<RobotTask>();

        private void refreshDGVList(List<RobotTask> taskList)
        {
            if (taskList.Count > dataGridView1DTP.Rows.Count)
                dataGridView1DTP.Rows.Add(taskList.Count - dataGridView1DTP.Rows.Count);
            else if (taskList.Count < dataGridView1DTP.Rows.Count)
            {
                dataGridView1DTP.Rows.Clear();

                if (taskList.Count > 0)
                    dataGridView1DTP.Rows.Add(taskList.Count);
            }

            for (int i = 0; i < taskList.Count; i++)
            {
                dataGridView1DTP.Rows[i].Cells["colID"].Value = taskList[i].ID;
                dataGridView1DTP.Rows[i].Cells["colX"].Value = taskList[i].X / 100;

                dataGridView1DTP.Rows[i].Cells["colY"].Value = taskList[i].Y / 100;
                dataGridView1DTP.Rows[i].Cells["colAngle"].Value = taskList[i].Angle;
                dataGridView1DTP.Rows[i].Cells["colTaskID"].Value = taskList[i].TaskID;
                dataGridView1DTP.Rows[i].Cells["colTask_X"].Value = taskList[i].Task_X / 100;
                dataGridView1DTP.Rows[i].Cells["colTask_Y"].Value = taskList[i].Task_Y / 100;

                dataGridView1DTP.Rows[i].Cells["colTask_Name"].Value = taskList[i].Task_Name;
                dataGridView1DTP.Rows[i].Cells["colIsEnd"].Value = taskList[i].IsEnd;
            }
        }

        #endregion

        private void worldNameTextBox_TextChanged(object sender, EventArgs e)
        {
            worldName = worldNameTextBox.Text;
        }

        private void iTimeout_ValueChanged(object sender, EventArgs e)
        {
            iWorldTimeout = Convert.ToInt32(iTimeout.Value);
        }

        private void MazeEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            butStop.PerformClick();
            butStopDTP.PerformClick();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double dSearchFactor = Convert.ToDouble(this.dSearchFactor.Value);
            double dPassageFactor = Convert.ToDouble(this.dPassageFactor.Value);
            double sumCost = RunSimulation(dSearchFactor, dPassageFactor);

            MessageBox.Show(this, string.Format("Sum cost BFS: {0}",sumCost.ToString()));
        }

        /* private void txtIP_TextChanged(object sender, EventArgs e)
         {

         }

         private void button1_Click(object sender, EventArgs e)
         {
             for (int i = 0; i < RobotsPF.Length; i++)
                 RobotsPF[i].Selected = false;

             if (dataGridView1.SelectedRows.Count > 0)
             {
                 for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                 {
                     int index = int.Parse(dataGridView1.SelectedRows[i].Cells["ID"].Value.ToString());

                     RobotsPF[index].Selected = true;
                 }
             }

             this.Invoke((MethodInvoker)delegate()
             {
                 mazePanel_Paint(this, null);
             });
         }*/

        #region ::  BFS ::

        public double RunSimulation(double dSearchFactor, double dPassageFactor)
        {
            MazeRobot currentRobot;
            mazeRobotsComparerClass compRobot = new mazeRobotsComparerClass();
            int minCostRoomToSearchIndex = -1;
            int roomRobotCurrentIndex = -1;

            int roomMinCostIndex = -1;
            double dCurrentPassageCost = 0.0;
            int roomToSearchCount = 0;
            
            Dijkstras.Graph graph = initGraph(mazeNodeNodes);
            List<string> spaceNodeList = getSpaceNode(mazeGraph.MazeGraphNodes);
             
            List<MazeSpaceNodesArea> allRooms =  initRoomToSearch(spaceNodeList,mazeSpaceNode, mazeRooms);
            roomToSearchCount = allRooms.Count; // -mazeRobots.Count;

            double[][] shortPathCost = initShortPathCost(allRooms, graph); //przelicz kosztu przejscia pomiedzy spaceNodeami (pokojami)

            initRobotsSpaceRoom(mazeRobots, mazeSpaceRobots); //kazdy robot trafil do innego pomieszzczenie 

            //for (int i = 0; i < mazeRobots.Count; i++) //ustaw pomieszczenia w ktorych sa roboty jako przeszukane i dodaj do nadaj robotom koszt 
            //{
            //    currentRobot = mazeRobots[i] as MazeRobot;
            //    roomRobotCurrentIndex = getIndexRoomByName(currentRobot.CurrentRoom, allRooms); //pobieram index pokoju w ktorym jest aktualnie robot

            //    currentRobot.TracePathRobot = string.Format("{0};", allRooms[roomRobotCurrentIndex].SpaceId);
            //    currentRobot.PassageCost += allRooms[roomRobotCurrentIndex].Area * dSearchFactor; //koszt przeszukania pokoju w ktorym aktualnie jest robot 
            //    currentRobot.TraceCostRobot += string.Format("{0};", currentRobot.PassageCost.ToString());

            //    allRooms[roomRobotCurrentIndex].Searched = true;   //oznacz pokoj jako przeszukany             
            //}

            while (roomToSearchCount != 0)
            {
                currentRobot = mazeRobots[0] as MazeRobot;

                roomRobotCurrentIndex = getIndexRoomByName(currentRobot.CurrentRoom, allRooms); //pobieram index pokoju w ktorym jest aktualnie robot

                if (!allRooms[roomRobotCurrentIndex].Searched) //pomieszczenie nie zostalo jeszcze przeszukane
                {
                    currentRobot.TracePathRobot = string.Format("{0};", allRooms[roomRobotCurrentIndex].SpaceId);
                    currentRobot.PassageCost += allRooms[roomRobotCurrentIndex].Area * dSearchFactor; //koszt przeszukania pokoju w ktorym aktualnie jest robot 
                    currentRobot.TraceCostRobot += string.Format("{0};", currentRobot.PassageCost.ToString());

                    allRooms[roomRobotCurrentIndex].Searched = true;   //oznacz pokoj jako przeszukany             
                }
                else
                {
                    if (roomRobotCurrentIndex == -1)
                        throw new Exception("Wrong getIndexRoomByName -> roomRobotCurrentIndex");

                    minCostRoomToSearchIndex = getIndexMinCost(shortPathCost[roomRobotCurrentIndex], allRooms); //szukam najblizszy pokoj do przeszukania

                    if (minCostRoomToSearchIndex == -1)
                        throw new Exception("Wrong getIndexMinCost -> minCostRoomToSearchIndex");

                    dCurrentPassageCost = shortPathCost[roomRobotCurrentIndex][minCostRoomToSearchIndex]; //koszt przejscia do pomieszczenia 
                    currentRobot.PassageCost += dCurrentPassageCost * dPassageFactor; //dodanie kosztu przejscia do najblizszego pomieszczenia
                    currentRobot.TraceCostRobot += string.Format("{0};", dCurrentPassageCost);

                    currentRobot.CurrentRoom = allRooms[minCostRoomToSearchIndex].SpaceId; //ustawienie aktulnie przeszukiwanego pomieszczenia

                    currentRobot.TracePathRobot += string.Format("{0};", currentRobot.CurrentRoom);
                    currentRobot.PassageCost += allRooms[minCostRoomToSearchIndex].Area * dSearchFactor; //koszt przeszukania pokoju w ktorym aktualnie jest robot
                    currentRobot.TraceCostRobot += string.Format("{0};", allRooms[minCostRoomToSearchIndex].Area * dSearchFactor);

                    allRooms[minCostRoomToSearchIndex].Searched = true; //oznacz pokoj jako przeszukany 
                }

                roomToSearchCount--;
                mazeRobots.Sort(0, mazeRobots.Count, compRobot);

            }

            double sumRobotCost = 0.0;
            double maxRobotCost = 0.0;

            foreach (MazeRobot robot in mazeRobots)
            {
                System.Diagnostics.Trace.Write(string.Format("{0}\tCost: {1} \tCostRobot: {2}\n", robot.TracePathRobot, robot.PassageCost.ToString(), robot.TraceCostRobot));
                
                sumRobotCost += robot.PassageCost;

                if (robot.PassageCost > maxRobotCost)
                    maxRobotCost = robot.PassageCost;
            }

            //return sumRobotCost;
            return maxRobotCost;
        }

        private int getIndexMinCost(double[] dCosts, List<MazeSpaceNodesArea> allRooms)
        {
            double tempCost = double.MaxValue;
            int index = -1;

            for (int i = 0; i < dCosts.Length; i++)
            {
                if ((dCosts[i] > 0.0) && (allRooms[i].Searched == false))
                {
                    if (dCosts[i] < tempCost)
                    {
                        index = i;
                        tempCost = dCosts[i];
                    } 
                }
            }

            return index;
        }

        private int getIndexRoomByName(string sSpaceId, List<MazeSpaceNodesArea> allRooms)
        {
            for (int i = 0; i < allRooms.Count; i++)
                if (allRooms[i].SpaceId == sSpaceId)
                    return i;

            return -1;
        }

        public class mazeRobotsComparerClass : IComparer
        {
            int IComparer.Compare(Object robot1, Object robot2)
            {
                MazeRobot tempRobot1 = robot1 as MazeRobot;
                MazeRobot tempRobot2 = robot2 as MazeRobot;

                if (tempRobot1.PassageCost == tempRobot2.PassageCost)
                    return 0;
                else if (tempRobot1.PassageCost > tempRobot2.PassageCost)
                    return 1;
                else
                    return -1;
            }
        }

        private void initRobotsSpaceRoom(ArrayList mazeRobots, ArrayList mazeSpaceRobots)
        {
            MazeSpaceRobots tempSpace;
            int index;

            foreach (MazeRobot itemRobot in mazeRobots)
            {
                index = getSpaceIndexByRobotName(itemRobot.ID,mazeSpaceRobots);

                if(index == -1)
                    throw new Exception("Cannot getSpaceNameByRobotName -> sMazeSpaceRobot");
            
                tempSpace = mazeSpaceRobots[index] as MazeSpaceRobots;

                itemRobot.CurrentRoom = tempSpace.SpaceId;
                itemRobot.TracePathRobot = string.Format("{0};", itemRobot.CurrentRoom);

                  
                //sMazeSpaceRobot = roomToSerch[index].SpaceId;

                //dCost = getRoomCostByName(sMazeSpaceRobot,roomToSerch) * dSearchFactor;

                //if(dCost <= 0.0)
                //    throw new Exception("Wrong cost getRoomCostByName -> dCost");

                ////itemRobot.PassageCost += dCost;
                //itemRobot.CurrentRoom = sMazeSpaceRobot;

                ////roomToSerch.RemoveAt(index); //usun z listy pomieszczeni do przeszukania
            }
        }

        private double getRoomCostByName(string sRoomName, List<MazeSpaceNodesArea> roomToSerch)
        {
            foreach (var room in roomToSerch)
                if (sRoomName == room.SpaceId)
                    return room.Area;

            return 0.0;
        }

        private int getSpaceIndexByRobotName(string sRobotId, ArrayList mazeSpaceRobots)
        {
            MazeSpaceRobots itemSpaceRobot;

            for (int i = 0; i < mazeSpaceRobots.Count; i++)
			{
                itemSpaceRobot = mazeSpaceRobots[i] as MazeSpaceRobots;
                
                if (itemSpaceRobot.RobotId == sRobotId)
                    return i;
			} 
              
            return -1;
        }

        private double[][] initShortPathCost(List<MazeSpaceNodesArea> roomToSerch, Dijkstras.Graph graph)
        {
            double[][] shortPathCostTemp = new double[roomToSerch.Count][];

            for (int i = 0; i < shortPathCostTemp.Length; i++)
                shortPathCostTemp[i] = new double[shortPathCostTemp.Length];

            MazeSpaceNodesArea tempStart,tempEnd;

            for (int i = 0; i < roomToSerch.Count; i++)
            {
                tempStart = roomToSerch[i] as MazeSpaceNodesArea;

                for (int j = 0; j < roomToSerch.Count; j++)
                {  
                    tempEnd = roomToSerch[j] as MazeSpaceNodesArea; 
                    shortPathCostTemp[i][j] =  graph.SumShortestPathConst(tempStart.NodeId,tempEnd.NodeId);
                }
            }

            return shortPathCostTemp;
        }

        private List<string> getSpaceNode(ArrayList mazeGraphNodes)
        {
            List<string> tempSpaceNodes = new List<string>();

            foreach (MazeNode node in mazeGraphNodes)
            {
                if (node.MazeGraphNodeType == MazeNodeType.SpaceNode)
                    tempSpaceNodes.Add(node.ID);
            }


            return tempSpaceNodes;
        }

        private List<MazeSpaceNodesArea> initRoomToSearch(List<string> spaceNodeList, ArrayList mazeSpaceNode, ArrayList mazeRooms)
        {
            string sCurrentSpaceID;
            double? dArea;
            List<MazeSpaceNodesArea> tempSpaceNodeRoom = new List<MazeSpaceNodesArea>();
            int iCurrentIndex = 0;

            foreach (string itemNode in spaceNodeList)
            {
                sCurrentSpaceID = getSpaceID(itemNode, mazeSpaceNode);

                 if(string.IsNullOrEmpty(sCurrentSpaceID))
                     throw new Exception("Cannot find Space ID by itemNode ");

                 dArea = getArea(sCurrentSpaceID,mazeRooms);

                if(!dArea.HasValue)
                    throw new Exception("Cannot get Area from  sCurrentSpaceID");

                tempSpaceNodeRoom.Add(new MazeSpaceNodesArea(sCurrentSpaceID, itemNode, dArea.Value, iCurrentIndex));
                iCurrentIndex++;
            }

            return tempSpaceNodeRoom;
        }

        private double? getArea(string sCurrentSpaceID, ArrayList mazeRooms)
        {
            foreach (MazeSpace item in mazeRooms)
            {
                if (sCurrentSpaceID == item.ID)
                    return item.Area;
            }
            return null;
        }


        private string getSpaceID(string itemNode, ArrayList mazeSpaceNode)
        {
            foreach (MazeSpaceNodes item in mazeSpaceNode)
                if (item.NodeId == itemNode)
                    return item.SpaceId;

            return null;
        }

        private MazeSpace getSpaceByName( ArrayList mazeRooms,string sId)
        {
            foreach (MazeSpace item in mazeRooms)
                if (item.ID == sId)
                    return item;

            return null;
        }

        private Dijkstras.Graph initGraph(ArrayList mazeNodeNades)
        {
            Dijkstras.Graph temp = new Dijkstras.Graph();

            foreach (MazeNodeNodes item in mazeNodeNades)
            {
                temp.add_vertex(item.NodeFromId, item.NodeToId, item.Cost);
                System.Diagnostics.Trace.Write(string.Format("{0};{1};{2};\n", item.NodeFromId, item.NodeToId, item.Cost));
            }

            return temp;
        }

        private void butSelectDirectory_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                txtWorkingPath.Text = folderBrowserDialog1.SelectedPath;
        }

        private void butRun_Click(object sender, EventArgs e)
        {
            string[] fileRosonList;
            string result = string.Empty;
            double dSearchFactor = Convert.ToDouble(this.dSearchFactor.Value);
            double dPassageFactor =Convert.ToDouble(this.dPassageFactor.Value);
            double dSumCost;

            if (Directory.Exists(txtWorkingPath.Text))
            {
                fileRosonList = Directory.GetFiles(txtWorkingPath.Text, "*.roson");

                foreach (var item in fileRosonList)
                {
                    LoadMaze(item);
                    dSumCost = RunSimulation(dSearchFactor, dPassageFactor);
                    result += string.Format("{0};{1};\n", item, dSumCost);
                }  
            }

            File.WriteAllText(string.Format("{0}\\result.csv", txtWorkingPath.Text), result);
        }

        private void butWallUpdate_Click(object sender, EventArgs e)
        {
            MazeWall wall = mazeWallsListBox.SelectedItem as MazeWall;

            wall.points[0].X = float.Parse(txtFromX.Text);
            wall.points[0].Y = float.Parse(txtFromY.Text);
            wall.points[1].X = float.Parse(txtToX.Text);
            wall.points[1].Y = float.Parse(txtToY.Text);
        }

        private void butRemoveCurrentWall_Click(object sender, EventArgs e)
        {
            MazeWall wall = mazeWallsListBox.SelectedItem as MazeWall;
            MazeWall selectedWallTemp = null;

            foreach (MazeWall item in mazeWalls)
            {
                if (wall.ID == item.ID)
                {
                    selectedWallTemp = item;
                    break;
                }
            }

            if (selectedWallTemp != null)
            {
                mazeWalls.Remove(selectedWallTemp);
                RefreshMazeWallsTreeView();
            }
            mazePanel_Paint(this, null);
        }

        //private double[][] calculatePathCosts()
        //{

        //}

        #endregion

    }
}

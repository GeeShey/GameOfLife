using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Testing
{
    public partial class Form1 : Form
    {
        // The universe array
        bool[,] universe = new bool[10, 10];

        //decides how the border should behave
        bool toroidal = false;

        //path of the directory where the file is saved
        string filename;

        //view menu booleans
        bool displaySmallGrid = false;
        bool displayBigGrid = true;
        bool displayNeighborCount = false;
        bool displayHud = true;


       

        //customizable colors
        Color GridBorder = Color.Black;
        Color GridAliveFill = Color.Gray;
        Color GridDedFill = Color.White;
        Color AliveNeighbor = Color.Green;
        Color DedNeighbor = Color.Red;
        Color BigGridBorder = Color.Black;



        // integers to keep track of alive cells
        int alive;
        int ded;

        // The Timer class
        Timer timer = new Timer();
        int interval = 100;

        // Generation count
        int generations = 0;

        //accumulating smaller squares to highlight bigger squares
        int graphSections = 5;

        public Form1()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = interval; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // timer running status
            label1.Text = "";//HUD text

        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            bool[,] nextGen = calculateNextGen();
            universe = nextGen;
            graphicsPanel1.Invalidate();

            // Increment generation count
            generations++;


            // Update status strip generations
            alive = aliveCellCount();
            ded = (universe.GetLength(0) * universe.GetLength(0)) - alive;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString()+ " | Alive: "+ alive.ToString() + " | Dead: " + ded;
        }
        // The event called by the timer every Interval milliseconds.

        private int aliveCellCount()
        {
            int count = 0;
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x, y])
                    {
                        count++;
                    }
                }

            }
            return count;


        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private int countNeighbours(int _xCoordinate, int _yCoordinate)
            //this is hard-coded to compare the elements in the universe array
        {
            int neighbors = 0;

            for (int x = _xCoordinate - 1; x <= _xCoordinate + 1; x++)
                //starting from the element on the top left of the coordinate
                //I traverse all the way to the bottom right of the coordinate
            {
                for (int y = _yCoordinate - 1; y <= _yCoordinate + 1; y++)
                {
                    if (x == _xCoordinate && y == _yCoordinate)
                    {
                        continue;
                    }
                    bool outOfBoundsFlag = false;//a flag to see if an element outside the bounds is requested
                                                   //only true when toroidal is false and an outofBounds element is requested
                    int circularX = x;
                    int circularY = y;

                    if (y == universe.GetLength(0) || y < 0)
                    {
                        //element is on the bottom/top row

                        //element is on bottom row
                        if (y == universe.GetLength(0))
                        {
                            if (toroidal)
                                circularY = 0;
                            else
                                outOfBoundsFlag =true;
                        }
                        else //element is on the top row
                        {
                            if (toroidal)
                                circularY = universe.GetLength(0) - 1;
                            else
                                outOfBoundsFlag = true;

                        }

                    }
                    if (x == universe.GetLength(0) || x < 0)
                    {
                        //element is on the leftmost/rightmost column

                        //element is on the right
                        if (x == universe.GetLength(0))
                        {
                            if (toroidal)
                                circularX = 0;
                            else
                                outOfBoundsFlag = true;


                        }
                        else //element is on the left
                        {
                            if (toroidal)
                                circularX = universe.GetLength(0) - 1;
                            else
                                outOfBoundsFlag = true;

                        }

                    }

                    //no conflict(element is not on the edge
                    if (!outOfBoundsFlag)
                    {
                        if (universe[circularX, circularY])
                        {
                            neighbors++;
                        }
                    }
                }
            }
            return neighbors;

        }

        private bool nextGenStatus(int x, int y)//returns the state of an element in the next Generation
        {
            int neighbors = countNeighbours(x, y);
            bool _nextGenStatus = false;
            if (universe[x, y])//cell is currently alive
            {


                if (neighbors < 2 || neighbors > 3)
                {
                    _nextGenStatus = false;
                }

                else
                {
                    _nextGenStatus = true;
                }

            }
            else//cell is currently dead
            {
                if (neighbors == 3)
                {
                    _nextGenStatus = true;
                }
            }

            return _nextGenStatus;

        }

        private bool[,] calculateNextGen()//returns a 2D bool array with the status of all values in the next Gen
        {
            
            bool[,] nextGen = new bool[universe.GetLength(0), universe.GetLength(1)];
            //this is just to initialise a new 2D bool array with the same dimensions as universe

            nextGen = turnOffEverything(nextGen);//initialising everything to false(dead cells)

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (nextGenStatus(x, y))
                    {
                        nextGen[x, y] = true;
                    }
                }

            }


                    return nextGen;
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            updateHUD();

           graphicsPanel1.BackColor = GridDedFill;
            //TODO FIX THE SCALE OF EACH RECTANGLE SO THAT IT STICKS TO THE EDGE OF THE RECTANGLE

            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellWidth =(float) graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            float cellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(GridBorder, 1);
            

            //A pen for the bigger square regions
            Pen thicc = new Pen(BigGridBorder, 2);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(GridAliveFill);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    RectangleF cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;
                    

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);

                        Font font = new Font("Arial", 15);

                        //adding the code to display neighbor count on correspoding cells
                        
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        int neighbors = countNeighbours(x, y);
                        System.Drawing.SolidBrush b;
                        if (nextGenStatus(x, y))//if the cell will be alive next generation make the font green
                        {
                            b = new System.Drawing.SolidBrush(AliveNeighbor);


                        }
                        else
                        {
                            b = new System.Drawing.SolidBrush(DedNeighbor);
                        }
                        if(!displayNeighborCount)
                        e.Graphics.DrawString(""+neighbors+"", font, b, cellRect, stringFormat);

                        
                        b.Dispose();
                    }
                    else
                    {
                        //This is the code to display neigbours in dead cells
                        if (countNeighbours(x,y)>0)
                        {

                            Font font = new Font("Arial", 15);

                            //adding the code to display neighbor count on correspoding cells

                            StringFormat stringFormat = new StringFormat();
                            stringFormat.Alignment = StringAlignment.Center;
                            stringFormat.LineAlignment = StringAlignment.Center;

                            int neighbors = countNeighbours(x, y);
                            System.Drawing.SolidBrush b ;
                            if (nextGenStatus(x, y))//if the cell will be alive next generation make the font green
                            {
                                b = new System.Drawing.SolidBrush(AliveNeighbor);


                            }
                            else
                            {
                                b = new System.Drawing.SolidBrush(DedNeighbor);
                            }
                            if (!displayNeighborCount)
                                e.Graphics.DrawString("" + neighbors + "", font, b, cellRect, stringFormat);


                            b.Dispose();

                        }
                        
                        if(!displaySmallGrid)    
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    }

                    //drawing the bigger sections 
                    if (displayBigGrid)
                    {
                        if ((x + graphSections) % graphSections == 0 && (y + graphSections) % graphSections == 0)
                        {

                            //drawing the bigger squares
                            e.Graphics.DrawRectangle(thicc, cellRect.X,
                                                            cellRect.Y,
                                                            cellRect.Width * graphSections,
                                                            cellRect.Height * graphSections);

                        }
                    }
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
            
        }

        private void clearScreen()
        {
            turnOffEverything(universe);
            graphicsPanel1.Invalidate();

        }

        private void playGenerations()
        {
            timer.Enabled = true;

        }

        private void pauseGenerations()
        {
            timer.Enabled = false;
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            float cellWidth = (float)graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            float cellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1);
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                pauseGenerations();
                

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = (int)(e.X / cellWidth);
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = (int)(e.Y / cellHeight);

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }

            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show((Control)sender , new Point(e.X, e.Y));

            }


        }

        private void randomizeUniverse()
        {
            
            Random rand = new Random();
            int seed = rand.Next(100000);
            rand = new Random(seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int boolVal = rand.Next(2);
                    if (boolVal == 1)
                        universe[x, y] = true;
                    else
                        universe[x, y] = false;
                }
            }

            //resetting the toolbar values
            alive = aliveCellCount();
            ded = (universe.GetLength(0) * universe.GetLength(1)) - alive;
            generations = 0;
            

        }

        //this function displays a dialogue box with the "title" and returns a numeric function
        private int getIntegerInput(String title)
        {
            int input = 0;
            //displaying a seed menu
            Form dlg1 = new Form();
            dlg1.Text = title;

            dlg1.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg1.MinimizeBox = false;
            dlg1.MaximizeBox = false;

            Button ok = new Button();
            Button cancel = new Button();

            ok.Text = "OK";
            cancel.Text = "Cancel";

            ok.DialogResult = DialogResult.OK;
            cancel.DialogResult = DialogResult.Cancel;

            ok.Location = new Point(100, 100);
            cancel.Location = new Point(ok.Left, ok.Height + ok.Top + 10);

            dlg1.AcceptButton = ok;
            dlg1.CancelButton = cancel;

            int size = dlg1.Size.Width;

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(200, 23);
            textBox.Location = new System.Drawing.Point(50, 50);
            dlg1.Controls.Add(textBox);


            dlg1.Controls.Add(ok);
            dlg1.Controls.Add(cancel);
            dlg1.ShowDialog();

            if (dlg1.DialogResult == DialogResult.OK)
            {

                int result = 0;
                bool isNumeric = int.TryParse(textBox.Text, out result);

                if (isNumeric)
                {
                    input = result;
                }
                else
                {
                    MessageBox.Show("Please enter a valid integer :)");
                    dlg1.Close();
                }
            }

            return input;
        }

        private void randomizeUniverse(int seed)//randomize function which takes an int as the seed
        {

            Random rand = new Random();
            rand = new Random(seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int boolVal = rand.Next(2);
                    if (boolVal == 1)
                        universe[x, y] = true;
                    else
                        universe[x, y] = false;
                }
            }

            //updating the status bar
            alive = aliveCellCount();
            ded = (universe.GetLength(0) * universe.GetLength(0)) - alive;
            generations = 0;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString() + " | Alive: " + alive.ToString() + " | Dead: " + ded;


        }

        private bool[,] turnOffEverything(bool[,] _input)//just changes the values of everything in the universe to 0
        {
            for (int y = 0; y < _input.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < _input.GetLength(0); x++)
                {
                    _input[x, y] = false;
                }
            }

            return _input;
        
        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pauseGenerations();
            clearScreen();
            alive = aliveCellCount();
            ded = (universe.GetLength(0) * universe.GetLength(0)) - alive;
            generations = 0;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString() + " | Alive: " + alive.ToString() + " | Dead: " + ded;

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //when a new instance is created, program loads previous pref data
            loadPrefs();

            updateHUD();
            graphicsPanel1.Invalidate();
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            pauseGenerations();
            clearScreen();
        }

        private void playToolStripButton_Click(object sender, EventArgs e)
        {
            playGenerations();
        }

        private void pauseToolStripButton_Click(object sender, EventArgs e)
        {
            pauseGenerations();
        }

        private void stepToolStripButton_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }

            NextGeneration();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse();
            graphicsPanel1.Invalidate();
        }

        private void timeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        //all the randomize functions

        //getting the time data using system.time
        private void seedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int input = getIntegerInput("Choose Seed");
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse(input);
            graphicsPanel1.Invalidate();

        }
        private void secondsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse(DateTime.Now.Second);
            graphicsPanel1.Invalidate();
        }

        private void millisecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse(DateTime.Now.Millisecond);
            graphicsPanel1.Invalidate();

        }

        private void minutesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse(DateTime.Now.Minute);
            graphicsPanel1.Invalidate();

        }

        private void hoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse(DateTime.Now.Hour);
            graphicsPanel1.Invalidate();


        }

        private void daysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                pauseGenerations();
            }
            randomizeUniverse(DateTime.Now.Day);
            graphicsPanel1.Invalidate();
        }

        //when the users chooses to save the data, we will first check if they have saved it already
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //checking if already saved
            if (filename==null)
            {
                saveAsToolStripMenuItem_Click(sender,e);

            }
            else
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "All Files|*.*|Cells|*.cells";
                dlg.FilterIndex = 2; dlg.DefaultExt = "cells";


                    StreamWriter writer = new StreamWriter(filename);

                    writer.WriteLine("!File generated by Shekhar Sai's program.");

                    // Iterate through the universe one row at a time.
                    for (int y = 0; y < universe.GetLength(1); y++)
                    {
                        // Create a string to represent the current row.
                        String currentRow = string.Empty;

                        // Iterate through the current row one cell at a time.
                        for (int x = 0; x < universe.GetLength(0); x++)
                        {
                            if (universe[x, y])
                            {
                                currentRow = currentRow + 'O';
                            }
                            else
                            {
                                currentRow = currentRow + '.';
                            }
                        }
                        writer.WriteLine(currentRow);
                    }
                    writer.Close();
                

            }
           
        }

        //implementing the save-as functionality
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";

            
            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);
                filename = dlg.FileName;

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine("!File generated by Shekhar Sai's program.");

                // Iterate through the universe one row at a time.
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;

                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        if (universe[x, y])
                        {
                            currentRow = currentRow + 'O';
                        }
                        else
                        {
                            currentRow = currentRow + '.';
                        }
                        // If the universe[x,y] is alive then append 'O' (capital O)
                        // to the row string.

                        // Else if the universe[x,y] is dead then append '.' (period)
                        // to the row string.
                    }

                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                    writer.WriteLine(currentRow);
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }

        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(sender, e);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);

        }

        //opening an existing file
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;//we are assuming that we will only deal with squares

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    //ignoring comments
                    if(!row.StartsWith("!"))
                    {
                        maxWidth = row.Length;

                    }

                }

                if (maxWidth != universe.GetLength(0))
                {
                    MessageBox.Show("Grid has been resized");
                }
                universe = new bool[maxWidth, maxWidth];
                // Resizing the current universe 

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                int yPos=0;
                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();
                    if (!row.StartsWith("!"))
                    {
                        maxWidth = row.Length;

                        // If the row is not a comment then 
                        // it is a row of cells and needs to be iterated through.
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos].Equals('O'))
                            {
                                universe[xPos,yPos] = true;
                            }
                            else
                            {
                                universe[xPos, yPos] = false;
                            }
                            // If row[xPos] is a 'O' (capital O) then
                            // set the corresponding cell in the universe to alive.

                            // If row[xPos] is a '.' (period) then
                            // set the corresponding cell in the universe to dead.
                        }
                        yPos++;

                    }
                    
                }

                // Close the file.
                reader.Close();
                graphicsPanel1.Invalidate();
            }


        }

        private void changeIntervalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pauseGenerations();

            //getting the interval input value from the user

            interval = getIntegerInput("Interval");
            timer.Interval = interval;


        }

        //implementing the resize feature
        private void resizeUniverseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pauseGenerations();

            int temp = getIntegerInput("size of the new universe");
            universe = new bool[temp, temp];
            graphicsPanel1.Invalidate();
        }

        private void neighborToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (displayNeighborCount)
                displayNeighborCount = false;
            else
            {
                displayNeighborCount = true;
            }

            graphicsPanel1.Invalidate();
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(displayBigGrid)
            displayBigGrid = false;
            else
            {
                displayBigGrid = true;
            }

            graphicsPanel1.Invalidate();
        }

        private void enableDisableSmallGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (displaySmallGrid)
                displaySmallGrid = false;
            else
            {
                displaySmallGrid = true;
            }

            graphicsPanel1.Invalidate();

        }

        private Color GetColor(String msg, Color col)
        {
            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(200, 23);
            textBox.Location = new System.Drawing.Point(50, 50);
            textBox.Text = msg;


            ColorDialog MyDialog = new ColorDialog();
            // Keeps the user from selecting a custom color.
            MyDialog.AllowFullOpen = false;

            MyDialog.Color = col;
            // Allows the user to get help. (The default is false.)
            MyDialog.ShowHelp = true;
            // Sets the initial color select to the current text color.

            // Update the text box color if the user clicks OK 
            if (MyDialog.ShowDialog() == DialogResult.OK)
                textBox.ForeColor = MyDialog.Color;

            return MyDialog.Color;

        }

        private void customizeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void changeAliveNeighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DedNeighbor = GetColor("Choose Neighbor Count Color for alive cells",DedNeighbor);
            graphicsPanel1.Invalidate();
        }

        private void changeDeadNeigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AliveNeighbor = GetColor("Choose Neighbour Count Color for dead cells",AliveNeighbor);
            graphicsPanel1.Invalidate();

        }

        private void changeGridBorderColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridBorder = GetColor("Choose Small Grid Color",GridBorder);
            graphicsPanel1.Invalidate();
        }

        private void changeBigGridBorderColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BigGridBorder = GetColor("Choose Big Grid Color",BigGridBorder);
            graphicsPanel1.Invalidate();
        }

        private void changeGridFillColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridAliveFill = GetColor("Choose Grid Fill Color", GridAliveFill);
            graphicsPanel1.Invalidate();
        }

        private void changeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridDedFill = GetColor("Choose Background Color", GridDedFill);
            graphicsPanel1.Invalidate();
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (toroidal)
                toroidal = false;
            else
                toroidal = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            pauseGenerations();
        }

        private void hUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (displayHud)
            {
                displayHud = false;
                updateHUD();
                graphicsPanel1.Invalidate();
            }
            else
            {
                displayHud = true;
                updateHUD();
                graphicsPanel1.Invalidate();
            }
                
        }

        private void updateHUD()
        {
            String HUDmsg;
            if (displayHud)
            {
                HUDmsg = "";
            }
            else
            {
                string small, big, NC;
                if (!displaySmallGrid)
                {
                    small = "Small Gridlines: Enabled";
                }
                else
                {
                    small = "Small Gridlines: Disabled";
                }

                if (displayBigGrid)
                {
                    big = "Big Gridlines: Enabled";
                }
                else
                {
                    big = "Big Gridlines: Disabled";
                }

                if (!displayNeighborCount)
                {
                    NC = "Neighbour Count: Enabled";
                }
                else
                {
                    NC = "Neighbour Count: Disabled";
                }
                HUDmsg = small + "\n" + big + "\n" + NC + "\n ";
            }
            label1.Text = HUDmsg;
            label1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);


        }

        private void playPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playGenerations();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pauseGenerations();
        }

        private void hUDToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            displayHud = !displayHud;
            graphicsPanel1.Invalidate();
        }

        private void clearScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearScreen();

        }

        private void savePreferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            storePrefs();

        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadPrefs();
            graphicsPanel1.Invalidate();
        }

        private void loadPrefs()
        {
            //if file does not exits, show a message box
            if (File.Exists("prefs.txt"))
            {
                StreamReader reader = new StreamReader("prefs.txt");

                string row = reader.ReadLine();
                universe = new bool[int.Parse(row), int.Parse(row)];

                row = reader.ReadLine();
                timer.Interval = int.Parse(row);

                row = reader.ReadLine();
                GridBorder = Color.FromArgb(Convert.ToInt32(row));

                row = reader.ReadLine();
                GridAliveFill = Color.FromArgb(Convert.ToInt32(row));

                row = reader.ReadLine();
                GridDedFill = Color.FromArgb(Convert.ToInt32(row));

                row = reader.ReadLine();
                AliveNeighbor = Color.FromArgb(Convert.ToInt32(row));

                row = reader.ReadLine();
                DedNeighbor = Color.FromArgb(Convert.ToInt32(row));

                row = reader.ReadLine();
                BigGridBorder = Color.FromArgb(Convert.ToInt32(row));

                row = reader.ReadLine();
                displaySmallGrid = Boolean.Parse(row);

                row = reader.ReadLine();
                displayBigGrid = Boolean.Parse(row);

                row = reader.ReadLine();
                displayNeighborCount = Boolean.Parse(row);

                row = reader.ReadLine();
                displayHud = Boolean.Parse(row);


                reader.Close();
                MessageBox.Show("preference file loaded");

            }

            else
            {
                MessageBox.Show("preference file does not exist");

            }
            

        }

        private void storePrefs()
        {
            StreamWriter writer = new StreamWriter("prefs.txt");//creates a file in the local directory

            //storing preference data

            writer.WriteLine(universe.GetLength(0));
            writer.WriteLine(timer.Interval);

            writer.WriteLine(Convert.ToString(GridBorder.ToArgb()));
            writer.WriteLine(Convert.ToString(GridAliveFill.ToArgb()));
            writer.WriteLine(Convert.ToString(GridDedFill.ToArgb()));
            writer.WriteLine(Convert.ToString(AliveNeighbor.ToArgb()));
            writer.WriteLine(Convert.ToString(DedNeighbor.ToArgb()));
            writer.WriteLine(Convert.ToString(BigGridBorder.ToArgb()));
            writer.WriteLine(displaySmallGrid.ToString());
            writer.WriteLine(displayBigGrid.ToString());
            writer.WriteLine(displayNeighborCount.ToString());
            writer.WriteLine(displayHud.ToString());

            writer.Close();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            storePrefs();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {

            displaySmallGrid = false;
            displayBigGrid = true;
            displayNeighborCount = false;
            displayHud = true;

            GridBorder = Color.Black;
            GridAliveFill = Color.Gray;
            GridDedFill = Color.White;
            AliveNeighbor = Color.Green;
            DedNeighbor = Color.Red;
            BigGridBorder = Color.Black;
            interval = 100;
            graphicsPanel1.Invalidate();

        }
    }
}

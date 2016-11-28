using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Draw1
{
    public partial class Form1 : Form
    {
        bool lineStarted = false;
        bool ellipseSarted = false;
        bool firstPoint = true;
        bool textStarted = false;
        bool gradBoxStarted = false;
        Color lastColor = Color.Gold;
        Point start;
        Point end;
        float penSize = 1.0f;
        ArrayList drawing = new ArrayList();
        Font gFont = null;

       // private Graphics g;
        BufferedGraphicsContext currentContext;
        BufferedGraphics myBuffer;


        public enum drawinObjectType { none, point, line, ellipse, text, gradentBox};

        public class drawingObject
        {
            Color color = Color.Blue;
            Color color2 = Color.Green;
            float penSize = 1.0f;
            drawinObjectType dot = drawinObjectType.none;
            public bool selected = false;
            string text;
            Point start;            
            Point end;
            bool highlight = false;
            Font sfont;
            float mTop;
            float mBottom;
            float b;
            public static int SnapSetting = 25; //Number * Number or a^2 dist check (Or Snap is 5 pixels)

            public drawingObject(Color co, drawinObjectType lt, Point s, Point e, float penSz)
            {
                color = co;
                dot = lt;
                start = s;
                end = e;
                penSize = penSz;
            }
            public drawingObject(Color co, drawinObjectType lt, Point s, string msg, float penSz, Font f)
            {
                color = co;
                dot = lt;
                start = s;
                text = msg;
                penSize = penSz;
                sfont = f;
            }
            public drawingObject(Color co1, Color co2, drawinObjectType lt, Point s, Point e, float penSz)
            {
                color = co1;
                color2 = co2;                
                dot = lt;
                start = s;
                end = e;
                penSize = penSz;
            }

            public void Draw(Graphics g)
            {
                switch (dot)
                {
                    case drawinObjectType.line:
                        Pen p;
                        if (highlight)
                        {
                            if (color != color2)
                                p = new Pen(color2, penSize+2);
                            else
                                p = new Pen(Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B), penSize+2); 
                        }
                        else
                            p = new Pen(color, penSize);
                        g.DrawLine(p, start, end);
                        break;
                    case drawinObjectType.ellipse:
                        if (highlight)
                        {
                            if (color != color2)
                            {
                                p = new Pen(color2, penSize + 2);
                                p.DashStyle = DashStyle.Dash;
                            }

                            else
                            {
                                p = new Pen(Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B), penSize + 2);
                                p.DashStyle = DashStyle.Dash;
                            }
                        }
                        else
                            p = new Pen(color, penSize);
                        float radius = (float)Math.Sqrt(end.X * end.X + end.Y * end.Y);
                        g.DrawEllipse(p, start.X - radius, start.Y - radius, 2 * radius, 2 * radius);//new Rectangle(start, (Size)end));                        
                        g.DrawLine(p, start.X - 5, start.Y, start.X + 5, start.Y);
                        g.DrawLine(p, start.X, start.Y - 5, start.X, start.Y + 5);
                        break;
                    case drawinObjectType.text:
                        p = new Pen(color, penSize);
                        System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(color);
                        g.DrawString(text, sfont, myBrush, start);
                        //float stringLength = g.MeasureString(text, new Font(FontFamily.GenericSerif, 12.0f));
                        break;
                    case drawinObjectType.gradentBox:
                        Rectangle rect = new Rectangle(start, new Size(end.X - start.X, end.Y - start.Y));
                        if (rect.Width == 0)
                            break;
                        if (rect.Height == 0)
                            break;
                        LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(
   rect,
   color,
   color2,
   LinearGradientMode.Horizontal);
                        g.FillRectangle(myLinearGradientBrush, rect);
                        myLinearGradientBrush.Dispose();
                        break;
                }                    
            }
            private bool TestPoint(Point p1, Point p2)
            {
                //a*a = b*b + c*c -- Dist Between 2 points
                int xval = p2.X - p1.X;
                int yval = p2.Y - p1.Y;
                return (!(SnapSetting < ((xval * xval) + (yval * yval))));    
                //return false;
            }
            public bool SnapToPoints(Point testPoint, out Point sp)
            {
                bool ValidSnap = false;
                sp = new Point();
                switch (dot)
                {
                    case drawinObjectType.line:
                        if (TestPoint(start, testPoint))
                        {
                            sp = start;
                            //MessageBox.Show("Line Start Snap");
                            ValidSnap = true;
                        }
                        else if (TestPoint(end, testPoint))
                        {
                            sp = end;
                            //MessageBox.Show("Line End Snap");
                            ValidSnap = true;
                        }
                        break;
                    case drawinObjectType.ellipse:
                        //float radius = (float)Math.Sqrt(end.X * end.X + end.Y * end.Y);
                        //g.DrawEllipse(p, start.X - radius, start.Y - radius, 2 * radius, 2 * radius);
                        
                        if (TestPoint(start, testPoint))
                        {
                            sp = start;
                            //MessageBox.Show("Line Start Snap");
                            ValidSnap = true;

                            //float rchk = (float)Math.Sqrt(testPoint.X * testPoint.X + testPoint.Y * testPoint.Y);
                            //float radius = (float)Math.Sqrt(end.X * end.X + end.Y * end.Y);
                            //float snp = (float)Math.Sqrt(SnapSetting);

                            //if ((rchk - radius) < snp)
                            //ValidSnap = true;
                            //else if ((radius - rchk) > snp)
                            //    ValidSnap = true;

                            ////g.DrawEllipse(p, start.X - radius, start.Y - radius, 2 * radius, 2 * radius);//new Rectangle(start, (Size)end));                        
                            //sp = start;
                            ////MessageBox.Show("Circle Start Snap");
                            ////ValidSnap = true;
                        }
                        break;
                }
                return ValidSnap;      
                       
            }
            public bool HitTest(Point testPoint)
            {
                switch (dot)
                {
                    case drawinObjectType.line:
                        //get slope 
                        float slope;
                        bool vertical = false;
                        if ((end.X - start.X) == 0)
                        {
                            slope = 0;
                            vertical = true;
                        }
                        else
                            slope = (end.Y - start.Y) / (end.X - start.X);
                        //Organize box
                        int left = start.X;
                        int right = end.X;
                        if (end.X < left)
                        {
                            left = end.X;
                            right = start.X;
                        }
                        int top = end.Y;
                        int bottom = start.Y;
                        if (top > bottom)
                        {
                            bottom = end.Y;
                            top = start.Y;
                        }
                        if (vertical)
                        {
                            if (testPoint.X == start.X)
                            {
                                highlight = true;
                                return true;
                            }                            
                        }
                        else if (slope == 0)
                        {
                            if (testPoint.Y == start.Y)
                            {
                                highlight = true;
                                return true;
                            } 
                        }

                        if (end.X == start.X)
                        {
                            //Vertical Line test
                            if (testPoint.Y < top)
                                if (testPoint.Y > bottom)
                                    if (testPoint.X > (end.X - SnapSetting))
                                        if (testPoint.X < (end.X + SnapSetting))
                                        {
                                            highlight = true;
                                            return true;
                                        }
                        }
                        else if (testPoint.X > left)
                        {
                            if (testPoint.X < right)
                                if (testPoint.Y < bottom)
                                    if (testPoint.Y > top)
                                    {
                                        highlight = true;
                                        return true;
                                    }
                        }

                        
                        


                        //Rectangle r = new Rectangle(start.X, start.Y, (end.X - start.X), (end.Y - start.Y));
                        //if (r.Contains(testPoint))
                        //{
                        //    highlight = true;
                        //    return true;
                        //}
                        break;
                    case drawinObjectType.ellipse:
                       
                        break;
                    case drawinObjectType.text:
                        
                        break;
                    case drawinObjectType.gradentBox:
                        
                        break;
                }
                highlight = false;
                return false;
            }
            public void genLineData() 
            {
                mTop = end.Y - start.Y;
                mBottom = end.X - end.Y;
                if (mBottom != 0f)
                {
                    //y = mx + b
                    //y - mx = b
                    b = (float)start.Y - (mTop / mBottom);
                }
            }
        }

        private bool CheckSnaps(out Point ep, Point ck)
        {
            Point newSnap = new Point();
            bool snaped = false;
            ep = new Point();

            foreach (drawingObject od in drawing)
            {
                if (od.SnapToPoints(ck, out newSnap))
                {
                    //MessageBox.Show("Snap Detected");
                    ep = newSnap;
                    snaped = true;
                    if (newSnap.X == 0)
                        MessageBox.Show("Zero Snap Detected");
                    break;
                }
            }
            
            return snaped;
        }

        private void HitTest(Point ck)
        {
            foreach (drawingObject od in drawing)
            {
                od.HitTest(ck);
            }            
        }

        public Form1()
        {            
            InitializeComponent();
            currentContext = BufferedGraphicsManager.Current;
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(), this.panel1.DisplayRectangle);

            //backbuffer = new Bitmap(this.Width, this.Height);
            //g = Graphics.FromImage(backbuffer);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Make a Line Button clicked
            this.lineStarted = true;
            this.panel1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.toolStripStatusLabel1.Text = "Drawing Line Started";
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                //Do Cancel
                this.lineStarted = false;
                this.ellipseSarted = false;
                this.firstPoint = true;
                this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                this.toolStripStatusLabel1.Text = "Canceled";
            }
            if (lineStarted)
            {
                if (this.firstPoint)
                {
                    Point checkSnap = e.Location;
                    Point checkOut;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        start = checkOut;
                    else
                        start = e.Location;
                    this.firstPoint = false;
                }
                else
                {
                    Point checkSnap = e.Location;
                    Point checkOut;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        end = checkOut;//new Point(e.X, e.Y);
                    else
                        end = e.Location;
                    drawingObject od = new drawingObject(this.toolStripButton3.BackColor, drawinObjectType.line, start, end, penSize);
                    this.drawing.Add(od);                    
                    this.firstPoint = true;
                    //this.lineStarted = false;
                    this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                    this.toolStripStatusLabel1.Text = "Line Completed";
                    this.panel1.Invalidate();

                    //start next line
                    checkSnap = e.Location;                    
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        start = checkOut;
                    else
                        start = e.Location;
                    this.firstPoint = false;
                }
            }
            if (gradBoxStarted)
            {
                if (this.firstPoint)
                {
                    Point checkSnap = e.Location;
                    Point checkOut;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        start = checkOut;
                    else
                        start = e.Location;
                    this.firstPoint = false;
                }
                else
                {
                    Point checkSnap = e.Location;
                    Point checkOut;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        end = checkOut;//new Point(e.X, e.Y);
                    else
                        end = e.Location;
                    drawingObject od = new drawingObject(this.toolStripButton3.BackColor, this.lastColor, drawinObjectType.gradentBox, start, end, penSize);
                    this.drawing.Add(od);
                    this.firstPoint = true;
                    this.gradBoxStarted = false;
                    this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                    this.toolStripStatusLabel1.Text = "Gradent Box Completed";
                    this.panel1.Invalidate();
                }
                 
            }
            if (ellipseSarted)
            {
                if (this.firstPoint)
                {
                    Point checkSnap = e.Location;
                    Point checkOut;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        start = checkOut;
                    else
                        start = e.Location;
                    this.firstPoint = false;
                }
                else
                {
                    Point checkSnap = new Point(e.X - start.X, e.Y - start.Y);
                    //Point checkSnap = e.Location;
                    Point checkOut;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                        end = checkOut;//new Point(e.X, e.Y);
                    else
                        end = new Point(e.X - start.X, e.Y - start.Y); 
                    //this.CheckSnaps(checkSnap);                    
                    end = checkSnap;// new Point(e.X - start.X, e.Y - start.Y);

                    drawingObject od = new drawingObject(this.toolStripButton3.BackColor, drawinObjectType.ellipse, start, end, penSize);
                    this.drawing.Add(od);
                    this.firstPoint = true;
                    this.ellipseSarted = false;
                    this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                    this.toolStripStatusLabel1.Text = "Ellipse Completed";
                    this.panel1.Invalidate();
                }
            }
            if (textStarted)
            {
                start = new Point(e.X, e.Y);
                if (gFont == null)
                {
                    gFont = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Regular);
                }
                drawingObject od = new drawingObject(this.toolStripButton3.BackColor, drawinObjectType.text, start, this.textBox1.Text, penSize, gFont);
                this.drawing.Add(od);
                this.firstPoint = true;
                this.textStarted = false;
                this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                this.toolStripStatusLabel1.Text = "Text Completed";
                this.panel1.Invalidate();
            }
            

                 
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            doDrawing(myBuffer.Graphics);

            Point checkSnap = e.Location;
            Point checkOut;
            if (this.CheckSnaps(out checkOut, checkSnap))
            {
                //If snap Identify and display interactivity for user
                g.DrawRectangle(new Pen(Color.Orange,2), checkOut.X - 5, checkOut.Y - 5, 11, 11);
                //MessageBox.Show("Snap Detected");
            }
            HitTest(e.Location);           

            if (lineStarted)
                if (!firstPoint)
                { 
                    doDrawing(myBuffer.Graphics);
                    Pen p = new Pen(Color.DarkGreen, penSize);
                    checkSnap = e.Location;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                    {
                        //If snap Identify and display interactivity for user
                        g.DrawRectangle(new Pen(Color.Orange), checkOut.X - 5, checkOut.Y - 5, 11, 11);
                    }
                    else
                        checkOut = e.Location;
                    g.DrawLine(p, start, checkOut);
                    //g.Dispose();
                }
            if (ellipseSarted)
                if (!firstPoint)
                {
                    //Graphics g = panel1.CreateGraphics();
                    doDrawing(myBuffer.Graphics);
                    Pen p = new Pen(Color.DarkGreen, penSize);
                    checkSnap = new Point(e.X - start.X, e.Y - start.Y);
                    if (this.CheckSnaps(out checkOut, checkSnap))
                    {
                        //If snap Identify and display interactivity for user
                        g.DrawRectangle(new Pen(Color.Orange), checkOut.X - 5, checkOut.Y - 5, 11, 11);
                    }
                    else
                        checkOut = new Point(e.X - start.X, e.Y - start.Y); ;
                    //PointF end = new PointF(e.X - start.X, e.Y - start.Y);
                    float radius = (float)Math.Sqrt(checkOut.X * checkOut.X + checkOut.Y * checkOut.Y);
                    g.DrawEllipse(p, start.X - radius, start.Y - radius, 2*radius, 2*radius);
                    //g.DrawEllipse(p, start.X, start.Y, e.X - start.X, e.Y - start.Y);
                    //g.Dispose();
                    
                }
            if (gradBoxStarted)
                if (!firstPoint)
                {
                    doDrawing(myBuffer.Graphics);
                    Pen p = new Pen(Color.DarkGreen, penSize);
                    checkSnap = e.Location;
                    if (this.CheckSnaps(out checkOut, checkSnap))
                    {
                        //If snap Identify and display interactivity for user
                        g.DrawRectangle(new Pen(Color.Orange), checkOut.X - 5, checkOut.Y - 5, 11, 11);
                    }
                    else
                        checkOut = e.Location;
                    Rectangle rect = new Rectangle(start, new Size(checkOut.X - start.X, checkOut.Y - start.Y));
                    if (rect.Width > 9)
                        if (rect.Height > 9)
                        {
                            LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(
        rect,
        Color.DarkGray,
        Color.LightGray,
        LinearGradientMode.Horizontal);
                            g.FillRectangle(myLinearGradientBrush, rect);
                            myLinearGradientBrush.Dispose();
                        }
                    //g.DrawLine(p, start, checkOut);

                }
            g.Dispose();
        }

        private void doDrawing(Graphics g)
        {
            g.Clear(Color.White);
            //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //g.CompositingQuality = CompositingQuality.HighQuality;

            Pen p = new Pen(Color.Blue, 2.0f);
            p.DashStyle = DashStyle.Dash;
            //Point[] pt = {new Point(100,100), new Point(200,100), new Point(300,300)};
            //g.DrawClosedCurve(p, pt);
            g.DrawEllipse(p, new Rectangle(100, 100, 200, 200));
            
            if (this.drawing.Count > 0)
            {
                foreach (drawingObject od in drawing)
                {
                    od.Draw(g);
                }
            }
            myBuffer.Render();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            doDrawing(myBuffer.Graphics);            
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //Menu Start Drawing Ellipse
            this.ellipseSarted = true;
            this.panel1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.toolStripStatusLabel1.Text = "Drawing Ellipse Started";

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.drawing.Clear();
            this.panel1.Invalidate();
            this.lineStarted = false;
            this.ellipseSarted = false;
            this.firstPoint = true;
            this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
            this.toolStripStatusLabel1.Text = "New Drawing";
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Graphics g = this.panel1.CreateGraphics();
            Bitmap saveMe = new Bitmap(this.panel1.Width, this.panel1.Height);
            this.panel1.DrawToBitmap(saveMe, new Rectangle(0, 0, this.panel1.Width, this.panel1.Height));
            
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveMe.Save(this.saveFileDialog1.FileName,System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                this.lastColor = this.toolStripButton3.BackColor;
                this.toolStripButton3.BackColor = colorDialog1.Color;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {            
            //Menu Start Text      
            this.textStarted = true;
            this.panel1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.toolStripStatusLabel1.Text = "Drawing Text Started";
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //Menu Start Grad Box
            gradBoxStarted = true;
            this.panel1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.toolStripStatusLabel1.Text = "Drawing Gradent Box Started";
        }

        private void penSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            penSize++;
            this.toolStripStatusLabel1.Text = "Pen Size ++Now: " + penSize.ToString();
        }

        private void penSizeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            penSize--;
            this.toolStripStatusLabel1.Text = "Pen Size --Now: " + penSize.ToString();
        }

        private void panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                //Do Cancel
                this.lineStarted = false;
                this.ellipseSarted = false;
                this.firstPoint = true;
                this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                this.toolStripStatusLabel1.Text = "Canceled";
            }

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("Found a Key");
            if (e.KeyCode == Keys.Escape)
            {
                //Do Cancel
                this.lineStarted = false;
                this.ellipseSarted = false;
                this.firstPoint = true;
                this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                this.toolStripStatusLabel1.Text = "Canceled";
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                //Do Cancel
                this.lineStarted = false;
                this.ellipseSarted = false;
                this.firstPoint = true;
                this.panel1.Cursor = System.Windows.Forms.Cursors.Default;
                this.toolStripStatusLabel1.Text = "Canceled";
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                gFont = fontDialog1.Font;
            }
        }

        private void antiAlisingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            if (myBuffer != null)
                myBuffer.Dispose();
            if (currentContext != null)
                myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(), this.panel1.DisplayRectangle);
        }        
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;


namespace BlobSurvival
{
    public partial class Form1 : Form
    {
        private Timer t;
        private readonly Camera _camera;
        private double xr = 0, yr = 0, zr = 0;
        private double xrp = 0, yrp = 0, zrp =0;
        private bool move = false;
        private readonly GameState _gameState;

        private Blob _playerBlob;

        //start timer and initalize component 
        public Form1()
        {

            t = new Timer
            {
                Enabled = true,
                Interval = 20
            };
            t.Tick += t_Tick;

            _gameState = new GameState(5000);

            InitializeComponent();

            _camera = new Camera(0, 0, 0);
            
        }

        //periodicaly called to calculate motion given keystrokes and redraw.
        void t_Tick(object sender, EventArgs e)
        {
            xrp += xr;
            yrp += yr;
            zrp += zr;
            xr = yr = zr = 0;
            double xm, ym, zm;
            Move(move, xrp, yrp, zrp, out xm, out ym, out zm);
            _gameState.MovePlayer(xm,ym,zm);
            move = false;
            MoveCamera(_camera, _gameState.GetPlayer());
            Invalidate();
        }

        //uses the camera to draw blobs to th screen
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                _camera.RenderObjects(_gameState.GetBlobs(), e);
            _camera.SetCenter(Width / 2, Height / 2);
        }

        //positions the camera so that it is behind the players blob
        private void MoveCamera(Camera c, Blob b)
        {
            if (b == null) return;
            c.UpdateAngel(xrp, yrp, zrp);
            Blob relativeCamLocation = TransformVertex(new Blob(0, 0, 20, 0, Color.Black), new Blob(0, 0, 0, 0, Color.Black), -xrp, 0, 0);
            relativeCamLocation = TransformVertex(relativeCamLocation, new Blob(0, 0, 0, 0, Color.Black), 0, -yrp, 0);
            relativeCamLocation = TransformVertex(relativeCamLocation, new Blob(0, 0, 0, 0, Color.Black), 0, 0, -zrp);
            c.UpdateLocation(b.X + relativeCamLocation.X, b.Y + relativeCamLocation.Y, relativeCamLocation.Z + b.Z);
        }

        //calculates the motion vector of the players blob based on rotation
        public void Move(bool b, double xr, double yr, double zr, out double xMove, out double yMove, out double zMove)
        {
            xMove = 0;
            yMove = 0;
            zMove = 0;
            if (b) {
                Blob relativeCamLocation = TransformVertex(new Blob(0, 0, -1, 0, Color.Black), new Blob(0, 0, 0, 0, Color.Black), -xr, 0, 0);
                relativeCamLocation = TransformVertex(relativeCamLocation, new Blob(0, 0, 0, 0, Color.Black), 0, -yr, 0);
                relativeCamLocation = TransformVertex(relativeCamLocation, new Blob(0, 0, 0, 0, Color.Black), 0, 0, -zr);
                xMove = relativeCamLocation.X;
                yMove = relativeCamLocation.Y;
                zMove = relativeCamLocation.Z;
            }

        }

        //rotate a blob around another bolb goven x,y,z rotation
        private static Blob TransformVertex(Blob v, Blob center, double xr2, double yr2, double zr2)
        {
            Blob v2 = new Blob(v.X, v.Y, v.Z, v.Radius, Color.AliceBlue);
            double vx = v.X;
            double vy = v.Y;
            double vz = v.Z;
            v2.X = Math.Cos(yr2) * (Math.Sin(zr2) * (vy - center.Y) + Math.Cos(zr2) * (vx - center.X)) - Math.Sin(yr2) * (vz - center.Z);
            v2.Y = Math.Sin(xr2) * (Math.Cos(yr2) * (vz - center.Z) + Math.Sin(yr2) * (Math.Sin(zr2) * (vy - center.Y) + Math.Cos(zr2) * (vx - center.X))) + Math.Cos(xr2) * (Math.Cos(zr2) * (vy - center.Y) - Math.Sin(zr2) * (vx - center.X));
            v2.Z = Math.Cos(xr2) * (Math.Cos(yr2) * (vz - center.Z) + Math.Sin(yr2) * (Math.Sin(zr2) * (vy - center.Y) + Math.Cos(zr2) * (vx - center.X))) - Math.Sin(xr2) * (Math.Cos(zr2) * (vy - center.Y) - Math.Sin(zr2) * (vx - center.X));
            return v2;
        }

        //listen for keystrokes and record them
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {     
            if (e.KeyCode.Equals(Keys.Space))
            {
                move = true;
            }
            else if (e.KeyCode.Equals(Keys.Up))
            {
                xr = 0.1;
            }
            else if (e.KeyCode.Equals(Keys.Down))
            {
                xr = -0.1;
            }
            else if (e.KeyCode.Equals(Keys.Left))
            {
                yr = 0.1;
            }
            else if (e.KeyCode.Equals(Keys.Right))
            {
                yr = -0.1;
            }
        }

    }
}

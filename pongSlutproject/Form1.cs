using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Reflection;

/*
 * Credits
 * Johannes
 * TEINF 13A
 * Programering Slutproject
 */

namespace pongSlutproject
{
    public partial class gameArea : Form
    {
        #region pre Initializing
        //adds pictureboxes and a timer
        PictureBox picBoxPlayer, picBoxAI, picBoxBall;
        Timer gameTime;
        Label playerPoints, aiPoints;

        //set the varibels
        const int SCREEN_WIDTH = 640;
        const int SCREEN_HEIGHT = 480;

        const float AI_MOVE_SPEED = 2f;

        Size sizePlayer = new Size(15, 100);
        Size sizeAI = new Size(15, 100);
        Size sizeBall = new Size(20, 20);

        int init_ballspeed = 3;

        int ballSpeedX = 3;
        int ballSpeedY = 3;
        int gameTimeInterval = 1;

        float aiVelocityY = 0;

        int aiScore = 0;
        int plrScore = 0;

        //adds pong sound
        SoundPlayer pongPadelSound = new SoundPlayer(Util.GetStream("pong.wav"));
        SoundPlayer winSound = new SoundPlayer(Util.GetStream("win.wav"));
        SoundPlayer wallHitSound = new SoundPlayer(Util.GetStream("wall.wav"));
        #endregion

        #region Fullscreen
        struct FullscreenRestoreState
        {
            public Point location;
            public int width;
            public int height;
            public FormWindowState state;
            public FormBorderStyle style;
        };
        FullscreenRestoreState restoreState;

        bool isFullscreen = false;
        #endregion

        public gameArea()
        {

            #region Initializing
            InitializeComponent();

            ballSpeedX = init_ballspeed;
            ballSpeedY = init_ballspeed;

            //Initializes the PictureBoxes
            picBoxPlayer = new PictureBox();
            picBoxAI = new PictureBox();
            picBoxBall = new PictureBox();

            //Initializes the Timer
            gameTime = new Timer();

            //Initializes the labels
            playerPoints = new Label();
            aiPoints = new Label();

            //Enables the Timer and set the timer's interval
            gameTime.Enabled = true;
            gameTime.Interval = gameTimeInterval;

            //Creates the Timer's Tick event
            gameTime.Tick += new EventHandler(gameTime_Tick);

            //sets the forms size and opens it in the center of the screen
            this.Width = SCREEN_WIDTH;
            this.Height = SCREEN_HEIGHT;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            #endregion

            #region keyEventHandler
            KeyDown += new KeyEventHandler(keybinds);
            #endregion

            #region Seting up pictureboxes and scores
            /* sets the size of the picturebox
             * sets it's location (centered)
             * fills the picturebox with a color
             * adds the picture box to the form
             */

            //player picturebox
            picBoxPlayer.Size = sizePlayer;
            picBoxPlayer.Location = new Point(picBoxPlayer.Width / 2, ClientSize.Height / 2 - picBoxPlayer.Height / 2);
            picBoxPlayer.BackColor = Color.Blue;
            this.Controls.Add(picBoxPlayer);

            //AI picturebox
            picBoxAI.Size = sizeAI;
            picBoxAI.Location = new Point(ClientSize.Width - (picBoxAI.Width + picBoxAI.Width / 2), ClientSize.Height / 2 - picBoxPlayer.Height / 2);
            picBoxAI.BackColor = Color.Blue;
            this.Controls.Add(picBoxAI);

            //ball picturebox
            picBoxBall.Size = sizeBall;
            picBoxBall.Location = new Point(ClientSize.Width / 2 - picBoxBall.Width / 2, ClientSize.Height / 2 - picBoxBall.Height / 2);
            picBoxBall.BackColor = Color.Azure;
            this.Controls.Add(picBoxBall);

            /*
             *sets the starting text
             *sets the font and size
             *sets the color
             *sets the lacation
             *removes cutting of of text
             *shows / adds the label
             */
            //ponits & scores
            playerPoints.Text = plrScore.ToString("D2");
            playerPoints.Font = new Font("Arial", 20);
            playerPoints.ForeColor = Color.Cyan;
            playerPoints.Location = new Point((ClientSize.Width / 2 - 30), 20);
            playerPoints.AutoSize = true;
            this.Controls.Add(playerPoints);

            aiPoints.Text = aiScore.ToString("D2");
            aiPoints.Font = new Font("Arial", 20);
            aiPoints.ForeColor = Color.Cyan;
            aiPoints.Location = new Point((ClientSize.Width / 2 + 10), 20);
            aiPoints.AutoSize = true;
            this.Controls.Add(aiPoints);
            #endregion
        }
        #region keybinds
        private void keybinds(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.R)
            {
                aiScore = 0;
                plrScore = 0;
                playerPoints.Text = plrScore.ToString("D2");
                aiPoints.Text = aiScore.ToString("D2");
            }
            if (e.KeyCode == Keys.F1)
            {
                MessageBox.Show("Move the Mouse up en down to move the padel\n press R to reset the point(s)\n press F for fullscreen toggle\n press ESC to exit the program","help");
            }
            if (e.KeyCode == Keys.F)
            {
                ToggleFullscreen();
                //reset wear the ball is and wear the AI padel is
                resetBall();
                aiMovement();
            }
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
            if (e.KeyCode == Keys.Enter)
            {
                picBoxPlayer.Location = new Point(picBoxPlayer.Width / 2, ClientSize.Height / 2 - picBoxPlayer.Height / 2);
                resetBall();
                aiMovement();
            }
        }
        #endregion

        #region Fullscreen
        void ToggleFullscreen()
        {
            if (!isFullscreen)
            {
                // save state information to restoreState and go fullscreen
                restoreState.location = Location;
                restoreState.width = Width;
                restoreState.height = Height;
                restoreState.state = WindowState;
                restoreState.style = FormBorderStyle;

                TopMost = true;
                Location = new Point(0, 0);
                FormBorderStyle = FormBorderStyle.None;
                Width = Screen.PrimaryScreen.Bounds.Width;
                Height = Screen.PrimaryScreen.Bounds.Height;
            }
            else
            {
                // restore from restoreState
                TopMost = false;
                Location = restoreState.location;
                Width = restoreState.width;
                Height = restoreState.height;
                WindowState = restoreState.state;
                FormBorderStyle = restoreState.style;
            }

            // toggle our variable
            isFullscreen = !isFullscreen;
        }
        #endregion

        void gameTime_Tick(object sender, EventArgs e)
        {
            //Sets the new location
            picBoxBall.Location = new Point(picBoxBall.Location.X + ballSpeedX, picBoxBall.Location.Y + ballSpeedY);

            //Checks for collisions with the form's border
            gameAreaCollisions();

            //Checks for collisions with the padlles
            padlleCollision();

            //Updates the player's position
            playerMovement();

            //Updates the ai's position
            aiMovement();

            //draws the score in the center
            aiPoints.Location = new Point((ClientSize.Width / 2 + 10), 20);
            playerPoints.Location = new Point((ClientSize.Width / 2 - 30), 20);

        }

        #region Collisions
        private void gameAreaCollisions()
        {
            //if the ball has hit the client aka window border
            if (picBoxBall.Location.Y > ClientSize.Height - picBoxBall.Height || picBoxBall.Location.Y < 0)
            {
                wallHitSound.Play();
                //bounces
                ballSpeedY = -ballSpeedY;
            }

            //if ball is outside the gameArea aka goal
            else if (picBoxBall.Location.X > ClientSize.Width)
            {
                //AI lost
                resetBall();
                //add a point to plr
                addPonts(false);
            }
            else if (picBoxBall.Location.X < 0)
            {
                //plr lost
                resetBall();
                //add a point to AI
                addPonts(true);
            }
        }
        
        private void padlleCollision()
        {

            // Check player Y collision
            if (picBoxBall.Bottom > picBoxPlayer.Top && picBoxBall.Top < picBoxPlayer.Bottom)
            {
                // Check player X collision
                if (picBoxBall.Left < picBoxPlayer.Right)
                {
                    picBoxBall.Left = picBoxPlayer.Right;
                    ballSpeedX = -(ballSpeedX);
                    pongPadelSound.Play();
                    //incresses or decresses speed on x axes
                    if (ballSpeedX > 0)
                    {
                        ballSpeedX++;
                    }
                    else
                    {
                        ballSpeedX--;
                    }

                    //incresses speed on y axes
                    if (new Random().Next(0, 2) == 1)
                    {
                        if (ballSpeedY > 0)
                            ballSpeedY++;
                        else
                            ballSpeedY--;
                    }
                }
            }

            // Check AI Y collision
            if (picBoxBall.Bottom > picBoxAI.Top && picBoxBall.Top < picBoxAI.Bottom)
            {
                // Check AI X collision
                if (picBoxBall.Right > picBoxAI.Left)
                {
                    picBoxBall.Left = picBoxAI.Left - picBoxBall.Width;
                    ballSpeedX = -ballSpeedX;
                    pongPadelSound.Play();
                }
            }
        }
        #endregion
        
        #region Movment
        private void playerMovement()
        {
            //mouse movement
            if (this.PointToClient(MousePosition).Y >= picBoxPlayer.Height / 2 && this.PointToClient(MousePosition).Y <= ClientSize.Height - picBoxPlayer.Height / 2)
            {
                //ingores the X axes and sets the Y axes the the mouse possition
                int playerX = picBoxPlayer.Width / 2;
                int playerY = this.PointToClient(MousePosition).Y - picBoxPlayer.Height / 2;

                //sets the new location
                picBoxPlayer.Location = new Point(playerX, playerY);
            }
        }
        
        private void aiMovement()
        {
            //sets ball center on the Y axes
            int ballCenterY = picBoxBall.Bottom - picBoxAI.Bottom + picBoxAI.Height / 2 + picBoxBall.Height / 2;
            //ballCenterY += Math.Abs(picBoxBall.Left - picBoxAI.Left) * ballSpeedY / 5;

            //sets the Velocity of AI
            //if the AI padel is above the ball
            if (ballCenterY >= 20)
            {
                if (new Random().Next(0, 100) < 80)
                {
                    aiVelocityY += AI_MOVE_SPEED;
                }
            }
            //if the AI padel is below the ball
            else if (ballCenterY < -20)
            {
                if (new Random().Next(0, 100) < 80)
                {
                    aiVelocityY -= AI_MOVE_SPEED;
                }
            }
            //friction
            aiVelocityY *= .95f;

            //set a new place w/ the velocity and friction for the AI
            picBoxAI.Location = new Point(
                    ClientSize.Width - (picBoxAI.Width + picBoxAI.Width / 2),
                    picBoxAI.Location.Y + (int)Math.Round(aiVelocityY));
        }
        #endregion

        private void resetBall()
        {
            //sets the ball in the midel
            picBoxBall.Location = new Point(ClientSize.Width / 2 - picBoxBall.Width / 2, ClientSize.Height / 2 - picBoxBall.Height / 2);
            ballSpeedX = init_ballspeed;
            ballSpeedY = init_ballspeed;
        }
       
        private void addPonts(bool isAI)
        {
            winSound.Play();
            //adds one point to the score
            //sees if it is AI or plr the have score the point
            if (isAI == true)
            {
                //ads one pont
                aiScore += 1;
                //prints the score on the labe w/ two numbers w/ the D2 flag
                aiPoints.Text = aiScore.ToString("D2");
            }
            else
            {
                //ads one pont
                plrScore += 1;
                //prints the score on the labe w/ two numbers w/ the D2 flag
                playerPoints.Text = plrScore.ToString("D2");
            }
        }
    }
}

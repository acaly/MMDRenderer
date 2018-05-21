using LightDx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMDRenderer
{
    class Camera
    {
        public Camera(Control ctrl)
        {
            ctrl.KeyDown += Ctrl_KeyDown;
            ctrl.KeyUp += Ctrl_KeyUp;

            var form = ctrl.FindForm();
            form.Activated += Form_Activated;
            form.LostFocus += Form_LostFocus;
            form.LocationChanged += Form_PositionChanged;
            form.SizeChanged += Form_PositionChanged;

            _formActivated = form.Focused;
            Form_PositionChanged(form, EventArgs.Empty);
        }

        private volatile bool _formActivated;
        private volatile int _formX, _formY, _formW, _formH;
        private volatile bool _down, _up, _forward, _backward, _left, _right;
        private bool _buttonState = false;
        private float _rX, _rY;
        private int _mx, _my;

        private void Ctrl_MouseDown(object sender, MouseEventArgs e)
        {
            _mx = e.X;
            _my = e.Y;
        }

        private void Ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _rX = e.X - _mx;
                _rY = e.Y - _my;
                _mx = e.X;
                _my = e.Y;
            }
        }

        private void Ctrl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S: _forward = true; break;
                case Keys.W: _backward = true; break;
                case Keys.A: _left = true; break;
                case Keys.D: _right = true; break;
                case Keys.Q: _up = true; break;
                case Keys.E: _down = true; break;
            }
        }

        private void Ctrl_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S: _forward = false; break;
                case Keys.W: _backward = false; break;
                case Keys.A: _left = false; break;
                case Keys.D: _right = false; break;
                case Keys.Q: _up = false; break;
                case Keys.E: _down = false; break;
            }
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            _formActivated = true;
        }

        private void Form_LostFocus(object sender, EventArgs e)
        {
            _formActivated = false;
        }

        private void Form_PositionChanged(object sender, EventArgs e)
        {
            var form = (Form)sender;
            var pos = form.Location;
            var size = form.Size;
            var client = form.ClientRectangle;
            var clientPos = form.PointToScreen(new System.Drawing.Point(0, 0));
            _formX = clientPos.X;
            _formY = clientPos.Y;
            _formW = client.Width;
            _formH = client.Height;
        }

        public Vector3 Position { get; set; } = new Vector3(0, 10, -30);
        public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0);

        public void Update()
        {
            var speed = 0.1f;
            var move = new Vector3();
            if (_forward) move += new Vector3(0, 0, -speed);
            if (_backward) move += new Vector3(0, 0, speed);
            if (_left) move += new Vector3(-speed, 0, 0);
            if (_right) move += new Vector3(speed, 0, 0);
            if (_up) move += new Vector3(0, speed, 0);
            if (_down) move += new Vector3(0, -speed, 0);
            {
                var rot = Matrix4x4.CreateFromYawPitchRoll(Rotation.Y * 0.015f, Rotation.X * 0.015f, Rotation.Z * 0.015f);
                move = Vector3.TransformNormal(move, rot);
            }
            Position += move;
            if (_formActivated)
            {
                var button = Control.MouseButtons;
                var pos = Control.MousePosition;
                if (button == MouseButtons.Left && !_buttonState)
                {
                    if (pos.X >= _formX && pos.X < _formX + _formW &&
                        pos.Y >= _formY && pos.Y < _formY + _formH)
                    {
                        _buttonState = true;
                        _mx = pos.X;
                        _my = pos.Y;
                    }
                }
                else if (button != MouseButtons.Left && _buttonState)
                {
                    _buttonState = false;
                }
                if (_buttonState)
                {
                    _rX = pos.X - _mx;
                    _rY = pos.Y - _my;
                    _mx = pos.X;
                    _my = pos.Y;
                }
            }
            else
            {
                _buttonState = false;
            }
            var rSpeed = 0.1f;
            Rotation += new Vector3(-_rY * rSpeed, -_rX * rSpeed, 0);
            _rX = _rY = 0;
        }

        public Matrix4x4 GetViewMatrix()
        {
            var up = new Vector3(0, 1, 0);
            var lookAt = new Vector3(0, 0, 1);

            var rot = Matrix4x4.CreateFromYawPitchRoll(Rotation.Y * 0.015f, Rotation.X * 0.015f, Rotation.Z * 0.015f);
            up = Vector3.TransformNormal(up, rot);
            lookAt = Vector3.TransformNormal(lookAt, rot);

            lookAt += Position;

            return MatrixHelper.CreateLookAt(Position, lookAt, up);
        }
    }
}

﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Extract
{
    public partial class TransparentPanel : Panel
    {
        private const int WS_EX_TRANSPARENT = 0x20;

        public TransparentPanel()
        {

            InitializeComponent();

            SetStyle(ControlStyles.Opaque, true);

        }

        public TransparentPanel(IContainer con)
        {

            con.Add(this);

            InitializeComponent();

        }

        private int opacity = 50;

        [DefaultValue(50)]
        public int Opacity
        {
            get
            {

                return this.opacity;

            }

            set
            {

                if (value < 0 || value > 100)


                    throw new ArgumentException("value must be between 0 and 100");


                this.opacity = value;
            }

        }

        protected override CreateParams CreateParams
        {

            get
            {

                CreateParams cpar = base.CreateParams;

                cpar.ExStyle = cpar.ExStyle | WS_EX_TRANSPARENT;

                return cpar;

            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {

            using (var brush = new SolidBrush(Color.FromArgb
               (this.opacity * 255 / 100, this.BackColor)))
            {

                e.Graphics.FillRectangle(brush, this.ClientRectangle);

            }

            base.OnPaint(e);
        }


    }
}




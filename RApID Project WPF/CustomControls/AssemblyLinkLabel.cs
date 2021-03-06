﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RApID_Project_WPF.CustomControls
{
    /// <summary>
    /// Custom <see cref="ListViewItem"/> implementation for ease of use.
    /// </summary>
    public partial class AssemblyLinkLabel : LinkLabel
    {
        private MouseEventHandler _changeLink;
        /// <summary>
        /// Exposes the change link event to other classes
        /// </summary>
        public event MouseEventHandler ChangeLink {
            add {
                if (_changeLink == null || !_changeLink.GetInvocationList().Contains(value))
                    _changeLink += value;
            }
            remove {
                _changeLink -= value;
            }
        }

        /// <summary> URL or File Path </summary>
        [Description("URL or File Path")]
        public new string Link { get; set; }

        /// <summary> Tracks the board to be returned after adding a new board number. </summary>
        [Description("Tracks the board to be returned after adding a new board number. Also used to add boards to the Interaction tab's board list.")]
        public bool IsMarkedActive { get; set; }

        public string ECO { get => lblECO.Text; set => lblECO.Text = value; }
        public string REV { get => lblREV.Text; set => lblREV.Text = value; }
        
        #region Constructors
        /// <summary>
        /// Designer ctor
        /// </summary>
        public AssemblyLinkLabel() : this(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Desktop") {}

        /// <summary>
        /// Default constructor - many optional args for flexibility.
        /// </summary>
        /// <param name="link">[Optional] Sets this <see cref="Link"/></param>
        /// <param name="displayText">[Optional] Sets this <see cref="LinkLabel.Text"/></param>
        /// <param name="img">[Optional] The image icon for this file extension.</param>
        /// <param name="name">[Optional] The name of this control. </param>
        /// <param name="handler">[Optional] Callback handler for whenever a mouse click occurs without Left Mouse Button</param>
        /// <param name="rev">[Optional] Relates this link to a speicified BoM revision.</param>
        /// <param name="eco">[Optional] Describes a link to a file that's based on an outstanding Engineering Change Order.</param>
        public AssemblyLinkLabel(string link = "", string displayText = "", Image img = null,
            string name = "", MouseEventHandler handler = null,
            string rev = "", string eco = "")
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(link))
            {
                Link = link;
            }

            if (!string.IsNullOrWhiteSpace(displayText))
            {
                Text = displayText;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }

            if (!string.IsNullOrWhiteSpace(rev)) {
                REV = rev;
            }

            if (!string.IsNullOrWhiteSpace(eco)) {
                ECO = eco;
            }

            Image = img ?? null;
            
            Enabled = true;
            ForeColor = Color.Cyan;
            LinkColor = Color.Cyan;
            ActiveLinkColor = Color.MediumSpringGreen;
            VisitedLinkColor = Color.Magenta;
            DisabledLinkColor = Color.DarkCyan;
            ImageAlign = ContentAlignment.MiddleLeft;
            Font = new Font("Microsoft Sans Serif", 10);
            Width = 250;
            Height = 60;
            Margin = new Padding(5);
            Padding = new Padding(32,4,4,4);
            BorderStyle = BorderStyle.Fixed3D;           
            Cursor = Cursors.Hand;
            MouseDown += AssemblyLinkLabel_MouseDown;
            if (handler != null) ChangeLink += handler;
        }
        #endregion

        private void AssemblyLinkLabel_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
            {
                Activate();
            } else if(_changeLink != null)
            {
                _changeLink?.Invoke(sender, e);
            }
        }

        /// <summary> Will start the default associated process on the linked item. </summary>
        public void Activate()
        {
            if (Link == null || Link.Equals("BOMFile") || Link.Equals("ASSYLink")) SystemSounds.Beep.Play();

            try
            {
                Console.WriteLine(LogActivation() + "\n\t|--> Link Activated!\n");
                Process.Start(frmBoardFileManager.ELECROOTDIR + Link);
            }
            catch (ArgumentException ae) {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate", ae);
                MessageBox.Show("The path used bad characters!",
                    "Link::Activate() - ArgumentException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Win32Exception wex)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate)", wex);
                MessageBox.Show("Couldn't start the process meant for this link.\nPlease ensure you have permission to access the path given.",
                    "Link::Activate() - Win32Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ObjectDisposedException ode)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate", ode);
                MessageBox.Show("The process object was disposed before the process stopped!",
                    "Link::Activate() - ObjectDisposedException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException fnfe)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate", fnfe);
                MessageBox.Show("Couldn't find the file specificed in the link!",
                    "Link::Activate() - FileNotFoundException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Logging formatted string
        /// </summary>
        /// <returns></returns>
        public string LogActivation() => $"[AssemblyLinkItem({Name})]: {Text} => {Link}";

        /// <summary>
        /// Custom ToString format for custom <see cref="ListViewItem"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Text}|{Tag}";

        /// <summary>
        /// Custom paint method to allow for unique visual variations.
        /// </summary>
        /// <param name="e"><see cref="PaintEventArgs"/> instance</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Point upperLeftCorner = new Point(ClientRectangle.Location.X + 100, ClientRectangle.Location.Y + 25);
            Rectangle targetRect = new Rectangle(upperLeftCorner,new Size(40,40));
            if (IsMarkedActive) e.Graphics.DrawImage(Properties.Resources.Board, targetRect);
        }
    }
}

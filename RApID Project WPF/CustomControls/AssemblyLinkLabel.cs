using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// Exposes the change link event to other classes
        /// </summary>
        public static event MouseEventHandler ChangeLink;

        /// <summary> URL or File Path </summary>
        [Description("URL or File Path")]
        public new string Link { get; set; }

        #region Constructors
        /// <summary>
        /// Designer ctor
        /// </summary>
        public AssemblyLinkLabel() : this(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Desktop") {}

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="link">[Optional] Sets this <see cref="Link"/></param>
        /// <param name="displayText">[Optional] Sets this <see cref="LinkLabel.Text"/></param>
        /// <param name="img">[Optional] The image icon for this file extension.</param>
        public AssemblyLinkLabel(string link = "", string displayText = "", Image img = null)
        {
            if (!string.IsNullOrWhiteSpace(link))
            {
                Link = link;
            }

            if (!string.IsNullOrWhiteSpace(displayText))
            {
                Text = displayText;
            }

            if(img != null) Image = img;

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
            MouseDown += AssemblyLinkLabel_MouseDown;
        }
        #endregion

        private void AssemblyLinkLabel_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
            {
                Activate();
            } else
            {
                ChangeLink?.Invoke(sender, e);
            }
        }

        /// <summary> Will start the default associated process on the linked item. </summary>
        public void Activate()
        {
            try
            {
                Console.WriteLine(ToString() + "\n\t|--> Link Activated!\n");
                Process.Start(Link);
            }
            catch (Win32Exception wex)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate->({Link})", wex);
                MessageBox.Show("Couldn't start the process meant for this link.\nPlease ensure you have permission to access the path given.",
                    "Link::Activate() - Win32Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ObjectDisposedException ode)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate->({Link})", ode);
                MessageBox.Show("The process object was disposed before the process stopped!",
                    "Link::Activate() - ObjectDisposedException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException fnfe)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate->({Link})", fnfe);
                MessageBox.Show("Couldn't find the file specificed in the link!",
                    "Link::Activate() - FileNotFoundException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Custom ToString format for custom <see cref="ListViewItem"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[AssemblyLinkItem({Name})]: {Text} => {Link}";
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialTestApp.Controls
{
    public partial class LedControl : Krypton.Toolkit.KryptonCheckBox
    {
        public LedControl() : base()
        {
        }

        public Color OnColour { get; set; }

        public Color OffColour { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testing
{
    public partial class UniverseSizeWindow : Form
    {
        public UniverseSizeWindow()
        {
            InitializeComponent();

        }

        public int getNum1()
        {
            return (int)numericUpDown1.Value;

        }

        public int getNum2()
        {
            return (int)numericUpDown2.Value;

        }

        public void setNum1(int x)
        {
            numericUpDown1.Value = x;

        }

        public void setNum2(int y)
        {
            numericUpDown2.Value = y;

        }

    }


}

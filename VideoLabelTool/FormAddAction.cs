using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoLabelTool
{
    public partial class FormAddAction : Form
    {
        private FormFrameCapture formFrameCapture;        
        public FormAddAction(FormFrameCapture formFrameCapture)
        {
            InitializeComponent();
            this.formFrameCapture = formFrameCapture;
        }

        private void bntSelConfirm_Click(object sender, EventArgs e)
        {
            this.formFrameCapture.newActionName = textBoxAddAction.Text;            
            this.Close();
        }

        private void bntSelClose_Click(object sender, EventArgs e)
        {            
            this.Close();
        }
    }
}

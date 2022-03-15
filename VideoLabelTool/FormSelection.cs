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
    public partial class FormSelection : Form
    {
        private FormFrameCapture formFrameCapture;

        public FormSelection(FormFrameCapture formFrameCapture)
        {
            InitializeComponent();
            this.formFrameCapture = formFrameCapture;

            listBoxSelection.DataSource = formFrameCapture.selectedPersonID;
        }


        private void listBoxSelection_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bntSelConfirm_Click(object sender, EventArgs e)
        {
            formFrameCapture.selectedPersonIDUnique = (int) listBoxSelection.SelectedItem;            
            this.Close();
        }

        private void bntSelClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

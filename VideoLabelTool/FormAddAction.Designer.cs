
namespace VideoLabelTool
{
    partial class FormAddAction
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxAddAction = new System.Windows.Forms.TextBox();
            this.labelAddAction = new System.Windows.Forms.Label();
            this.bntSelConfirm = new System.Windows.Forms.Button();
            this.bntSelClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxAddAction
            // 
            this.textBoxAddAction.Location = new System.Drawing.Point(180, 67);
            this.textBoxAddAction.Name = "textBoxAddAction";
            this.textBoxAddAction.Size = new System.Drawing.Size(169, 20);
            this.textBoxAddAction.TabIndex = 0;
            // 
            // labelAddAction
            // 
            this.labelAddAction.AutoSize = true;
            this.labelAddAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelAddAction.Location = new System.Drawing.Point(12, 63);
            this.labelAddAction.Name = "labelAddAction";
            this.labelAddAction.Size = new System.Drawing.Size(162, 24);
            this.labelAddAction.TabIndex = 1;
            this.labelAddAction.Text = "New action name:";
            // 
            // bntSelConfirm
            // 
            this.bntSelConfirm.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntSelConfirm.Location = new System.Drawing.Point(196, 151);
            this.bntSelConfirm.Name = "bntSelConfirm";
            this.bntSelConfirm.Size = new System.Drawing.Size(75, 25);
            this.bntSelConfirm.TabIndex = 3;
            this.bntSelConfirm.Text = "Confirm";
            this.bntSelConfirm.UseVisualStyleBackColor = true;
            this.bntSelConfirm.Click += new System.EventHandler(this.bntSelConfirm_Click);
            // 
            // bntSelClose
            // 
            this.bntSelClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntSelClose.Location = new System.Drawing.Point(292, 151);
            this.bntSelClose.Name = "bntSelClose";
            this.bntSelClose.Size = new System.Drawing.Size(75, 25);
            this.bntSelClose.TabIndex = 4;
            this.bntSelClose.Text = "Close";
            this.bntSelClose.UseVisualStyleBackColor = true;
            this.bntSelClose.Click += new System.EventHandler(this.bntSelClose_Click);
            // 
            // FormAddAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 198);
            this.Controls.Add(this.bntSelClose);
            this.Controls.Add(this.bntSelConfirm);
            this.Controls.Add(this.labelAddAction);
            this.Controls.Add(this.textBoxAddAction);
            this.Name = "FormAddAction";
            this.Text = "FormAddAction";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxAddAction;
        private System.Windows.Forms.Label labelAddAction;
        private System.Windows.Forms.Button bntSelConfirm;
        private System.Windows.Forms.Button bntSelClose;
    }
}
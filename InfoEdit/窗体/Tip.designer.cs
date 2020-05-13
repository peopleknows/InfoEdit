namespace InfoEdit
{
    partial class Tip
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
            this.lbl_info = new DevExpress.XtraEditors.LabelControl();
            this.textEdit1 = new DevExpress.XtraEditors.MemoEdit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_info
            // 
            this.lbl_info.Location = new System.Drawing.Point(26, 12);
            this.lbl_info.Name = "lbl_info";
            this.lbl_info.Size = new System.Drawing.Size(72, 22);
            this.lbl_info.TabIndex = 2;
            this.lbl_info.Text = "使用说明";
            // 
            // textEdit1
            // 
            this.textEdit1.EditValue = "1.生成编辑后的xml文件需要点击根节点（即有子节点的节点），否则只能生成单个的节点信息";
            this.textEdit1.Location = new System.Drawing.Point(12, 40);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Properties.LinesCount = 1000;
            this.textEdit1.Properties.ReadOnly = true;
            this.textEdit1.Size = new System.Drawing.Size(423, 328);
            this.textEdit1.TabIndex = 3;
            // 
            // Tip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 380);
            this.Controls.Add(this.lbl_info);
            this.Controls.Add(this.textEdit1);
            this.Name = "Tip";
            this.Text = "使用说明";
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lbl_info;
        private DevExpress.XtraEditors.MemoEdit textEdit1;
    }
}
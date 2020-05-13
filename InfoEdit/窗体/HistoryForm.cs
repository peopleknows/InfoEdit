using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace InfoEdit
{
    public partial class HistoryForm : DevExpress.XtraEditors.XtraForm
    {
        public HistoryForm()
        {
            InitializeComponent();
            this.textEdit1.ReadOnly = true;
            this.textEdit1.Select(0, 0);
        }
    }
}
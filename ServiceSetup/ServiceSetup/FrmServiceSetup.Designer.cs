namespace ServiceSetup
{
    partial class FrmServiceSetup
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmServiceSetup));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打开主界面ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnInstallOrUninstall = new System.Windows.Forms.Button();
            this.btnStartOrEnd = new System.Windows.Forms.Button();
            this.btnGetStatus = new System.Windows.Forms.Button();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.lblServiceName = new System.Windows.Forms.Label();
            this.lblMsg = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.gbMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开主界面ToolStripMenuItem,
            this.退出ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 48);
            // 
            // 打开主界面ToolStripMenuItem
            // 
            this.打开主界面ToolStripMenuItem.Name = "打开主界面ToolStripMenuItem";
            this.打开主界面ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.打开主界面ToolStripMenuItem.Text = "打开主界面";
            this.打开主界面ToolStripMenuItem.Click += new System.EventHandler(this.打开主界面ToolStripMenuItem_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            // 
            // btnInstallOrUninstall
            // 
            this.btnInstallOrUninstall.Location = new System.Drawing.Point(11, 48);
            this.btnInstallOrUninstall.Name = "btnInstallOrUninstall";
            this.btnInstallOrUninstall.Size = new System.Drawing.Size(75, 23);
            this.btnInstallOrUninstall.TabIndex = 1;
            this.btnInstallOrUninstall.Text = "安装服务";
            this.btnInstallOrUninstall.UseVisualStyleBackColor = true;
            this.btnInstallOrUninstall.Click += new System.EventHandler(this.btnInstallOrUninstall_Click);
            // 
            // btnStartOrEnd
            // 
            this.btnStartOrEnd.Location = new System.Drawing.Point(103, 47);
            this.btnStartOrEnd.Name = "btnStartOrEnd";
            this.btnStartOrEnd.Size = new System.Drawing.Size(75, 23);
            this.btnStartOrEnd.TabIndex = 2;
            this.btnStartOrEnd.Text = "启动服务";
            this.btnStartOrEnd.UseVisualStyleBackColor = true;
            this.btnStartOrEnd.Click += new System.EventHandler(this.btnStartOrEnd_Click);
            // 
            // btnGetStatus
            // 
            this.btnGetStatus.Location = new System.Drawing.Point(201, 47);
            this.btnGetStatus.Name = "btnGetStatus";
            this.btnGetStatus.Size = new System.Drawing.Size(75, 23);
            this.btnGetStatus.TabIndex = 3;
            this.btnGetStatus.Text = "获取状态";
            this.btnGetStatus.UseVisualStyleBackColor = true;
            this.btnGetStatus.Click += new System.EventHandler(this.btnGetStatus_Click);
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.lblServiceName);
            this.gbMain.Controls.Add(this.btnStartOrEnd);
            this.gbMain.Controls.Add(this.btnInstallOrUninstall);
            this.gbMain.Controls.Add(this.btnGetStatus);
            this.gbMain.Controls.Add(this.lblMsg);
            this.gbMain.Controls.Add(this.label2);
            this.gbMain.Controls.Add(this.label1);
            this.gbMain.Location = new System.Drawing.Point(4, 4);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(285, 118);
            this.gbMain.TabIndex = 4;
            this.gbMain.TabStop = false;
            this.gbMain.Text = "即时通讯服务";
            // 
            // lblServiceName
            // 
            this.lblServiceName.AutoSize = true;
            this.lblServiceName.Location = new System.Drawing.Point(69, 25);
            this.lblServiceName.Name = "lblServiceName";
            this.lblServiceName.Size = new System.Drawing.Size(59, 12);
            this.lblServiceName.TabIndex = 3;
            this.lblServiceName.Text = "Wechat";
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Location = new System.Drawing.Point(69, 79);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(29, 12);
            this.lblMsg.TabIndex = 2;
            this.lblMsg.Text = "none";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "服务状态：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务名称：";
            // 
            // FrmServiceSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 125);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.gbMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmServiceSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "即时通讯服务--密牙科技";
            this.contextMenuStrip1.ResumeLayout(false);
            this.gbMain.ResumeLayout(false);
            this.gbMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Button btnInstallOrUninstall;
        private System.Windows.Forms.Button btnStartOrEnd;
        private System.Windows.Forms.Button btnGetStatus;
        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblServiceName;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.ToolStripMenuItem 打开主界面ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
    }
}


namespace FileDeploy
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.templatesSearchBox = new System.Windows.Forms.TextBox();
            this.templatesListBox = new System.Windows.Forms.ListBox();
            this.parameterInputLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.comfirmInputButton = new System.Windows.Forms.Button();
            this.openTemplateFolderButton = new System.Windows.Forms.Button();
            this.currentTemplateDirLable = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // templatesSearchBox
            // 
            this.templatesSearchBox.Location = new System.Drawing.Point(12, 10);
            this.templatesSearchBox.Name = "templatesSearchBox";
            this.templatesSearchBox.Size = new System.Drawing.Size(213, 21);
            this.templatesSearchBox.TabIndex = 1;
            this.templatesSearchBox.TextChanged += new System.EventHandler(this.templatesSearchBox_TextChanged);
            this.templatesSearchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.templatesSearchBox_KeyDown);
            // 
            // templatesListBox
            // 
            this.templatesListBox.FormattingEnabled = true;
            this.templatesListBox.ItemHeight = 12;
            this.templatesListBox.Location = new System.Drawing.Point(12, 52);
            this.templatesListBox.Name = "templatesListBox";
            this.templatesListBox.Size = new System.Drawing.Size(213, 304);
            this.templatesListBox.TabIndex = 2;
            this.templatesListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.templatesListBox_KeyDown);
            // 
            // parameterInputLayout
            // 
            this.parameterInputLayout.BackColor = System.Drawing.SystemColors.ControlDark;
            this.parameterInputLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.parameterInputLayout.Location = new System.Drawing.Point(250, 52);
            this.parameterInputLayout.Name = "parameterInputLayout";
            this.parameterInputLayout.Size = new System.Drawing.Size(326, 304);
            this.parameterInputLayout.TabIndex = 3;
            // 
            // comfirmInputButton
            // 
            this.comfirmInputButton.Enabled = false;
            this.comfirmInputButton.Location = new System.Drawing.Point(602, 52);
            this.comfirmInputButton.Name = "comfirmInputButton";
            this.comfirmInputButton.Size = new System.Drawing.Size(84, 75);
            this.comfirmInputButton.TabIndex = 0;
            this.comfirmInputButton.Text = "Confirm";
            this.comfirmInputButton.UseVisualStyleBackColor = true;
            this.comfirmInputButton.Click += new System.EventHandler(this.comfirmInputButton_Click);
            // 
            // openTemplateFolderButton
            // 
            this.openTemplateFolderButton.Location = new System.Drawing.Point(12, 397);
            this.openTemplateFolderButton.Name = "openTemplateFolderButton";
            this.openTemplateFolderButton.Size = new System.Drawing.Size(141, 25);
            this.openTemplateFolderButton.TabIndex = 4;
            this.openTemplateFolderButton.TabStop = false;
            this.openTemplateFolderButton.Text = "Open Template Folder";
            this.openTemplateFolderButton.UseVisualStyleBackColor = true;
            this.openTemplateFolderButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // currentTemplateDirLable
            // 
            this.currentTemplateDirLable.AutoSize = true;
            this.currentTemplateDirLable.Location = new System.Drawing.Point(159, 397);
            this.currentTemplateDirLable.Name = "currentTemplateDirLable";
            this.currentTemplateDirLable.Size = new System.Drawing.Size(41, 12);
            this.currentTemplateDirLable.TabIndex = 5;
            this.currentTemplateDirLable.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(703, 450);
            this.Controls.Add(this.currentTemplateDirLable);
            this.Controls.Add(this.openTemplateFolderButton);
            this.Controls.Add(this.comfirmInputButton);
            this.Controls.Add(this.parameterInputLayout);
            this.Controls.Add(this.templatesListBox);
            this.Controls.Add(this.templatesSearchBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.TextBox templatesSearchBox;
        private System.Windows.Forms.ListBox templatesListBox;
        private System.Windows.Forms.FlowLayoutPanel parameterInputLayout;
        private System.Windows.Forms.Button comfirmInputButton;
        private System.Windows.Forms.Button openTemplateFolderButton;
        private System.Windows.Forms.Label currentTemplateDirLable;
    }
}


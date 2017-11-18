namespace TestForm
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.btn = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.button8 = new System.Windows.Forms.Button();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button9 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.px_CycleNum = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.px_Cycleradius = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.px_Boatincremental = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.px_Boatdistnct = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.px_Longitude = new System.Windows.Forms.NumericUpDown();
            this.px_Latitude = new System.Windows.Forms.NumericUpDown();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
            this.ckAircycle = new System.Windows.Forms.CheckBox();
            this.ckIsJianhua = new System.Windows.Forms.CheckBox();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.btnBeginListen = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_CycleNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Cycleradius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Boatincremental)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Boatdistnct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Longitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Latitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "高度";
            // 
            // btn
            // 
            this.btn.Location = new System.Drawing.Point(138, 20);
            this.btn.Name = "btn";
            this.btn.Size = new System.Drawing.Size(75, 23);
            this.btn.TabIndex = 2;
            this.btn.Text = "高度加减";
            this.btn.UseVisualStyleBackColor = true;
            this.btn.Click += new System.EventHandler(this.btn_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 115);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1100, 319);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(951, 84);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "清空";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(303, 51);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "航点设置";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // richTextBox2
            // 
            this.richTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox2.Location = new System.Drawing.Point(11, 440);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(1100, 183);
            this.richTextBox2.TabIndex = 6;
            this.richTextBox2.Text = "";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1032, 84);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "解析收到的信息";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(528, 2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 42);
            this.button4.TabIndex = 8;
            this.button4.Tag = "上";
            this.button4.Text = "↑";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(447, 41);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 36);
            this.button5.TabIndex = 9;
            this.button5.Tag = "左";
            this.button5.Text = "←";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button4_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(608, 40);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 37);
            this.button6.TabIndex = 10;
            this.button6.Tag = "右";
            this.button6.Text = "→";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button4_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(528, 70);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 38);
            this.button7.TabIndex = 11;
            this.button7.Tag = "下";
            this.button7.Text = "↓";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button4_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(528, 47);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(75, 21);
            this.numericUpDown1.TabIndex = 12;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(218, 86);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 13;
            this.button8.Text = "航线切换";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(52, 87);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(55, 21);
            this.numericUpDown2.TabIndex = 14;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(156, 87);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            800,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(55, 21);
            this.numericUpDown3.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "航线";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(121, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 17;
            this.label3.Text = "航点";
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(870, 83);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 18;
            this.button9.Text = "盘旋切换";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(995, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "盘旋圈数";
            // 
            // px_CycleNum
            // 
            this.px_CycleNum.Location = new System.Drawing.Point(1051, 34);
            this.px_CycleNum.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.px_CycleNum.Name = "px_CycleNum";
            this.px_CycleNum.Size = new System.Drawing.Size(55, 21);
            this.px_CycleNum.TabIndex = 21;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(692, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 24;
            this.label6.Text = "经度";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(911, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 26;
            this.label7.Text = "纬度";
            // 
            // px_Cycleradius
            // 
            this.px_Cycleradius.Location = new System.Drawing.Point(744, 32);
            this.px_Cycleradius.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.px_Cycleradius.Name = "px_Cycleradius";
            this.px_Cycleradius.Size = new System.Drawing.Size(88, 21);
            this.px_Cycleradius.TabIndex = 28;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(692, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 27;
            this.label8.Text = "盘旋半径";
            // 
            // px_Boatincremental
            // 
            this.px_Boatincremental.Location = new System.Drawing.Point(744, 86);
            this.px_Boatincremental.Maximum = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.px_Boatincremental.Minimum = new decimal(new int[] {
            1800,
            0,
            0,
            -2147483648});
            this.px_Boatincremental.Name = "px_Boatincremental";
            this.px_Boatincremental.Size = new System.Drawing.Size(88, 21);
            this.px_Boatincremental.TabIndex = 30;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(692, 89);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 29;
            this.label9.Text = "航向增量";
            // 
            // px_Boatdistnct
            // 
            this.px_Boatdistnct.Location = new System.Drawing.Point(901, 35);
            this.px_Boatdistnct.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.px_Boatdistnct.Name = "px_Boatdistnct";
            this.px_Boatdistnct.Size = new System.Drawing.Size(88, 21);
            this.px_Boatdistnct.TabIndex = 32;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(848, 38);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 31;
            this.label10.Text = "目标距离";
            // 
            // px_Longitude
            // 
            this.px_Longitude.Location = new System.Drawing.Point(744, 7);
            this.px_Longitude.Maximum = new decimal(new int[] {
            180000000,
            0,
            0,
            0});
            this.px_Longitude.Minimum = new decimal(new int[] {
            180000000,
            0,
            0,
            -2147483648});
            this.px_Longitude.Name = "px_Longitude";
            this.px_Longitude.Size = new System.Drawing.Size(161, 21);
            this.px_Longitude.TabIndex = 33;
            // 
            // px_Latitude
            // 
            this.px_Latitude.Location = new System.Drawing.Point(946, 8);
            this.px_Latitude.Maximum = new decimal(new int[] {
            90000000,
            0,
            0,
            0});
            this.px_Latitude.Minimum = new decimal(new int[] {
            90000000,
            0,
            0,
            -2147483648});
            this.px_Latitude.Name = "px_Latitude";
            this.px_Latitude.Size = new System.Drawing.Size(161, 21);
            this.px_Latitude.TabIndex = 34;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(744, 61);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(95, 16);
            this.radioButton1.TabIndex = 35;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "跟踪地面目标";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(845, 61);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(119, 16);
            this.radioButton2.TabIndex = 36;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "沿航线飞向目标点";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(970, 61);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(95, 16);
            this.radioButton3.TabIndex = 37;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "改变飞行航向";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(52, 21);
            this.numericUpDown4.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown4.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(70, 21);
            this.numericUpDown4.TabIndex = 38;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 40;
            this.label5.Text = "航线";
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(52, 55);
            this.numericUpDown5.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(55, 21);
            this.numericUpDown5.TabIndex = 39;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(108, 59);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(53, 12);
            this.label11.TabIndex = 42;
            this.label11.Text = "航点个数";
            // 
            // numericUpDown6
            // 
            this.numericUpDown6.Location = new System.Drawing.Point(163, 55);
            this.numericUpDown6.Maximum = new decimal(new int[] {
            800,
            0,
            0,
            0});
            this.numericUpDown6.Name = "numericUpDown6";
            this.numericUpDown6.Size = new System.Drawing.Size(55, 21);
            this.numericUpDown6.TabIndex = 41;
            // 
            // ckAircycle
            // 
            this.ckAircycle.AutoSize = true;
            this.ckAircycle.Location = new System.Drawing.Point(225, 55);
            this.ckAircycle.Name = "ckAircycle";
            this.ckAircycle.Size = new System.Drawing.Size(72, 16);
            this.ckAircycle.TabIndex = 43;
            this.ckAircycle.Text = "航线循环";
            this.ckAircycle.UseVisualStyleBackColor = true;
            // 
            // ckIsJianhua
            // 
            this.ckIsJianhua.AutoSize = true;
            this.ckIsJianhua.Location = new System.Drawing.Point(380, 25);
            this.ckIsJianhua.Name = "ckIsJianhua";
            this.ckIsJianhua.Size = new System.Drawing.Size(48, 16);
            this.ckIsJianhua.TabIndex = 52;
            this.ckIsJianhua.Text = "讲话";
            this.ckIsJianhua.UseVisualStyleBackColor = true;
            // 
            // numericPort
            // 
            this.numericPort.Location = new System.Drawing.Point(303, 21);
            this.numericPort.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericPort.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericPort.Name = "numericPort";
            this.numericPort.Size = new System.Drawing.Size(70, 21);
            this.numericPort.TabIndex = 51;
            this.numericPort.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // btnBeginListen
            // 
            this.btnBeginListen.Location = new System.Drawing.Point(225, 17);
            this.btnBeginListen.Name = "btnBeginListen";
            this.btnBeginListen.Size = new System.Drawing.Size(75, 27);
            this.btnBeginListen.TabIndex = 50;
            this.btnBeginListen.Tag = "上";
            this.btnBeginListen.Text = "开启监听";
            this.btnBeginListen.UseVisualStyleBackColor = true;
            this.btnBeginListen.Click += new System.EventHandler(this.btnBeginListen_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(434, 15);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 27);
            this.btnSend.TabIndex = 53;
            this.btnSend.Tag = "上";
            this.btnSend.Text = "发送";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1131, 633);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.ckIsJianhua);
            this.Controls.Add(this.numericPort);
            this.Controls.Add(this.btnBeginListen);
            this.Controls.Add(this.ckAircycle);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.numericUpDown6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericUpDown5);
            this.Controls.Add(this.numericUpDown4);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.px_Latitude);
            this.Controls.Add(this.px_Longitude);
            this.Controls.Add(this.px_Boatdistnct);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.px_Boatincremental);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.px_Cycleradius);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.px_CycleNum);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown3);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "验证解析方法";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_CycleNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Cycleradius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Boatincremental)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Boatdistnct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Longitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.px_Latitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown px_CycleNum;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown px_Cycleradius;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown px_Boatincremental;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown px_Boatdistnct;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown px_Longitude;
        private System.Windows.Forms.NumericUpDown px_Latitude;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericUpDown6;
        private System.Windows.Forms.CheckBox ckAircycle;
        private System.Windows.Forms.CheckBox ckIsJianhua;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Button btnBeginListen;
        private System.Windows.Forms.Button btnSend;
    }
}


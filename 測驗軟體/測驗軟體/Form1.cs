using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

namespace 測驗軟體
{/// 字彙題+選擇題
    public partial class Form1 : Form
    {
        public static Font ITxtBoxFont = new Font("標楷體", 20, FontStyle.Regular);
        public static Font InputFont = new Font("標楷體", 20, FontStyle.Regular);
        public static Font TabFont = new Font("標楷體", 15, FontStyle.Regular);
        public static Font ButtonFont = new Font("標楷體", 15, FontStyle.Regular);
        IntPtr m_hImc;
        public const int WM_IME_SETCONTEXT = 0x0281;
        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
        public Form1()
        {
            InitializeComponent();
            m_hImc = ImmGetContext(this.Handle);
            Form1.CheckForIllegalCrossThreadCalls = false;
            this.KeyPreview = true;
            this.FormClosing += Form1_FormClosing;
            this.Shown += Form1_Shown;
            this.KeyUp += Form1_KeyUp;
            this.Size = new Size(1000, 500);
            this.WindowState = FormWindowState.Maximized;
            this.Font = ITxtBoxFont;
        }
        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            KeysQueue.Enqueue(e.KeyData);
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // the usercontrol will receive a WM_IME_SETCONTEXT message when it gets focused and loses focus respectively
            // when the usercontrol gets focused, the m.WParam is 1
            // when the usercontrol loses focus, the m.WParam is 0
            // only when the usercontrol gets focused, we need to call the IMM function to associate itself to the default input context
            if (m.Msg == WM_IME_SETCONTEXT && m.WParam.ToInt32() == 1)
            {
                ImmAssociateContext(this.Handle, m_hImc);
            }
        }
        public partial class ITxtBox : TextBox
        {
            IntPtr m_hImc;
            public const int WM_IME_SETCONTEXT = 0x0281;
            [DllImport("Imm32.dll")]
            public static extern IntPtr ImmGetContext(IntPtr hWnd);
            [DllImport("Imm32.dll")]
            public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
            public ITxtBox()
            {
                this.Multiline = true;
                m_hImc = ImmGetContext(this.Handle);
            }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                // the usercontrol will receive a WM_IME_SETCONTEXT message when it gets focused and loses focus respectively
                // when the usercontrol gets focused, the m.WParam is 1
                // when the usercontrol loses focus, the m.WParam is 0
                // only when the usercontrol gets focused, we need to call the IMM function to associate itself to the default input context
                if (m.Msg == WM_IME_SETCONTEXT && m.WParam.ToInt32() == 1)
                {
                    ImmAssociateContext(this.Handle, m_hImc);
                }
            }
        }
        public static bool IsInserted;
        public static string InsertString;
        public struct All
        {
            public static TabControl Tab;
            public static void Reset()
            {
                Tab = new TabControl();
                Setting.Reset();
                EnglishTest.Reset();
                ChineseTest.Reset();
                ChemistryTest.Reset();
                Dictionary.Reset();
                AboutCreator.Reset();
                EnglishTest.Add();
                ChineseTest.Add();
                ChemistryTest.Add();
                Dictionary.Add();
                Setting.Add();
                AboutCreator.Add();
                
                Tab.Dock = DockStyle.Fill;
                Tab.Font = TabFont;
                Tab.SelectedIndexChanged += Dictionary.FocusInsert;
                Tab.SelectedIndex = Dictionary.TabIndex;
            }
        }
        public struct EnglishTest
        {
            public class VocabularyTestLine
            {
                public int TimesE;
                public int TimesC;
                public DateTime DateE;
                public DateTime DateC;
                public string Word;
                public string Explanation;
                public bool IsToTestE()
                {
                    if (DateE > DateTime.Now.Date) return false;
                    if (TimesE == Setting.reviewdays.Length) return false;
                    if (DateE < DateTime.Now.Date) EnglishTest.Missed++;
                    return true;
                }
                public bool IsToTestC()
                {
                    if (DateC > DateTime.Now.Date) return false;
                    if (TimesC == Setting.reviewdays.Length) return false;
                    if (DateC < DateTime.Now.Date) EnglishTest.Missed++;
                    return true;
                }
                public bool PushDateE()
                {
                    if (TimesE < Setting.reviewdays.Length)
                    {
                        DateE = DateTime.Now.AddDays(Setting.reviewdays[TimesE]);
                        TimesE++;
                        return true;
                    }
                    else if (TimesE == Setting.reviewdays.Length)
                    {
                        TimesE++;
                        return false;
                    }
                    else throw new ArgumentException("已經經過最後一個複習時間Setting.reviewdays[" + (Setting.reviewdays.Length - 1).ToString() + "]==" + Setting.reviewdays[Setting.reviewdays.Length - 1].ToString());
                }
                public void PullDateE()
                {
                    if (TimesE >= 2) TimesE -= 2;
                    else if (TimesE == 1) TimesE--;
                }
                public bool PushDateC()
                {
                    if (TimesC < Setting.reviewdays.Length)
                    {
                        DateC = DateTime.Now.AddDays(Setting.reviewdays[TimesC]);
                        TimesC++;
                        return true;
                    }
                    else if (TimesC == Setting.reviewdays.Length)
                    {
                        TimesC++;
                        return false;
                    }
                    else throw new ArgumentException("已經經過最後一個複習時間Setting.reviewdays[" + (Setting.reviewdays.Length - 1).ToString() + "]==" + Setting.reviewdays[Setting.reviewdays.Length - 1].ToString());
                }
                public void PullDateC()
                {
                    if (TimesC >= 2) TimesC -= 2;
                    else if (TimesC == 1) TimesC--;
                }
                public void SetFrom(string a)
                {
                    if (IsFit(a, "n\tnnnn/nn/nn\tn\tnnnn/nn/nn\t'"))
                    {
                        a = a.Remove(26) + a.Substring(27);
                        EnglishTest.Tab.FindForm().Text = a;
                    }
                    else if (!IsFit(a, "n\tnnnn/nn/nn\tn\tnnnn/nn/nn\t"))
                    {
                        a = "0\t" + DateToString(DateTime.Now) + "\t0\t" + DateToString(DateTime.Now) + "\t" + a;
                        EnglishTest.Tab.FindForm().Text = a;
                    }
                    string[] b = a.Split('\t');
                    TimesE = int.Parse(b[0]);
                    DateE = DateTime.Parse(b[1]);
                    TimesC = int.Parse(b[2]);
                    DateC = DateTime.Parse(b[3]);
                    string d = "";
                    for (int c = 4; true; c++)
                    {
                        if (c == b.Length - 1)
                        {
                            d += b[c];
                            break;
                        }
                        else d += b[c] + "\t";
                    }
                    string[] e = d.Split(EnglishTest.FillMark);
                    while (e.Length != 2)
                    {
                        d = Interaction.InputBox("錯誤:\r\n以下符號的數量不正確\" " + EnglishTest.FillMark + " \"", "錯誤更正", d);
                        e = d.Split(EnglishTest.FillMark);
                    }
                    Word = d.Split(EnglishTest.FillMark)[0];
                    Explanation = d.Split(EnglishTest.FillMark)[1];
                }
                public override string ToString()
                {
                    return TimesE.ToString() + "\t" + DateToString(DateE) + "\t" + TimesC.ToString() + "\t" + DateToString(DateC) + "\t" + Word + EnglishTest.FillMark + Explanation;
                }
                public string StringWithoutRecord()
                {
                    return Word + EnglishTest.FillMark.ToString() + Explanation;
                }
                public bool ShowTestE(int num)
                {
                    string a = num.ToString() + ". _";
                    int r1 = random.Next(0, Word.Length), r2 = random.Next(0, Word.Length);
                    while (r1 == r2) r2 = random.Next(0, Word.Length);
                    if (r1 < r2) a += Word[r1].ToString() + "_" + Word[r2].ToString();
                    else a += Word[r2].ToString() + "_" + Word[r1].ToString();
                    a += "_ \t" + Explanation + "\r\n";
                    EnglishTest.Txb1.AppendText(a);
                    Reform(true);
                    EnglishTest.Txb2.Focus();
                    bool ToBreak = false, CanRemove = false;
                    while (!ToBreak)
                    {
                        KeyEventHandler textchangedevent = new KeyEventHandler(Txb2_TextChanged);
                        DateTime past = DateTime.Now;
                        IsInserted = false;
                        EnglishTest.Txb2.ResetText();
                        EnglishTest.Txb2.KeyUp += textchangedevent;
                        while (!IsInserted) Application.DoEvents();
                        EnglishTest.Txb2.KeyUp -= textchangedevent;
                        a = EnglishTest.ShowReactTime(past);
                        if (EnglishTest.Txb2.Text == Word)
                        {
                            bool Pushed = PushDateE();
                            if (Pushed) a += EnglishTest.ShowTestResult(EnglishTest.Txb2.Text, DateE);
                            else a += EnglishTest.ShowTestResult(EnglishTest.Txb2.Text);
                            CanRemove = true;
                            ToBreak = true;
                        }
                        else
                        {
                            string input = EnglishTest.Txb2.Text;
                            List<int> b = new List<int>();
                            for(int i=0;i<Question.Length;i++)
                            {
                                if (Question[i].Word == input) b.Add(i);
                            }
                            if(b.Count==0)
                            {
                                PullDateE();
                                a += "\t\"" + input + "\" isn't existed!\r\n";
                                ToBreak = true;
                                a += "\tWrong!\tAns:" + Word + "\r\n\r\n";
                            }
                            else
                            {
                                bool IsAnotherAns = false;
                                for(int i=0;i<b.Count;i++)
                                {
                                    if(Question[b[i]].Explanation==Explanation)
                                    {
                                        IsAnotherAns = true;
                                        if (Question[b[i]].IsToTestE())
                                        {
                                            if (Question[b[i]].PushDateE()) a += EnglishTest.ShowTestResult(input, Question[b[i]].DateE);
                                            else a += EnglishTest.ShowTestResult(input);
                                        }
                                    }
                                }
                                if(IsAnotherAns)
                                {
                                    a += "\t\" "+input+" \" is Correct. ANOTHER ONE?\r\n";
                                }
                                else
                                {
                                    PullDateE();
                                    a += "\t[ " + input + " ]\r\n";
                                    if (b.Count == 1)
                                    {
                                        a += "\t => " + Question[b[0]].Explanation + "\r\n";
                                    }
                                    else
                                    {
                                        for (int i = 0; i < b.Count; i++)
                                        {
                                            a += "\t    " + (i + 1).ToString() + ". " + Question[b[i]].Explanation + "\r\n";
                                        }
                                    }
                                    ToBreak = true;
                                    a += "\tWrong!\tAns:" + Word + "\r\n\r\n";
                                }
                            }
                        }
                        EnglishTest.Txb1.AppendText(a);
                    }
                    EnglishTest.Phonetic_Thread(Word);
                    return CanRemove;
                }
                public bool ShowTestC(int num)
                {
                    string a = num.ToString() + ". "+Word+"\r\n";
                    VocabularyTestLine[] Option = new VocabularyTestLine[EnglishTest.OptionSum];
                    for (int i = 0; i < Option.Length; i++)
                    {
                        Option[i] = EnglishTest.Question[random.Next(0, EnglishTest.Question.Length)];
                        if (Option[i].Word == this.Word || Option[i].Explanation == this.Explanation) { i--;continue; }
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (Option[i].Word == Option[j].Word || Option[i].Explanation == Option[j].Explanation)
                            {
                                i--;
                                break;
                            }
                        }
                    }
                    int ans=random.Next(0, Option.Length);
                    Option[ans] = this;
                    for (int i = 0; i < Option.Length; i++) a += "(" + ((char)('A' + i)).ToString() + ")\t" + Option[i].Explanation+"\r\n";
                    EnglishTest.Txb1.AppendText(a);
                    Reform(false);
                    EventHandler clickevent = new EventHandler(Btns_Click);
                    KeyEventHandler keyupevent = new KeyEventHandler(Form1_KeyUp);
                    EnglishTest.Tlp2.Focus();
                    bool ToBreak = false, CanRemove = false;
                    while (!ToBreak)
                    {
                        EnglishTest.Tab.FindForm().KeyUp += keyupevent;
                        for (int i = 0; i < EnglishTest.Btns.Length; i++) EnglishTest.Btns[i].Click += clickevent;
                        DateTime past = DateTime.Now;
                        IsInserted = false;
                        InsertString = "";
                        while (!IsInserted) Application.DoEvents();
                        EnglishTest.Tab.FindForm().KeyUp -= keyupevent;
                        for (int i = 0; i < EnglishTest.Btns.Length; i++) EnglishTest.Btns[i].Click -= clickevent;
                        int yourans = InsertString[0] - 'A';
                        a = EnglishTest.ShowReactTime(past);
                        if(yourans==ans)
                        {
                            bool Pushed = PushDateC();
                            if (Pushed) a += EnglishTest.ShowTestResult("(" + InsertString + ")\t" + Option[yourans].Word + "\t" + Option[yourans].Explanation, DateE);
                            else a += EnglishTest.ShowTestResult("(" + InsertString + ")\t" + Option[yourans].Word + "\t" + Option[yourans].Explanation);
                            CanRemove = true;
                            ToBreak = true;
                        }
                        else
                        {
                            PullDateC();
                            a += EnglishTest.ShowTestResult("(" + InsertString + ")" + Option[yourans].Word + "\t" + Option[yourans].Explanation, "(" + ((char)('A' + ans)).ToString() + ")\t" + Explanation);
                            ToBreak = true;
                        }
                        EnglishTest.Txb1.AppendText(a);
                    }
                    return CanRemove;
                }
                public void Reform(bool isE)
                {
                    foreach (Control a in EnglishTest.Tlp1.Controls) if (a != EnglishTest.Txb1) EnglishTest.Tlp1.Controls.Remove(a);
                    if (isE)
                    {
                        EnglishTest.Txb2 = new ITxtBox();
                        EnglishTest.Txb2.Font = InputFont;
                        EnglishTest.Tlp1.Controls.Add(EnglishTest.Txb2);
                        EnglishTest.Txb2.Dock = DockStyle.Fill;
                        EnglishTest.Txb2.Multiline = true;
                        EnglishTest.Txb2.ImeMode = ImeMode.Off;
                    }
                    else
                    {
                        EnglishTest.Tlp2 = new TableLayoutPanel();
                        EnglishTest.Tlp1.Controls.Add(EnglishTest.Tlp2);
                        EnglishTest.Tlp2.Dock = DockStyle.Fill;
                        EnglishTest.Tlp2.ColumnCount = EnglishTest.OptionSum;
                        EnglishTest.Tlp2.RowCount = 1;
                        Array.Resize(ref EnglishTest.Btns,EnglishTest.OptionSum);
                        for(int i=0;i<EnglishTest.Btns.Length;i++) EnglishTest.Btns[i]=new Button();
                        EnglishTest.Tlp2.Controls.AddRange(EnglishTest.Btns);
                        {
                            for(int i=0;i<EnglishTest.Btns.Length;i++)
                            {
                                EnglishTest.Tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                                EnglishTest.Tlp2.SetCellPosition(EnglishTest.Btns[i], new TableLayoutPanelCellPosition(i, 0));
                                EnglishTest.Btns[i].Dock = DockStyle.Fill;
                                EnglishTest.Btns[i].Font = ButtonFont;
                                EnglishTest.Btns[i].Text = ((char)('A' + i)).ToString();
                            }
                        }
                    }
                }
                public void Txb2_TextChanged(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    if (e.KeyData == Keys.Enter)
                    {
                        int b = EnglishTest.Txb2.Text.IndexOf("\r\n");
                        if (b != -1)
                        {
                            string a = EnglishTest.Txb2.Text;
                            EnglishTest.Txb2.Text = a.Remove(b) + (b + 2 == a.Length ? "" : a.Substring(b + 2));
                        IsInserted = true;
                        }
                    }
                }
                void Form1_KeyUp(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    if (e.KeyData.ToString().Length == 1)
                        if (e.KeyData.ToString()[0] >= 'A' && e.KeyData.ToString()[0] <= 'A' + EnglishTest.OptionSum - 1)
                        {
                            //MessageBox.Show(e.KeyData.ToString());
                            Btns_Click(e.KeyData.ToString(), null);
                        }
                }
                void Btns_Click(object sender, EventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    if (sender.GetType() == typeof(string))
                    {
                        InsertString = (string)sender;
                        IsInserted = true;
                    }
                    else
                    {
                        InsertString = (sender as Button).Text;
                        IsInserted = true;
                    }
                }
            }
            public struct MultiChoiceTestLine
            {
                public struct OptionPart
                {
                    public bool IsAns;
                    public string Text;
                    public bool Checked;
                    public void SetFrom(string a)
                    {
                        if (IsFit(a, "(E)")) IsAns = false;
                        else if (IsFit(a, "(e)")) IsAns = true;
                        else
                        {
                            MessageBox.Show("EnglishTest.OptionPart.SetFrom:\r\na==" + a);
                            return;
                        }
                        Text = a.Substring(3);
                    }
                }
                public Color[] CheckedColor;
                public string Text;
                public OptionPart[] Option;
                public DateTime Date;
                public int Times;
                public Color[] NormalColor;
                public bool ShowTest(int num)
                {
                    MessOrder();
                    string a = num.ToString() + ". " + Text;
                    for (int i = 0; i < Option.Length;i++ ) a += "("+((char)('A' + i)).ToString() +")"+ Option[i].Text;
                    EnglishTest.Txb1.AppendText(a);
                    Reform(Option.Length);
                    ShowChecked();
                    KeyEventHandler keyupevent = new KeyEventHandler(MultiChoiceTestLine_KeyUp);
                    EventHandler clickevent = new EventHandler(MultiChoiceTestLine_Click);
                    EventHandler doubleclickevent = new EventHandler(Txb1_DoubleClick);
                    EnglishTest.Txb1.DoubleClick += doubleclickevent;
                    EnglishTest.Tab.FindForm().KeyUp += keyupevent;
                    for (int i = 0; i < Option.Length;i++ )
                    {
                        EnglishTest.Btns[i].Text = ((char)('A' + i)).ToString();
                        EnglishTest.Btns[i].Click += clickevent;
                    }
                    DateTime past = DateTime.Now;
                    IsInserted = false;
                    while (!IsInserted) Application.DoEvents();
                    EnglishTest.Txb1.DoubleClick -= doubleclickevent;
                    EnglishTest.Tab.FindForm().KeyUp -= keyupevent;
                    a = EnglishTest.ShowReactTime(past);
                    string ans = "",yourans="";
                    bool CanRemove = true;
                    for (int i = 0; i < Option.Length; i++)
                    {
                        EnglishTest.Btns[i].Click -= clickevent;
                        if (Option[i].IsAns) ans += ((char)('A' + i)).ToString();
                        if (Option[i].Checked) yourans += ((char)('A' + i)).ToString();
                        if (Option[i].Checked != Option[i].IsAns) CanRemove = false;
                    }
                    if(CanRemove)
                    {
                        bool Pushed = PushDate();
                        if (Pushed) a += EnglishTest.ShowTestResult(yourans, Date);
                        else a += EnglishTest.ShowTestResult(yourans);
                    }
                    else
                    {
                        PullDate();
                        a += EnglishTest.ShowTestResult(yourans, ans);
                    }
                    EnglishTest.Txb1.AppendText(a);
                    { }
                    return CanRemove;
                }
                public void MessOrder()
                {
                    OptionPart[] tmpOption = new OptionPart[Option.Length];
                    bool[] visited = new bool[Option.Length];
                    for (int i = Option.Length; i > 0; i--)
                    {
                        int j = random.Next(0, i);
                        for (int k = 0; ; k++)
                        {
                            if (!visited[k])
                            {
                                if (j == 0)
                                {
                                    tmpOption[k] = Option[i - 1];
                                    visited[k] = true;
                                    break;
                                }
                                j--;
                            }
                        }
                    }
                    Option = tmpOption;
                }
                void Txb1_DoubleClick(object sender, EventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    IsInserted = true;
                }
                void MultiChoiceTestLine_Click(object sender, EventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    int k = -1;
                    if(sender.GetType()==typeof(int)) k = (int)sender;
                    else k = EnglishTest.Tlp2.GetCellPosition(sender as Button).Column;
                    if (Option[k].Checked) Option[k].Checked = false;
                    else Option[k].Checked = true;
                    ShowChecked();
                }
                private void MultiChoiceTestLine_KeyUp(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    string a = e.KeyData.ToString();
                    if (a.Length == 1 && a[0] >= 'A' && a[0] - 'A' < Option.Length)
                    {
                        MultiChoiceTestLine_Click(a[0] - 'A', null);
                    }
                    else if (e.KeyData == Keys.Enter) IsInserted = true;
                }
                public void ShowChecked()
                {
                    for(int i=0;i<Option.Length;i++)
                    {
                        if (Option[i].Checked && (EnglishTest.Btns[i].BackColor != CheckedColor[0] || EnglishTest.Btns[i].ForeColor != CheckedColor[1]))
                        {
                            EnglishTest.Btns[i].BackColor = CheckedColor[0];
                            EnglishTest.Btns[i].ForeColor = CheckedColor[1];
                        }
                        else if(!Option[i].Checked&&(EnglishTest.Btns[i].BackColor!=NormalColor[0]||EnglishTest.Btns[i].ForeColor!=NormalColor[0]))
                        {
                            EnglishTest.Btns[i].BackColor = NormalColor[0];
                            EnglishTest.Btns[i].ForeColor = NormalColor[1];
                        }
                    }
                }
                public void Reform(int a)
                {
                    foreach (Control b in EnglishTest.Tlp1.Controls) if (b != EnglishTest.Txb1) EnglishTest.Tlp1.Controls.Remove(b);
                    EnglishTest.Tlp2 = new TableLayoutPanel();
                    EnglishTest.Tlp2.Dock = DockStyle.Fill;
                    EnglishTest.Tlp2.ColumnCount = a;
                    EnglishTest.Tlp2.RowCount = 1;
                    EnglishTest.Btns = new Button[a];
                    for(int i=0;i<a;i++)
                    {
                        EnglishTest.Btns[i] = new Button();
                        EnglishTest.Btns[i].Font = ButtonFont;
                        EnglishTest.Btns[i].Dock = DockStyle.Fill;
                        EnglishTest.Tlp2.Controls.Add(EnglishTest.Btns[i]);
                        EnglishTest.Tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        EnglishTest.Tlp2.SetCellPosition(EnglishTest.Btns[i], new TableLayoutPanelCellPosition(i, 0));
                    }
                    NormalColor = new Color[] { EnglishTest.Btns[0].BackColor, EnglishTest.Btns[0].ForeColor };
                    EnglishTest.Tlp1.Controls.Add(EnglishTest.Tlp2);
                    EnglishTest.Tlp1.SetCellPosition(EnglishTest.Tlp2, new TableLayoutPanelCellPosition(0, 1));
                }
                public bool PushDate()
                {
                    if (Times < Setting.reviewdays.Length)
                    {
                        Date = DateTime.Now.AddDays(Setting.reviewdays[Times]);
                        Times++;
                        return true;
                    }
                    else if (Times == Setting.reviewdays.Length)
                    {
                        Times++;
                        return false;
                    }
                    else throw new ArgumentException("已經經過最後一個複習時間Setting.reviewdays[" + (Setting.reviewdays.Length - 1).ToString() + "]==" + Setting.reviewdays[Setting.reviewdays.Length - 1].ToString());
                }
                public void PullDate()
                {
                    if (Times >= 2) Times -= 2;
                    else if (Times == 1) Times--;
                }
                public void SetFrom(string a)
                {
                    CheckedColor = new Color[] { Color.FromArgb(0, 0, 255), Color.FromArgb(255, 255, 255) };
                    if (!IsFit(a, "n\tnnnn/nn/nn\t"))
                    {
                        a = "0\t" + DateToString(DateTime.Now) + "\t" + a;
                        EnglishTest.Tab.FindForm().Text = a;
                    }
                    Times = int.Parse(a.Substring(0, 1));
                    Date = DateTime.Parse(a.Substring(2, 10));
                    a = a.Substring(13);
                    int index = OptionIndex(a, 0);
                    Text = a.Remove(index);
                    int nowindex=index;
                    index=OptionIndex(a,index+1);
                    Option = new OptionPart[0];
                    while(index!=-1)
                    {
                        Array.Resize(ref Option,Option.Length+1);
                        Option[Option.Length - 1] = new OptionPart();
                        Option[Option.Length - 1].SetFrom(a.Substring(nowindex, index - nowindex));
                        nowindex=index;
                        index=OptionIndex(a,index+1);
                    }
                    Array.Resize(ref Option, Option.Length + 1);
                    Option[Option.Length - 1].SetFrom(a.Substring(nowindex));
                }
                public int OptionIndex(string a,int startindex)
                {
                    while (!IsFit(a.Substring(startindex), "(E)") && !IsFit(a.Substring(startindex), "(e)") && startindex < a.Length) startindex++;
                    if (startindex == a.Length) return -1;
                    else return startindex;
                }
                public bool IsToTest()
                {
                    if (Date > DateTime.Now.Date) return false;
                    if (Times == Setting.reviewdays.Length) return false;
                    if (Date < DateTime.Now.Date) EnglishTest.Missed++;
                    return true;
                }
                public override string ToString()
                {
                    string a = "選擇:" + Times.ToString() + "\t" + DateToString(Date) + "\t" + Text;
                    for(int i=0;i<Option.Length;i++)
                    {
                        if (Option[i].IsAns) a += "(" + ((char)('a' + i)).ToString() + ")";
                        else a += "(" + ((char)('A' + i)).ToString() + ")";
                        a += Option[i].Text;
                    }
                    return a;
                }
                public string StringWithoutRecord()
                {
                    string a = Text;
                    int[] order = new int[Option.Length];
                    for (int i = 0; i < order.Length; i++) order[i] = i;
                    for(int i=0;i<order.Length;i++)
                    {
                        for(int j=i+1;j<order.Length;j++)
                        {
                            if(IsLargerString(Option[order[i]].Text,Option[order[j]].Text))
                            {
                                int k = order[i];
                                order[i] = order[j];
                                order[j] = k;
                            }
                        }
                    }
                    for (int i = 0; i < order.Length; i++) a += "(" + ((char)('A' + i)).ToString() + ")" + Option[order[i]].Text;
                    return "選擇:" + a;
                }
                public bool IsLargerString(string a,string b)
                {
                    for(int i=0;i<a.Length&&i<b.Length;i++)
                    {
                        if (a[i] > b[i]) return true;
                        else if (a[i] < b[i]) return false;
                    }
                    if (a.Length > b.Length) return true;
                    else if(a.Length<b.Length) return false;
                    else
                    {
                        MessageBox.Show("EnglishTestTabPart.MultiChoiceTestLine.IsLargerString(string a,string b):\r\nError! The two string are exactly the same!\r\n" + a);
                        return false;
                    }
                }
            }
            public struct OtherTestLine
            {
                public struct DateTimeAnsPair
                {
                    public int Times;
                    public DateTime Date;
                    public string Text;
                    public string Ans;
                    public bool PushDate()
                    {
                        if (Times < Setting.reviewdays.Length)
                        {
                            Date = DateTime.Now.AddDays(Setting.reviewdays[Times]);
                            Times++;
                            return true;
                        }
                        else if (Times == Setting.reviewdays.Length)
                        {
                            Times++;
                            return false;
                        }
                        else throw new ArgumentException("已經經過最後一個複習時間Setting.reviewdays[" + (Setting.reviewdays.Length - 1).ToString() + "]==" + Setting.reviewdays[Setting.reviewdays.Length - 1].ToString());
                    }
                    public void PullDate()
                    {
                        if (Times >= 2) Times -= 2;
                        else if (Times == 1) Times--;
                    }
                    public bool IsToTest()
                    {
                        if (Date > DateTime.Now.Date) return false;
                        if (Times == Setting.reviewdays.Length) return false;
                        if (Date < DateTime.Now.Date) EnglishTest.Missed++;
                        return true;
                    }
                    public override string ToString()
                    {
                        return Text + EnglishTest.FillMark.ToString() + Ans + EnglishTest.FillMark.ToString() + Times.ToString() + "\t" + DateToString(Date) + EnglishTest.FillMark.ToString();
                    }
                }
                public DateTimeAnsPair[] DateAns;
                public string End;
                public int[] ToTest;
                public bool ShowTest(int num)
                {
                    Reform();
                    string a = num.ToString() + ". ";
                    for (int i = 0; i < DateAns.Length;i++ )
                    {
                        a += DateAns[i].Text;
                        if (DateAns[i].IsToTest()) a += "__";
                        else a += DateAns[i].Ans;
                    }
                    a += End + "\r\n";
                    EnglishTest.Txb1.AppendText(a);
                    KeyEventHandler textchangedevent = new KeyEventHandler(OtherTestLine_TextChanged);
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        EnglishTest.Insert[i].KeyUp += textchangedevent;
                        EnglishTest.Insert[i].ImeMode = ImeMode.Off;
                    }
                    int focusindex = 0;
                    DateTime past = DateTime.Now;
                    while(focusindex<ToTest.Length)
                    {
                        EnglishTest.Insert[focusindex].Focus();
                        IsInserted = false;
                        while (!IsInserted) Application.DoEvents();
                        for (int i = 0; i < ChineseTest.Txb2.Length; i++)
                        {
                            if (ChineseTest.Txb2[i].Focused)
                            {
                                focusindex = i;
                                break;
                            }
                        }
                        focusindex++;
                    }
                    a = EnglishTest.ShowReactTime(past);
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        EnglishTest.Insert[i].KeyUp -= textchangedevent;
                        if(EnglishTest.Insert[i].Text==DateAns[ToTest[i]].Ans)
                        {
                            bool Pushed=DateAns[ToTest[i]].PushDate();
                            if (Pushed) a += EnglishTest.ShowTestResult(EnglishTest.Insert[i].Text, DateAns[ToTest[i]].Date);
                            else a += EnglishTest.ShowTestResult(EnglishTest.Insert[i].Text);
                        }
                        else
                        {
                            DateAns[ToTest[i]].PullDate();
                            a += EnglishTest.ShowTestResult(EnglishTest.Insert[i].Text, DateAns[ToTest[i]].Ans);
                        }
                    }
                    EnglishTest.Txb1.AppendText(a);
                    { }
                    return !IsToTest();//Unfinished
                }
                void OtherTestLine_TextChanged(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != EnglishTest.TabIndex) return;
                    if (e.KeyData == Keys.Enter)
                    {
                        ITxtBox a = sender as ITxtBox;
                        if (a.Text.IndexOf("\r\n") != -1)
                        {
                            int b = a.Text.IndexOf("\r\n");
                            a.Text = a.Text.Remove(b) + (b + 2 == a.Text.Length ? "" : a.Text.Substring(b + 2));
                        IsInserted = true;
                        }
                    }
                }
                public void Reform()
                {
                    foreach (Control a in EnglishTest.Tlp1.Controls) if (a != EnglishTest.Txb1) EnglishTest.Tlp1.Controls.Remove(a);
                    TableLayoutPanel tlp = EnglishTest.Tlp2;
                    tlp = new TableLayoutPanel();
                    tlp.Dock = DockStyle.Fill;
                    tlp.ColumnCount = ToTest.Length;
                    tlp.RowCount = 1;
                    Array.Resize(ref EnglishTest.Insert, ToTest.Length);
                    ITxtBox[] txb = EnglishTest.Insert;
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        txb[i] = new ITxtBox();
                        txb[i].Font = InputFont;
                        txb[i].Dock = DockStyle.Fill;
                        txb[i].Multiline = true;
                        tlp.Controls.Add(txb[i]);
                        tlp.SetCellPosition(txb[i], new TableLayoutPanelCellPosition(i, 0));
                        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                    }
                    EnglishTest.Tlp1.Controls.Add(tlp);
                    EnglishTest.Tlp1.SetCellPosition(tlp, new TableLayoutPanelCellPosition(0, 1));
                }
                public void SetFrom(string a)
                {
                    DateAns = new DateTimeAnsPair[0];
                    int nowindex = 0;
                    int index = a.IndexOf(EnglishTest.FillMark);
                    while(index!=-1)
                    {
                        Array.Resize(ref DateAns, DateAns.Length + 1);
                        DateAns[DateAns.Length - 1].Text = a.Substring(nowindex, index - nowindex);
                        nowindex = index + 1;
                        index = a.IndexOf(EnglishTest.FillMark, nowindex);
                        DateAns[DateAns.Length - 1].Ans = a.Substring(nowindex, index - nowindex);
                        nowindex = index + 1;
                        try { index = a.IndexOf(EnglishTest.FillMark, nowindex); }
                        catch (Exception) { index = -1; }
                        if (index == -1 || !IsFit(a.Substring(nowindex, index - nowindex), "n\tnnnn/nn/nn"))
                        {
                            DateAns[DateAns.Length - 1].Times = 0;
                            DateAns[DateAns.Length - 1].Date = DateTime.Now.Date;
                        }
                        else
                        {
                            string[] b = a.Substring(nowindex, index - nowindex).Split('\t');
                            DateAns[DateAns.Length - 1].Times = int.Parse(b[0]);
                            DateAns[DateAns.Length - 1].Date = DateTime.Parse(b[1]);
                            nowindex = index + 1;
                            try { index = a.IndexOf(EnglishTest.FillMark, nowindex); }
                            catch (Exception) { index = -1; }
                        }
                    }
                    End = a.Substring(nowindex);
                }
                public bool IsToTest()
                {
                    ToTest = new int[0];
                    bool totest = false;
                    for (int i = 0; i < DateAns.Length; i++)
                    {
                        if (DateAns[i].IsToTest())
                        {
                            totest = true;
                            Array.Resize(ref ToTest, ToTest.Length + 1);
                            ToTest[ToTest.Length - 1] = i;
                        }
                    }
                    return totest;
                }
                public override string ToString()
                {
                    string a = "填充:";
                    for (int i = 0; i < DateAns.Length; i++) a += DateAns[i].ToString();
                    return a + End;
                }
                public string StringWithoutRecord()
                {
                    string a = "填充:";
                    for(int i=0;i<DateAns.Length;i++) a += DateAns[i].Text + EnglishTest.FillMark.ToString() + DateAns[i].Ans + EnglishTest.FillMark.ToString();
                    return a + End;
                }
            }
            public static char FillMark;
            public static TabPage Tab;
            public static TableLayoutPanel Tlp1;
            public static TableLayoutPanel Tlp2;
            public static ITxtBox Txb1;
            public static ITxtBox Txb2;
            public static ITxtBox[] Insert;
            public static Button[] Btns;
            public static bool IsReplaceNew;
            public static int TabIndex;
            public static int Missed;
            public static int Repeated;
            public static VocabularyTestLine[] Question;
            public static MultiChoiceTestLine[] MultiChoice;
            public static OtherTestLine[] OtherTest;
            static HashSet<int> ToTestE;
            static HashSet<int> ToTestC;
            static HashSet<int> ToTestM;
            static HashSet<int> ToTestO;
            public static Thread[] THREAD;
            public static int OptionSum;
            public static bool NeedSave { get { if (!IsTested)return false; return IsSaved; } }
            public static bool IsSaved;
            public static bool IsTested;
            public static string Path;
            public static string ShowReactTime(DateTime a)
            {
                return "\t" + (DateTime.Now - a).TotalSeconds.ToString("F3") + " s\r\n";
            }
            public static string ShowTestResult(string yourans, string ans)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tWrong!\tAns:\t" + ans + "\r\n\r\n";
            }
            public static string ShowTestResult(string yourans, DateTime nextreviewtime)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tCorrect!\tNext Review Time:\t" + DateToString(nextreviewtime) + "\r\n\r\n";
            }
            public static string ShowTestResult(string yourans)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tCorrect!\tCongratulations! You've finished this question!\r\n\r\n";
            }
            public static void SetTabText(int a)
            {
                if (a == 0) Tab.Text = "English";
                else Tab.Text = "English(" + a.ToString() + ")";
            }
            public static void Reset()
            {
                FillMark='`';
                IsReplaceNew = true;
                Question = new VocabularyTestLine[0];
                IsInserted = false;
                OptionSum = 5;
                IsSaved = false;
                IsTested = false;
                Path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
                THREAD = new Thread[1];
                Tab = new TabPage();
                Tab.Font = TabFont;
                Tlp1 = new TableLayoutPanel(); Tab.Controls.Add(Tlp1);

                Tlp1.ColumnCount = 1;
                Tlp1.RowCount = 2;
                Tlp1.Dock = DockStyle.Fill;
                Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
                Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                Txb1 = new ITxtBox(); Tlp1.Controls.Add(Txb1); Tlp1.SetCellPosition(Txb1, new TableLayoutPanelCellPosition(0, 0));
                {
                    Txb1.Dock = DockStyle.Fill;
                    Txb1.Multiline = true;
                    Txb1.ScrollBars = ScrollBars.Vertical;
                    Txb1.Font = ITxtBoxFont;
                }
            }
            public static void Add()
            {
                All.Tab.TabPages.Add(Tab);
                TabIndex = -1;
                for (int i = 0; i < All.Tab.TabCount; i++)
                {
                    if (All.Tab.TabPages[i] == Tab)
                    {
                        TabIndex = i;
                        break;
                    }
                }
            }
            public static void Load()
            {
                string[] str=new string[0];
                if (new FileInfo(Path + "英文測驗.txt").Exists)
                {
                    StreamReader reader = new StreamReader(Path + "英文測驗.txt", Encoding.Default);
                    string filestr = reader.ReadToEnd();
                    reader.Close();
                    str = filestr.Split('\n');
                }
                else Txb1.Text = "桌面不存在英文測驗.txt\r\n";
                Dictionary<string, int> repeatedQ = new Dictionary<string, int>();
                Dictionary<string, int> repeatedO = new Dictionary<string, int>();
                Dictionary<string, int> repeatedM = new Dictionary<string, int>();
                Question = new VocabularyTestLine[0];
                MultiChoice = new MultiChoiceTestLine[0];
                OtherTest = new OtherTestLine[0];
                Missed = 0;
                Repeated = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    str[i] = str[i].TrimEnd('\r');
                    if (str[i].Length == 0) continue;
                    switch (str[i].Substring(0, 3))
                    {
                        case "填充:":
                            {
                                Array.Resize(ref OtherTest, OtherTest.Length + 1);
                                OtherTest[OtherTest.Length - 1].SetFrom(str[i].Substring(3));
                                if (repeatedO.ContainsKey(OtherTest[OtherTest.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    Array.Resize(ref OtherTest, OtherTest.Length - 1);
                                }
                                else repeatedO.Add(OtherTest[OtherTest.Length - 1].StringWithoutRecord(), OtherTest.Length - 1);
                                break;
                            }
                        case "選擇:":
                            {
                                Array.Resize(ref MultiChoice, MultiChoice.Length + 1);
                                MultiChoice[MultiChoice.Length - 1].SetFrom(str[i].Substring(3));
                                if (repeatedM.ContainsKey(MultiChoice[MultiChoice.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    Array.Resize(ref MultiChoice, MultiChoice.Length - 1);
                                }
                                else repeatedM.Add(MultiChoice[MultiChoice.Length - 1].StringWithoutRecord(), MultiChoice.Length - 1);
                                break;
                            }
                        default:
                            {
                                Array.Resize(ref Question, Question.Length + 1);
                                Question[Question.Length - 1] = new VocabularyTestLine();
                                Question[Question.Length - 1].SetFrom(str[i]);
                                if (repeatedQ.ContainsKey(Question[Question.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    int j = i;
                                    if (IsReplaceNew)
                                    {
                                        j = repeatedQ[Question[Question.Length - 1].StringWithoutRecord()];
                                        Question[j].SetFrom(str[i]);
                                        Array.Resize(ref Question, Question.Length - 1);
                                    }

                                }
                                else repeatedQ.Add(Question[Question.Length - 1].StringWithoutRecord(), i);
                                break;
                            }
                    }
                }
                ToTestE = new HashSet<int>();
                ToTestC = new HashSet<int>();
                ToTestM = new HashSet<int>();
                ToTestO = new HashSet<int>();
                for (int i = 0; i < Question.Length; i++)
                {
                    if (Question[i].IsToTestE()) ToTestE.Add(i);
                    if (Question[i].IsToTestC()) ToTestC.Add(i);
                }
                for (int i = 0; i < MultiChoice.Length; i++) if (MultiChoice[i].IsToTest()) ToTestM.Add(i);
                for (int i = 0; i < OtherTest.Length; i++) if (OtherTest[i].IsToTest()) ToTestO.Add(i);
                SetTabText(ToTestE.Count + ToTestC.Count + ToTestM.Count + ToTestO.Count);
                if (ToTestE.Count + ToTestC.Count + ToTestM.Count + ToTestO.Count == 0)
                {
                    Txb1.AppendText("Finished!");
                    IsSaved = true;
                }
            }
            public static void Save()
            {
                DirectoryInfo drtinfo = new DirectoryInfo(Path + "測驗紀錄備份");
                if (!drtinfo.Exists) drtinfo.Create();

                StreamReader reader = new StreamReader(Path + "英文測驗.txt", Encoding.Default);
                StreamWriter writer = new StreamWriter(Path + "測驗紀錄備份\\英文測驗-歷史紀錄.txt", true, Encoding.UTF8);
                writer.WriteLine(DateTime.Now.ToString());
                writer.WriteLine(reader.ReadToEnd());
                reader.Close();
                writer.Close();

                writer = new StreamWriter(Path + "測驗紀錄備份\\測驗紀錄.txt", true, Encoding.UTF8);
                writer.WriteLine(DateTime.Now.ToString());
                writer.WriteLine(Txb1.Text);
                writer.Close();

                writer = new StreamWriter(Path + "英文測驗.txt", false, Encoding.Default);
                for (int i = 0; i < Question.Length; i++) writer.WriteLine(Question[i].ToString());
                for (int i = 0; i < MultiChoice.Length; i++) writer.WriteLine(MultiChoice[i].ToString());
                for (int i = 0; i < OtherTest.Length; i++) writer.WriteLine(OtherTest[i].ToString());
                writer.Close();
                IsSaved = true;
            }
            public static void Start()
            {
                if(!IsSaved) All.Tab.SelectedIndex = TabIndex;
                string MessageString = "";
                if (Missed > 0) MessageString += "Missed " + Missed.ToString() + " questions!\r\n";
                if (Repeated > 0) MessageString += "Find " + Repeated.ToString() + " Repeats!\r\n";
                if (MessageString.Length > 0) MessageBox.Show(MessageString);
                IsTested = true;
                while (ToTestE.Count + ToTestC.Count + ToTestM.Count + ToTestO.Count > 0)
                {
                    int j = -1, b = ToTestE.Count, c = ToTestC.Count, d = ToTestM.Count, e = ToTestO.Count, num = b + c + d + e;
                    if (Setting.TestOrder.Option[0].Checked && b > 0) j = random.Next(0, b - 1);
                    else if (Setting.TestOrder.Option[1].Checked && c > 0) j = random.Next(b, b + c - 1);
                    else j = random.Next(0, b + c + d + e - 1);
                    if (j < b)
                    {
                        int k = ToTestE.ElementAt(j);
                        bool CanRemove=true;
                        if (Question[k].IsToTestE()) CanRemove = Question[k].ShowTestE(num);
                        if (CanRemove) ToTestE.Remove(k);
                    }
                    else if (j < b + c)
                    {
                        int k = ToTestC.ElementAt(j - b);
                        bool CanRemove = Question[k].ShowTestC(num);
                        if (CanRemove) ToTestC.Remove(k);
                    }
                    else if (j < b + c + d)
                    {
                        int k = ToTestM.ElementAt(j - b - c);
                        bool CanRemove = MultiChoice[k].ShowTest(num);
                        if (CanRemove) ToTestM.Remove(k);
                    }
                    else
                    {
                        int k = ToTestO.ElementAt(j - b - c - d);
                        bool CanRemove = OtherTest[k].ShowTest(num);
                        if (CanRemove) ToTestO.Remove(k);
                    }
                    SetTabText(ToTestE.Count + ToTestC.Count + ToTestM.Count + ToTestO.Count);
                }
                if (!IsSaved)
                {
                    Save();
                    MessageBox.Show("Saved! You can be assured to close the program.");
                }
            }
            public static string[] LookUp(string key, bool IsWord)
            {
                string[] a = new string[0];
                if (IsWord)
                {
                    for (int i = 0; i < Question.Length; i++)
                    {
                        if (Question[i].Word == key)
                        {
                            Array.Resize(ref a, a.Length + 1);
                            a[a.Length - 1] = Question[i].Explanation;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Question.Length; i++)
                    {
                        if (Question[i].Explanation == key)
                        {
                            Array.Resize(ref a, a.Length + 1);
                            a[a.Length - 1] = Question[i].Word;
                        }
                    }
                }
                return a;
            }
            public static void Phonetic_Thread(string word)
            {
                if (THREAD[0] != null) THREAD[0].Abort();
                THREAD[0] = new Thread(() =>
                {
                    EnglishTest.Phonetic(word);
                });
                THREAD[0].IsBackground = true;
                THREAD[0].Start();
            }
            public static void Phonetic(string word)
            {
                string a = "";
                try
                {
                    byte[] b = new WebClient().DownloadData("https://tw.dictionary.yahoo.com/dictionary?p=" + word);
                    a = Encoding.UTF8.GetString(b);
                }
                catch (Exception) { Tab.FindForm().Text = "Internet Unavailable"; }
                bool found = true;
                do
                {
                    int b = a.IndexOf("KK");
                    if (b == -1) { found = false; break; }
                    b = a.IndexOf('[', b);
                    if (b == -1) { found = false; break; }
                    int c = a.IndexOf(']', b);
                    if (c == -1) { found = false; break; }
                    Tab.FindForm().Text = a.Substring(b, c - b + 1);
                }while(false);
                if (!found) Tab.FindForm().Text = "Can't find KK phonetic of \"" + word + "\"";
            }
        }
        public struct ChineseTestTabPart
        {
            public struct OtherTestLine
            {
                public struct DateTimeAnsPair
                {
                    public int Times;
                    public DateTime Date;
                    public string Text;
                    public string Ans;
                    public override string ToString()
                    {
                        return Text + ChineseTest.FillMark.ToString() + Ans + ChineseTest.FillMark.ToString() + Times.ToString() + "\t" + DateToString(Date) + ChineseTest.FillMark.ToString();
                    }
                    public bool IsToTest()
                    {
                        if (Date > DateTime.Now.Date) return false;
                        if (Times == Setting.reviewdays.Length) return false;
                        if (Date < DateTime.Now.Date) ChineseTest.Missed++;
                        return true;
                    }
                    public bool PushDate()
                    {
                        if (Times == Setting.reviewdays.Length) return false;
                        Date = DateTime.Now.Date.AddDays(Setting.reviewdays[Times]);
                        Times++;
                        return true;
                    }
                    public void PullDate()
                    {
                        if (Times == 1) Times--;
                        else if (Times > 1) Times -= 2;
                    }
                }
                public struct ZhuyinPart
                {
                    public char Initial;
                    public char MotherReferred;
                    public char Final;
                    public char Tone;
                    public void SetFrom(string a)
                    {
                        bool[] changed = new bool[4];
                        for(int i=0;i<a.Length;i++)
                        {
                            int j = DetectZhuyin(a[i]);
                            switch(j)
                            {
                                case 1:
                                    Initial = a[i];
                                    changed[0] = true;
                                    break;
                                case 2:
                                    MotherReferred = a[i];
                                    changed[1] = true;
                                    break;
                                case 3:
                                    Final = a[i];
                                    changed[2] = true;
                                    break;
                                case 4:
                                    Tone = a[i];
                                    changed[3] = true;
                                    break;
                            }
                        }
                        if (!changed[0]) Initial = '\0';
                        if (!changed[1]) MotherReferred = '\0';
                        if (!changed[2]) Final = '\0';
                        if (!changed[3]) Tone = ' ';
                    }
                    public override string ToString()
                    {
                        string a = "";
                        if (Initial != '\0') a += Initial.ToString();
                        if (MotherReferred != '\0') a += MotherReferred.ToString();
                        if (Final != '\0') a += Final.ToString();
                        if (Tone != ' ') a += Tone.ToString();
                        return a;
                    }
                    public int DetectZhuyin(char a)
                    {
                        if (a >= 'ㄅ' && a <= 'ㄙ') return 1;
                        else if (a >= 'ㄚ' && a <= 'ㄦ') return 3;
                        else if (a >= 'ㄧ' && a <= 'ㄩ') return 2;
                        else if (a == ' ' || a == 'ˊ' || a == 'ˇ' || a == 'ˋ' || a == '˙') return 4;
                        else return 0;
                    }
                }
                public string Tag;
                public int[] ToTest;
                public ZhuyinPart[] Zhuyin;
                public DateTimeAnsPair[] DateAns;
                public string End;
                public void SetFrom(string a)
                {
                    DateAns = new DateTimeAnsPair[0];
                    int nowindex = 0;
                    int index = a.IndexOf(ChineseTest.FillMark);
                    while (index != -1)
                    {
                        Array.Resize(ref DateAns, DateAns.Length + 1);
                        DateAns[DateAns.Length - 1].Text = a.Substring(nowindex, index - nowindex);
                        nowindex = index + 1;
                        index = a.IndexOf(ChineseTest.FillMark, nowindex);
                        DateAns[DateAns.Length - 1].Ans = a.Substring(nowindex, index - nowindex);
                        nowindex = index + 1;
                        try { index = a.IndexOf(ChineseTest.FillMark, nowindex); }
                        catch (Exception) { index = -1; }
                        if (index == -1 || !IsFit(a.Substring(nowindex, index - nowindex), "n\tnnnn/nn/nn"))
                        {
                            DateAns[DateAns.Length - 1].Times = 0;
                            DateAns[DateAns.Length - 1].Date = DateTime.Now.Date;
                        }
                        else
                        {
                            string[] b = a.Substring(nowindex, index - nowindex).Split('\t');
                            DateAns[DateAns.Length - 1].Times = int.Parse(b[0]);
                            DateAns[DateAns.Length - 1].Date = DateTime.Parse(b[1]);
                            nowindex = index + 1;
                            try { index = a.IndexOf(ChineseTest.FillMark, nowindex); }
                            catch (Exception) { index = -1; }
                        }
                    }
                    End = a.Substring(nowindex);
                }
                public override string ToString()
                {
                    string a = "";
                    for (int i = 0; i < DateAns.Length; i++)
                    {
                        a += DateAns[i].ToString();
                    }
                    a += End;
                    return Tag+a;
                }
                public string StringWithoutRecord()
                {
                    string a = "";
                    for (int i = 0; i < DateAns.Length; i++) a += DateAns[i].Text + ChineseTest.FillMark.ToString() + DateAns[i].Ans + ChineseTest.FillMark.ToString();
                    return Tag + a + End;
                }
                public bool ShowTest(int num)
                {
                    Reform();
                    string a = num.ToString() + ". "+Tag;
                    for (int i = 0; i < DateAns.Length; i++)
                    {
                        a += DateAns[i].Text;
                        if (!DateAns[i].IsToTest()) a += DateAns[i].Ans;
                    }
                    a += End + "\r\n";
                    ChineseTest.Txb1.AppendText(a);
                    if (Tag == "國字:" || Tag == "通同:")
                    {
                        ChineseTest.TabTipProcess.Start();
                        //Unfinished: Scroll Txb1 to the bottom
                    }
                    Zhuyin = new ZhuyinPart[ToTest.Length];
                    KeyEventHandler textchangedevent = new KeyEventHandler(OtherTestLine_TextChanged);
                    KeyEventHandler keyupevent = new KeyEventHandler(InsertZhuyin);
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        ChineseTest.Txb2[i].KeyUp += textchangedevent;
                        if (Tag == "注音:")
                        {
                            Zhuyin[i] = new ZhuyinPart();
                            Zhuyin[i].SetFrom("");
                            ChineseTest.Txb2[i].ImeMode = ImeMode.Off;
                            ChineseTest.Txb2[i].KeyUp += keyupevent;
                        }
                        else
                        {
                            ChineseTest.Txb2[i].ImeMode = ImeMode.On;
                        }
                    }
                    int focusindex = 0;
                    DateTime past = DateTime.Now;
                    while (focusindex < ToTest.Length)
                    {
                        ChineseTest.Txb2[focusindex].Focus();
                        IsInserted = false;
                        while (!IsInserted) Application.DoEvents();
                        for (int i = 0; i < ChineseTest.Txb2.Length;i++ )
                        {
                            if(ChineseTest.Txb2[i].Focused)
                            {
                                focusindex = i;
                                break;
                            }
                        }
                        focusindex++;
                    }
                    a = ChineseTest.ShowReactTime(past);
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        ChineseTest.Txb2[i].KeyUp -= textchangedevent;
                        ChineseTest.Txb2[i].KeyUp -= keyupevent;
                        if (ChineseTest.Txb2[i].Text == DateAns[ToTest[i]].Ans)
                        {
                            bool Pushed = DateAns[ToTest[i]].PushDate();
                            if (Pushed) a += ChineseTest.ShowTestResult(ChineseTest.Txb2[i].Text, DateAns[ToTest[i]].Date);
                            else a += ChineseTest.ShowTestResult(ChineseTest.Txb2[i].Text);
                        }
                        else
                        {
                            DateAns[ToTest[i]].PullDate();
                            a += ChineseTest.ShowTestResult(ChineseTest.Txb2[i].Text, DateAns[ToTest[i]].Ans);
                        }
                    }
                    ChineseTest.Txb1.AppendText(a);
                    { }
                    return !IsToTest();
                }
                public void Reform()
                {
                    foreach (Control a in ChineseTest.Tlp1.Controls) if (a != ChineseTest.Txb1) ChineseTest.Tlp1.Controls.Remove(a);
                    TableLayoutPanel tlp = ChineseTest.Tlp2;
                    tlp = new TableLayoutPanel();
                    tlp.Dock = DockStyle.Fill;
                    tlp.ColumnCount = ToTest.Length;
                    tlp.RowCount = 1;
                    Array.Resize(ref ChineseTest.Txb2, ToTest.Length);
                    ITxtBox[] txb = ChineseTest.Txb2;
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        txb[i] = new ITxtBox();
                        txb[i].Dock = DockStyle.Fill;
                        txb[i].Multiline = true;
                        tlp.Controls.Add(txb[i]);
                        tlp.SetCellPosition(txb[i], new TableLayoutPanelCellPosition(i, 0));
                        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                    }
                    ChineseTest.Tlp1.Controls.Add(tlp);
                    ChineseTest.Tlp1.SetCellPosition(tlp, new TableLayoutPanelCellPosition(0, 1));
                }
                public bool IsToTest()
                {
                    bool totest = false;
                    ToTest = new int[0];
                    for(int i=0;i<DateAns.Length;i++)
                    {
                        if(DateAns[i].IsToTest())
                        {
                            totest = true;
                            Array.Resize(ref ToTest, ToTest.Length + 1);
                            ToTest[ToTest.Length - 1] = i;
                        }
                    }
                    return totest;
                }
                public void InsertZhuyin(object sender, KeyEventArgs e)
                {
                    int a = ChineseTest.Tlp2.GetCellPosition(sender as ITxtBox).Column;
                    switch(e.KeyData)
                    {
                        case Keys.D1:
                            if (Zhuyin[a].Initial == 'ㄅ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄅ';
                            break;
                        case Keys.Q:
                            if (Zhuyin[a].Initial == 'ㄆ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄆ';
                            break;
                        case Keys.A:
                            if (Zhuyin[a].Initial == 'ㄇ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄇ';
                            break;
                        case Keys.Z:
                            if (Zhuyin[a].Initial == 'ㄈ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄈ';
                            break;
                        case Keys.D2:
                            if (Zhuyin[a].Initial == 'ㄉ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄉ';
                            break;
                        case Keys.W:
                            if (Zhuyin[a].Initial == 'ㄊ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄊ';
                            break;
                        case Keys.S:
                            if (Zhuyin[a].Initial == 'ㄋ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄋ';
                            break;
                        case Keys.X:
                            if (Zhuyin[a].Initial == 'ㄌ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄌ';
                            break;
                        case Keys.D3:
                            Zhuyin[a].Tone = 'ˇ';
                            IsInserted = true;
                            break;
                        case Keys.E:
                            if (Zhuyin[a].Initial == 'ㄍ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄍ';
                            break;
                        case Keys.D:
                            if (Zhuyin[a].Initial == 'ㄎ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄎ';
                            break;
                        case Keys.C:
                            if (Zhuyin[a].Initial == 'ㄏ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄏ';
                            break;
                        case Keys.D4:
                            Zhuyin[a].Tone = 'ˋ';
                            IsInserted = true;
                            break;
                        case Keys.R:
                            if (Zhuyin[a].Initial == 'ㄐ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄐ';
                            break;
                        case Keys.F:
                            if (Zhuyin[a].Initial == 'ㄑ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄑ';
                            break;
                        case Keys.V:
                            if (Zhuyin[a].Initial == 'ㄒ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄒ';
                            break;
                        case Keys.D5:
                            if (Zhuyin[a].Initial == 'ㄓ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄓ';
                            break;
                        case Keys.T:
                            if (Zhuyin[a].Initial == 'ㄔ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄔ';
                            break;
                        case Keys.G:
                            if (Zhuyin[a].Initial == 'ㄕ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄕ';
                            break;
                        case Keys.B:
                            if (Zhuyin[a].Initial == 'ㄖ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄖ';
                            break;
                        case Keys.D6:
                            Zhuyin[a].Tone = 'ˊ';
                            IsInserted = true;
                            break;
                        case Keys.Y:
                            if (Zhuyin[a].Initial == 'ㄗ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄗ';
                            break;
                        case Keys.H:
                            if (Zhuyin[a].Initial == 'ㄘ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄘ';
                            break;
                        case Keys.N:
                            if (Zhuyin[a].Initial == 'ㄙ') Zhuyin[a].Initial = '\0';
                            else Zhuyin[a].Initial = 'ㄙ';
                            break;
                        case Keys.D7:
                            Zhuyin[a].Tone = '˙';
                            IsInserted = true;
                            break;
                        case Keys.U:
                            if (Zhuyin[a].MotherReferred == 'ㄧ') Zhuyin[a].MotherReferred = '\0';
                            else Zhuyin[a].MotherReferred='ㄧ';
                            break;
                        case Keys.J:
                            if (Zhuyin[a].MotherReferred == 'ㄨ') Zhuyin[a].MotherReferred = '\0';
                            else Zhuyin[a].MotherReferred = 'ㄨ';
                            break;
                        case Keys.M:
                            if (Zhuyin[a].MotherReferred == 'ㄩ') Zhuyin[a].MotherReferred = '\0';
                            else Zhuyin[a].MotherReferred = 'ㄩ';
                            break;
                        case Keys.D8:
                            if (Zhuyin[a].Final == 'ㄚ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄚ';
                            break;
                        case Keys.I:
                            if (Zhuyin[a].Final == 'ㄛ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄛ';
                            break;
                        case Keys.K:
                            if (Zhuyin[a].Final == 'ㄜ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄜ';
                            break;
                        case Keys.Oemcomma:
                            if (Zhuyin[a].Final == 'ㄝ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄝ';
                            break;
                        case Keys.D9:
                            if (Zhuyin[a].Final == 'ㄞ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄞ';
                            break;
                        case Keys.O:
                            if (Zhuyin[a].Final == 'ㄟ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄟ';
                            break;
                        case Keys.L:
                            if (Zhuyin[a].Final == 'ㄠ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄠ';
                            break;
                        case Keys.OemPeriod:
                            if (Zhuyin[a].Final == 'ㄡ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄡ';
                            break;
                        case Keys.D0:
                            if (Zhuyin[a].Final == 'ㄢ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄢ';
                            break;
                        case Keys.P:
                            if (Zhuyin[a].Final == 'ㄣ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄣ';
                            break;
                        case Keys.OemSemicolon:
                            if (Zhuyin[a].Final == 'ㄤ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄤ';
                            break;
                        case Keys.OemQuestion:
                            if (Zhuyin[a].Final == 'ㄥ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄥ';
                            break;
                        case Keys.OemMinus:
                            if (Zhuyin[a].Final == 'ㄦ') Zhuyin[a].Final = '\0';
                            else Zhuyin[a].Final = 'ㄦ';
                            break;
                        case Keys.Space:
                            Zhuyin[a].Tone = ' ';
                            IsInserted = true;
                            break;
                        default:
                            Zhuyin[a].SetFrom((sender as ITxtBox).Text);
                            break;
                    }
                    (sender as ITxtBox).Text = Zhuyin[a].ToString();
                }
                void OtherTestLine_TextChanged(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex !=ChineseTest.TabIndex) return;
                    if (e.KeyData == Keys.Enter)
                    {
                        ITxtBox a = sender as ITxtBox;
                        int b = a.Text.IndexOf("\r\n");
                        if (b != -1)
                        {
                            a.Text = a.Text.Remove(b) + a.Text.Substring(b + 2);
                        IsInserted = true;
                        }
                    }
                }
            }
            public struct MultiChoiceTestLine
            {
                public struct OptionPart
                {
                    public bool IsAns;
                    public string Text;
                    public bool Checked;
                }
                public DateTime Date;
                public int Times;
                public bool[] Checked;
                public string Text;
                public OptionPart[] Option;
                public Color[] NormalColor;
                public int OptionIndex(string a,int index)
                {
                    for(int i=index;i<a.Length;i++)
                    {
                        if (IsFit(a.Substring(i), "(E)") || IsFit(a.Substring(i), "(e)")) return i;
                    }
                    return a.Length;
                }
                public void SetFrom(string a)
                {
                    if (IsFit(a, "n nnnn/nn/nn\t"))
                    {
                        DateTime date = DateTime.Parse(a.Substring(2, 10));
                        a = a.Substring(0, 1) + "\t" + DateToString(date) + "\t" + a.Substring(13);
                        //MessageBox.Show(a);
                    }
                    else if(!IsFit(a,"n\tnnnn/nn/nn\t")) a = "0\t" + DateToString(DateTime.Now) + "\t" + a;
                    int b = a.IndexOf("\t");
                    Times = int.Parse(a.Remove(b));
                    a = a.Substring(b + 1);
                    b = a.IndexOf("\t");
                    Date = DateTime.Parse(a.Remove(b));
                    a = a.Substring(b + 1);
                    Option = new OptionPart[0];
                    b = OptionIndex(a,0);
                    Text = a.Remove(b);
                    for(int i=b;i<a.Length;)
                    {
                        if(IsFit(a.Substring(i),"(E)"))
                        {
                            int j = OptionIndex(a, i + 1);
                            Array.Resize(ref Option, Option.Length + 1);
                            Option[Option.Length - 1].Text = a.Substring(i+3, j - i-3);
                            Option[Option.Length - 1].IsAns = false;
                            Option[Option.Length - 1].Checked = false;
                            i = j;
                        }
                        else if(IsFit(a.Substring(i),"(e)"))
                        {
                            int j = OptionIndex(a, i + 1);
                            Array.Resize(ref Option, Option.Length + 1);
                            Option[Option.Length - 1].Text = a.Substring(i+3, j - i-3);
                            Option[Option.Length - 1].IsAns = true;
                            Option[Option.Length - 1].Checked = false;
                            i = j;
                        }
                    }
                    //MessageBox.Show(this.ToString());
                }
                public override string ToString()
                {
                    string a = Times.ToString() + "\t" + DateToString(Date) + "\t" + Text;
                    for(int i=0;i<Option.Length;i++)
                    {
                        if (Option[i].IsAns) a += "(" + ((char)('a' + i)).ToString() + ")";
                        else a += "(" + ((char)('A' + i)).ToString() + ")";
                        a += Option[i].Text;
                    }
                    return "選擇:" + a;
                }
                public string StringWithoutRecord()
                {
                    string a = Text;
                    int[] order = new int[Option.Length];
                    for (int i = 0; i < order.Length; i++) order[i] = i;
                    for(int i=0;i<order.Length;i++)
                    {
                        for(int j=i+1;j<order.Length;j++)
                        {
                            if (IsLargerString(Option[order[j]].Text, Option[order[i]].Text))
                            {
                                int k = order[i];
                                order[i] = order[j];
                                order[j] = k;
                            }
                        }
                    }
                    for (int i = 0; i < order.Length; i++) a += ((char)('A' + i)).ToString() + Option[order[i]].Text;
                    return "選擇:" + a;
                }
                public bool IsLargerString(string a,string b)
                {
                    for(int i=0;i<a.Length&&i<b.Length;i++)
                    {
                        if (a[i] > b[i]) return true;
                        else if (a[i] < b[i]) return false;
                    }
                    if (a.Length > b.Length) return true;
                    else if(a.Length<b.Length) return false;
                    else
                    {
                        MessageBox.Show("ChineseTestTabPart.MultiChoiceTestLine.IsLargerString(string a,string b):\r\nError! The two string are exactly the same!\r\n" + a);
                        return false;
                    }
                }
                public bool IsToTest()
                {
                    if (Date > DateTime.Now.Date) return false;
                    if (Times == Setting.reviewdays.Length) return false;
                    if (Date < DateTime.Now.Date) ChineseTest.Missed++;
                    return true;
                }
                public bool ShowTest(int num)
                {
                    MessOrder();
                    string a = num.ToString() + ". 選擇:" + Text + "\r\n";
                    Reform(Option.Length);
                    IsInserted = false;
                    ShowChecked();
                    for (int i = 0; i < Option.Length;i++ ) a += "(" + ((char)('A' + i)).ToString() + ")"+Option[i].Text;
                    ChineseTest.Txb1.AppendText(a + "\r\n");

                    KeyEventHandler keyupevent = new KeyEventHandler(MultiChoiceTestLine_KeyUp);
                    EventHandler clickevent = new EventHandler(MultiChoiceTestLine_Click);
                    EventHandler doubleclickevent = new EventHandler(Txb1_DoubleClick);
                    ChineseTest.Txb1.DoubleClick += Txb1_DoubleClick;
                    ChineseTest.Tab.FindForm().KeyUp += keyupevent;
                    for (int i = 0; i < Option.Length; i++)
                    {
                        ChineseTest.Btns[i].Text = ((char)('A' + i)).ToString();
                        ChineseTest.Btns[i].Click += clickevent;
                    }
                    DateTime past = DateTime.Now;
                    IsInserted = false;
                    while (!IsInserted) Application.DoEvents();
                    ChineseTest.Txb1.DoubleClick -= doubleclickevent;
                    ChineseTest.Tab.FindForm().KeyUp -= keyupevent;
                    a = ChineseTest.ShowReactTime(past);
                    string ans = "",yourans="";
                    bool CanRemove = true;
                    for (int i = 0; i < Option.Length; i++)
                    {
                        ChineseTest.Btns[i].Click -= clickevent;
                        if (Option[i].IsAns) ans += ((char)('A' + i)).ToString();
                        if (Option[i].Checked) yourans += ((char)('A' + i)).ToString();
                        if (Option[i].Checked != Option[i].IsAns) CanRemove = false;
                    }
                    if (CanRemove)
                    {
                        bool Pushed = PushDate();
                        if (Pushed) a += ChineseTest.ShowTestResult(yourans, Date);
                        else a += ChineseTest.ShowTestResult(yourans);
                    }
                    else
                    {
                        PullDate();
                        a += ChineseTest.ShowTestResult(yourans, ans);
                    }
                    ChineseTest.Txb1.AppendText(a);
                    { }
                    return CanRemove;
                }
                public void Reform(int a)
                {
                    foreach (Control b in ChineseTest.Tlp1.Controls) if (b != ChineseTest.Txb1) ChineseTest.Tlp1.Controls.Remove(b);
                    ChineseTest.Tlp2.Controls.Clear();
                    ChineseTest.Tlp2 = new TableLayoutPanel();
                    ChineseTest.Tlp2.Dock = DockStyle.Fill;
                    ChineseTest.Tlp2.ColumnCount = a;
                    Array.Resize(ref ChineseTest.Btns, a);
                    for (int i = 0; i < a; i++)
                    {
                        ChineseTest.Btns[i] = new Button();
                        ChineseTest.Btns[i].Dock = DockStyle.Fill;
                        ChineseTest.Tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        ChineseTest.Tlp2.Controls.Add(ChineseTest.Btns[i]);
                        ChineseTest.Tlp2.SetCellPosition(ChineseTest.Btns[i], new TableLayoutPanelCellPosition(i, 0));
                    }
                    NormalColor = new Color[] { ChineseTest.Btns[0].BackColor, ChineseTest.Btns[0].ForeColor };
                    ChineseTest.Tlp1.Controls.Add(ChineseTest.Tlp2);
                }
                void Txb1_DoubleClick(object sender, EventArgs e)
                {
                    IsInserted = true;
                }
                public void MessOrder()
                {
                    OptionPart[] tmpOption = new OptionPart[Option.Length];
                    bool[] visited = new bool[Option.Length];
                    for(int i=Option.Length;i>0;i--)
                    {
                        int j = random.Next(0, i);
                        for(int k=0;;k++)
                        {
                            if(!visited[k])
                            {
                                if(j==0)
                                {
                                    tmpOption[k] = Option[i - 1];
                                    visited[k] = true;
                                    break;
                                }
                                j--;
                            }
                        }
                    }
                    Option = tmpOption;
                }
                public void MultiChoiceTestLine_KeyUp(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != ChineseTest.TabIndex) return;
                    string a = e.KeyData.ToString();
                    if (a.Length == 1 && a[0] >= 'A' && a[0] - 'A' < Option.Length)
                    {
                        MultiChoiceTestLine_Click(a[0] - 'A', null);
                    }
                    else if(e.KeyData==Keys.Enter)
                    {
                        IsInserted = true;
                    }
                }
                public bool PushDate()
                {
                    if (Times == Setting.reviewdays.Length) return false;
                    Date = DateTime.Now.Date.AddDays(Setting.reviewdays[Times]);
                    Times++;
                    return true;
                }
                public void PullDate()
                {
                    if (Times == 1) Times--;
                    else if (Times > 1) Times -= 2;
                }
                public void ShowChecked()
                {
                    for(int i=0;i<Option.Length;i++)
                    {
                        if(Option[i].Checked&&(ChineseTest.Btns[i].BackColor!=ChineseTest.CheckedColor[0]||ChineseTest.Btns[i].ForeColor!=ChineseTest.CheckedColor[1]))
                        {
                            ChineseTest.Btns[i].BackColor = ChineseTest.CheckedColor[0];
                            ChineseTest.Btns[i].ForeColor = ChineseTest.CheckedColor[1];
                        }
                        else if(!Option[i].Checked&&(ChineseTest.Btns[i].BackColor!=NormalColor[0]||ChineseTest.Btns[i].ForeColor!=NormalColor[1]))
                        {
                            ChineseTest.Btns[i].BackColor = NormalColor[0];
                            ChineseTest.Btns[i].ForeColor = NormalColor[1];
                        }
                    }
                }
                public void MultiChoiceTestLine_Click(object sender, EventArgs e)
                {
                    if (All.Tab.SelectedIndex != ChineseTest.TabIndex) return;
                    int k = -1;
                    if (sender.GetType() == typeof(int)) k = (int)sender;
                    else k = ChineseTest.Tlp2.GetCellPosition(sender as Button).Column;
                    if (Option[k].Checked) Option[k].Checked = false;
                    else Option[k].Checked = true;
                    ShowChecked();
                }
            }
            public partial class FittingTestForm : Form
            {
                public partial class MatchPart:RadioButton
                {

                }
                public partial class OptionPart:Button
                {
                    public string Ans;
                    public int Times;
                    public DateTime Date;
                }
                public TableLayoutPanel tlp1 = new TableLayoutPanel();
                public TableLayoutPanel tlp2 = new TableLayoutPanel();
                public TableLayoutPanel tlp3 = new TableLayoutPanel();
                public Panel left = new Panel();
                public Panel right = new Panel();
                public Label Head = new Label();
                public Button OK = new Button();
                public Button GiveUp = new Button();
                public MatchPart[] Match;
                public GroupBox[] Group;
                public OptionPart[] Option;
                public FittingTestForm()
                {
                    this.Visible = false;
                    this.WindowState = FormWindowState.Maximized;
                    this.Controls.Add(tlp1);
                    tlp1.Dock = DockStyle.Fill;
                    tlp1.RowCount = 3;
                    tlp1.ColumnCount = 1;
                    tlp1.RowStyles.Add(new RowStyle(SizeType.AutoSize, 1));
                    tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                    tlp1.RowStyles.Add(new RowStyle(SizeType.AutoSize, 1));
                    tlp1.Controls.AddRange(new Control[] { Head, tlp2, tlp3 });
                    tlp1.SetCellPosition(Head, new TableLayoutPanelCellPosition(0, 0));
                    {
                        Head.Dock = DockStyle.Fill;
                    }
                    tlp1.SetCellPosition(tlp2, new TableLayoutPanelCellPosition(0, 1));
                    {
                        tlp2.Dock = DockStyle.Fill;
                        tlp2.RowCount = 1;
                        tlp2.ColumnCount = 2;
                        tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        tlp2.Controls.AddRange(new Control[] { left, right });
                        tlp2.SetCellPosition(left, new TableLayoutPanelCellPosition(0, 0));
                        tlp2.SetCellPosition(right, new TableLayoutPanelCellPosition(1, 0));
                        left.Dock = DockStyle.Fill;
                        left.VerticalScroll.Enabled = true;
                        left.HorizontalScroll.Enabled = true;
                        right.Dock = DockStyle.Fill;
                        right.VerticalScroll.Enabled = true;
                        left.HorizontalScroll.Enabled = true;
                    }
                    tlp1.SetCellPosition(tlp3, new TableLayoutPanelCellPosition(0, 2));
                    {
                        tlp3.Dock = DockStyle.Fill;
                        tlp3.RowCount = 1;
                        tlp3.ColumnCount = 2;
                        tlp3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        tlp3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        tlp3.Controls.AddRange(new Control[] { OK, GiveUp });
                        tlp3.SetCellPosition(OK, new TableLayoutPanelCellPosition(0, 0));
                        {
                            OK.Dock = DockStyle.Fill;
                            OK.Text = "OK";
                        }
                        tlp3.SetCellPosition(GiveUp, new TableLayoutPanelCellPosition(1, 0));
                        {
                            GiveUp.Dock = DockStyle.Fill;
                            GiveUp.Text = "Give Up";
                        }
                    }
                }
                public void SetFrom(string a)//Text`Match`...`Match`Match``1st Option`1st Option``2st Option`...``last Option`last Option
                {
                    int index = a.IndexOf(ChineseTest.FillMark);
                    Head.Text = a.Remove(index);
                    int apart = a.IndexOf(ChineseTest.FillMark.ToString() + ChineseTest.FillMark.ToString());
                    int nowindex = index;
                    index = a.IndexOf(ChineseTest.FillMark, index + 1);
                    Match = new MatchPart[0];
                    Group = new GroupBox[0];
                    while (index != -1 && index < apart)
                    {
                        Array.Resize(ref Match, Match.Length + 1);
                        Array.Resize(ref Group, Group.Length + 1);
                        Match[Match.Length - 1] = new MatchPart();
                        Group[Group.Length - 1] = new GroupBox();
                        left.Controls.AddRange(new Control[] { Match[Match.Length - 1], Group[Group.Length - 1] });
                        Match[Match.Length - 1].Dock = DockStyle.Top;
                        Match[Match.Length - 1].BringToFront();
                        Group[Group.Length - 1].Dock = DockStyle.Top;
                        Group[Group.Length - 1].BringToFront();
                        Match[Match.Length - 1].Text = a.Substring(nowindex, index - nowindex);
                        nowindex = index;
                        index = a.IndexOf(ChineseTest.FillMark);
                    }
                    int nowapart = apart;
                    apart = a.IndexOf(ChineseTest.FillMark.ToString() + ChineseTest.FillMark.ToString());
                    Option = new OptionPart[0];
                    int c = 0;
                    string b = "";
                    while (apart != -1)
                    {
                        b = a.Substring(nowapart, apart - nowapart).Trim(ChineseTest.FillMark);
                        nowindex = 0;
                        index = b.IndexOf(ChineseTest.FillMark);
                        while (index != -1)
                        {
                            Array.Resize(ref Option, Option.Length + 1);
                            Option[Option.Length - 1] = new OptionPart();
                            right.Controls.Add(Option[Option.Length - 1]);
                            Option[Option.Length - 1].Dock = DockStyle.Top;
                            Option[Option.Length - 1].Ans = Match[c].Text;
                            Option[Option.Length - 1].Text = b.Substring(nowindex, index - nowindex);
                            nowindex = index;
                            index = (index + 1 < b.Length ? b.IndexOf(ChineseTest.FillMark, index + 1) : -1);
                            if (index == -1 || !IsFit(b.Substring(nowindex), "n\tnnnn/nn/nn"))
                            {
                                Option[Option.Length - 1].Times = 0;
                                Option[Option.Length - 1].Date = DateTime.Now.Date;
                            }
                            else
                            {
                                string d = b.Substring(nowindex, index - nowindex);
                                Option[Option.Length - 1].Times = int.Parse(d.Remove(1));
                                Option[Option.Length - 1].Date = DateTime.Parse(d.Substring(2));
                                nowindex = index;
                                index = (index + 1 < b.Length ? b.IndexOf(ChineseTest.FillMark, index + 1) : -1);
                            }
                        }
                        nowapart = apart;
                        apart = a.IndexOf(ChineseTest.FillMark.ToString() + ChineseTest.FillMark.ToString());
                        c++;
                    }
                    b = a.Substring(nowapart).Trim(ChineseTest.FillMark);
                    nowindex = 0;
                    index = b.IndexOf(ChineseTest.FillMark);
                    while (index != -1)
                    {
                        Array.Resize(ref Option, Option.Length + 1);
                        Option[Option.Length - 1] = new OptionPart();
                        Option[Option.Length - 1].Ans = Match[c].Text;
                        Option[Option.Length - 1].Text = b.Substring(nowindex, index - nowindex);
                        nowindex = index;
                        index = (index + 1 < b.Length ? b.IndexOf(ChineseTest.FillMark, index + 1) : -1);
                        if (index == -1 || !IsFit(b.Substring(nowindex), "n\tnnnn/nn/nn"))
                        {
                            Option[Option.Length - 1].Times = 0;
                            Option[Option.Length - 1].Date = DateTime.Now.Date;
                        }
                        else
                        {
                            string d = b.Substring(nowindex, index - nowindex);
                            Option[Option.Length - 1].Times = int.Parse(d.Remove(1));
                            Option[Option.Length - 1].Date = DateTime.Parse(d.Substring(2));
                            nowindex = index;
                            index = (index + 1 < b.Length ? b.IndexOf(ChineseTest.FillMark, index + 1) : -1);
                        }
                    }
                    nowapart = apart;
                    apart = a.IndexOf(ChineseTest.FillMark.ToString() + ChineseTest.FillMark.ToString());
                }
                public void ShowTest(int num)
                {
                    this.Show();
                    this.Close();
                }
            }
            public TableLayoutPanel Tlp1;
            public TableLayoutPanel Tlp2;
            public ITxtBox Txb1;
            public ITxtBox[] Txb2;
            public Button[] Btns;
            public char FillMark;
            public string[] QA;
            public string Path;
            public bool IsSaved;
            public int TabIndex;
            public int Missed;
            public int Repeated;
            public Process TabTipProcess;
            public TabPage Tab;
            public Color[] CheckedColor;
            public OtherTestLine[] OtherTest;
            public MultiChoiceTestLine[] MultiChoice;
            public HashSet<int> OtherToTest;
            public HashSet<int> MultiToTest;
            public string ShowReactTime(DateTime a)
            {
                return "\t" + (DateTime.Now - a).TotalSeconds.ToString("F3") + " s\r\n";
            }
            public string ShowTestResult(string yourans, string ans)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tWrong!\tAns:\t" + ans + "\r\n\r\n";
            }
            public string ShowTestResult(string yourans, DateTime nextreviewtime)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tCorrect!\tNext Review Time:\t" + DateToString(nextreviewtime) + "\r\n\r\n";
            }
            public string ShowTestResult(string yourans)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tCorrect!\tCongratulations! You've finished this question!\r\n\r\n";
            }
            public void SetTabText(int a)
            {
                if (a == 0) Tab.Text = "國文";
                else Tab.Text = "國文(" + a.ToString() + ")";
            }
            public void Reset()
            {
                CheckedColor = new Color[] { Color.FromArgb(0, 0, 255), Color.FromArgb(255, 255, 255) };
                TabTipProcess = new Process();
                TabTipProcess.StartInfo.FileName = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
                IsSaved = false;
                FillMark = '\'';
                Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\國文測驗.txt";
                OtherTest = new OtherTestLine[0];
                MultiChoice = new MultiChoiceTestLine[0];
                Tab = new TabPage();
                Tlp1 = new TableLayoutPanel();
                Tlp2 = new TableLayoutPanel();
                Txb1 = new ITxtBox();
                Txb2 = new ITxtBox[0];
                Btns = new Button[0];

                Tab.Controls.Add(Tlp1); Tlp1.Dock = DockStyle.Fill;
                Tlp1.ColumnCount = 1; Tlp1.RowCount = 2;
                Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 80)); Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                Tlp1.Controls.Add(Txb1); Tlp1.SetCellPosition(Txb1, new TableLayoutPanelCellPosition(0, 0));
                {
                    Txb1.Dock = DockStyle.Fill;
                    Txb1.Multiline = true;
                    Txb1.ScrollBars = ScrollBars.Vertical;
                    Txb1.Font = ITxtBoxFont;
                }
                Tlp1.Controls.Add(Tlp2); Tlp1.SetCellPosition(Tlp2, new TableLayoutPanelCellPosition(0, 1));
                {
                    Tlp2.Dock = DockStyle.Fill;
                    Tlp2.RowCount = 1;
                }
            }
            public void Add()
            {
                All.Tab.TabPages.Add(Tab);
                TabIndex = -1;
                for (int i = 0; i < All.Tab.TabCount; i++)
                {
                    if (All.Tab.TabPages[i] == Tab)
                    {
                        TabIndex = i;
                        break;
                    }
                }
            }
            public void Start()
            {
                if(!IsSaved) All.Tab.SelectedIndex = TabIndex;
                string MessageString = "";
                if (Missed > 0) MessageString += "Missed " + Missed.ToString() + " questions!\r\n";
                if (Repeated > 0) MessageString += "Find " + Repeated.ToString() + " repeats!\r\n";
                if (MessageString.Length > 0) MessageBox.Show(MessageString);
                while(OtherToTest.Count()+MultiToTest.Count>0)
                {
                    int i = -1, b = OtherToTest.Count(), c = MultiToTest.Count();
                    i = random.Next(0, b + c - 1);
                    if(i<OtherToTest.Count())
                    {
                        int j = OtherToTest.ElementAt(i);
                        bool CanRemove = OtherTest[j].ShowTest(b + c);
                        if (CanRemove) OtherToTest.Remove(j);
                    }
                    else
                    {
                        int j = MultiToTest.ElementAt(i - b);
                        bool CanRemove = MultiChoice[j].ShowTest(b + c);
                        if (CanRemove) MultiToTest.Remove(j);
                    }
                    SetTabText(OtherToTest.Count() + MultiToTest.Count);
                }
                if (!IsSaved)
                {
                    Save();
                    MessageBox.Show("Saved! You can be assured to close the program.");
                }
            }
            public void Load()
            {
                string[] filestr = new string[0];
                if (new FileInfo(Path).Exists)
                {
                    StreamReader reader = new StreamReader(Path, Encoding.Default);
                    filestr = reader.ReadToEnd().Replace("\r\n", "\r").Split('\r');
                }
                else Txb1.Text = "桌面不存在國文測驗.txt\r\n";
                Dictionary<string, int> repeatedO = new Dictionary<string, int>();
                Dictionary<string, int> repeatedM = new Dictionary<string, int>();
                Repeated = 0;
                Missed = 0;
                for(int i=0;i<filestr.Length;i++)
                {
                    if (filestr[i].Length == 0) continue;
                    switch (filestr[i].Substring(0, 3))
                    {
                        case "注音:":
                        case "國字:":
                        case "常識:":
                        case "解釋:":
                        case "成語:":
                        case "作者:":
                        case "季節:":
                        case "通同:":
                        case "借代:":
                            {
                                Array.Resize(ref OtherTest, OtherTest.Length + 1);
                                OtherTest[OtherTest.Length - 1].Tag = filestr[i].Substring(0, 3);
                                OtherTest[OtherTest.Length - 1].SetFrom(filestr[i].Substring(3));
                                if (repeatedO.ContainsKey(OtherTest[OtherTest.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    Array.Resize(ref OtherTest, OtherTest.Length - 1);
                                }
                                else repeatedO.Add(OtherTest[OtherTest.Length - 1].StringWithoutRecord(), OtherTest.Length - 1);
                                break;
                            }
                        case "選擇:":
                            {
                                Array.Resize(ref MultiChoice, MultiChoice.Length + 1);
                                MultiChoice[MultiChoice.Length - 1].SetFrom(filestr[i].Substring(3));
                                if (repeatedM.ContainsKey(MultiChoice[MultiChoice.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    Array.Resize(ref MultiChoice, MultiChoice.Length - 1);
                                }
                                else repeatedM.Add(MultiChoice[MultiChoice.Length - 1].StringWithoutRecord(), MultiChoice.Length - 1);
                                break;
                            }
                        default:
                            {
                                MessageBox.Show(filestr[i] + "\r\nThe Tag is:\"" + filestr[i].Substring(0, 3) + "\"");
                                break;
                            }
                    }
                }
                IsSaved = false;
                OtherToTest = new HashSet<int>();
                MultiToTest = new HashSet<int>();
                Missed = 0;
                for (int i = 0; i < OtherTest.Length; i++) if (OtherTest[i].IsToTest()) OtherToTest.Add(i);
                for (int i = 0; i < MultiChoice.Length; i++) if (MultiChoice[i].IsToTest()) MultiToTest.Add(i);
                SetTabText(OtherToTest.Count + MultiToTest.Count);
                if (OtherToTest.Count + MultiToTest.Count == 0)
                {
                    Txb1.AppendText("Finished!");
                    IsSaved = true;
                }
            }
            public void Save()
            {
                StreamWriter writer = new StreamWriter(Path, false, Encoding.Default);
                for (int i = 0; i < MultiChoice.Length; i++) writer.WriteLine(MultiChoice[i].ToString());
                for (int i = 0; i < OtherTest.Length; i++) writer.WriteLine(OtherTest[i].ToString());
                writer.Close();
                IsSaved = true;
            }
        }
        public struct ChemistryTestTabPart
        {
            public struct OtherTestLine
            {
                public struct DateTimeAnsPair
                {
                    public int Times;
                    public DateTime Date;
                    public string Text;
                    public string Ans;
                    public override string ToString()
                    {
                        return Text + ChemistryTest.FillMark.ToString() + Ans + ChemistryTest.FillMark.ToString() + Times.ToString() + "\t" + DateToString(Date) + ChemistryTest.FillMark.ToString();
                    }
                    public bool IsToTest()
                    {
                        if (Date > DateTime.Now.Date) return false;
                        if (Times == Setting.reviewdays.Length) return false;
                        if (Date < DateTime.Now.Date) ChemistryTest.Missed++;
                        return true;
                    }
                    public bool PushDate()
                    {
                        if (Times == Setting.reviewdays.Length) return false;
                        Date = DateTime.Now.Date.AddDays(Setting.reviewdays[Times]);
                        Times++;
                        return true;
                    }
                    public void PullDate()
                    {
                        if (Times == 1) Times--;
                        else if (Times > 1) Times -= 2;
                    }
                }
                public string Tag;
                public int[] ToTest;
                public DateTimeAnsPair[] DateAns;
                public string End;
                public void SetFrom(string a)
                {
                    DateAns = new DateTimeAnsPair[0];
                    int nowindex = 0;
                    int index = a.IndexOf(ChemistryTest.FillMark);
                    while (index != -1)
                    {
                        Array.Resize(ref DateAns, DateAns.Length + 1);
                        DateAns[DateAns.Length - 1].Text = a.Substring(nowindex, index - nowindex);
                        nowindex = index + 1;
                        index = a.IndexOf(ChemistryTest.FillMark, nowindex);
                        DateAns[DateAns.Length - 1].Ans = a.Substring(nowindex, index - nowindex);
                        nowindex = index + 1;
                        try { index = a.IndexOf(ChemistryTest.FillMark, nowindex); }
                        catch (Exception) { index = -1; }
                        if (index == -1 || !IsFit(a.Substring(nowindex, index - nowindex), "n\tnnnn/nn/nn"))
                        {
                            DateAns[DateAns.Length - 1].Times = 0;
                            DateAns[DateAns.Length - 1].Date = DateTime.Now.Date;
                        }
                        else
                        {
                            string[] b = a.Substring(nowindex, index - nowindex).Split('\t');
                            DateAns[DateAns.Length - 1].Times = int.Parse(b[0]);
                            DateAns[DateAns.Length - 1].Date = DateTime.Parse(b[1]);
                            nowindex = index + 1;
                            try { index = a.IndexOf(ChemistryTest.FillMark, nowindex); }
                            catch (Exception) { index = -1; }
                        }
                    }
                    End = a.Substring(nowindex);
                }
                public override string ToString()
                {
                    string a = "";
                    for (int i = 0; i < DateAns.Length; i++)
                    {
                        a += DateAns[i].ToString();
                    }
                    a += End;
                    return Tag + a;
                }
                public string StringWithoutRecord()
                {
                    string a = "";
                    for (int i = 0; i < DateAns.Length; i++) a += DateAns[i].Text + ChemistryTest.FillMark.ToString() + DateAns[i].Ans + ChemistryTest.FillMark.ToString();
                    return Tag + a + End;
                }
                public bool ShowTest(int num)
                {
                    Reform();
                    string a = num.ToString() + ". ";
                    for (int i = 0; i < DateAns.Length; i++)
                    {
                        a += DateAns[i].Text;
                        if (!DateAns[i].IsToTest()) a += DateAns[i].Ans;
                    }
                    a += End + "\r\n";
                    ChemistryTest.Txb1.AppendText(a);
                    KeyEventHandler textchangedevent = new KeyEventHandler(OtherTestLine_TextChanged);
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        ChemistryTest.Txb2[i].KeyUp += textchangedevent;
                        ChemistryTest.Txb2[i].ImeMode = ImeMode.Off;
                    }
                    int focusindex = 0;
                    DateTime past = DateTime.Now;
                    while (focusindex < ToTest.Length)
                    {
                        ChemistryTest.Txb2[focusindex].Focus();
                        IsInserted = false;
                        while (!IsInserted) Application.DoEvents();
                        for (int i = 0; i < ChemistryTest.Txb2.Length; i++)
                        {
                            if (ChemistryTest.Txb2[i].Focused)
                            {
                                focusindex = i;
                                break;
                            }
                        }
                        focusindex++;
                    }
                    a = ChemistryTest.ShowReactTime(past);
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        ChemistryTest.Txb2[i].KeyUp -= textchangedevent;
                        if (ChemistryTest.Txb2[i].Text == DateAns[ToTest[i]].Ans)
                        {
                            bool Pushed = DateAns[ToTest[i]].PushDate();
                            if (Pushed) a += ChemistryTest.ShowTestResult(ChemistryTest.Txb2[i].Text, DateAns[ToTest[i]].Date);
                            else a += ChemistryTest.ShowTestResult(ChemistryTest.Txb2[i].Text);
                        }
                        else
                        {
                            DateAns[ToTest[i]].PullDate();
                            a += ChemistryTest.ShowTestResult(ChemistryTest.Txb2[i].Text, DateAns[ToTest[i]].Ans);
                        }
                    }
                    ChemistryTest.Txb1.AppendText(a);
                    { }
                    return !IsToTest();
                }
                public void Reform()
                {
                    foreach (Control a in ChemistryTest.Tlp1.Controls) if (a != ChemistryTest.Txb1) ChemistryTest.Tlp1.Controls.Remove(a);
                    TableLayoutPanel tlp = ChemistryTest.Tlp2;
                    tlp = new TableLayoutPanel();
                    tlp.Dock = DockStyle.Fill;
                    tlp.ColumnCount = ToTest.Length;
                    tlp.RowCount = 1;
                    Array.Resize(ref ChemistryTest.Txb2, ToTest.Length);
                    ITxtBox[] txb = ChemistryTest.Txb2;
                    for (int i = 0; i < ToTest.Length; i++)
                    {
                        txb[i] = new ITxtBox();
                        txb[i].Dock = DockStyle.Fill;
                        txb[i].Multiline = true;
                        tlp.Controls.Add(txb[i]);
                        tlp.SetCellPosition(txb[i], new TableLayoutPanelCellPosition(i, 0));
                        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                    }
                    ChemistryTest.Tlp1.Controls.Add(tlp);
                    ChemistryTest.Tlp1.SetCellPosition(tlp, new TableLayoutPanelCellPosition(0, 1));
                }
                public bool IsToTest()
                {
                    bool totest = false;
                    ToTest = new int[0];
                    for (int i = 0; i < DateAns.Length; i++)
                    {
                        if (DateAns[i].IsToTest())
                        {
                            totest = true;
                            Array.Resize(ref ToTest, ToTest.Length + 1);
                            ToTest[ToTest.Length - 1] = i;
                        }
                    }
                    return totest;
                }
                void OtherTestLine_TextChanged(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != ChemistryTest.TabIndex) return;
                    if (e.KeyData == Keys.Enter)
                    {
                        ITxtBox a = sender as ITxtBox;
                        int b = a.Text.IndexOf("\r\n");
                        if (b != -1)
                        {
                            a.Text = a.Text.Remove(b) + a.Text.Substring(b + 2);
                        IsInserted = true;
                        }
                    }
                }
            }
            public struct MultiChoiceTestLine
            {
                public struct OptionPart
                {
                    public bool IsAns;
                    public string Text;
                    public bool Checked;
                }
                public DateTime Date;
                public int Times;
                public bool[] Checked;
                public string Text;
                public OptionPart[] Option;
                public Color[] NormalColor;
                public int OptionIndex(string a, int index)
                {
                    for (int i = index; i < a.Length; i++)
                    {
                        if (IsFit(a.Substring(i), "(E)") || IsFit(a.Substring(i), "(e)")) return i;
                    }
                    return a.Length;
                }
                public void SetFrom(string a)
                {
                    if (IsFit(a, "n nnnn/nn/nn\t"))
                    {
                        DateTime date = DateTime.Parse(a.Substring(2, 10));
                        a = a.Substring(0, 1) + "\t" + DateToString(date) + "\t" + a.Substring(13);
                        //MessageBox.Show(a);
                    }
                    else if (!IsFit(a, "n\tnnnn/nn/nn\t")) a = "0\t" + DateToString(DateTime.Now) + "\t" + a;
                    int b = a.IndexOf("\t");
                    Times = int.Parse(a.Remove(b));
                    a = a.Substring(b + 1);
                    b = a.IndexOf("\t");
                    Date = DateTime.Parse(a.Remove(b));
                    a = a.Substring(b + 1);
                    Option = new OptionPart[0];
                    b = OptionIndex(a, 0);
                    Text = a.Remove(b);
                    for (int i = b; i < a.Length; )
                    {
                        if (IsFit(a.Substring(i), "(E)"))
                        {
                            int j = OptionIndex(a, i + 1);
                            Array.Resize(ref Option, Option.Length + 1);
                            Option[Option.Length - 1].Text = a.Substring(i + 3, j - i - 3);
                            Option[Option.Length - 1].IsAns = false;
                            Option[Option.Length - 1].Checked = false;
                            i = j;
                        }
                        else if (IsFit(a.Substring(i), "(e)"))
                        {
                            int j = OptionIndex(a, i + 1);
                            Array.Resize(ref Option, Option.Length + 1);
                            Option[Option.Length - 1].Text = a.Substring(i + 3, j - i - 3);
                            Option[Option.Length - 1].IsAns = true;
                            Option[Option.Length - 1].Checked = false;
                            i = j;
                        }
                    }
                    //MessageBox.Show(this.ToString());
                }
                public override string ToString()
                {
                    string a = Times.ToString() + "\t" + DateToString(Date) + "\t" + Text;
                    for (int i = 0; i < Option.Length; i++)
                    {
                        if (Option[i].IsAns) a += "(" + ((char)('a' + i)).ToString() + ")";
                        else a += "(" + ((char)('A' + i)).ToString() + ")";
                        a += Option[i].Text;
                    }
                    return "選擇:" + a;
                }
                public string StringWithoutRecord()
                {
                    string a = Text;
                    int[] order = new int[Option.Length];
                    for (int i = 0; i < order.Length; i++) order[i] = i;
                    for (int i = 0; i < order.Length; i++)
                    {
                        for (int j = i + 1; j < order.Length; j++)
                        {
                            if (IsLargerString(Option[order[j]].Text, Option[order[i]].Text))
                            {
                                int k = order[i];
                                order[i] = order[j];
                                order[j] = k;
                            }
                        }
                    }
                    for (int i = 0; i < order.Length; i++) a += ((char)('A' + i)).ToString() + Option[order[i]].Text;
                    return "選擇:" + a;
                }
                public bool IsLargerString(string a, string b)
                {
                    for (int i = 0; i < a.Length && i < b.Length; i++)
                    {
                        if (a[i] > b[i]) return true;
                        else if (a[i] < b[i]) return false;
                    }
                    if (a.Length > b.Length) return true;
                    else if (a.Length < b.Length) return false;
                    else
                    {
                        MessageBox.Show("ChemistryTestTabPart.MultiChoiceTestLine.IsLargerString(string a,string b):\r\nError! The two string are exactly the same!\r\n" + a);
                        return false;
                    }
                }
                public bool IsToTest()
                {
                    if (Date > DateTime.Now.Date) return false;
                    if (Times == Setting.reviewdays.Length) return false;
                    if (Date < DateTime.Now.Date) ChemistryTest.Missed++;
                    return true;
                }
                public bool ShowTest(int num)
                {
                    MessOrder();
                    string a = num.ToString() + ". 選擇:" + Text + "\r\n";
                    Reform(Option.Length);
                    IsInserted = false;
                    ShowChecked();
                    for (int i = 0; i < Option.Length; i++) a += "(" + ((char)('A' + i)).ToString() + ")" + Option[i].Text;
                    ChemistryTest.Txb1.AppendText(a + "\r\n");

                    KeyEventHandler keyupevent = new KeyEventHandler(MultiChoiceTestLine_KeyUp);
                    EventHandler clickevent = new EventHandler(MultiChoiceTestLine_Click);
                    EventHandler doubleclickevent = new EventHandler(Txb1_DoubleClick);
                    ChemistryTest.Txb1.DoubleClick += Txb1_DoubleClick;
                    ChemistryTest.Tab.FindForm().KeyUp += keyupevent;
                    for (int i = 0; i < Option.Length; i++)
                    {
                        ChemistryTest.Btns[i].Text = ((char)('A' + i)).ToString();
                        ChemistryTest.Btns[i].Click += clickevent;
                    }
                    DateTime past = DateTime.Now;
                    IsInserted = false;
                    while (!IsInserted) Application.DoEvents();
                    ChemistryTest.Txb1.DoubleClick -= doubleclickevent;
                    ChemistryTest.Tab.FindForm().KeyUp -= keyupevent;
                    a = ChemistryTest.ShowReactTime(past);
                    string ans = "", yourans = "";
                    bool CanRemove = true;
                    for (int i = 0; i < Option.Length; i++)
                    {
                        ChemistryTest.Btns[i].Click -= clickevent;
                        if (Option[i].IsAns) ans += ((char)('A' + i)).ToString();
                        if (Option[i].Checked) yourans += ((char)('A' + i)).ToString();
                        if (Option[i].Checked != Option[i].IsAns) CanRemove = false;
                    }
                    if (CanRemove)
                    {
                        bool Pushed = PushDate();
                        if (Pushed) a += ChemistryTest.ShowTestResult(yourans, Date);
                        else a += ChemistryTest.ShowTestResult(yourans);
                    }
                    else
                    {
                        PullDate();
                        a += ChemistryTest.ShowTestResult(yourans,ans);
                    }
                    ChemistryTest.Txb1.AppendText(a);
                    { }
                    return CanRemove;
                }
                public void Reform(int a)
                {
                    foreach (Control b in ChemistryTest.Tlp1.Controls) if (b != ChemistryTest.Txb1) ChemistryTest.Tlp1.Controls.Remove(b);
                    ChemistryTest.Tlp2.Controls.Clear();
                    ChemistryTest.Tlp2 = new TableLayoutPanel();
                    ChemistryTest.Tlp2.Dock = DockStyle.Fill;
                    ChemistryTest.Tlp2.ColumnCount = a;
                    Array.Resize(ref ChemistryTest.Btns, a);
                    for (int i = 0; i < a; i++)
                    {
                        ChemistryTest.Btns[i] = new Button();
                        ChemistryTest.Btns[i].Dock = DockStyle.Fill;
                        ChemistryTest.Tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        ChemistryTest.Tlp2.Controls.Add(ChemistryTest.Btns[i]);
                        ChemistryTest.Tlp2.SetCellPosition(ChemistryTest.Btns[i], new TableLayoutPanelCellPosition(i, 0));
                    }
                    NormalColor = new Color[] { ChemistryTest.Btns[0].BackColor, ChemistryTest.Btns[0].ForeColor };
                    ChemistryTest.Tlp1.Controls.Add(ChemistryTest.Tlp2);
                }
                void Txb1_DoubleClick(object sender, EventArgs e)
                {
                    IsInserted = true;
                }
                public void MessOrder()
                {
                    OptionPart[] tmpOption = new OptionPart[Option.Length];
                    bool[] visited = new bool[Option.Length];
                    for (int i = Option.Length; i > 0; i--)
                    {
                        int j = random.Next(0, i);
                        for (int k = 0; ; k++)
                        {
                            if (!visited[k])
                            {
                                if (j == 0)
                                {
                                    tmpOption[k] = Option[i - 1];
                                    visited[k] = true;
                                    break;
                                }
                                j--;
                            }
                        }
                    }
                    Option = tmpOption;
                }
                public void MultiChoiceTestLine_KeyUp(object sender, KeyEventArgs e)
                {
                    if (All.Tab.SelectedIndex != ChemistryTest.TabIndex) return;
                    string a = e.KeyData.ToString();
                    if (a.Length == 1 && a[0] >= 'A' && a[0] - 'A' < Option.Length)
                    {
                        MultiChoiceTestLine_Click(a[0] - 'A', null);
                    }
                    else if (e.KeyData == Keys.Enter)
                    {
                        IsInserted = true;
                    }
                }
                public bool PushDate()
                {
                    if (Times == Setting.reviewdays.Length) return false;
                    Date = DateTime.Now.Date.AddDays(Setting.reviewdays[Times]);
                    Times++;
                    return true;
                }
                public void PullDate()
                {
                    if (Times == 1) Times--;
                    else if (Times > 1) Times -= 2;
                }
                public void ShowChecked()
                {
                    for (int i = 0; i < Option.Length; i++)
                    {
                        if (Option[i].Checked && (ChemistryTest.Btns[i].BackColor != ChemistryTest.CheckedColor[0] || ChemistryTest.Btns[i].ForeColor != ChemistryTest.CheckedColor[1]))
                        {
                            ChemistryTest.Btns[i].BackColor = ChemistryTest.CheckedColor[0];
                            ChemistryTest.Btns[i].ForeColor = ChemistryTest.CheckedColor[1];
                        }
                        else if (!Option[i].Checked && (ChemistryTest.Btns[i].BackColor != NormalColor[0] || ChemistryTest.Btns[i].ForeColor != NormalColor[1]))
                        {
                            ChemistryTest.Btns[i].BackColor = NormalColor[0];
                            ChemistryTest.Btns[i].ForeColor = NormalColor[1];
                        }
                    }
                }
                public void MultiChoiceTestLine_Click(object sender, EventArgs e)
                {
                    if (All.Tab.SelectedIndex != ChemistryTest.TabIndex) return;
                    int k = -1;
                    if (sender.GetType() == typeof(int)) k = (int)sender;
                    else k = ChemistryTest.Tlp2.GetCellPosition(sender as Button).Column;
                    if (Option[k].Checked) Option[k].Checked = false;
                    else Option[k].Checked = true;
                    ShowChecked();
                }
            }
            public TableLayoutPanel Tlp1;
            public TableLayoutPanel Tlp2;
            public ITxtBox Txb1;
            public ITxtBox[] Txb2;
            public Button[] Btns;
            public char FillMark;
            public string[] QA;
            public string Path;
            public bool IsSaved;
            public int TabIndex;
            public int Missed;
            public int Repeated;
            public Process TabTipProcess;
            public TabPage Tab;
            public Color[] CheckedColor;
            public OtherTestLine[] OtherTest;
            public MultiChoiceTestLine[] MultiChoice;
            public HashSet<int> OtherToTest;
            public HashSet<int> MultiToTest;
            public string ShowReactTime(DateTime a)
            {
                return "\t" + (DateTime.Now - a).TotalSeconds.ToString("F3") + " s\r\n";
            }
            public string ShowTestResult(string yourans,string ans)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tWrong!\tAns:\t" + ans + "\r\n\r\n";
            }
            public string ShowTestResult(string yourans,DateTime nextreviewtime)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tCorrect!\tNext Review Time:\t" + DateToString(nextreviewtime) + "\r\n\r\n";
            }
            public string ShowTestResult(string yourans)
            {
                return "\tYour Ans:\t" + yourans + "\r\n\tCorrect!\tCongratulations! You've finished this question!\r\n\r\n";
            }
            public void SetTabText(int a)
            {
                if (a == 0) Tab.Text = "化學";
                else Tab.Text = "化學(" + a.ToString() + ")";
            }
            public void Reset()
            {
                CheckedColor = new Color[] { Color.FromArgb(0, 0, 255), Color.FromArgb(255, 255, 255) };
                TabTipProcess = new Process();
                TabTipProcess.StartInfo.FileName = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
                IsSaved = false;
                FillMark = '\'';
                Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\化學測驗.txt";
                OtherTest = new OtherTestLine[0];
                MultiChoice = new MultiChoiceTestLine[0];
                Tab = new TabPage();
                Tlp1 = new TableLayoutPanel();
                Tlp2 = new TableLayoutPanel();
                Txb1 = new ITxtBox();
                Txb2 = new ITxtBox[0];
                Btns = new Button[0];

                Tab.Controls.Add(Tlp1); Tlp1.Dock = DockStyle.Fill;
                Tlp1.ColumnCount = 1; Tlp1.RowCount = 2;
                Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 80)); Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                Tlp1.Controls.Add(Txb1); Tlp1.SetCellPosition(Txb1, new TableLayoutPanelCellPosition(0, 0));
                {
                    Txb1.Dock = DockStyle.Fill;
                    Txb1.Multiline = true;
                    Txb1.ScrollBars = ScrollBars.Vertical;
                    Txb1.Font = ITxtBoxFont;
                }
                Tlp1.Controls.Add(Tlp2); Tlp1.SetCellPosition(Tlp2, new TableLayoutPanelCellPosition(0, 1));
                {
                    Tlp2.Dock = DockStyle.Fill;
                    Tlp2.RowCount = 1;
                }
            }
            public void Add()
            {
                All.Tab.TabPages.Add(Tab);
                TabIndex = -1;
                for (int i = 0; i < All.Tab.TabCount; i++)
                {
                    if (All.Tab.TabPages[i] == Tab)
                    {
                        TabIndex = i;
                        break;
                    }
                }
            }
            public void Start()
            {
                if (!IsSaved) All.Tab.SelectedIndex = TabIndex;
                string MessageString = "";
                if (Missed > 0) MessageString += "Missed " + Missed.ToString() + " questions!\r\n";
                if (Repeated > 0) MessageString += "Find " + Repeated.ToString() + " repeats!\r\n";
                if (MessageString.Length > 0) MessageBox.Show(MessageString);
                while (OtherToTest.Count() + MultiToTest.Count > 0)
                {
                    int i = -1, b = OtherToTest.Count(), c = MultiToTest.Count();
                    i = random.Next(0, b + c - 1);
                    if (i < OtherToTest.Count())
                    {
                        int j = OtherToTest.ElementAt(i);
                        bool CanRemove = OtherTest[j].ShowTest(b + c);
                        if (CanRemove) OtherToTest.Remove(j);
                    }
                    else
                    {
                        int j = MultiToTest.ElementAt(i - b);
                        bool CanRemove = MultiChoice[j].ShowTest(b + c);
                        if (CanRemove) MultiToTest.Remove(j);
                    }
                    SetTabText(OtherToTest.Count() + MultiToTest.Count);
                }
                if (!IsSaved)
                {
                    Save();
                    MessageBox.Show("Saved! You can be assured to close the program.");
                }
            }
            public void Load()
            {
                string[] filestr = new string[0];
                if (new FileInfo(Path).Exists)
                {
                    StreamReader reader = new StreamReader(Path, Encoding.Default);
                    filestr = reader.ReadToEnd().Replace("\r\n", "\r").Split('\r');
                }
                else Txb1.Text = "桌面不存在化學測驗.txt\r\n";
                Dictionary<string, int> repeatedO = new Dictionary<string, int>();
                Dictionary<string, int> repeatedM = new Dictionary<string, int>();
                Repeated = 0;
                Missed = 0;
                for (int i = 0; i < filestr.Length; i++)
                {
                    if (filestr[i].Length == 0) continue;
                    switch (filestr[i].Substring(0, 3))
                    {
                        case "填充:":
                            {
                                Array.Resize(ref OtherTest, OtherTest.Length + 1);
                                OtherTest[OtherTest.Length - 1].Tag = filestr[i].Substring(0, 3);
                                OtherTest[OtherTest.Length - 1].SetFrom(filestr[i].Substring(3));
                                if (repeatedO.ContainsKey(OtherTest[OtherTest.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    Array.Resize(ref OtherTest, OtherTest.Length - 1);
                                }
                                else repeatedO.Add(OtherTest[OtherTest.Length - 1].StringWithoutRecord(), OtherTest.Length - 1);
                                break;
                            }
                        case "選擇:":
                            {
                                Array.Resize(ref MultiChoice, MultiChoice.Length + 1);
                                MultiChoice[MultiChoice.Length - 1].SetFrom(filestr[i].Substring(3));
                                if (repeatedM.ContainsKey(MultiChoice[MultiChoice.Length - 1].StringWithoutRecord()))
                                {
                                    Repeated++;
                                    Array.Resize(ref MultiChoice, MultiChoice.Length - 1);
                                }
                                else repeatedM.Add(MultiChoice[MultiChoice.Length - 1].StringWithoutRecord(), MultiChoice.Length - 1);
                                break;
                            }
                        default:
                            {
                                MessageBox.Show(filestr[i] + "\r\nThe Tag is:\"" + filestr[i].Substring(0, 3) + "\"");
                                break;
                            }
                    }
                }
                IsSaved = false;
                OtherToTest = new HashSet<int>();
                MultiToTest = new HashSet<int>();
                Missed = 0;
                for (int i = 0; i < OtherTest.Length; i++) if (OtherTest[i].IsToTest()) OtherToTest.Add(i);
                for (int i = 0; i < MultiChoice.Length; i++) if (MultiChoice[i].IsToTest()) MultiToTest.Add(i);
                SetTabText(OtherToTest.Count + MultiToTest.Count);
                if (OtherToTest.Count + MultiToTest.Count == 0)
                {
                    Txb1.AppendText("Finished!");
                    IsSaved = true;
                }
            }
            public void Save()
            {
                StreamWriter writer = new StreamWriter(Path, false, Encoding.Default);
                for (int i = 0; i < MultiChoice.Length; i++) writer.WriteLine(MultiChoice[i].ToString());
                for (int i = 0; i < OtherTest.Length; i++) writer.WriteLine(OtherTest[i].ToString());
                writer.Close();
                IsSaved = true;
            }
        }
        public struct DictionaryTabPart
        {
            public struct DictionaryTabPage
            {
                public TabPage Tab;
                public SplitContainer Split1;
                public TableLayoutPanel TLP1;
                public Panel Panel1;
                public RadioButton[] Option;
                public ITxtBox Insert;
                public WebBrowser Browser;
                public Thread[] Thread_;
                public Label SelectedLabel;
                public ToolTip ToolTip_;
                public int PageIndex;
                public void FocusInsert() { Insert.Focus(); }
                public void Reset()
                {
                    PageIndex = Dictionary.Page.Length - 1;
                    Thread_ = new Thread[2];
                    SelectedLabel = new Label();
                    Tab = new TabPage();
                    Split1 = new SplitContainer();
                    //Split2 = new SplitContainer();
                    TLP1 = new TableLayoutPanel();
                    Panel1 = new Panel();
                    Insert = new ITxtBox();
                    Browser = new WebBrowser();
                    ToolTip_ = new ToolTip();
                    Option = new RadioButton[3];
                    for (int i = 0; i < Option.Length; i++) Option[i] = new RadioButton();
                    Dictionary.TabControl_.TabPages.Add(Tab);
                    Tab.Controls.Add(Split1);
                    {
                        Split1.Dock = DockStyle.Fill;
                        Split1.Orientation = Orientation.Vertical;
                        Split1.Panel1.Controls.Add(TLP1);
                        {
                            TLP1.Dock = DockStyle.Fill;
                            TLP1.ColumnCount = 1;
                            TLP1.RowCount = 5;
                            TLP1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            TLP1.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                            TLP1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            TLP1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            TLP1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            TLP1.Controls.Add(Insert); TLP1.SetCellPosition(Insert, new TableLayoutPanelCellPosition(0, 0));
                            {
                                Insert.Dock = DockStyle.Fill;
                                Insert.Height = 30;
                                Insert.MouseWheel += Split2_Panel1_MouseWheel;
                                Insert.DoubleClick += JustNavigate;
                            }
                            TLP1.Controls.Add(Panel1); TLP1.SetCellPosition(Panel1, new TableLayoutPanelCellPosition(0, 1));
                            {
                                Panel1.Dock = DockStyle.Fill;
                                Panel1.AutoScroll = true;
                                Panel1.VerticalScroll.Enabled = true;
                                Panel1.HorizontalScroll.Enabled = true;
                                Panel1.VerticalScroll.Visible = true;
                                Panel1.HorizontalScroll.Visible = true;
                                Panel1.MouseWheel += Split2_Panel1_MouseWheel;
                            }
                            TLP1.Controls.AddRange(Option); TLP1.SetCellPosition(Option[0], new TableLayoutPanelCellPosition(0, 2)); TLP1.SetCellPosition(Option[1], new TableLayoutPanelCellPosition(0, 3)); TLP1.SetCellPosition(Option[2], new TableLayoutPanelCellPosition(0, 4));
                            {
                                Option[0].Text = "Yahoo Dictionary";
                                Option[0].Dock = DockStyle.Fill;
                                Option[0].BringToFront();
                                Option[1].Text = "Google 翻譯";
                                Option[1].Dock = DockStyle.Fill;
                                Option[1].BringToFront();
                                Option[2].Text = "教育部重編國語辭典修訂本";
                                Option[2].Dock = DockStyle.Fill;
                                Option[2].BringToFront();
                                Option[0].CheckedChanged += Option_CheckedChanged;
                                Option[1].CheckedChanged += Option_CheckedChanged;
                                Option[2].CheckedChanged += Option_CheckedChanged;
                                Option[0].Checked = true;
                            }
                        }
                        Split1.Panel2.Controls.Add(Browser);
                        {
                            Browser.Dock = DockStyle.Fill;
                            Browser.ScrollBarsEnabled = true;
                            Browser.ScriptErrorsSuppressed = true;
                            //Browser.ContainsFocus
                            //Browser.Navigated += Browser_Navigated;
                        }
                    }
                }
                private void JustNavigate(object sender, EventArgs e)
                {
                    if (Dictionary.Page[0].Tab.Text == Insert.Text) return;
                    Navigate(Insert.Text);
                }
                public double similarity(string key, string value)
                {
                    int a = value.IndexOf(key);
                    if (a >= 0) return (double)key.Length / value.Length;
                    else return 0;
                }
                public partial class Label2: Label
                {
                    public int PageIndex;
                    public Label2(int a)
                    {
                        PageIndex = a;
                        this.MouseDown += Label2_MouseDown;
                        this.MouseUp += Label2_MouseUp;
                        this.MouseEnter += Label2_MouseEnter;
                        this.MouseLeave += Label2_MouseLeave;
                    }
                    void Label2_MouseLeave(object sender, EventArgs e)
                    {
                        if (this == Dictionary.Page[PageIndex].SelectedLabel) this.ForeColor = Color.FromArgb(255, 0, 0);
                        else this.ForeColor = Color.FromArgb(0, 0, 0);
                    }
                    void Label2_MouseEnter(object sender, EventArgs e)
                    {
                        this.ForeColor = Color.FromArgb(0, 0, 255);
                        string[] b = this.Text.Split('\t');
                        if (Dictionary.Page[PageIndex].ToolTip_.ToolTipTitle == b[0]) return;
                        Dictionary.Page[PageIndex].ToolTip_.RemoveAll();
                        Dictionary.Page[PageIndex].ToolTip_.ToolTipTitle = b[0];
                        string c = b[1];
                        for (int i = 2; i < b.Length; i++) c += "\t" + b[i];
                        Dictionary.Page[PageIndex].ToolTip_.Show(c, this);
                        Application.DoEvents();
                    }
                    void Label2_MouseUp(object sender, MouseEventArgs e)
                    {
                        Dictionary.Page[PageIndex].Insert.Focus();
                    }
                    void Label2_MouseDown(object sender, MouseEventArgs e)
                    {
                        Dictionary.Page[PageIndex].SelectedLabel.ForeColor = Color.FromArgb(0, 0, 0);
                        Dictionary.Page[PageIndex].SelectedLabel = this;
                        Dictionary.Page[PageIndex].SelectedLabel.ForeColor = Color.FromArgb(255, 0, 0);
                        string b = Dictionary.Page[PageIndex].SelectedLabel.Text;
                        b = b.Remove(b.IndexOf('\t')).Substring(b.IndexOf('%') + 1).Trim(' ');
                        Dictionary.Page[PageIndex].Navigate(b);
                        Dictionary.Page[0].Tab.Text = b;
                        Dictionary.Page[PageIndex].Panel1.Focus();
                    }
                }
                public void ListWords(string b)
                {
                    if (Thread_[1] != null) Thread_[1].Abort();
                    if (b.Length == 0) return;
                    Navigate(b);
                    Dictionary.Page[0].Tab.Text = b;

                    double[] a = new double[EnglishTest.Question.Length];
                    for (int i = 0; i < EnglishTest.Question.Length; i++)
                    {
                        a[i] = Math.Max(similarity(b, EnglishTest.Question[i].Word), similarity(b, EnglishTest.Question[i].Explanation));
                    }
                    double c = 0;
                    Panel1.Controls.Clear();

                    while (true)
                    {
                        int h = -1;
                        c = 0;
                        for (int i = 0; i < a.Length; i++)
                        {
                            if (a[i] > c)
                            {
                                c = a[i];
                                h = i;
                            }
                        }
                        if (c > 0)
                        {
                            for (int i = h; i < a.Length; i++)
                            {
                                if (a[i] == c)
                                {
                                    a[i] = 0;
                                    Label2 g = new Label2(PageIndex);
                                    Panel1.Controls.Add(g);
                                    g.Text = c.ToString("P0").PadRight(5) + EnglishTest.Question[i].Word + " \t" + EnglishTest.Question[i].Explanation;
                                    g.AutoSize = true;
                                    g.BringToFront();
                                    g.Dock = DockStyle.Top;
                                    Application.DoEvents();
                                    if (Insert.Text != b) return;
                                }
                            }
                        }
                        else break;
                    }
                    Label f = new Label();
                    Panel1.Controls.Add(f);
                    f.Font = new Font(f.Font.FontFamily, 10);
                    f.ForeColor = Color.FromArgb(100, 100, 100);
                    f.Text = "          (Search Completed)";
                    f.AutoSize = true;
                    f.BringToFront();
                    f.Dock = DockStyle.Top;
                }
                public void Navigate(string word)
                {
                    if (Thread_[0] != null) Thread_[0].Abort();
                    int pageindex = PageIndex;
                    string dictionaryurl = DictionaryUrl(word);
                    Thread_[0] = new Thread(() =>
                    {
                        try
                        {
                            Dictionary.Page[pageindex].Browser.Navigate(dictionaryurl);
                            /*Browser.Invoke(new Action(() =>
                            {
                                while (Browser.IsBusy) Application.DoEvents();
                                for (int i = 0; i < 500; i++)
                                {
                                    if (Browser.ReadyState != WebBrowserReadyState.Complete)
                                    {
                                        Application.DoEvents();
                                        Thread.Sleep(10);
                                    }
                                    else break;
                                }
                                Application.DoEvents();
                                StreamReader reader = new StreamReader(Browser.DocumentStream, Encoding.GetEncoding(Browser.Document.Encoding));
                                string b = reader.ReadToEnd();
                                reader.Close();
                                if (b.IndexOf("alert(\"找不到\");") != -1 || b.IndexOf("alert(\"可能離上次操作太久, 您將被導引至首頁, 請重新查詢!\");") != -1) return;
                                else
                                {
                                    Browser.Stop();
                                    Application.DoEvents();
                                    //Browser.DocumentStream = tmpwebbroser.DocumentStream;
                                }
                            }));*/
                        }
                        catch (Exception) { }
                    });
                    Thread_[0].IsBackground = true;
                    Thread_[0].Start();
                }
                public void SearchInsert(object sender, EventArgs e)
                {
                    if (Dictionary.Page[0].Tab.Text == Insert.Text) return;
                    SelectedLabel.ForeColor = Color.FromArgb(0, 0, 0);
                    SelectedLabel = new Label();
                    if (Insert.Text.Length != 0)
                    {
                        ListWords(Insert.Text);
                    }
                }
                public string DictionaryUrl(string a)
                {
                    if (Option[0].Checked) return "https://tw.dictionary.yahoo.com/dictionary?p=" + a;
                    else if (Option[1].Checked) return "https://translate.google.com.tw/#zh-CN/en/" + a;
                    else if (Option[2].Checked) return "http://dict.revised.moe.edu.tw/cgi-bin/newDict/dict.sh?cond=" + a + "&fld=1";
                    else
                    {
                        MessageBox.Show("Please make sure you check the right dictionary.");
                        return "";
                    }
                }
                private void Option_CheckedChanged(object sender, EventArgs e)
                {
                    if (Option[0].Checked)
                    {
                        Insert.ImeMode = ImeMode.Off;
                        Insert.TextChanged += SearchInsert;
                    }
                    else if(Option[1].Checked)
                    {
                        Insert.ImeMode = ImeMode.On;
                        Insert.TextChanged -= SearchInsert;
                    }
                    else if (Option[2].Checked)
                    {
                        Insert.ImeMode = ImeMode.On;
                        Insert.TextChanged -= SearchInsert;
                    }
                    else MessageBox.Show("Please make sure you check the right dictionary.");
                    Insert.Focus();
                }
                private void Split2_Panel1_MouseWheel(object sender, MouseEventArgs e)
                {
                    VScrollProperties a = Panel1.VerticalScroll;
                    int b = 50;
                    if (e.Delta < 0) a.Value += (a.Value + b <= a.Maximum ? b : a.Maximum - a.Value);
                    else a.Value -= (a.Value - b >= a.Minimum ? b : a.Value - a.Minimum);
                }
            }
            public int TabIndex;
            public TabPage Tab;
            public TabControl TabControl_;
            public DictionaryTabPage[] Page;
            public void Reset()
            {
                Tab = new TabPage();
                TabControl_ = new TabControl();
                Tab.Text = "Dictionary";
                Tab.Controls.Add(TabControl_);
                TabControl_.Dock = DockStyle.Fill;
                Page = new DictionaryTabPage[1];
                for (int i = 0; i < Page.Length; i++)
                {
                    Page[i] = new DictionaryTabPage();
                    Page[i].Reset();
                }
            }
            public void Add()
            {
                All.Tab.TabPages.Add(Tab);
                TabIndex = -1;
                for(int i=0;i<All.Tab.TabCount;i++)
                {
                    if(All.Tab.TabPages[i]==Tab)
                    {
                        TabIndex = i;
                        break;
                    }
                }
            }
            public void FocusInsert(object sender, EventArgs e)
            {
                if (All.Tab.SelectedTab == this.Tab) Page[TabControl_.SelectedIndex].FocusInsert();
            }
        }
        public struct SettingTabPage
        {
            public TabPage Tab;
            public TestOrderPart TestOrder;
            public AddNewWordsPart AddNewWord;
            public AddChineseTestPart AddChinese;
            public char fillingmark;
            public int[] reviewdays;
            public void Reset()
            {
                fillingmark = '\'';
                reviewdays = new int[] { 1, 2, 3, 5, 14, 30, 60 };
                Tab = new TabPage();
                TestOrder = new TestOrderPart();
                AddNewWord = new AddNewWordsPart();
                AddChinese = new AddChineseTestPart();
                TestOrder.Reset();
                AddNewWord.Reset();
                AddChinese.Reset();
                Tab.Text = "Setting";
            }
            public void Add()
            {
                All.Tab.TabPages.Add(Tab);
            }
            public struct TestOrderPart
            {
                public GroupBox Main;
                public RadioButton[] Option;
                public void Reset()
                {
                    Main = new GroupBox();
                    Option = new RadioButton[3];
                    for (int i = 0; i < Option.Length; i++) Option[i] = new RadioButton();
                    Setting.Tab.Controls.Add(Main);
                    Main.Text = "Test Order";
                    Main.Dock = DockStyle.Top;
                    Main.AutoSize = true;
                    Main.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    Main.Controls.AddRange(Option);
                    {
                        for (int i = 0; i < Option.Length; i++)
                        {
                            Option[i].Dock = DockStyle.Top;
                            Option[i].BringToFront();
                        }
                        Option[0].Text = "Vocabulary Tests First";
                        Option[1].Text = "Multiple Choice Tests First";
                        Option[2].Text = "Random";
                        Option[2].Select();
                    }
                }
            }
            public struct AddNewWordsPart
            {
                public partial class Form2 : Form
                {
                    public ITxtBox[] Txb = new ITxtBox[2];
                    public Button[] Btns = new Button[5];
                    public TableLayoutPanel[] Tlp = new TableLayoutPanel[4];
                    public SplitContainer[] Split = new SplitContainer[1];
                    public int State = -1, BtnHeight = 100;
                    public PictureBox PictureBox_ = new PictureBox();
                    public string Path = "";
                    public DirectoryInfo DirectoryInfo_ = null;
                    public string[] ImagePath = new string[0];
                    public int ImageSum = 0;
                    public void SetControls()
                    {
                        for (int i = 0; i < Txb.Length; i++) Txb[i] = new ITxtBox();
                        for (int i = 0; i < Btns.Length; i++) Btns[i] = new Button();
                        for (int i = 0; i < Tlp.Length; i++) Tlp[i] = new TableLayoutPanel();
                        for (int i = 0; i < Split.Length; i++) Split[i] = new SplitContainer();
                        this.Controls.Add(Split[0]);
                        {
                            Split[0].Dock = DockStyle.Fill;
                            Split[0].SplitterDistance = Split[0].Width * 7 / 10;
                            Split[0].FixedPanel = FixedPanel.None;
                        }
                        Split[0].Panel1.Controls.Add(Tlp[0]);
                        {
                            Tlp[0].Dock = DockStyle.Fill;
                            Tlp[0].ColumnCount = 1;
                            Tlp[0].RowCount = 2;
                            Tlp[0].RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                            Tlp[0].RowStyles.Add(new RowStyle(SizeType.Absolute, BtnHeight));
                            Tlp[0].SetCellPosition(Txb[0], new TableLayoutPanelCellPosition(0, 0));
                            Tlp[0].SetCellPosition(Tlp[1], new TableLayoutPanelCellPosition(0, 1));
                            Tlp[0].Controls.Add(Txb[0]);
                            {
                                Txb[0].Dock = DockStyle.Fill;
                                Txb[0].Multiline = true;
                                Txb[0].ScrollBars = ScrollBars.Both;
                                Txb[0].WordWrap = false;
                                Txb[0].Font = ITxtBoxFont;
                            }
                            Tlp[0].Controls.Add(Tlp[1]);
                            {
                                Tlp[1].Dock = DockStyle.Fill;
                                Tlp[1].ColumnCount = 3;
                                Tlp[1].RowCount = 1;
                                Tlp[1].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                                Tlp[1].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                                Tlp[1].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                                Tlp[1].SetCellPosition(Btns[0], new TableLayoutPanelCellPosition(0, 0));
                                Tlp[1].SetCellPosition(Btns[1], new TableLayoutPanelCellPosition(1, 0));
                                Tlp[1].SetCellPosition(Btns[2], new TableLayoutPanelCellPosition(2, 0));
                                Tlp[1].Controls.Add(Btns[0]);
                                {
                                    Btns[0].Text = "OK";
                                    Btns[0].Dock = DockStyle.Fill;
                                    Btns[0].AutoSize = true;
                                    Btns[0].AutoSizeMode = AutoSizeMode.GrowAndShrink;
                                    Btns[0].Click += Btns_0_Click;
                                }
                                Tlp[1].Controls.Add(Btns[1]);
                                {
                                    Btns[1].Text = "Cancel";
                                    Btns[1].Dock = DockStyle.Fill;
                                    Btns[0].AutoSize = true;
                                    Btns[0].AutoSizeMode = AutoSizeMode.GrowAndShrink;
                                    Btns[1].Click += Btns_1_Click;
                                }
                                Tlp[1].Controls.Add(Btns[2]);
                                {
                                    Btns[2].Text = "Process";
                                    Btns[2].Dock = DockStyle.Fill;
                                    Btns[0].AutoSize = true;
                                    Btns[0].AutoSizeMode = AutoSizeMode.GrowAndShrink;
                                    Btns[2].Click += Btns_2_Click;
                                }
                            }
                        }
                        Split[0].Panel2.Controls.Add(Tlp[2]);
                        {
                            Tlp[2].Dock = DockStyle.Fill;
                            Tlp[2].ColumnCount = 1;
                            Tlp[2].RowCount = 2;
                            Tlp[2].RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                            Tlp[2].RowStyles.Add(new RowStyle(SizeType.Absolute, BtnHeight));
                            Tlp[2].Controls.Add(PictureBox_);
                            Tlp[2].Controls.Add(Tlp[3]);
                            Tlp[2].SetCellPosition(PictureBox_, new TableLayoutPanelCellPosition(0, 0));
                            {
                                PictureBox_.Dock = DockStyle.Fill;
                                PictureBox_.SizeMode = PictureBoxSizeMode.Zoom;
                            }
                            Tlp[2].SetCellPosition(Tlp[3], new TableLayoutPanelCellPosition(0, 1));
                            {
                                Tlp[3].Dock = DockStyle.Fill;
                                Tlp[3].ColumnCount = 3;
                                Tlp[3].RowCount = 1;
                                Tlp[3].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                                Tlp[3].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                                Tlp[3].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                                Tlp[3].Controls.Add(Btns[3]);
                                Tlp[3].Controls.Add(Txb[1]);
                                Tlp[3].Controls.Add(Btns[4]);
                                Tlp[3].SetCellPosition(Btns[3], new TableLayoutPanelCellPosition(0, 0));
                                {
                                    Btns[3].Dock = DockStyle.Fill;
                                    Btns[3].Text = "Previous";
                                    Btns[3].Click += Btns_3_Click;
                                }
                                Tlp[3].SetCellPosition(Txb[1], new TableLayoutPanelCellPosition(1, 0));
                                {
                                    Txb[1].Dock = DockStyle.Fill;
                                    Txb[1].TextChanged += Txb_1_TextChanged;
                                }
                                Tlp[3].SetCellPosition(Btns[4], new TableLayoutPanelCellPosition(2, 0));
                                {
                                    Btns[4].Dock = DockStyle.Fill;
                                    Btns[4].Text = "Next";
                                    Btns[4].Click += Btns_4_Click;
                                }
                            }
                        }
                    }
                    public void ChangeImage(int a, bool append)
                    {
                        Thread thread = new Thread(() =>
                        {
                            if (append)
                            {
                                ImageSum += a;
                                if (ImageSum < 0) ImageSum = 0;
                                else if (ImageSum >= ImagePath.Length) ImageSum = ImagePath.Length - 1;
                            }
                            else
                            {
                                ImageSum = a;
                            }
                            PictureBox_.Image = Image.FromFile(ImagePath[ImageSum]);
                            Txb[1].Text = ImageSum.ToString();
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                    private void Btns_4_Click(object sender, EventArgs e)
                    {
                        ChangeImage(1, true);
                    }
                    private void Txb_1_TextChanged(object sender, EventArgs e)
                    {
                        try
                        {
                            ImageSum = int.Parse(Txb[1].Text);
                            ChangeImage(ImageSum, false);
                        }
                        catch (Exception) { }
                    }
                    private void Btns_3_Click(object sender, EventArgs e)
                    {
                        ChangeImage(-1, true);
                    }
                    public bool IsChinese(string a, int b)
                    {
                        if (Encoding.Default.GetBytes(a.Substring(b, 1)).Length == 2)
                        {
                            return true;
                        }
                        else return false;
                    }
                    public bool IsUpper(string a, int b)
                    {
                        if (a[b] >= 'A' && a[b] <= 'Z' && !IsChinese(a, b)) return true;
                        else return false;
                    }
                    public bool IsLower(string a, int b)
                    {
                        if (a[b] >= 'a' && a[b] <= 'z' && !IsChinese(a, b)) return true;
                        else return false;
                    }
                    public bool IsLetter(string a, int b)
                    {
                        if (IsUpper(a, b) || IsLower(a, b)) return true;
                        else return false;
                    }
                    void Btns_0_Click(object sender, EventArgs e)
                    {
                        State = 1;
                        this.OnFormClosing(new FormClosingEventArgs(CloseReason.None, false));
                    }
                    void Btns_1_Click(object sender, EventArgs e)
                    {
                        State = 0;
                        this.OnFormClosing(new FormClosingEventArgs(CloseReason.None, false));
                    }
                    void Btns_2_Click(object sender, EventArgs e)
                    {
                        string a = Txb[0].Text;
                        a = a.Replace('（', '(').Replace('）', ')').Replace('；', ';').Replace('：', ':').Replace('，', ',').Replace("...", "…").Replace('’','\'');
                        int index = a.IndexOf(' ');
                        while (index != -1 && index + 1 < a.Length)
                        {
                            if (!IsLetter(a, index - 1) || !IsLetter(a, index + 1))
                            {
                                a = a.Remove(index) + a.Substring(index + 1);
                            }
                            index = a.IndexOf(' ', index + 1);
                        }
                        index = a.IndexOf("\r\n");
                        while (index != -1 && index + 1 < a.Length)
                        {
                            int c = RecognizePOS(a, index + 2);
                            if ((index+2<a.Length&&!IsLetter(a, index + 2)) || c > 0)
                            {
                                if (c > 0) a = POSToLower(a, index + 2, c);
                                if (IsLetter(a, index - 1)) a = a.Remove(index).TrimEnd(' ') + EnglishTest.FillMark.ToString() + a.Substring(index + 2).TrimStart(' ');
                                else a = a.Remove(index) + a.Substring(index + 2);
                            }
                            index = a.IndexOf("\r\n", index + 1);
                        }
                        byte[] b = Encoding.Unicode.GetBytes(a);

                        a = Encoding.Default.GetString(Encoding.Convert(Encoding.Unicode, Encoding.Default, b));
                        Txb[0].Text = a;
                        MessageBox.Show("done");
                        return;
                    }
                    public string POSToLower(string a, int b, int c)
                    {
                        return a.Remove(b) + a.Substring(b, c).ToLower() + a.Substring(b + c);
                    }
                    public int RecognizePOS(string a, int b)
                    {
                        if (b + 2 >= a.Length) return 0;
                        switch (a.Substring(b, 2).ToLower())
                        {
                            case "n.":
                            case "v.":
                                {
                                    return 2;
                                }
                            default:
                                {
                                    switch (a.Substring(b, 3).ToLower())
                                    {
                                        case "ph.":
                                            {
                                                return 3;
                                            }
                                        default:
                                            {
                                                switch (a.Substring(b, 4).ToLower())
                                                {
                                                    case "adj.":
                                                    case "adv":
                                                        {
                                                            return 4;
                                                        }
                                                    default:
                                                        {
                                                            switch (a.Substring(b, 5).ToLower())
                                                            {
                                                                case "prep.":
                                                                    {
                                                                        return 5;
                                                                    }
                                                                default:
                                                                    {
                                                                        return 0;
                                                                    }
                                                            }
                                                        }
                                                }
                                            }
                                    }
                                }
                        }
                    }
                    public Form2()
                    {
                        Form2.CheckForIllegalCrossThreadCalls = false;
                        this.WindowState = FormWindowState.Maximized;
                        SetControls();
                        State = 0;
                        this.Shown += Form2_Shown;
                    }
                    void Form2_Shown(object sender, EventArgs e)
                    {
                        State = 0;
                        while (Path == null) Application.DoEvents();
                        DirectoryInfo_ = new DirectoryInfo(Path.Remove(Path.LastIndexOf('\\')));
                        if (!DirectoryInfo_.Exists) return;
                        foreach (FileInfo a in DirectoryInfo_.GetFiles())
                        {
                            if (a.Extension.ToLower() == ".png" && a.Name[0] == '0')
                            {
                                Array.Resize(ref ImagePath, ImagePath.Length + 1);
                                ImagePath[ImagePath.Length - 1] = a.FullName;
                            }
                        }
                        if(ImagePath.Length>0) ChangeImage(0, false);
                        else
                        {
                            Split[0].FixedPanel = FixedPanel.Panel2;
                            Split[0].Panel2.Enabled = false;
                            Split[0].SplitterDistance = 0;
                        }
                    }
                }
                public GroupBox Main;
                public Button EditManual;
                public Button FromFile;
                public ITxtBox FilePath;
                public Form2 Form_;
                public void Reset()
                {
                    Main = new GroupBox();
                    EditManual = new Button();
                    FromFile = new Button();
                    FilePath = new ITxtBox();
                    Form_ = new Form2();
                    Setting.Tab.Controls.Add(Main);
                    Main.Text = "Add New Words";
                    Main.Dock = DockStyle.Top;
                    Main.AutoSize = true;
                    Main.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    Main.Controls.Add(EditManual);
                    {
                        EditManual.Text = "Edit Manual";
                        EditManual.Dock = DockStyle.Top;
                        EditManual.AutoSize = true;
                        EditManual.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        EditManual.Click += AddNewWords;
                    }
                    Main.Controls.Add(FromFile);
                    {
                        FromFile.Text = "From File...";
                        FromFile.Dock = DockStyle.Top;
                        FromFile.AutoSize = true;
                        FromFile.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        FromFile.BringToFront();
                        FromFile.Click += AddNewWords;
                    }
                    Main.Controls.Add(FilePath);
                    {
                        FilePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\手滑背單字\Untitled.FR12.txt";
                        FilePath.Dock = DockStyle.Top;
                        FilePath.BringToFront();
                    }
                }
                private void AddNewWords(object sender, EventArgs e)
                {
                    string text = "";
                    if ((sender as Button).Text == "From File...")
                    {
                        StreamReader reader= new StreamReader(FilePath.Text, Encoding.Unicode);
                        text = reader.ReadToEnd();
                        reader.Close();
                    }
                    if (Form_ != null) Form_.Dispose();
                    Form_ = new Form2();
                    //Form_.parent = this;
                    //if(!this.ow)
                    Form_.Path = FilePath.Text;
                    Form_.Show();
                    Form_.FormClosing += Form2_FormClosing;
                    while (Form_.State == -1) Application.DoEvents();
                    Form_.Txb[0].Text = text;
                }
                private void Form2_FormClosing(object sender, FormClosingEventArgs e)
                {
                    if (Form_.State != 1) return;
                    StreamReader reader = new StreamReader(EnglishTest.Path + "英文測驗.txt", Encoding.Default);
                    string a = Form_.Txb[0].Text;
                    if (a.Substring(a.Length - 2) != "\r\n") a += "\r\n";
                    int b = a.Split('\n').Length;
                    a += reader.ReadToEnd();
                    reader.Close();
                    StreamWriter writer = new StreamWriter(EnglishTest.Path + "英文測驗.txt", false, Encoding.Default);
                    writer.Write(a);
                    writer.Close();
                    e.Cancel = true;
                    Form_.Hide();
                    MessageBox.Show(b.ToString() + " New Words Added!");
                    EnglishTest.IsSaved = false;
                    EnglishTest.IsTested = false;
                    EnglishTest.Start();
                }
            }
            public struct AddChineseTestPart
            {
                public partial class Form2:Form
                {
                    public struct ConfirmMultiChoiceAns
                    {
                        public partial class ConfirmMultiChoiceAnsForm : Form
                        {
                            public partial class OptionBtn : Button
                            {
                                public bool IsAns = false;
                                public Color[] anscolor = new Color[] { Color.FromArgb(0, 0, 255), Color.FromArgb(255, 255, 255) };
                                public Color[] formalcolor = new Color[2];
                                public OptionBtn(string OptionText)
                                {
                                    this.Text = OptionText;
                                    this.Dock = DockStyle.Fill;
                                    formalcolor = new Color[] { this.BackColor, this.ForeColor };
                                    this.Click += OptionBtn_Click;
                                }
                                void OptionBtn_Click(object sender, EventArgs e)
                                {
                                    if (IsAns)
                                    {
                                        IsAns = false;
                                        this.BackColor = formalcolor[0];
                                        this.ForeColor = formalcolor[1];
                                    }
                                    else
                                    {
                                        IsAns = true;
                                        this.BackColor = anscolor[0];
                                        this.ForeColor = anscolor[1];
                                    }
                                }
                            }
                            public OptionBtn[] Btn = new OptionBtn[0];
                            public TableLayoutPanel tlp1 = new TableLayoutPanel();
                            public TableLayoutPanel tlp2 = new TableLayoutPanel();
                            public ITxtBox Txb = new ITxtBox();
                            public Button OK = new Button();
                            public Button Edit = new Button();
                            public Panel panel = new Panel();
                            public bool readytoclose = false;
                            public ConfirmMultiChoiceAnsForm(string Text, string[] Option)
                            {
                                this.WindowState = FormWindowState.Maximized;
                                tlp2.Dock = DockStyle.Fill;
                                tlp2.AutoSize = true;
                                tlp2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                                tlp2.ColumnCount = 3;
                                tlp2.RowCount = 1;
                                tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 9));
                                tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                                tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                                Txb.Dock = DockStyle.Top;
                                Txb.TextChanged += Txb_TextChanged;
                                Edit.Dock = DockStyle.Fill;
                                Edit.AutoSize = true;
                                Edit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                                Edit.Text = "Edit";
                                Edit.Click += Edit_Click;
                                OK.Dock = DockStyle.Fill;
                                OK.AutoSize = true;
                                OK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                                OK.Text = "OK";
                                OK.Click += OK_Click;
                                SetFrom(Text, Option);
                                tlp1.Dock = DockStyle.Fill;
                                tlp1.ColumnCount = 1;
                                tlp1.RowCount = 2;
                                tlp1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                                tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                                tlp2.Controls.AddRange(new Control[] { Txb, Edit, OK });
                                tlp2.SetCellPosition(Txb, new TableLayoutPanelCellPosition(0, 0));
                                tlp2.SetCellPosition(Edit, new TableLayoutPanelCellPosition(1, 0));
                                tlp2.SetCellPosition(OK, new TableLayoutPanelCellPosition(2, 0));
                                tlp1.Controls.AddRange(new Control[] { tlp2, panel });
                                tlp1.SetCellPosition(tlp2, new TableLayoutPanelCellPosition(0, 0));
                                tlp1.SetCellPosition(panel, new TableLayoutPanelCellPosition(0, 1));
                                panel.Dock = DockStyle.Fill;
                                panel.VerticalScroll.Enabled = true;
                                this.Controls.Add(tlp1);
                            }
                            public void SetFrom(string Text, string[] Option)
                            {
                                panel.Controls.Clear();
                                Btn = null;
                                Array.Resize(ref Btn, Option.Length);
                                this.Text = Text;
                                for (int i = 0; i < Btn.Length; i++)
                                {
                                    Btn[i] = new OptionBtn(Option[i]);
                                    panel.Controls.Add(Btn[i]);
                                    Btn[i].Dock = DockStyle.Top;
                                    Btn[i].BringToFront();
                                }
                            }
                            void Edit_Click(object sender, EventArgs e)
                            {
                                string a = new CorrectErrorDialog().Show(this.ToString(), true, "Multiple Choice Question Editor");
                                int index = OptionIndex(a, 0);
                                int nowindex = index;
                                string b = a.Remove(nowindex);
                                string[] c = new string[0];
                                index=OptionIndex(a,index+1);
                                while(index!=-1)
                                {
                                    Array.Resize(ref c, c.Length + 1);
                                    c[c.Length - 1] = a.Substring(nowindex, index - nowindex);
                                    nowindex = index;
                                    index = OptionIndex(a, index + 1);
                                    //index=OptionIndex()
                                }
                                Array.Resize(ref c, c.Length + 1);
                                c[c.Length - 1] = a.Substring(nowindex);
                                SetFrom(b, c);
                            }
                            void OK_Click(object sender, EventArgs e)
                            {
                                readytoclose = true;
                            }
                            void Txb_TextChanged(object sender, EventArgs e)
                            {
                                panel.Controls.Clear();
                                for (int i = 0; i < Btn.Length; i++)
                                {
                                    if (Txb.Text.Length == 0 || Btn[i].Text.IndexOf(Txb.Text) != -1)
                                    {
                                        panel.Controls.Add(Btn[i]);
                                        Btn[i].Dock = DockStyle.Top;
                                        Btn[i].BringToFront();
                                    }
                                }
                            }
                            public override string ToString()
                            {
                                string a = Text;
                                for(int i=0;i<Btn.Length;i++)
                                {
                                    if (Btn[i].IsAns) a += "(" + ((char)('a' + i)).ToString() + ")";
                                    else a += "(" + ((char)('A' + i)).ToString() + ")";
                                    if (OptionIndex(Btn[i].Text, 0) == 0) Btn[i].Text = Btn[i].Text.Substring(3);
                                    a += Btn[i].Text;
                                }
                                return a;
                            }
                            public int OptionIndex(string a, int startindex)
                            {
                                while (!IsFit(a.Substring(startindex), "(E)") && !IsFit(a.Substring(startindex), "(e)") && startindex < a.Length) startindex++;
                                if (startindex == a.Length) return -1;
                                else return startindex;
                            }
                        }
                        public string Show(string Text, string[] Option)
                        {
                            ConfirmMultiChoiceAnsForm form = new ConfirmMultiChoiceAnsForm(Text, Option);
                            form.Show();
                            while (!form.readytoclose) Application.DoEvents();
                            string a = form.ToString();
                            form.Dispose();
                            return a;
                        }
                    }
                    public bool readytoclose = false;
                    public TableLayoutPanel Main = new TableLayoutPanel();
                    public TableLayoutPanel Tlp1 = new TableLayoutPanel();
                    public TableLayoutPanel Tlp2 = new TableLayoutPanel();
                    public TableLayoutPanel TagTlp = new TableLayoutPanel();
                    public ITxtBox Txb1 = new ITxtBox();
                    public Button Check = new Button();
                    public Button OK = new Button();
                    public Button Cancel = new Button();
                    public Button[] Btns = new Button[10];
                    public HashSet<string> Completed = new HashSet<string>();
                    public int DetectZhuyin(char a)
                    {
                        if (a >= 'ㄅ' && a <= 'ㄙ') return 1;
                        else if (a >= 'ㄚ' && a <= 'ㄦ') return 3;
                        else if (a >= 'ㄧ' && a <= 'ㄩ') return 2;
                        else if (a == ' ' || a == 'ˊ' || a == 'ˇ' || a == 'ˋ' || a == '˙') return 4;
                        else return 0;
                    }
                    public int ZhuyinIndex(string a,int b)
                    {
                        if (DetectZhuyin(a[b]) == 0)
                        {
                            while (b < a.Length && DetectZhuyin(a[b]) == 0) b++;
                            if (b == a.Length) return -1;
                            else return b;
                        }
                        else
                        {
                            while (b < a.Length && DetectZhuyin(a[b]) > 0) b++;
                            if (b == a.Length) return -1;
                            else return b;
                        }
                    }
                    void Check_Click(object sender, EventArgs e)
                    {
                        string[] a = Txb1.Text.Split('\n');
                        for(int i=0;i<a.Length;i++)
                        {
                            a[i] = a[i].TrimEnd('\r');
                            if (a[i].Length <= 3) continue;
                            string tag = a[i].Remove(3);
                            switch (tag)
                            {
                                case "國字:":
                                    if (!Completed.Contains(a[i]))
                                    {
                                        a[i] = Process國字(a[i]);
                                        if (a[i].Length > 0) Completed.Add(a[i]);
                                    }
                                    break;
                                case "注音:":
                                    if (!Completed.Contains(a[i]))
                                    {
                                        a[i] = Process注音(a[i]);
                                        if (a[i].Length > 0) Completed.Add(a[i]);
                                    }
                                    break;
                                case "通同:":
                                    if (!Completed.Contains(a[i]))
                                    {
                                        a[i] = Process通同(a[i]);
                                        if (a[i].Length > 0) Completed.Add(a[i]);
                                    }
                                    break;
                                case "借代:":
                                case "解釋:":
                                    if (!Completed.Contains(a[i]))
                                    {
                                        a[i] = Process解釋借代(a[i]);
                                        if (a[i].Length > 0) Completed.Add(a[i]);
                                    }
                                    break;
                                case "常識:":
                                case "成語:":
                                case "作者:":
                                    if (!Completed.Contains(a[i]))
                                    {
                                        a[i] = Process常識成語作者(a[i]);
                                        if (a[i].Length > 0) Completed.Add(a[i]);
                                    }
                                    break;
                                case "選擇:":
                                    if (!Completed.Contains(a[i]))
                                    {
                                        a[i] = Process選擇(a[i]);
                                        if (a[i].Length > 0) Completed.Add(a[i]);
                                    }
                                    break;
                                case "季節:":
                                    a[i] = "";
                                    break;
                                default:
                                    MessageBox.Show("SettingTabPage.AddChineseTestPart.Form2.Check_Click(object sender, EventArgs e)\r\ntag==" + tag);
                                    break;
                            }
                        }
                        string c = "";
                        for (int i = 0; i < a.Length; i++)
                        {
                            if (a[i].Length > 0) c += a[i] + "\r\n";
                        }
                        Txb1.Text = c;
                    }
                    public string Process國字(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        string b = "";
                        int[] index = new int[] { 0, ZhuyinIndex(a, 0) };
                        while (index[index.Length - 1] != -1)
                        {
                            Array.Resize(ref index, index.Length + 1);
                            index[index.Length - 1] = ZhuyinIndex(a, index[index.Length - 2]);
                        }
                        index[index.Length - 1] = a.Length;
                        for (int j = 1; j < index.Length; j++)
                        {
                            string k = a.Substring(index[j - 1], index[j] - index[j - 1]);
                            if (j % 2 == 1) b += k.Remove(k.Length - 1) + (j == index.Length - 1 ? k.Substring(k.Length - 1) : "「" + ChineseTest.FillMark.ToString() + k.Substring(k.Length - 1) + ChineseTest.FillMark.ToString());
                            else b += k + "」";
                        }
                        return b;
                    }
                    public string Process注音(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        string b = "";
                        int[] index = new int[] { 0, ZhuyinIndex(a, 0) };
                        while (index[index.Length - 1] != -1)
                        {
                            Array.Resize(ref index, index.Length + 1);
                            index[index.Length - 1] = ZhuyinIndex(a, index[index.Length - 2]);
                        }
                        index[index.Length - 1] = a.Length;
                        for (int j = 1; j < index.Length; j++)
                        {
                            string k = a.Substring(index[j - 1], index[j] - index[j - 1]);
                            if (j % 2 == 1) b += k.Remove(k.Length - 1) + (j == index.Length - 1 ? "" : "「") + k.Substring(k.Length - 1);
                            else b += ChineseTest.FillMark.ToString() + k + ChineseTest.FillMark.ToString() + "」";
                        }
                        return b;
                    }
                    public string Process通同(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        a = a.Remove(3) + a.Substring(3, 1) + "=「" + ChineseTest.FillMark.ToString() + a.Substring(4, 1) +ChineseTest.FillMark.ToString()+ "」";
                        return a;
                    }
                    public string Process解釋借代(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        int b = a.IndexOf(':', 3);
                        int c = a.IndexOf('\'');
                        int d = -1;
                        if (c != -1)
                        {
                            d = a.IndexOf('\'', c + 1);
                            if (d < b && d != -1) a = a.Remove(c) + "「" + a.Substring(c + 1, d - c - 1) + "」" + a.Substring(d + 1);
                        }
                        while (b == -1 || d > b || (c != -1 && d == -1))
                        {
                            a = new CorrectErrorDialog().Show(a, false, "錯誤:\r\n不符合\"解釋借代\"的格式");
                            if (a == null) return null;
                            b = a.IndexOf(':', 3);
                            c = a.IndexOf('\'');
                            if (c != -1)
                            {
                                d = a.IndexOf('\'', c + 1);
                                if (d < b && d != -1) a = a.Remove(c) + "「" + a.Substring(c + 1, d - c - 1) + "」" + a.Substring(d + 1);
                            }
                        }
                        a = a.Remove(b + 1) + ChineseTest.FillMark.ToString() + a.Substring(b + 1) + ChineseTest.FillMark.ToString();
                        return a;
                    }
                    public string Process常識成語作者(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        bool CanBreak=false;
                        string b = "";
                        while (true)
                        {
                            b = "";
                            int index = a.IndexOf('\'');
                            if (index != -1)
                            {
                                b += a.Remove(index);
                                int nowindex = index;
                                index = (index + 1 < a.Length ? a.IndexOf('\'', index + 1) : -1);
                                int marknum = 0;
                                while (index != -1)
                                {
                                    if (marknum % 2 == 0) b += "「" + a.Substring(nowindex, index - nowindex + 1);
                                    else b += "」" + (nowindex + 1 < a.Length ? a.Substring(nowindex + 1, index - nowindex - 1) : "");
                                    nowindex = index;
                                    index = (index + 1 < a.Length ? a.IndexOf('\'', index + 1) : -1);
                                    marknum++;
                                }
                                if (marknum % 2 == 1)
                                {
                                    b += "」" + (nowindex + 1 < a.Length ? a.Substring(nowindex + 1) : "");
                                    CanBreak = true;
                                }
                            }
                            if (CanBreak) break;
                            else
                            {
                                a = new CorrectErrorDialog().Show(a, false, "This isn't the format of \"常識成語作者\". Please correct it.");
                                if (a == null) return null;
                            }
                        }
                        return b;
                    }
                    public string Process選擇(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        //MessageBox.Show(a);
                        int index = OptionIndex(a, 0);
                        int nowindex = index;
                        string b = a.Remove(nowindex);
                        index = OptionIndex(a, index + 1);
                        string[] c = new string[0];
                        while(index!=-1)
                        {
                            Array.Resize(ref c, c.Length + 1);
                            c[c.Length - 1] = a.Substring(nowindex, index - nowindex);
                            nowindex = index;
                            index = OptionIndex(a, index + 1);
                        }
                        Array.Resize(ref c, c.Length + 1);
                        c[c.Length - 1] = a.Substring(nowindex);
                        string d = new ConfirmMultiChoiceAns().Show(b, c);
                        return d;
                    }
                    public string Process季節(string a)
                    {
                        a = ReplaceFulltoHalf(a);
                        return null;//Unfinished
                    }
                    public int OptionIndex(string a, int startindex)
                    {
                        while (!IsFit(a.Substring(startindex), "(E)") && !IsFit(a.Substring(startindex), "(e)") && startindex < a.Length) startindex++;
                        if (startindex == a.Length) return -1;
                        else return startindex;
                    }
                    void OK_Click(object sender, EventArgs e)
                    {
                        readytoclose = true;
                    }
                    void Cancel_Click(object sender, EventArgs e)
                    {
                        Txb1.Text = "";
                        readytoclose = true;
                    }
                    public Form2()
                    {
                        this.WindowState = FormWindowState.Maximized;
                        Main.Dock = DockStyle.Fill;
                        Main.ColumnCount = 2;
                        Main.RowCount = 1;
                        Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 9));
                        this.Controls.Add(Main);
                        TagTlp.Dock = DockStyle.Fill;
                        TagTlp.RowCount = Btns.Length;
                        string[] BtnText = new string[] { "國字:", "注音:", "通同:", "解釋:", "借代:", "常識:", "成語:", "作者:", "選擇:", "季節:" };
                        for (int i = 0; i < Btns.Length; i++)
                        {
                            TagTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                            Btns[i] = new Button();
                            Btns[i].Dock = DockStyle.Fill;
                            Btns[i].Text = BtnText[i];
                            Btns[i].Click += Kind_Click;
                            TagTlp.Controls.Add(Btns[i]);
                            TagTlp.SetCellPosition(Btns[i], new TableLayoutPanelCellPosition(0, i));
                        }
                        Main.Controls.Add(TagTlp); Main.SetCellPosition(TagTlp, new TableLayoutPanelCellPosition(0, 0));
                        Tlp1.Dock = DockStyle.Fill;
                        Tlp1.ColumnCount = 1;
                        Tlp1.RowCount = 2;
                        Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
                        Tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                        Main.Controls.Add(Tlp1); Main.SetCellPosition(Tlp1, new TableLayoutPanelCellPosition(1, 0));
                        Txb1.Dock = DockStyle.Fill;
                        Txb1.Multiline = true;
                        Txb1.Font = ITxtBoxFont;
                        Txb1.ScrollBars = ScrollBars.Both;
                        Tlp1.Controls.Add(Txb1); Tlp1.SetCellPosition(Txb1, new TableLayoutPanelCellPosition(0, 0));
                        Tlp2.Dock = DockStyle.Fill;
                        Tlp2.ColumnCount = 3;
                        Tlp2.RowCount = 1;
                        for (int i = 0; i < Tlp2.ColumnCount; i++) Tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        Tlp1.Controls.Add(Tlp2); Tlp1.SetCellPosition(Tlp2, new TableLayoutPanelCellPosition(0, 1));
                        {
                            Tlp2.Controls.Add(Check); Tlp2.SetCellPosition(Check, new TableLayoutPanelCellPosition(0, 0));
                            {
                                Check.Dock = DockStyle.Fill;
                                Check.Text = "Check";
                                Check.Click += Check_Click;
                            }
                            Tlp2.Controls.Add(OK); Tlp2.SetCellPosition(OK, new TableLayoutPanelCellPosition(1, 0));
                            {
                                OK.Dock = DockStyle.Fill;
                                OK.Text = "OK";
                                OK.Click += OK_Click;
                            }
                            Tlp2.Controls.Add(Cancel); Tlp2.SetCellPosition(Cancel, new TableLayoutPanelCellPosition(2, 0));
                            {
                                Cancel.Dock = DockStyle.Fill;
                                Cancel.Text = "Cancel";
                                Cancel.Click += Cancel_Click;
                            }
                        }
                    }
                    void Kind_Click(object sender, EventArgs e)
                    {
                        string a=Txb1.Text;
                        if (a.Length > 2 && a.Substring(a.Length - 2) != "\r\n") Txb1.AppendText("\r\n" + (sender as Button).Text);
                        else Txb1.AppendText((sender as Button).Text);
                        Txb1.Focus();
                    }
                    public string ReplaceFulltoHalf(string a)
                    {
                        a = a.Replace('）', ')').Replace('（', '(').Replace('，', ',').Replace('；', ';').Replace('：', ':').Replace('’', '\'').Replace('‵', '`').Replace('？', '?').Replace('！', '!').Replace('～', '~').Replace('＜','<').Replace('＝','=').Replace('＞','>')
                            .Replace("...", "…").Replace("「" + ChineseTest.FillMark.ToString(), "").Replace(ChineseTest.FillMark.ToString() + "」", "").Replace("「", "").Replace("」", "").Replace(" ", "");
                        string b = "";
                        for(int i=0;i<a.Length;i++)
                        {
                            if (a[i] >= '０' && a[i] <= '９') b += ((char)(a[i] - '０' + '0')).ToString();
                            else if (a[i] >= 'Ａ' && a[i] <= 'Ｚ') b += ((char)(a[i] - 'Ａ' + 'A')).ToString();
                            else if (a[i] >= 'ａ' && a[i] <= 'ｚ') b += ((char)(a[i] - 'ａ' + 'a')).ToString();
                            else b += a[i].ToString();
                        }
                        return b;
                    }
                }
                public GroupBox Main;
                public Button[] Btns;
                public Form2 form;
                public void Reset()
                {
                    Main = new GroupBox();
                    Main.Dock = DockStyle.Top;
                    Main.Text = "Add Chinese Tests";
                    Setting.Tab.Controls.Add(Main);
                    Btns = new Button[1];//{Chinese,Zhuyin,常識,解釋,成語,作者,季節,通同,借代,MultiChoice}
                    Btns[0] = new Button();
                    Btns[0].Dock = DockStyle.Top;
                    Btns[0].Text = "Chinese";
                    Btns[0].Click += Chinese_Click;
                    Main.Controls.Add(Btns[0]);
                }
                private void Chinese_Click(object sender, EventArgs e)
                {
                    if (form != null) form.Close();
                    form = new Form2();
                    form.Show();
                    while (!form.readytoclose) Application.DoEvents();
                    //Get result from ChineseString
                    if (form.Txb1.Text.Length > 0)
                    {
                        StreamWriter writer = new StreamWriter(ChineseTest.Path, true, Encoding.Default);
                        string[] a = form.Txb1.Text.Split('\n');
                        for(int i=0;i<a.Length;i++)
                        {
                            a[i] = a[i].Trim('\r');
                            if (a[i].Length == 0) continue;
                            writer.WriteLine(a[i]);
                        }
                        writer.Close();
                        MessageBox.Show(a.Length.ToString() + " Words Added!");
                        ChineseTest.Load();
                        ChineseTest.Start();
                    }
                }
            }
        }
        public struct AboutCreatorTabPage
        {
            public TabPage Tab;
            public TextBox Txb;
            public void Reset()
            {
                Tab = new TabPage();
                Tab.Text = "About Me";
                Txb = new TextBox();
                Tab.Controls.Add(Txb);
                Txb.Dock = DockStyle.Fill;
                Txb.Font = new Font("標楷體", 20, FontStyle.Regular);
                Txb.WordWrap = true;
                Txb.Multiline = true;
                Txb.AppendText("嗨!為了讓大家更容易使用，我把這裡改成中文版了，以下是我的聯絡方式:\r\n");
                Txb.AppendText("我叫余柏序(英文名字:Burney，飾演主角:莫帝威)\r\n");
                Txb.AppendText("電子信箱: fsps60312@yahoo.com.tw\r\n");
                Txb.AppendText("臉書Facebook: https://www.facebook.com/fsps60312\r\n");
                Txb.AppendText("粉專(請往下滑找到我倒數五天的倒數文): https://www.facebook.com/KSHS105.Mobius\r\n");
                Txb.AppendText("電話: 0919508359\r\n");
                Txb.AppendText("請注意，因為我要準備資訊奧林匹亞競賽，很可惜的將暫時停止此軟體之維護，相反的，我可以跟你說遇到什麼樣的問題要用甚麼方法解決 :D\r\n\r\n");
                Txb.AppendText("請注意，題庫要自己建立，不過我建立題庫時不是自己打字，我是將書上的頁面拍下來，然後用Abbyy Fine Reader(破解版)讀取圖片上的文字\r\n");
                Txb.AppendText("圖庫中同一道題目要塞進同一行\r\n");
                Txb.AppendText("英文測驗存檔格式說明:\r\n");
                Txb.AppendText("英文單字`中文解釋\r\n");
                Txb.AppendText("填充:題目一部分`填空的地方`題目一部分`填空的地方`題目的一部分......(依此類推)\r\n");
                Txb.AppendText("選擇:題目(一個英文字母小寫代表答案)選項一(一個英文字母小寫代表答案)選項二......(依此類推)\r\n");
                Txb.AppendText("國文測驗存檔格式說明:\r\n");
                Txb.AppendText("注音國字常識解釋成語作者季節通同借代九選一:題目一部分`填空的地方`題目一部分`填空的地方`題目的一部分......(依此類推)\r\n");
                Txb.AppendText("選擇:題目(一個英文字母小寫代表答案)選項一(一個英文字母小寫代表答案)選項二......(依此類推)\r\n");
                Txb.AppendText("化學測驗存檔格式說明:\r\n");
                Txb.AppendText("和國文一樣，只是選項只有填充和選擇\r\n");
                Txb.AppendText("範例可參考隨附之三個文字檔\r\n");
            }
            public void Add()
            {
                All.Tab.TabPages.Add(Tab);
            }
        }
        public static ChineseTestTabPart ChineseTest = new ChineseTestTabPart();
        public static ChemistryTestTabPart ChemistryTest = new ChemistryTestTabPart();
        public static DictionaryTabPart Dictionary = new DictionaryTabPart();
        public static SettingTabPage Setting = new SettingTabPage();
        public static AboutCreatorTabPage AboutCreator = new AboutCreatorTabPage();
        public static Random random = new Random((int)DateTime.Now.ToBinary());
        public static Queue<Keys> KeysQueue = new Queue<Keys>();
        public static bool IsFit(string a,string b)
        {
            if (a.Length < b.Length) return false;
            for(int i=0;i<b.Length;i++)
            {
                switch (b[i])
                {
                    case 'e':
                        if (a[i] >= 'a' && a[i] <= 'z') break;
                        else return false;
                    case 'E':
                        if (a[i] >= 'A' && a[i] <= 'Z') break;
                        else return false;
                    case 'n':
                        if (a[i] >= '0' && a[i] <= '9') break;
                        else return false;
                    case 'c': break;
                    default:
                        if (a[i] == b[i]) break;
                        else return false;
                }
            }
            return true;
        }
        public static string DateToString(DateTime a)
        {
            return a.Year.ToString().PadLeft(4, '0') + "/" + a.Month.ToString().PadLeft(2, '0') + "/" + a.Day.ToString().PadLeft(2, '0');
        }
        void Form1_Shown(object sender, EventArgs e)
        {
            this.Text = "Initializing";
            All.Reset();
            this.Controls.Add(All.Tab);
            this.Text = "Loading";
            EnglishTest.Load();
            ChineseTest.Load();
            ChemistryTest.Load();
            this.Text = "Loaded";
            EnglishTest.Start();
            ChineseTest.Start();
            ChemistryTest.Start();
            this.Text = "All Finished";
            All.Tab.SelectedIndex = Dictionary.TabIndex;
            Dictionary.FocusInsert(null, null);
        }
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!EnglishTest.NeedSave)
            {
                All.Tab.SelectTab(EnglishTest.TabIndex);
                DialogResult dialogresult = MessageBox.Show("要儲存英文測驗紀錄嗎?", "測驗軟體", MessageBoxButtons.YesNoCancel);
                if (dialogresult == DialogResult.Yes)
                {
                    EnglishTest.Save();
                }
                else if(dialogresult!=DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if(!ChineseTest.IsSaved)
            {
                All.Tab.SelectTab(ChineseTest.TabIndex);
                DialogResult dialogresult = MessageBox.Show("要儲存國文測驗紀錄嗎?", "測驗軟體", MessageBoxButtons.YesNoCancel);
                if (dialogresult == DialogResult.Yes)
                {
                    ChineseTest.Save();
                }
                else if (dialogresult != DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (!ChemistryTest.IsSaved)
            {
                All.Tab.SelectTab(ChemistryTest.TabIndex);
                DialogResult dialogresult = MessageBox.Show("要儲存化學測驗紀錄嗎?", "測驗軟體", MessageBoxButtons.YesNoCancel);
                if (dialogresult == DialogResult.Yes)
                {
                    ChemistryTest.Save();
                }
                else if (dialogresult != DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Process.GetCurrentProcess().Kill();
        }
        public struct CorrectErrorDialog
        {
            public partial class CorrectErrorDialogForm : Form
            {
                public ITxtBox txb1 = new ITxtBox();
                TableLayoutPanel tlp1 = new TableLayoutPanel();
                TableLayoutPanel tlp2 = new TableLayoutPanel();
                Button btn1 = new Button();
                Button btn2 = new Button();
                public bool readytoclose = false;
                public CorrectErrorDialogForm(string Original, bool HasButton, string FormText)
                {
                    this.Text = FormText;
                    txb1.Dock = DockStyle.Fill;
                    txb1.Font = ITxtBoxFont;
                    txb1.Multiline = true;
                    if (HasButton)
                    {
                        txb1.ScrollBars = ScrollBars.Both;
                        tlp1.Dock = DockStyle.Fill;
                        tlp1.ColumnCount = 1;
                        tlp1.RowCount = 2;
                        tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
                        tlp1.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
                        tlp2.Dock = DockStyle.Fill;
                        tlp2.ColumnCount = 2;
                        tlp2.RowCount = 1;
                        tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        tlp2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
                        btn1.Dock = DockStyle.Fill;
                        btn1.Text = "OK";
                        btn2.Dock = DockStyle.Fill;
                        btn2.Text = "Skip";
                        tlp2.Controls.AddRange(new Control[] { btn1, btn2 });
                        tlp2.SetCellPosition(btn1, new TableLayoutPanelCellPosition(0, 0));
                        tlp2.SetCellPosition(btn2, new TableLayoutPanelCellPosition(1, 0));
                        tlp1.Controls.AddRange(new Control[] { txb1, tlp2 });
                        tlp1.SetCellPosition(txb1, new TableLayoutPanelCellPosition(0, 0));
                        tlp1.SetCellPosition(tlp2, new TableLayoutPanelCellPosition(0, 1));
                        btn1.Click += btn1_Click;
                        btn2.Click += btn2_Click;
                        this.Controls.Add(tlp1);
                    }
                    else
                    {
                        txb1.WordWrap = true;
                        this.KeyPreview = true;
                        this.KeyUp += CorrectErrorDialogForm_KeyUp;
                        this.Controls.Add(txb1);
                    }
                    txb1.Text = Original;
                }
                void btn1_Click(object sender, EventArgs e)
                {
                    readytoclose = true;
                }
                void btn2_Click(object sender, EventArgs e)
                {
                    txb1.Text = null;
                    readytoclose = true;
                }
                void CorrectErrorDialogForm_KeyUp(object sender, KeyEventArgs e)
                {
                    if (e.KeyData == Keys.Escape) btn2_Click(null, null);
                    else if (e.KeyData == Keys.Enter)
                    {
                        if(txb1.Focused)
                        {
                            string a = txb1.Text;
                            int b = txb1.SelectionStart;
                            txb1.Text = a.Remove(b - 2) + a.Substring(b);
                        }
                        btn1_Click(null, null);
                    }
                }
            }
            public string Show(string Original, bool HasButton, string FormText)
            {
                CorrectErrorDialogForm form = new CorrectErrorDialogForm(Original, HasButton, FormText);
                form.Show();
                while (!form.readytoclose) Application.DoEvents();
                string a = form.txb1.Text;
                form.Dispose();
                return a;
            }
        }
    }
}

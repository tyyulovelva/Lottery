using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace Lottery
{
    /// <summary>
    /// 產生樂透號碼，並依照期數提供查詢
    /// </summary>
    public partial class Lottery : Form
    {
        LotteryNum lotteryNum = new LotteryNum();      //初始化產生樂透號碼物件

        public Lottery()
        {
            InitializeComponent();
        }

        //畫面載入
        private void Lottery_Load(object sender, EventArgs e)
        {

        }

        //檢查輸入的值是否可轉為int型態的整數  (防呆機制)
        public int CheckInputNumber(string text)
        {
            int num = 0;

            //將輸入的值嘗試轉換int型態，若可轉，回傳true
            bool isNum = int.TryParse(text, out num);

            //可轉為int型態的整數
            if (isNum)
            {
                return num;
            }
            else
            {
                //不可轉換
                return -1;
            }
        }


        //按下開獎
        private void btnLotteryNum_Click(object sender, EventArgs e)
        {
            //產生樂透號碼，並將訊息回傳
            this.lblPeriod.Text = lotteryNum.CreateLotteryNumber(); 
               
            //將樂透號碼顯示畫面上
            this.lotteryNum1.Text = lotteryNum.listLotteryNum[0].ToString();    
            this.lotteryNum2.Text = lotteryNum.listLotteryNum[1].ToString();
            this.lotteryNum3.Text = lotteryNum.listLotteryNum[2].ToString();
            this.lotteryNum4.Text = lotteryNum.listLotteryNum[3].ToString();
            this.lotteryNum5.Text = lotteryNum.listLotteryNum[4].ToString();
            this.lotteryNum6.Text = lotteryNum.listLotteryNum[5].ToString();
            this.lotterySpecNum.Text = lotteryNum.specialNum.ToString();
        }

        //查詢第 ? 期樂透號碼與中獎號碼
        private void btnQuery_Click(object sender, EventArgs e)
        {
            string msgLotteryInfo = string.Empty;                //樂透號碼資訊訊息顯示
            string msgWinNumberInfo = string.Empty;              //中獎號碼資訊訊息顯示
            string[] lotteryInfo = new string[8];
            List<String> winLotteryInfo = new List<string>();
            this.listboxWin.Items.Clear();                       //清空項目

            //檢查輸入的值是否可以轉為數字 int 型態
            int period = CheckInputNumber(this.period.Text);

            //輸入的值可轉為數字 int 型態
            if (period != -1 && period != 0)
            {
                //傳入查詢期數，並回傳該期樂透資訊
                lotteryInfo = lotteryNum.QueryLotteryNumber(period);

                //查詢的期數若有開獎
                if (lotteryInfo[0] != null)
                {
                    //接收回傳的中獎號碼 list
                    winLotteryInfo = lotteryNum.QueryWinNumber(lotteryInfo, period);

                    //有中獎號碼
                    if (winLotteryInfo.Count != 0)
                    {
                        //將list清單使用foreach逐一取出
                        foreach (string listWin in winLotteryInfo)
                        {
                            //新增在listbox上
                            this.listboxWin.Items.Add(listWin);
                        }
                        msgWinNumberInfo = "第 " + period + " 期中獎清單如下：";
                    }
                    else
                    {
                        msgWinNumberInfo = "很遺憾地，第 " + period + " 期您無中獎！";
                    }
                    //訊息顯示
                    msgLotteryInfo = "第 " + period + " 期樂透開獎日期 " + lotteryInfo[7] + " 的開獎號碼如下：";   // lotteryInfo[7] 樂透開獎日期
                    this.lotteryPeriodNum1.Text = lotteryInfo[0];          //樂透第1個號碼
                    this.lotteryPeriodNum2.Text = lotteryInfo[1];          //樂透第2個號碼
                    this.lotteryPeriodNum3.Text = lotteryInfo[2];          //樂透第3個號碼
                    this.lotteryPeriodNum4.Text = lotteryInfo[3];          //樂透第4個號碼
                    this.lotteryPeriodNum5.Text = lotteryInfo[4];          //樂透第5個號碼
                    this.lotteryPeriodNum6.Text = lotteryInfo[5];          //樂透第6個號碼
                    this.lotteryPeriodSpecNum.Text = lotteryInfo[6];       //特別號
                }
                else
                {
                    msgLotteryInfo = "第 " + period + " 期樂透尚未開獎！";
                }
            }
            else   //輸入的值無法轉為int型態，如字串、小數
            {
                msgLotteryInfo = "請輸入不含小數點的正整數數字";
            }

            //訊息顯示
            this.lblPeriodOld.Text = msgLotteryInfo;
            this.lblWinListMsg.Text = msgWinNumberInfo;
        }

        //下注
        private void btnWager1000_Click(object sender, EventArgs e)
        {
            //檢查輸入的值是否可以轉為數字 int 型態
            int num = CheckInputNumber(this.txtWagerNum.Text);

            string msg = string.Empty;

            //輸入的值可轉為數字 int 型態
            if (num != -1 && num != 0)
            {
                //呼叫 LotteryNum 產生下注的號碼，並回寫資料庫
                string sPeriod = lotteryNum.DoWager(num);
                msg = "樂透第 " + sPeriod + " 期下注 " + num + " 張完成";
            }
            else
            {
                msg = "請輸入不含小數點的正整數數字";
            }
            //訊息顯示
            this.lblWagerMsg.Text = msg;
        }
    }
}

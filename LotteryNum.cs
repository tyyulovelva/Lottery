using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery
{
    /// <summary>
    /// 1.產生不重複的樂透號碼與特別號，記錄在list上，再呼叫資料庫物件並寫入資料庫
    /// 2.比對下注的樂透號碼，查詢中獎並回寫資料庫內容
    /// </summary>

    public class LotteryNum
    {
        
        public int[] listLotteryNum;             //六個不重複的樂透號碼
        public int specialNum;                   //特別號
        private int index = 0;                   // listLotteryNum 的 index
        DBConnect dbConnect = new DBConnect();   //初始化DB連線
        public string[] periodLottery;           //儲存回傳的樂透號碼、日期
        Random rnd = new Random();


        //建構子
        public LotteryNum()
        {
            //初始化樂透號碼
            listLotteryNum = new int[6];
            specialNum = 0;
            index = 0;
            periodLottery = new String[8];
        }

        //本期樂透開獎，產生樂透號碼及特別號
        public string CreateLotteryNumber()
        {
            //初始化樂透號碼
            listLotteryNum = new int[6];

            //取得本期期數
            string sPeriod = dbConnect.CountLotteryPeriod();        
            string msg = string.Empty;

            //初始化預設值
            index = 0;

            //期數不為空
            if (sPeriod != "0")  
            {
                msg = "第 " + sPeriod + " 期樂透彩開獎，號碼如下 (已排序)";

                //亂數產生6個不重複樂透號碼
                RandomLotteryNumber();

                //亂數產生與樂透號碼不重複的特別號
                RandomSpecialNumber();

                //排序樂透號碼
                Array.Sort(listLotteryNum); 

                //將本期樂透號碼、特別號與對應注數寫入資料庫
                dbConnect.ExecuteSqlCommand(listLotteryNum, specialNum, dbConnect.GetMaxWager() - 1);
            }
            else  //取不到期數
            {
                msg = "資料庫連線有狀況，請確認連線是否正常";
            }

            //訊息回傳
            return msg;
        }

        //查詢第 ? 期樂透號碼與特別號
        public string[] QueryLotteryNumber(int period)
        {
            //初始化樂透資訊
            periodLottery = new String[8];

            //取得該期的樂透號碼
            periodLottery = dbConnect.GetLotteryInfo(period);

            return periodLottery;
        }

        //亂數取樂透號碼，不可重複
        public void RandomLotteryNumber()
        {
            int num;
            int i;
            for (i = 1; i <= listLotteryNum.Length; i++)   
            {
                //亂數，1-49，不可超過50
                num = rnd.Next(1, 50);

                //檢查重複
                checkRepeat(num);         
            }            
        }

        //亂數取特別號，不可與樂透號碼重複
        public void RandomSpecialNumber()
        {
            int num = rnd.Next(1, 50);

            //檢查重複
            checkRepeat(num);
        }

        //檢查重複
        private void checkRepeat(int num)
        {
            int i;
            bool isRepeat = false;
            for (i = 0; i < listLotteryNum.Length; i++)
            {
                if (listLotteryNum[i] != 0)
                {
                    if (listLotteryNum[i] == num)
                    {
                        isRepeat = true;
                        break;
                    }
                }
            }
            //若重複，重新取亂數
            if (isRepeat)  
            {
                num = rnd.Next(1, 50);
                //再次檢查重複
                checkRepeat(num);   
            }
            else
            {
                if (index == 6)
                {
                    specialNum = num;
                }
                else
                {
                    //未重複，將號碼記錄到list內
                    listLotteryNum[index] = num;
                    index++;
                }
            }
        }

        //下注，需要6個樂透號碼，不需要特別號
        public string DoWager(int round)
        {
            int i;

            //取得目前下注的最大期數
            int wager = dbConnect.GetMaxWager();

            for (i = 1; i <= round; i++)
            {
                index = 0;
                listLotteryNum = new int[6];

                //產生6個不重複的號碼
                RandomLotteryNumber();    

                //排序樂透號碼
                Array.Sort(listLotteryNum);

                //將號碼批次寫入資料庫
                dbConnect.ExecuteWager(listLotteryNum, wager);
            }
            //取得目前樂透最大期數
            string sPeriod = dbConnect.CountLotteryPeriod();
            return sPeriod;
        }

        //查詢中獎
        public List<String> QueryWinNumber(string[] lotteryInfo, int period)
        {
            //查詢中獎
            //dbConnect.GetWagerLotteryNumber(dbConnect.GetMaxWager() - 1, listLotteryNum, specialNum, sPeriod);  //C# While 迴圈方式讀取並更新資料庫  1000筆資料量耗時19秒
            
            //比對該期下注的樂透號碼，將有中獎的該組號碼註記獎項
            //呼叫 SQL StoredProcedure 操作  1000筆資料量耗時8秒
            dbConnect.UpdWagerSQLProcedure(period, lotteryInfo);

            //將有中獎的號碼清單組成list，回傳
            List<String> winLotteryInfo = new List<string>();   //宣告一 list，讀取出來的中獎清單存入
            winLotteryInfo = dbConnect.QueryWinLotteryInfo(period);
            return winLotteryInfo;
        }

        //取得目前開獎的最大期數，以便下次開獎時+1
        public string CountLottery()
        {
            string sPeriod = dbConnect.CountLotteryPeriod();
            return sPeriod;
        }
    }
}

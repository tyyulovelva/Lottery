using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Lottery
{
    /// <summary>
    /// 1.連線DB
    /// 2.操作交談指令
    /// </summary>
    public class DBConnect
    {
        //連線資料庫的Procedure名稱
        private string insProcedure = "ins_LotteryNumberStored";                                      //ins procedure 名稱，將樂透號碼塞入資料庫
        private string getProcedure = "get_LotteryNumber";                                            //get procedure 名稱，取得第 ? 期樂透號碼
        private string countProcedure = "count_LotteryNumber";                                        //count procedure 名稱，計算總樂透期數
        private string insWagerProcedure = "ins_LotteryWager";                                        //下注的procedure名稱
        private string getWagerProcedure = "get_MaxWager";                                            //取得最大下注次數
        private string getWagerNumberProcedure = "get_LotteryWagerNumber";                            //取得該下注次數的所有樂透號碼
        private string updWagerNumberProcedure = "upd_LotteryWager";                                  //下注的號碼中獎時，更新獎項
        private string updWagerSQLProcedure = "upd_list_WinLottery";                                  //下注的號碼中獎時，更新獎項  使用SQL Procedure
        private string selWinLotteryInfo = "get_WinLotteryNumber";                                    //查詢中獎的樂透資訊
        
        public int[] wagerLotteryList = new int[6];                                                   //儲存自資料庫取得的下注的樂透號碼
        public int[] lotteryList = new int[6];                                                        //儲存自資料庫取得的本期開獎的樂透號碼及特別號
        LotteryWin lotteryWin = new LotteryWin();                                                     //查詢下注的號碼是否中獎以及獎項

        //建構子
        public DBConnect()
        {

        }

        //執行SQL command並寫入資料庫，將開獎號碼寫入
        public void ExecuteSqlCommand(int[] listLotteryNum, int specialNum, int wager)
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = insProcedure;
                
                //傳遞procedure參數
                cmd.Parameters.Add("@wager", SqlDbType.Int).Value = wager;
                cmd.Parameters.Add("@number1", SqlDbType.Int).Value = listLotteryNum[0];   //procedure內的變數@number1 給值
                cmd.Parameters.Add("@number2", SqlDbType.Int).Value = listLotteryNum[1];
                cmd.Parameters.Add("@number3", SqlDbType.Int).Value = listLotteryNum[2];
                cmd.Parameters.Add("@number4", SqlDbType.Int).Value = listLotteryNum[3];
                cmd.Parameters.Add("@number5", SqlDbType.Int).Value = listLotteryNum[4];
                cmd.Parameters.Add("@number6", SqlDbType.Int).Value = listLotteryNum[5];
                cmd.Parameters.Add("@number7", SqlDbType.Int).Value = specialNum;

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行

            }
            catch (Exception e1)
            {
                Console.WriteLine("[insProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[insProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }

        //獲取第 ? 期的樂透號碼、特別號及開獎日期
        public string[] GetLotteryInfo(int period)
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線
            SqlDataReader dr = null;            //建構讀取資料庫工具

            String[] periodLottery = new String[8];
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = getProcedure;

                //傳遞procedure參數
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = period;    //傳入第 ? 期的期數給 procedure

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行

                //讀取內容
                dr = cmd.ExecuteReader();

                //有資料
                if (dr.HasRows)
                {
                    //開始讀取，直到讀完所有的 row
                    while (dr.Read())
                    {
                        //將各欄位的值回傳
                        periodLottery[0] = dr["f_number1"].ToString();                         //樂透的第1個號碼
                        periodLottery[1] = dr["f_number2"].ToString();                         //樂透的第2個號碼
                        periodLottery[2] = dr["f_number3"].ToString();                         //樂透的第3個號碼
                        periodLottery[3] = dr["f_number4"].ToString();                         //樂透的第4個號碼
                        periodLottery[4] = dr["f_number5"].ToString();                         //樂透的第5個號碼
                        periodLottery[5] = dr["f_number6"].ToString();                         //樂透的第6個號碼
                        periodLottery[6] = dr["f_specialNum"].ToString();                      //特別號
                        periodLottery[7] = dr["f_createDate"].ToString().Substring(0, 10);     //樂透開獎日期
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
            return periodLottery;   //回傳查詢結果
        }
        //計算已開獎的樂透期數
        public string CountLotteryPeriod()
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線
            SqlDataReader dr = null;            //建構讀取資料庫工具
            int period = 0;                     //期數
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = countProcedure;

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行

                //讀取內容
                dr = cmd.ExecuteReader();

                //有資料
                if (dr.HasRows)
                {
                    //開始讀取，直到讀完所有的 row
                    while (dr.Read())
                    {
                        //將各欄位的值回傳
                        period = int.Parse(dr["f_count"].ToString());
                    }
                }
                period += 1;   //取出的期數為已開的期數，+1代表本期要開獎的期數
            }
            catch (Exception e1)
            {
                Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
            return period + "";
        }

        //下注
        public void ExecuteWager(int[] listLotteryNum, int wager)
        {
            //取得目前樂透最大期數
            string sPeriod = CountLotteryPeriod();

            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = insWagerProcedure;

                //傳遞procedure參數
                cmd.Parameters.Add("@period", SqlDbType.Int).Value = int.Parse(sPeriod);
                cmd.Parameters.Add("@wager", SqlDbType.Int).Value = wager;
                cmd.Parameters.Add("@number1", SqlDbType.Int).Value = listLotteryNum[0];    //procedure內的變數@number1 給值
                cmd.Parameters.Add("@number2", SqlDbType.Int).Value = listLotteryNum[1];
                cmd.Parameters.Add("@number3", SqlDbType.Int).Value = listLotteryNum[2];
                cmd.Parameters.Add("@number4", SqlDbType.Int).Value = listLotteryNum[3];
                cmd.Parameters.Add("@number5", SqlDbType.Int).Value = listLotteryNum[4];
                cmd.Parameters.Add("@number6", SqlDbType.Int).Value = listLotteryNum[5];

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行
            }
            catch (Exception e1)
            {
                Console.WriteLine("[insProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[insProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }

        //取得最大下注次數
        public int GetMaxWager()
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線
            SqlDataReader dr = null;            //建構讀取資料庫工具
            int maxWager = 0;

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = getWagerProcedure;

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行

                //讀取內容
                dr = cmd.ExecuteReader();

                //有資料
                if (dr.HasRows)
                {
                    //開始讀取，直到讀完所有的 row
                    while (dr.Read())
                    {
                        //將各欄位的值回傳
                        maxWager = int.Parse(dr["f_wager"].ToString());
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
            return maxWager;
        }

        //取得該下注次數的所有樂透號碼
        public void GetWagerLotteryNumber(int wager, int[] list, int specialNum, string period)
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線
            SqlDataReader dr = null;            //建構讀取資料庫工具
            int winAwards = 0;  //贏得的獎項
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = getWagerNumberProcedure;

                //傳遞procedure參數
                cmd.Parameters.Add("@wager", SqlDbType.Int).Value = wager;

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行

                //讀取內容
                dr = cmd.ExecuteReader();

                //有資料
                if (dr.HasRows)
                {
                    //開始讀取，直到讀完所有的 row
                    while (dr.Read())
                    {
                        //將各欄位的值回傳
                        wagerLotteryList[0] = int.Parse(dr["f_number1"].ToString());
                        wagerLotteryList[1] = int.Parse(dr["f_number2"].ToString());
                        wagerLotteryList[2] = int.Parse(dr["f_number3"].ToString());
                        wagerLotteryList[3] = int.Parse(dr["f_number4"].ToString());
                        wagerLotteryList[4] = int.Parse(dr["f_number5"].ToString());
                        wagerLotteryList[5] = int.Parse(dr["f_number6"].ToString());
                        //比對下注的所有號碼與本期樂透號碼，並接收中獎獎項
                        winAwards = lotteryWin.CompareWagerAndLootery(list, wagerLotteryList, specialNum, wager);
                        //將有中獎的號碼回寫獎項
                        UpdWagerWinAwards(winAwards, wager);
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }


        //下注的所有號碼中，將有中獎的號碼更新中獎的獎項
        public void UpdWagerSQLProcedure(int period, string[] list)
        {
            //將該期的6個樂透號碼組合，以供procedure比對使用
            string lotteryList = list[0] + "," + list[1] + "," + list[2] + "," + list[3] + "," + list[4] + "," + list[5] + ",";

            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = updWagerSQLProcedure;

                //傳遞procedure參數
                cmd.Parameters.Add("@specialNum", SqlDbType.Int).Value = int.Parse(list[6]);   //特別號
                cmd.Parameters.Add("@period", SqlDbType.Int).Value = period;                   //查詢的期數
                cmd.Parameters.Add("@lotteryList", SqlDbType.VarChar).Value = lotteryList;     //該期的樂透號碼

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行
            }
            catch (Exception e1)
            {
                Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }


        //更新中獎的下注號碼獎項
        public void UpdWagerWinAwards(int winAwards, int wager)
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = updWagerNumberProcedure;

                //傳遞procedure參數
                cmd.Parameters.Add("@win", SqlDbType.Int).Value = winAwards;
                cmd.Parameters.Add("@wager", SqlDbType.Int).Value = wager;
                cmd.Parameters.Add("@number1", SqlDbType.Int).Value = wagerLotteryList[0];    //procedure內的變數@number1 給值
                cmd.Parameters.Add("@number2", SqlDbType.Int).Value = wagerLotteryList[1];
                cmd.Parameters.Add("@number3", SqlDbType.Int).Value = wagerLotteryList[2];
                cmd.Parameters.Add("@number4", SqlDbType.Int).Value = wagerLotteryList[3];
                cmd.Parameters.Add("@number5", SqlDbType.Int).Value = wagerLotteryList[4];
                cmd.Parameters.Add("@number6", SqlDbType.Int).Value = wagerLotteryList[5];

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行
            }
            catch (Exception e1)
            {
                Console.WriteLine("[insProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[insProcedure] Message: {0}", e1.Message);
            }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }

        //查詢中獎的下注號碼，回傳資訊
        public List<String> QueryWinLotteryInfo(int period)
        {
            SqlCommand cmd = new SqlCommand();   //建構交談指令
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);  //App.config內容，資料庫連線
            SqlDataReader dr = null;            //建構讀取資料庫工具
            List<String> winLotteryInfo = new List<string>();   //宣告一 list，讀取出來的中獎清單存入

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = selWinLotteryInfo;

                //傳遞procedure參數
                cmd.Parameters.Add("@period", SqlDbType.Int).Value = period;

                cmd.Connection.Open();   //開啟連線
                cmd.ExecuteNonQuery();   //執行

                //讀取內容
                dr = cmd.ExecuteReader();

                //有資料
                if (dr.HasRows)
                {
                    //開始讀取，直到讀完所有的 row
                    while (dr.Read())
                    {
                        //將各欄位的值回傳
                        winLotteryInfo.Add(dr["f_info"].ToString());    //讀取出來的中獎清單存入list
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
             }
            finally
            {
                //清空參數
                cmd.Parameters.Clear();

                //關閉連線
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
            return winLotteryInfo;
        }
    }
}

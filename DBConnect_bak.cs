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
        private string sDBServer = "server=(local); database=TsungYung; Integrated Security=SSPI;";   //連接本機local的資料庫TsungYung，使用windows身分驗證SSPI
        private SqlCommand command;                                                                   //資料庫交談指令
        private SqlDataAdapter sqlDA = new SqlDataAdapter();                                          //將資料填入 DataSet
        private DataSet ds = new DataSet();                                                           //資料庫資料集
        
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
            //using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            //{
            //    using (command = dbconnect.CreateCommand())
            //    {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TsungYung"].ConnectionString);
                    try
                    {
                        //dbconnect.Open();
                        //command.Connection = dbconnect;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = insProcedure;
                        //command.Parameters["@wager"].Value = wager; 
                        //command.Parameters["@number1"].Value = listLotteryNum[0];    //procedure內的變數@number1 給值
                        //command.Parameters["@number2"].Value = listLotteryNum[1];
                        //command.Parameters["@number3"].Value = listLotteryNum[2];
                        //command.Parameters["@number4"].Value = listLotteryNum[3];
                        //command.Parameters["@number5"].Value = listLotteryNum[4];
                        //command.Parameters["@number6"].Value = listLotteryNum[5];
                        //command.Parameters["@number7"].Value = specialNum;
                        cmd.Parameters.Add("@wager", SqlDbType.Int).Value = wager;
                        cmd.Parameters.Add("@number1", SqlDbType.Int).Value = listLotteryNum[0];
                        cmd.Parameters.Add("@number2", SqlDbType.Int).Value = listLotteryNum[1];
                        cmd.Parameters.Add("@number3", SqlDbType.Int).Value = listLotteryNum[2];
                        cmd.Parameters.Add("@number4", SqlDbType.Int).Value = listLotteryNum[3];
                        cmd.Parameters.Add("@number5", SqlDbType.Int).Value = listLotteryNum[4];
                        cmd.Parameters.Add("@number6", SqlDbType.Int).Value = listLotteryNum[5];
                        cmd.Parameters.Add("@number7", SqlDbType.Int).Value = specialNum;
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        //sqlDA.SelectCommand = cmd;
                        //SqlCommandBuilder.DeriveParameters(cmd);
                        //sqlDA.Fill(ds);                                              //執行
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[insProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[insProcedure] Message: {0}", e1.Message);
                    }
                    finally
                    {
                        cmd.Parameters.Clear();
                    }
            //    }
            //}
        }

        //獲取第 ? 期的樂透號碼、特別號及開獎日期
        public string[] GetLotteryInfo(int period)
        {
            String[] periodLottery = new String[8];
            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = getProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = period;    //傳入第 ? 期的期數給 procedure
                        SqlDataReader dr = null;
                        dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
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
                    return periodLottery;   //回傳查詢結果
                }
            }
        }
        //計算已開獎的樂透期數
        public string CountLotteryPeriod()
        {
            int period = 0;
            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = countProcedure;
                        SqlDataReader dr = null;
                        dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                period = int.Parse(dr["f_count"].ToString());
                            }
                        }
                        period += 1;
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
                    }
                    return period + "";
                }
            }
        }

        //下注
        public void ExecuteWager(int[] listLotteryNum, int wager)
        {
            //取得目前樂透最大期數
            string sPeriod = CountLotteryPeriod();

            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = insWagerProcedure;
                        sqlDA.SelectCommand = command;
                        SqlCommandBuilder.DeriveParameters(command);
                        command.Parameters["@period"].Value = int.Parse(sPeriod);
                        command.Parameters["@wager"].Value = wager;
                        command.Parameters["@number1"].Value = listLotteryNum[0];    //procedure內的變數@number1 給值
                        command.Parameters["@number2"].Value = listLotteryNum[1];
                        command.Parameters["@number3"].Value = listLotteryNum[2];
                        command.Parameters["@number4"].Value = listLotteryNum[3];
                        command.Parameters["@number5"].Value = listLotteryNum[4];
                        command.Parameters["@number6"].Value = listLotteryNum[5];
                        sqlDA.Fill(ds);                                              //執行
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[insProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[insProcedure] Message: {0}", e1.Message);
                    }
                }
            }
        }

        //取得最大下注次數
        public int GetMaxWager()
        {
            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        int maxWager = 0;
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = getWagerProcedure;
                        SqlDataReader dr = null;
                        dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                maxWager = int.Parse(dr["f_wager"].ToString());
                            }
                            return maxWager;
                        }
                        else
                        {   //無下注資料
                            return 0;
                        }

                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
                        return 0;
                    }
                }
            }
        }

        //取得該下注次數的所有樂透號碼
        public void GetWagerLotteryNumber(int wager, int[] list, int specialNum, string period)
        {
            int winAwards = 0;  //贏得的獎項
            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = getWagerNumberProcedure;
                        command.Parameters.Add("@wager", SqlDbType.Int).Value = wager;
                        SqlDataReader dr = null;
                        dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
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
                }
            }
        }


        //下注的所有號碼中，將有中獎的號碼更新中獎的獎項
        public void UpdWagerSQLProcedure(int period, string[] list)
        {
            //將該期的6個樂透號碼組合，以供procedure比對使用
            string lotteryList = list[0] + "," + list[1] + "," + list[2] + "," + list[3] + "," + list[4] + "," + list[5] + ",";

            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = updWagerSQLProcedure;
                        sqlDA.SelectCommand = command;
                        SqlCommandBuilder.DeriveParameters(command);
                        command.Parameters["@specialNum"].Value = int.Parse(list[6]);   //特別號
                        command.Parameters["@period"].Value = period;                   //查詢的期數
                        command.Parameters["@lotteryList"].Value = lotteryList;         //該期的樂透號碼
                        sqlDA.Fill(ds);  
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
                    }
                }
            }
        }


        //更新中獎的下注號碼獎項
        public void UpdWagerWinAwards(int winAwards, int wager)
        {
            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = updWagerNumberProcedure;
                        sqlDA.SelectCommand = command;
                        SqlCommandBuilder.DeriveParameters(command);
                        command.Parameters["@win"].Value = winAwards;
                        command.Parameters["@wager"].Value = wager;
                        command.Parameters["@number1"].Value = wagerLotteryList[0];    //procedure內的變數@number1 給值
                        command.Parameters["@number2"].Value = wagerLotteryList[1];
                        command.Parameters["@number3"].Value = wagerLotteryList[2];
                        command.Parameters["@number4"].Value = wagerLotteryList[3];
                        command.Parameters["@number5"].Value = wagerLotteryList[4];
                        command.Parameters["@number6"].Value = wagerLotteryList[5];
                        sqlDA.Fill(ds);                                              //執行
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[insProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[insProcedure] Message: {0}", e1.Message);
                    }
                }
            }
        }

        //查詢中獎的下注號碼，回傳資訊
        public List<String> QueryWinLotteryInfo(int period)
        {
            using (SqlConnection dbconnect = new SqlConnection(sDBServer))
            {
                using (command = dbconnect.CreateCommand())
                {
                    List<String> winLotteryInfo = new List<string>();   //宣告一 list，讀取出來的中獎清單存入
                    try
                    {
                        dbconnect.Open();
                        command.Connection = dbconnect;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = selWinLotteryInfo;
                        command.Parameters.Add("@period", SqlDbType.Int).Value = period;
                        SqlDataReader dr = null;
                        dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                winLotteryInfo.Add(dr["f_info"].ToString());    //讀取出來的中獎清單存入list
                            }
                        }
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine("[getProcedure] Exception Type: {0}", e1.GetType());
                        Console.WriteLine("[getProcedure] Message: {0}", e1.Message);
                     }
                    return winLotteryInfo;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery
{
    /// <summary>
    /// 1.下注的號碼與本期樂透號碼比對，判斷是否有中獎
    /// 2.回傳中獎獎項
    /// </summary>
    public class LotteryWin
    {
        public LotteryWin()
        {

        }

        //比對開獎的樂透號碼與下注的樂透號碼，抓取中獎號碼
        public int CompareWagerAndLootery(int[] lotteryList,int[] wagerLotteryList,int specialNum, int wager)
        {
            int winAwards = 0;                   //贏得的獎項
            bool winningSpecialNum = false;      //是否對中特別號
            int winingNumbers = 0;               //對中幾個號碼

            int i, j;
            for (i = 0; i < lotteryList.Length; i++)
            {
                for (j = 0; j < wagerLotteryList.Length; j++)
                {
                    if (wagerLotteryList[j] == lotteryList[i])
                    {
                        winingNumbers++;
                    }
                    if (wagerLotteryList[j] == specialNum)
                    {
                        winningSpecialNum = true;
                    }
                }
            }

            switch (winingNumbers)
            {
                case 6:       //6個號碼全中，頭獎
                    winAwards = 1;
                    break;
                case 5:       //5個號碼全中
                    if (winningSpecialNum)   //對中特別號，貳獎
                    {
                        winAwards = 2;
                    }
                    else    //沒中特別號，参獎
                    {
                        winAwards = 3;
                    }
                    break;
                case 4:       //4個號碼全中
                    if (winningSpecialNum)   //對中特別號，肆獎
                    {
                        winAwards = 4;
                     }
                    else    //沒中特別號，伍獎
                    {
                        winAwards = 5;
                    }
                    break;
                case 3:       //3個號碼全中
                    if (winningSpecialNum)   //對中特別號，陸獎
                    {
                        winAwards = 6;
                    }
                    else    //沒中特別號，普獎
                    {
                        winAwards = 8;
                    }
                    break;
                case 2:       //2個號碼全中
                    if (winningSpecialNum)   //對中特別號，柒獎
                    {
                        winAwards = 7;
                    }
                    else    //沒中特別號，槓龜
                    {
                        winAwards = 0;
                    }
                    break;
                default:       //槓龜
                    winAwards = 0;
                    break;
            }
            return winAwards;
        }
    }
}

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 傳入完整字串格式:  [網址]?SID=[0~9][加密資料] 
/// (高英四方立道)
/// </summary>
public class Decoder
{
    public static string DES_KEY = "apex1234";
    public static string DES_IV = "apex1234";

    /**
     * Date:2019.05.15
     * Auther:JimLu
     * Desc : MD5 加密字串用的函式
     */
    public static String MD5Hash(string input)
    {
        MD5 md5 = MD5.Create();//建立一個MD5
        byte[] source = Encoding.Default.GetBytes(input);//將字串轉為Byte[]
        byte[] crypto = md5.ComputeHash(source);//進行MD5加密
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < crypto.Length; i++)
            result.Append(crypto[i].ToString("x2"));
        string s= result.ToString();
        return s;
    }

    /// <summary>   
    /// DES 加密字串   
    /// </summary>   
    /// <span  name="original" class="mceItemParam"></span>原始字串</param>   
    /// <span  name="key" class="mceItemParam"></span>Key，長度必須為 8 個 ASCII 字元</param>   
    /// <span  name="iv" class="mceItemParam"></span>IV，長度必須為 8 個 ASCII 字元</param>   
    /// <returns></returns>   
    public static string EncryptDES(string original)
    {
        try
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(DES_KEY);
            des.IV = Encoding.ASCII.GetBytes(DES_IV);
            byte[] s = Encoding.ASCII.GetBytes(original);
            ICryptoTransform desencrypt = des.CreateEncryptor();
            return BitConverter.ToString(desencrypt.TransformFinalBlock(s, 0, s.Length)).Replace("-", string.Empty);
        }
        catch { return original; }
    }




    /// <summary>   
    /// DES 解密字串   
    /// </summary>   
    /// <span  name="hexString" class="mceItemParam"></span>加密後 Hex String</param>   
    /// <span  name="key" class="mceItemParam"></span>Key，長度必須為 8 個 ASCII 字元</param>   
    /// <span  name="iv" class="mceItemParam"></span>IV，長度必須為 8 個 ASCII 字元</param>   
    /// <returns></returns>   
    public static string DecryptDES(string hexString)
    {
        try
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(DES_KEY);
            des.IV = Encoding.ASCII.GetBytes(DES_IV);

            byte[] s = new byte[hexString.Length / 2];
            int j = 0;
            for (int i = 0; i < hexString.Length / 2; i++)
            {
                s[i] = Byte.Parse(hexString[j].ToString() + hexString[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                j += 2;
            }
            ICryptoTransform desencrypt = des.CreateDecryptor();
            return Encoding.ASCII.GetString(desencrypt.TransformFinalBlock(s, 0, s.Length));
        }
        catch { return hexString; }
    }
    /// <summary>
    /// 加密金鑰 (高英)
    /// </summary>
    int[] KeyList = new int[10] { 0x36, 0x95, 0x75, 0x57, 0x59, 0x63, 0xA5, 0x5A, 0x55, 0xAA };
    
    public Decoder()
    {
        //
        // TODO: 在此加入建構函式的程式碼
        //
    }

    /// <summary>
    /// 加密副程式
    /// 輸入字串格式 : &G=GameCode&T=TeamCode&U=UserSerialNum&D=yyyyMMdd
    /// </summary>
    /// <param name="InputStr"></param>
    /// <returns></returns>
    public string SHGaoYingURLEncode(string InputStr)
    {
        string OutputStr = "";
        ///由Random決定使用的金鑰(0~9)
        Random randObj = new Random();
        int KeyIndex = randObj.Next(9);
        int EnCodeInt = 0;
        //xor : A ^ B
        byte[] InputStringByte = StrToByteArray(InputStr);
        for (int i = 0; i < InputStringByte.Length; i++)
        {

            EnCodeInt = (InputStringByte[i] ^ KeyList[KeyIndex]) + KeyIndex;    //(Byte value  XOR  Key) + KeyNo.
            if ((EnCodeInt >= 48 && EnCodeInt <= 57) || (EnCodeInt >= 65 && EnCodeInt <= 90) || (EnCodeInt >= 97 && EnCodeInt <= 122))
            {
                OutputStr = OutputStr + AsciiToChar(EnCodeInt);
            }
            else
            {
                // 32:""(空白), 45:"-" 46:"." 95:"_"               
                if (EnCodeInt == 32 || EnCodeInt == 45 || EnCodeInt == 46 || EnCodeInt == 95)
                {
                    if (EnCodeInt == 32) OutputStr = OutputStr + "+";//空白,以"+"替代
                    if (EnCodeInt == 45) OutputStr = OutputStr + "-";
                    if (EnCodeInt == 95) OutputStr = OutputStr + "_";
                    if (EnCodeInt == 46) OutputStr = OutputStr + ".";
                }
                else
                {
                    bool IsOver = false;
                    if (EnCodeInt >= 256)
                    {
                        EnCodeInt = EnCodeInt - 256;                  //>= 256 --> -=256
                        //IsOver = true;
                    }
                    string Tmp = ConvertString(EnCodeInt.ToString(), 10, 16).ToUpper(); //轉成16進位
                    if (Tmp.Length == 1) Tmp = "0" + Tmp;                               //補足2位
                    if (IsOver) Tmp = "1" + Tmp;
                    OutputStr = OutputStr + "%" + Tmp;
                }
            }
        }
        return KeyIndex.ToString() + OutputStr;
    }

    ///解碼副程式        
    /// <summary>
    /// 輸入: 型態: 字串
    ///       格式: [0~9][已加密資料]
    ///       exe: 6%89%FD%9E%F1%F0%FD%EE%EA%EF%89%F2%E7%9E%CA%DB%C6%E3%CE%D2%D4%C6
    /// 輸出: 型態: 字串
    ///       格式 :&G=GameCode&T=TeamCode&U=UserSerial&K=CheckKey
    /// </summary>
    /// <param name="InputStr"></param>
    /// <returns></returns>
    public string SHGaoYingURLDecodeTemp(string InputStr)
    {
        //取得金鑰位置: 輸入值第一位
        int KeyIndex = 0;
        try
        {
            KeyIndex = int.Parse(InputStr.Substring(0, 1));
            InputStr = InputStr.Substring(1, InputStr.Length - 1);
        }
        catch
        {
            return "";
        }
        if (KeyIndex < 0 || KeyIndex > 9) return "";

        int EnCodeInt = 0;
        string OutputStr = "";

        //以"%"區隔資料
        string[] InputList = InputStr.Split('%');

        for (int i = 0; i < InputList.Length; i++)
        {

            if (InputList[i].Length > 0)
            {
                string Tmp = InputList[i].ToString();
                ArrayList RecordList = new ArrayList();

                if (i == 0)
                {
                    for (int k = 0; k < Tmp.Length; k++)
                    {
                        RecordList.Add(Tmp.Substring(k, 1));
                    }
                }
                else
                {
                    if (Tmp.IndexOf("+") != -1 || Tmp.IndexOf("-") != -1 || Tmp.IndexOf("_") != -1 || Tmp.IndexOf(".") != -1)
                    {
                        string RecordStr = "";
                        for (int j = 0; j < Tmp.Length; j++)
                        {
                            if (Tmp[j] != '+' && Tmp[j] != '-' && Tmp[j] != '_' && Tmp[j] != '.')
                            {
                                if (RecordList.Count > 0)
                                {
                                    RecordList.Add(Tmp[j].ToString());
                                    RecordStr = "";
                                }
                                else RecordStr = RecordStr + Tmp[j].ToString();
                            }
                            else
                            {
                                if (RecordStr.Length > 0) RecordList.Add(RecordStr);
                                RecordList.Add(Tmp[j].ToString());
                                RecordStr = "";
                            }
                        }
                        if (RecordStr.Length > 0) RecordList.Add(RecordStr);
                    }
                    else RecordList.Add(Tmp);
                }

                for (int k = 0; k < RecordList.Count; k++)
                {
                    string CheckStr = RecordList[k].ToString();
                    if (CheckStr == "+") CheckStr = " ";
                    ArrayList TmpCheck = new ArrayList();
                    if (CheckStr == " " || CheckStr == "-" || CheckStr == "_" || CheckStr == ".")
                    {
                        TmpCheck.Add(CheckStr);
                    }
                    else
                    {
                        if (CheckStr.Length > 2)
                        {
                            int index = 0;
                            while (CheckStr.Length > 0)
                            {
                                if (index == 0)
                                {
                                    TmpCheck.Add(CheckStr.Substring(0, 2));
                                    CheckStr = CheckStr.Substring(2, CheckStr.Length - 2);
                                    index++;
                                }
                                else
                                {
                                    TmpCheck.Add(CheckStr.Substring(0, 1));
                                    CheckStr = CheckStr.Substring(1, CheckStr.Length - 1);
                                    index++;
                                }
                            }
                        }
                        else TmpCheck.Add(CheckStr);
                    }

                    for (int l = 0; l < TmpCheck.Count; l++)
                    {
                        string TmpCheckStr = TmpCheck[l].ToString();
                        string TenNumber = "";
                        if (TmpCheckStr.Length == 2) TenNumber = ConvertString(TmpCheckStr, 16, 10);  //149  
                        else TenNumber = CharToAscii(TmpCheckStr).ToString();

                        int TmpInt = int.Parse(TenNumber);                       //-KeyIndex
                        if (TmpInt < KeyIndex) TmpInt = TmpInt + 256;
                        TmpInt = TmpInt - KeyIndex;

                        EnCodeInt = TmpInt ^ KeyList[KeyIndex]; // XOR  
                        OutputStr = OutputStr + AsciiToChar(EnCodeInt);
                    }                    
                }
            }
        }
        return OutputStr;
    }

    ///上海高英解碼副程式        
    /// <summary>
    /// 輸入: 型態: 字串
    ///       格式: [0~9][已加密資料]
    ///       exe: 6%89%FD%9E%F1%F0%FD%EE%EA%EF%89%F2%E7%9E%CA%DB%C6%E3%CE%D2%D4%C6
    /// 輸出: 型態: 字串
    ///       格式 : &R=[VIP|NORMAL]&ID=[客戶帳號]
    ///       exe : &R=NORMAL&ID=apexmike
    /// </summary>
    /// <param name="InputStr"></param>
    /// <returns></returns>
    public string SHGaoYingURLDecode(string InputStr)
    {
        //取得金鑰位置: 輸入值第一位
        int KeyIndex = 0;
        try
        {
            KeyIndex = int.Parse(InputStr.Substring(0, 1));
            InputStr = InputStr.Substring(1, InputStr.Length - 1);
        }
        catch
        {
            return "";
        }
        if (KeyIndex < 0 || KeyIndex > 9) return "";

        int EnCodeInt = 0;
        string OutputStr = "";

        //以"%"區隔資料
        string[] InputList = InputStr.Split('%');

        for (int i = 0; i < InputList.Length; i++)
        {

            if (InputList[i].Length > 0)
            {
                string Tmp = InputList[i].ToString();
                ArrayList RecordList = new ArrayList();

                if (i == 0)
                {
                    for (int k = 0; k < Tmp.Length; k++)
                    {
                        RecordList.Add(Tmp.Substring(k, 1));
                    }
                }
                else
                {
                    if (Tmp.IndexOf("+") != -1 || Tmp.IndexOf("-") != -1 || Tmp.IndexOf("_") != -1 || Tmp.IndexOf(".") != -1)
                    {
                        string RecordStr = "";
                        for (int j = 0; j < Tmp.Length; j++)
                        {
                            if (Tmp[j] != '+' && Tmp[j] != '-' && Tmp[j] != '_' && Tmp[j] != '.')
                            {
                                if (RecordList.Count > 0)
                                {
                                    RecordList.Add(Tmp[j].ToString());
                                    RecordStr = "";
                                }
                                else RecordStr = RecordStr + Tmp[j].ToString();
                            }
                            else
                            {
                                if (RecordStr.Length > 0) RecordList.Add(RecordStr);
                                RecordList.Add(Tmp[j].ToString());
                                RecordStr = "";
                            }
                        }
                        if (RecordStr.Length > 0) RecordList.Add(RecordStr);
                    }
                    else RecordList.Add(Tmp);
                }

                for (int k = 0; k < RecordList.Count; k++)
                {
                    string CheckStr = RecordList[k].ToString();
                    if (CheckStr == "+") CheckStr = " ";
                    ArrayList TmpCheck = new ArrayList();
                    if (CheckStr == " " || CheckStr == "-" || CheckStr == "_" || CheckStr == ".")
                    {
                        TmpCheck.Add(CheckStr);
                    }
                    else
                    {
                        if (CheckStr.Length > 2)
                        {
                            int index = 0;
                            while (CheckStr.Length > 0)
                            {
                                if (index == 0)
                                {
                                    TmpCheck.Add(CheckStr.Substring(0, 2));
                                    CheckStr = CheckStr.Substring(2, CheckStr.Length - 2);
                                    index++;
                                }
                                else
                                {
                                    TmpCheck.Add(CheckStr.Substring(0, 1));
                                    CheckStr = CheckStr.Substring(1, CheckStr.Length - 1);
                                    index++;
                                }
                            }
                        }
                        else TmpCheck.Add(CheckStr);
                    }

                    for (int l = 0; l < TmpCheck.Count; l++)
                    {
                        string TmpCheckStr = TmpCheck[l].ToString();
                        string TenNumber = "";
                        if (TmpCheckStr.Length == 2) TenNumber = ConvertString(TmpCheckStr, 16, 10);  //149  
                        else TenNumber = CharToAscii(TmpCheckStr).ToString();

                        int TmpInt = int.Parse(TenNumber);                       //-KeyIndex
                        if (TmpInt < KeyIndex) TmpInt = TmpInt + 256;
                        TmpInt = TmpInt - KeyIndex;

                        EnCodeInt = TmpInt ^ KeyList[KeyIndex]; // XOR  
                        OutputStr = OutputStr + AsciiToChar(EnCodeInt);
                    }
                }
            }
        }
        return OutputStr;
    }

    private string ConvertString(string value, int fromBase, int toBase)
    {
        int intValue = Convert.ToInt32(value, fromBase);
        return Convert.ToString(intValue, toBase);
    }

    private string AsciiToChar(int asciiCode)
    {
        if (asciiCode >= 0 && asciiCode <= 255)
        {
            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            byte[] byteArray = new byte[] { (byte)asciiCode };
            string strCharacter = asciiEncoding.GetString(byteArray);
            return (strCharacter);
        }
        else
        {
            throw new Exception("ASCII Code is not valid.");
        }
    }

    private int CharToAscii(string character)
    {
        if (character.Length == 1)
        {
            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            int intAsciiCode = (int)asciiEncoding.GetBytes(character)[0];
            return (intAsciiCode);
        }
        else
        {
            throw new Exception("Character is not valid.");
        }
    }

    public static byte[] StrToByteArray(string str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        return encoding.GetBytes(str);
    }

}

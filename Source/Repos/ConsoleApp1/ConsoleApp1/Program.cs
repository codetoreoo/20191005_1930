using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            //密碼加密
            String salt = "F8stRh6Gu4Dib68WlW238E9T6CoZybBF";
            string pwd = "106stust06";
            pwd = MD5Hash(pwd + salt);
            Console.WriteLine("密文");
            Console.WriteLine(pwd);

            //帳號 -> token             
            //帳號:106stust06
            //帳號的Token:CE8B4F5695BC8C99FF77E0D9303B185589F04277DEA16978
            string uid = "106stust06";
            string token = "usr_id=" + uid;
            string token_dec = Decoder.EncryptDES(token);
            Console.WriteLine("明文");
            Console.WriteLine(token_dec);

            Console.ReadLine();
        }

  
        //加密
        public static String MD5Hash(string input)
        {
            MD5 md5 = MD5.Create();//建立一個MD5
            byte[] source = Encoding.Default.GetBytes(input);//將字串轉為Byte[]
            byte[] crypto = md5.ComputeHash(source);//進行MD5加密
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < crypto.Length; i++)
                result.Append(crypto[i].ToString("x2"));
            string s = result.ToString();
            return s;
        }

    }
}

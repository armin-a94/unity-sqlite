using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class SQLiteTools
{
  
    public static string MD5Hash(string inputString)
    {
        MD5 md5 = MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(inputString);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        StringBuilder hash = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            hash.Append(hashBytes[i].ToString("x2"));
        }
        return hash.ToString();
    }
    
}
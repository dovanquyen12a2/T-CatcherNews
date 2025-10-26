using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace WebXTB.Support
{
     public class Support
     {
          public string EncodePassword(string originalPassword)
          {
               
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(originalPassword));

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    stringBuilder.Append(data[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
          }
          public string GetFileExtension(string s)
          {
               string[] Name_extension = s.Split('.');
               int countarray = Name_extension.Count();
               s = Name_extension[countarray - 1];
               return s;
          }
     }
}
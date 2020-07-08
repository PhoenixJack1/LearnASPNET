using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WellEquipment.Models;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WellEquipment
{
    public class Log
    {
        public static string LogFile = "wwwroot/base/Log.txt";
        public static void AddEquipment(string user, Equipment equipment)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " добавил новое оборудование");
            list.AddRange(equipment.GetArrayForSave(0));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
        public static void AddParameter(string user, Equipment equipment, List<Parameter> par_list)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " добавил новый параметр");
            list.Add("Equipment ID: " + equipment.ID);
            foreach (Parameter par in par_list)
                list.Add(JsonConvert.SerializeObject(par));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
        public static void MoveEquipment(string user, SortedList<long, Parameter> slist)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " переместил оборудование");
            foreach (KeyValuePair<long, Parameter> pair in slist)
            {
                list.Add("Equipment ID: " + pair.Key);
                list.Add(JsonConvert.SerializeObject(pair.Value));
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
        public static void AddGroup(string user, string file, string value)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " добавил группу фильтров " + value + " в файл " + file);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
        public static void ChangeGroup(string user, string file, string oldvalue, string newvalue)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " переименовал группу фильтров " + oldvalue + " на "+newvalue+ " в файле " + file);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
        public static void AddFilter(string user, string file, string group, string value)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " добавил фильтр " + value + " в группу "+group+ " файла " + file);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
        public static void ChangeFilter(string user, string file, string oldvalue, string newvalue)
        {
            List<string> list = new List<string>();
            list.Add("-------------------------------------");
            list.Add("Пользователь " + user + " переименовал фильтр " + oldvalue + " на " + newvalue + " в файле " + file);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.AppendAllLines(LogFile, list.ToArray(), enc);
        }
    }
}

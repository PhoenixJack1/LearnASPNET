using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using WellEquipment.Models;

namespace WellEquipment
{
    public class SaveEquip
    {
        static SortedList<Values, bool> ValueIsSelectable;
        public static SortedList<Values, string[][]> ValuesList;
        public static SortedList<Values, string> ValuesNames;
        public static SortedList<long, Equipment> Equipments;
        public static SortedList<Values, string> HtmlNames;
        public static SortedList<string, Account> Accounts;
        public static List<Role> Roles = new List<Role>(new Role[] { new Role { Name = "admin" }, new Role { Name = "user" }, new Role { Name = "viewer" } });
        public static string LocationsFile = "wwwroot/filters/Location.txt";
        public static string TypeFile = "wwwroot/filters/Type.txt";
        public static string MakerFile = "wwwroot/filters/Maker.txt";
        public static string NewData = "wwwroot/base/Data3.txt";
        public static string OldData = "wwwroot/base/Data3.txt";
        //public static string User = "TestUser";
        public static string AccountsFile = "wwwroot/account/Accounts.txt";

        public static SortedList<uint, SortedList<uint, FilterGroup>> Filters_From_File;
    
        public static SortedList<string, OneFilter> All_Filters;
        public static void CreateValuesList()
        {
            ValueIsSelectable = new SortedList<Values, bool>();
            ValueIsSelectable.Add(Values.Title, false);
            ValueIsSelectable.Add(Values.Type, true);
            ValueIsSelectable.Add(Values.Location, true);
            ValueIsSelectable.Add(Values.Self_Number, false);
            ValueIsSelectable.Add(Values.Start_Cost, false);
            ValueIsSelectable.Add(Values.Current_Cost, false);
            ValueIsSelectable.Add(Values.Maker, true);
            ValueIsSelectable.Add(Values.Invetory_Number, false);
            ValueIsSelectable.Add(Values.Parameters, false);
            ValueIsSelectable.Add(Values.Info, false);

            Filters_From_File = new SortedList<uint, SortedList<uint, FilterGroup>>();

            ValuesList = new SortedList<Values, string[][]>();
            //ValuesList.Add(Values.Title, new string[0][]);
           // ValuesList.Add(Values.Type, ReadValuesList(TypeFile, out Filters.StartType));
            ReadFilters(TypeFile, Values.Type, out Filters.Start_Type);
            ReadFilters(LocationsFile, Values.Location, out Filters.Start_Location);
            ReadFilters(MakerFile, Values.Maker, out Filters.Start_Maker);
            //ValuesList.Add(Values.Location, ReadValuesList(LocationsFile, out Filters.StartLocation));
            //ValuesList.Add(Values.Self_Number, new string[0][]);
            //ValuesList.Add(Values.Start_Cost, new string[0][]);
            //ValuesList.Add(Values.Current_Cost, new string[0][]);
            //ValuesList.Add(Values.Maker, ReadValuesList(MakerFile, out Filters.StartMaker));
            //ValuesList.Add(Values.Invetory_Number, new string[0][]);
            //ValuesList.Add(Values.Parameters, new string[0][]);
            //ValuesList.Add(Values.Info, new string[0][]);

            ValuesNames = new SortedList<Values, string>();
            ValuesNames.Add(Values.Title, "Название");
            ValuesNames.Add(Values.Type, "Тип");
            ValuesNames.Add(Values.Location, "Расположение");
            ValuesNames.Add(Values.Self_Number, "Заводской номер");
            ValuesNames.Add(Values.Start_Cost, "Начальная цена");
            ValuesNames.Add(Values.Current_Cost, "Текущая цена");
            ValuesNames.Add(Values.Maker, "Производитель");
            ValuesNames.Add(Values.Invetory_Number, "Инвентарный номер");
            ValuesNames.Add(Values.Parameters, "Параметры");
            ValuesNames.Add(Values.Info, "Примечание");
            ValuesNames.Add(Values.LocationTime, "Время на позиции");

            HtmlNames = new SortedList<Values, string>();
            HtmlNames.Add(Values.Title, "EquipTitle");
            HtmlNames.Add(Values.Type, "EquipType");
            HtmlNames.Add(Values.Location, "EquipLocation");
            HtmlNames.Add(Values.Self_Number, "EquipSelf_Number");
            HtmlNames.Add(Values.Start_Cost, "EquipStart_Cost");
            HtmlNames.Add(Values.Current_Cost, "EquipCurrent_Cost");
            HtmlNames.Add(Values.Maker, "EquipMaker");
            HtmlNames.Add(Values.Invetory_Number, "EquipInventory_Number");
            HtmlNames.Add(Values.Parameters, "EquipParameters");
            HtmlNames.Add(Values.Info, "EquipInfo");
            HtmlNames.Add(Values.LocationTime, "EquipDate");
            HtmlNames.Add(Values.ID, "EquipID");
        }
        public static void ChangeFilter(string user, string filename, string oldfilter, string newfilter)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            List<string> result = new List<string>();
            foreach (string s in file)
            {
               if (s==oldfilter)
                    result.Add(newfilter);
                else
                    result.Add(s);
            }
            File.WriteAllLines(filename, result.ToArray(), enc);
            Log.ChangeFilter(user, filename, oldfilter, newfilter);
            if (filename == LocationsFile)
            {
                SaveEquip.ValuesList.Remove(Values.Location);
                SaveEquip.ValuesList.Add(Values.Location, ReadValuesList(LocationsFile, out Filters.StartLocation));
                foreach (Equipment equipment in SaveEquip.Equipments.Values)
                    equipment.ChangeFilter(Values.Location, oldfilter, newfilter);
            }
            else if (filename == TypeFile)
            {
                SaveEquip.ValuesList.Remove(Values.Type);
                SaveEquip.ValuesList.Add(Values.Type, ReadValuesList(TypeFile, out Filters.StartType));
                foreach (Equipment equipment in SaveEquip.Equipments.Values)
                    equipment.ChangeFilter(Values.Type, oldfilter, newfilter);
            }
            else if (filename == MakerFile)
            {
                SaveEquip.ValuesList.Remove(Values.Maker);
                SaveEquip.ValuesList.Add(Values.Maker, ReadValuesList(MakerFile, out Filters.StartMaker));
                foreach (Equipment equipment in SaveEquip.Equipments.Values)
                    equipment.ChangeFilter(Values.Maker, oldfilter, newfilter);
            }
            Filters.StartFilters = new Filters();
            SaveEquipments();
        }
        /// <summary> проверяет группу фильтров, есть ли с таким же названием </summary>
        public static bool CheckGroup(string filename, string groupname)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            foreach (string s in file)
            {
                string[] words = s.Split(' ');
                if (words.Length == 0 || words[0] != "GROUP")
                    continue;
                string curname = s.Substring(6, s.Length - 6);
                if (curname == groupname) return false;
            }
            return true;
        }
        /// <summary> меняет наименование группы </summary>
        public static void ChangeGroup(string filename, string oldgroupname, string newgroupname, string user)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            List<string> result = new List<string>();
            Regex reg = new Regex(oldgroupname);
            foreach (string s in file)
            {
                Match match = reg.Match(s);
                if (match.Success)
                    result.Add("GROUP " + newgroupname);
                else
                    result.Add(s);
            }
            File.WriteAllLines(filename, result.ToArray(), enc);
            Log.ChangeGroup(user, filename, oldgroupname, newgroupname);
            if (filename == LocationsFile)
            {
                ValuesList.Remove(Values.Location);
                ValuesList.Add(Values.Location, ReadValuesList(LocationsFile, out Filters.StartLocation));
            }
            else if (filename == TypeFile)
            {
                ValuesList.Remove(Values.Type);
                ValuesList.Add(Values.Type, ReadValuesList(TypeFile, out Filters.StartType));
            }
            else if (filename == MakerFile)
            {
                ValuesList.Remove(Values.Maker);
                ValuesList.Add(Values.Maker, ReadValuesList(MakerFile, out Filters.StartMaker));
            }
            Filters.StartFilters = new Filters();
        }
        /// <summary> добавляет группу фильтров</summary>
        public static void AddGroup(string filename, string groupname, string user)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] Lines = new string[]
            {
                "GROUP "+groupname,
            "ENDGROUP"
            };
            File.AppendAllLines(filename, Lines, enc);
            Log.AddGroup(user, filename, groupname);
        }
        /// <summary> Возвращает перечень групп с файла </summary>
        public static List<string> GetFilterGroups(string filename)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            List<string> result = new List<string>();
            foreach (string s in file)
            {
                string[] words = s.Split(' ');
                if (words.Length == 0 || words[0] != "GROUP")
                    continue;
                string curname = s.Substring(6, s.Length - 6);
                result.Add(curname);
            }
            return result;
        }
        /// <summary> проверяет, есть ли такой же фильтр</summary>
        public static bool CheckFilter(string[][] list, string filter)
        {
            foreach (string[] str in list)
            {
                if (str[0] == filter)
                    return false;
            }
            return true;
        }
        public static void AddFilter(string user, string filename, string groupname, string filter)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            List<string> result = new List<string>();
            string curgroup = "";
            foreach (string s in file)
            {
                if (s == "") continue;
                string[] words = s.Split(' ');
                if (words.Length == 0) continue;
                if (words[0] == "GROUP")
                    curgroup = s.Substring(6, s.Length - 6);
                if (words[0] == "ENDGROUP" && curgroup == groupname)
                    result.Add(filter);
                result.Add(s);
            }
            File.WriteAllLines(filename, result.ToArray(), enc);
            Log.AddFilter(user, filename, groupname, filter);
            if (filename == LocationsFile)
            {
                ValuesList.Remove(Values.Location);
                ValuesList.Add(Values.Location, ReadValuesList(LocationsFile, out Filters.StartLocation));
            }
            else if (filename==TypeFile)
            {
                ValuesList.Remove(Values.Type);
                ValuesList.Add(Values.Type, ReadValuesList(TypeFile, out Filters.StartType));
            }
            else if (filename==MakerFile)
            {
                ValuesList.Remove(Values.Maker);
                ValuesList.Add(Values.Maker, ReadValuesList(MakerFile, out Filters.StartMaker));
            }
            Filters.StartFilters = new Filters();

        }
        static void ReadFilters(string filename, Values value, out OneFilter startpos)
        {
            startpos = null;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            if (file == null || file.Length == 0)
                throw new Exception("Ошибка при считывании файла с фильтрами: Файл не найден или пуст "+filename);
            uint filter_id = 0;
            bool filter_id_parse_result = UInt32.TryParse(file[0], out filter_id);
            if (filter_id_parse_result == false)
                throw new Exception("Ошибка при считывании файла с фильтрами: ошибка структуры файла: " + filename);
            foreach (SortedList<uint, FilterGroup> a in Filters_From_File.Values)
                foreach (FilterGroup g in a.Values)
                    if (g.Filter_ID == filter_id)
                        throw new Exception("Ошибка при считывании файла с фильтрами: данный ID группы фильтров уже загружен: " + filename);
            bool havegroup = false;
            uint cur_group_id = 0;
            FilterGroup cur_group = null;
            SortedList<uint, FilterGroup> groups = new SortedList<uint, FilterGroup>();
            for (int i=1; i<file.Length; i++)
            {
                string s = file[i];
                if (s == "") continue;
                if (havegroup==false)
                {
                    string[] ss = s.Split(' ');
                    if (ss.Length < 3)
                        throw new Exception("Ошибка при считывании файла с фильтрами: ошибка инициализации группы в строке: " + i + " файла " + filename);
                    if (ss[0] != "GROUP")
                        throw new Exception("Ошибка при считывании файла с фильтрами: Начало группы не найдено в строке: " + i + " файла " + filename);
                    bool group_id_parse_result = UInt32.TryParse(ss[1], out cur_group_id);
                    if (group_id_parse_result==false)
                        throw new Exception("Ошибка при считывании файла с фильтрами: ID группы не найдено в строке: " + i + " файла " + filename);
                    if (groups.ContainsKey(cur_group_id))
                        throw new Exception("Ошибка при считывании файла с фильтрами: Дублирование ID группы в строке: " + i + " файла " + filename);
                    string group_name = "";
                    for (int j = 2; j < ss.Length; j++)
                    { group_name += ss[j]; if (j < ss.Length-1) group_name += " "; }
                    groups.Add(cur_group_id, new FilterGroup(group_name, cur_group_id, filter_id, value));
                    havegroup = true;
                    cur_group = groups[cur_group_id];
                }
                else
                {
                    if (s == "ENDGROUP")
                    {
                        havegroup = false;
                        cur_group = null;
                    }
                    else
                    {
                        string[] ss = s.Split(' ');
                        if (s.Length < 2)
                            throw new Exception("Ошибка при считывании файла с фильтрами: ошибка распознования в строке: " + i + " файла " + filename);
                        uint one_filter_id;
                        bool one_filter_id_parse_result = UInt32.TryParse(ss[0], out one_filter_id);
                        if (one_filter_id_parse_result == false)
                            throw new Exception("Ошибка при считывании файла с фильтрами: ошибка распознования ID одного фильтра в строке: " + i + " файла " + filename);
                        if (cur_group.Cur_Filters.ContainsKey(one_filter_id))
                            throw new Exception("Ошибка при считывании файла с фильтрами: Дублирование ID одного фильтра в строке: " + i + " файла " + filename);
                        string filter_name = "";
                        if (ss[1] == "START")
                        {
                            for (int j = 2; j < ss.Length; j++)
                            { filter_name += ss[j]; if (j < ss.Length - 1) filter_name += " "; }
                            cur_group.Cur_Filters.Add(one_filter_id, new OneFilter(filter_name, one_filter_id, cur_group));
                            if (startpos == null)
                                startpos = cur_group.Cur_Filters[one_filter_id];
                            else
                                throw new Exception("Ошибка при считывании файла с фильтрами: Дублирование стартового фильтра в строке: " + i + " файла " + filename);
                        }
                        else
                        {
                            for (int j = 1; j < ss.Length; j++)
                            { filter_name += ss[j]; if (j < ss.Length - 1) filter_name += " "; }
                            cur_group.Cur_Filters.Add(one_filter_id, new OneFilter(filter_name, one_filter_id, cur_group));
                        }
                    }
                }
                
            }
            if (havegroup != false)
                throw new Exception("Ошибка при считывании файла с фильтрами: нет конца группы в файле " + filename);
            Filters_From_File.Add(filter_id, groups);
            if (Program.Change_Base)
            {
                All_Filters = new SortedList<string, OneFilter>();
                foreach (SortedList<uint, FilterGroup> list in Filters_From_File.Values)
                    foreach (KeyValuePair<uint, FilterGroup> pair in list)
                        foreach (OneFilter f in pair.Value.Cur_Filters.Values)
                            All_Filters.Add(f.Name, f);
            }
        }
        static string[][] ReadValuesList(string filename, out string startpos)
        {
            startpos = "";
            List<string[]> list = new List<string[]>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            string[] file = File.ReadAllLines(filename, enc);
            bool havegroup = false;
            string group = "";
            foreach (string s in file)
            {
                if (s == "") continue;
                if (havegroup == false)
                {
                    string[] ss = s.Split(' ');
                    if (ss.Length < 2)
                        throw new Exception("Ошибка в инициализации группы");
                    if (ss[0] != "GROUP")
                        throw new Exception("Начало группы не найдено");
                    for (int i = 1; i < ss.Length; i++)
                    { group += ss[i]; if (i != ss.Length - 1) group += " "; }
                    havegroup = true;
                }
                else
                {
                    if (s == "ENDGROUP")
                    {
                        group = "";
                        havegroup = false;
                    }
                    else
                    {
                        string[] words = s.Split(' ');
                        if (words.Length > 0 && words[0] == "START")
                        {
                            string word = "";
                            for (int i = 1; i < words.Length; i++)
                            {
                                if (word == " ") continue;
                                word += words[i];
                                if (i < words.Length - 1) word += " ";
                            }
                            list.Add(new string[] { word, group });
                            startpos = word;
                        }
                        else
                            list.Add(new string[] { s, group });
                    }
                }

            }
            if (havegroup != false)
                throw new Exception("Нет начала группы");
            return list.ToArray();
        }
        public static void SaveEquipments()
        {
            SortedList<long, Equipment> equipments = Equipments;
            List<string> list = new List<string>();
            int i = 1;
            foreach (Equipment equip in equipments.Values)
            {
                list.AddRange(equip.GetArrayForSave(i));
                i++;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            File.WriteAllLines(NewData, list.ToArray(), Encoding.GetEncoding(1251));
        }
        public static void ReadAccounts(string filename)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string[] file = File.ReadAllLines(filename, Encoding.GetEncoding(1251));
            Accounts = new SortedList<string, Account>();
            foreach (string s in file)
            {
                if (s.Length == 0) continue;
                if (s[0] == '/') continue;
                string[] words = s.Split("\t");
                string name = "";
                for (int i = 3; i < words.Length; i++)
                {
                    name += words[i];
                    if (i < words.Length) name += " ";
                }
                Account acc = new Account(words[0], words[1], words[2], name);
                Accounts.Add(acc.Login, acc);
            }
        }
        public static void ReadEquipments(string filename)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string[] file = File.ReadAllLines(filename, Encoding.GetEncoding(1251));
            List<List<string>> list = new List<List<string>>();
            List<string> sublist = new List<string>();
            bool havestart = false;
            Regex EquipRegex = new Regex(@"^<EQUIPMENT \d+>");
            foreach (string s in file)
            {
                if (EquipRegex.Match(s).Success)
                {
                    if (havestart == true) throw new Exception("Ошибка №1 в файле сохранения");
                    havestart = true;
                    sublist = new List<string>();
                    string id = (new Regex(@"\D*")).Replace(s, "");
                    sublist.Add(id);
                }
                else if (s == "</EQUIPMENT>")
                {
                    if (havestart == false) throw new Exception("Ошибка №2 в файле сохранения");
                    havestart = false;
                    list.Add(sublist);
                }
                else
                {
                    if (havestart == false) throw new Exception("Ошибка №3 в файле сохранения");
                    sublist.Add(s);
                }
            }
            Equipments = new SortedList<long, Equipment>();
            for (int i = 0; i < list.Count; i++)
            {
                Equipment equip = Equipment.LoadEquipmentsFromFile(list[i]);
                if (equip == null)
                    Console.WriteLine($"Ошибка при чтении оборудования {i + 1}");
                else
                    Equipments.Add(equip.ID, equip);
            }
        }
        public static List<SortedList<Values, string>> GetEquipments(List<long> idlist)
        {
            List<SortedList<Values, string>> list = new List<SortedList<Values, string>>();
            foreach (long id in idlist)
                list.Add(Equipments[id].GetCurrentElements());
            return list;
        }
    }
}
   


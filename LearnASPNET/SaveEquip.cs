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
        public static SortedList<string, FilterGroup> AllGroups = new SortedList<string, FilterGroup>();
        public static SortedList<string, OneFilter> All_Filters = new SortedList<string, OneFilter>();
        /// <summary> Список параметров, которые предустановленные </summary>
        public static SortedList<Values, uint> FilteredValues = CreateFilteredValues();
        public static SortedList<string, Values> Filter_From_Html = CreateFilter_From_Html();
        static SortedList<Values, uint> CreateFilteredValues()
        {
            SortedList<Values, uint> result = new SortedList<Values, uint>();
            result.Add(Values.Type, 1);
            result.Add(Values.Location, 2);
            result.Add(Values.Maker, 3);
            return result;
        }
        static SortedList<string, Values> CreateFilter_From_Html()
        {
            SortedList<string, Values> result = new SortedList<string, Values>();
            result.Add("type", Values.Type);
            result.Add("well", Values.Location);
            result.Add("maker", Values.Maker);
            return result;
        }
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

            //ValuesList = new SortedList<Values, string[][]>();
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
        public static void ChangeFilter(string user, OneFilter oldfilter, string newname)
        {
            string oldname = oldfilter.Name;
            //переименовывание фильтра
            oldfilter.ChangeName(newname);
            //переименовывание фильтра во всех аккаунтах
            foreach (Account acc in SaveEquip.Accounts.Values)
                acc.AccountFilters.AllSingleFilters[oldfilter.StringID].ChangeName(newname);
            //сохранение базы
            string filename = SaveFilters(oldfilter.Parent.CurValues);
            foreach (Equipment equip in SaveEquip.Equipments.Values)
                foreach (Parameter par in equip.CurParameters.Values)
                    if (par.Filter == oldfilter)
                        par.SetTextValue();
            SaveEquip.SaveEquipments();
            //сохранения лога
            Log.ChangeFilter(user, filename, oldname, newname);
        }
       
        /// <summary> проверяет группу фильтров, есть ли с таким же названием</summary>
        public static bool CheckGroup(uint grouppos, string groupname)
        {
            SortedList<uint, FilterGroup> grouplist = SaveEquip.Filters_From_File[grouppos];
            foreach (FilterGroup group in grouplist.Values)
            {
                if (group.Name == groupname) return false;
            }
            return true;
        }
        /// <summary> Парсит текстовый ID группы и возвращает её </summary>
        public static FilterGroup GetGroup(string stringid)
        {
            if (stringid == null || stringid == "") return null;
            string[] poses = stringid.Split("_");
            if (poses.Length != 3 && poses[0] != "GF") return null;
            uint filterid = 0;
            if (UInt32.TryParse(poses[1], out filterid) == false) return null;
            if (Filters_From_File.ContainsKey(filterid) == false) return null;
            uint groupid = 0;
            if (UInt32.TryParse(poses[2], out groupid) == false) return null;
            if (Filters_From_File[filterid].ContainsKey(groupid) == false) return null;
            return Filters_From_File[filterid][groupid];
        }
        /// <summary> меняет наименование группы </summary>
        public static void ChangeGroup(Values val, string oldgroupstringid, string newgroupname, string user)
        {
            FilterGroup group = GetGroup(oldgroupstringid);
            string oldgroupname = group.Name;
            if (group == null) throw new Exception();
            if (val != group.CurValues) throw new Exception();
            group.Name = newgroupname;
            foreach (Account acc in Accounts.Values)
                acc.AccountFilters.ChangeGroupName(group, newgroupname);
            string filename = SaveFilters(val);
            Log.ChangeGroup(user, filename, oldgroupname, newgroupname);
        }
        /// <summary> добавляет группу фильтров</summary>
        public static void AddGroup(Values val, string groupname, string user)
        {
            //поиск свободного ID
            uint groupid = 0;
            uint filterid = FilteredValues[val];
            SortedList<uint, FilterGroup> list = Filters_From_File[filterid];
            for (; ; groupid++)
                if (list.Keys.Contains(groupid)) continue;
                else
                    break;
            FilterGroup group = new FilterGroup(groupname, groupid, filterid, val);
            //добавление группы в общий список
            list.Add(groupid, group);
            AllGroups.Add(group.StringID, group);
            //добавление группы в аккаунты
            foreach (Account acc in Accounts.Values)
                acc.AccountFilters.AddNewGroup(group);
            //сохранение базы
            string filename = SaveFilters(val);
            Log.AddGroup(user, filename, groupname);
        }
        static string SaveFilters(Values val)
        {
            OneFilter startfilter = null;
            uint filterid = FilteredValues[val];
            string filename = "";
            switch (val)
            {
                case Values.Type:
                    startfilter = Filters.Start_Type; filename = TypeFile; break;
                case Values.Location:
                    startfilter = Filters.Start_Location; filename = LocationsFile; break;
                case Values.Maker:
                    startfilter = Filters.Start_Maker; filename = MakerFile; break;
                default: throw new Exception();
            }
            List<string> file = new List<string>();
            file.Add(filterid.ToString());
            foreach (FilterGroup gr in Filters_From_File[filterid].Values)
            {
                file.Add(String.Format("GROUP {0} {1}", gr.ID, gr.Name));
                foreach (OneFilter filter in gr.Cur_Filters.Values)
                {
                    if (filter == startfilter)
                        file.Add(string.Format("{0} START {1}", filter.ID, filter.Name));
                    else
                        file.Add(string.Format("{0} {1}", filter.ID, filter.Name));
                }
                file.Add("ENDGROUP");
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(1251);
            File.WriteAllLines(filename, file, enc);
            return filename;
        }
        /// <summary> Возвращает перечень идентификаторов групп</summary>
        public static List<string> GetFilterGroups(Values val)
        {
            uint filterid = FilteredValues[val];
            List<string> result = new List<string>();
            foreach (FilterGroup group in Filters_From_File[filterid].Values)
                result.Add(group.StringID);
            return result;
        }
        /// <summary> Возвращает перечень групп с файла (Устаревшее) </summary>
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
        public static bool CheckFilter(Values val, string group_id, string filter_name)
        {
            SortedList<uint, FilterGroup> group_list = Filters_From_File[FilteredValues[val]];
            FilterGroup group = GetGroup(group_id);
            if (group == null)
                throw new Exception("Ошибка в ID группы - не найдена группа");
            if (group.CurValues != val)
                throw new Exception("Ошибка в ID группы - неверный тип фильтров");
            foreach (OneFilter filter in group.Cur_Filters.Values)
                if (filter.Name == filter_name) 
                    return true;
            return false;
        }
        /// <summary> проверяет, есть ли такой же фильтр (Устаревшее)</summary>
        public static bool CheckFilter(string[][] list, string filter)
        {
            foreach (string[] str in list)
            {
                if (str[0] == filter)
                    return false;
            }
            return true;
        }
        /// <summary> добавляет фильтр</summary>
        public static void AddFilter(string user, string filtername, string group_id, Values val)
        {
            //поиск группы
            FilterGroup group = GetGroup(group_id);
            //поиск свободного id
            uint id=0;
            for (id = 0; ; id++)
                if (group.Cur_Filters.Keys.Contains(id)) continue; else break;
            //создание фильтра
            OneFilter filter = new OneFilter(filtername, id, group);
            //добавление фильтра
            group.Cur_Filters.Add(id, filter);
            All_Filters.Add(filter.StringID, filter);
            //добавление фильтра в аккаунты
            foreach (Account acc in Accounts.Values)
                acc.AccountFilters.AddNewFilter(filter);
            //сохранение базы
            string filename = SaveFilters(val);
            //сохранение лога
            Log.AddFilter(user, filename, group.Name, filtername);
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
                    FilterGroup gr = new FilterGroup(group_name, cur_group_id, filter_id, value);
                    groups.Add(cur_group_id, gr);
                    AllGroups.Add(gr.StringID, gr);
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
                            OneFilter f = new OneFilter(filter_name, one_filter_id, cur_group);
                            cur_group.Cur_Filters.Add(one_filter_id, f);
                            All_Filters.Add(f.StringID, f);
                            if (startpos == null)
                                startpos = cur_group.Cur_Filters[one_filter_id];
                            else
                                throw new Exception("Ошибка при считывании файла с фильтрами: Дублирование стартового фильтра в строке: " + i + " файла " + filename);
                        }
                        else
                        {
                            for (int j = 1; j < ss.Length; j++)
                            { filter_name += ss[j]; if (j < ss.Length - 1) filter_name += " "; }
                            OneFilter f = new OneFilter(filter_name, one_filter_id, cur_group);
                            cur_group.Cur_Filters.Add(one_filter_id, f);
                            All_Filters.Add(f.StringID, f);
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
   


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WellEquipment.Models
{
    public class Filters
    {
        public SortedList<string, List<string>> Locations;
        public static string StartLocation;
        public static OneFilter Start_Location;
        public SortedList<string, List<string>> Types;
        public static string StartType;
        public static OneFilter Start_Type;
        public SortedList<string, List<string>> Makers;
        public static string StartMaker;
        public static OneFilter Start_Maker;
        public List<FilterGroup> LocationsFilters;
        public List<FilterGroup> TypesFilters;
        public List<FilterGroup> MakersFilters;
        
        public SortedList<string, OneFilter> AllSingleFilters;
        public SortedList<string, FilterGroup> AllGroupFilters;
        public SortedList<uint, SortedList<uint, FilterGroup>> List;
        public SortedList<uint, FilterGroup> Locations_Filters;
        public SortedList<uint, FilterGroup> Types_Filters;
        public SortedList<uint, FilterGroup> Makers_Filters;

        public static Filters StartFilters; //Ненужно
        public static string GroupWord = "Group_";
        public static string LocationWord = "Loc_";
        public static string TypeWord = "Type_";
        public static string MakerWord = "Maker_";
        public Filters()
        {
            AllSingleFilters = new SortedList<string, OneFilter>();
            AllGroupFilters = new SortedList<string, FilterGroup>();
            List = new SortedList<uint, SortedList<uint, FilterGroup>>();
            foreach (KeyValuePair<uint, SortedList<uint, FilterGroup>> pair in SaveEquip.Filters_From_File)
            {
                List.Add(pair.Key, new SortedList<uint, FilterGroup>());
                foreach (KeyValuePair<uint, FilterGroup> pair2 in pair.Value)
                {
                    FilterGroup accountgroup = new FilterGroup(pair2.Value.Name, pair2.Value.ID, pair2.Value.Filter_ID, pair2.Value.CurValues);
                    List[pair.Key].Add(pair2.Key, accountgroup);
                    AllGroupFilters.Add(accountgroup.StringID, accountgroup);
                    foreach (OneFilter filter in pair2.Value.Cur_Filters.Values)
                    {
                        OneFilter accountfilter = new OneFilter(filter.Name, filter.ID, accountgroup);
                        accountgroup.Cur_Filters.Add(accountfilter.ID, accountfilter);
                        AllSingleFilters.Add(accountfilter.StringID, accountfilter);
                    }
                }
            }
            Types_Filters = List[1];
            Locations_Filters = List[2];
            Makers_Filters = List[3];
           /* AllSingleFilters = new SortedList<string, OneFilter>();
            AllGroupFilters = new SortedList<string, FilterGroup>();
            Locations = new SortedList<string, List<string>>();
            for (int i = 0; i < SaveEquip.ValuesList[Values.Location].Length; i++)
            {
                string[] array = SaveEquip.ValuesList[Values.Location][i];
                if (Locations.ContainsKey(array[1]) == false)
                    Locations.Add(array[1], new List<string>());
                Locations[array[1]].Add(array[0]);
            }
            LocationsFilters = new List<FilterGroup>();
            foreach (KeyValuePair<string, List<string>> pair in Locations)
            {
                FilterGroup group = new FilterGroup(pair.Key, pair.Value, Values.Location);
                LocationsFilters.Add(group);
                AllGroupFilters.Add(group.ID, group);
                foreach (OneFilter filter in group.Cur_Filters.Values)
                    AllSingleFilters.Add(filter.ID, filter);
            }
            Types = new SortedList<string, List<string>>();
            for (int i = 0; i < SaveEquip.ValuesList[Values.Type].Length; i++)
            {
                string[] array = SaveEquip.ValuesList[Values.Type][i];
                if (Types.ContainsKey(array[1]) == false)
                    Types.Add(array[1], new List<string>());
                Types[array[1]].Add(array[0]);
            }
            TypesFilters = new List<FilterGroup>();
            foreach (KeyValuePair<string, List<string>> pair in Types)
            {
                FilterGroup group = new FilterGroup(pair.Key, pair.Value, Values.Type);
                TypesFilters.Add(group);
                AllGroupFilters.Add(group.ID, group);
                foreach (OneFilter filter in group.Cur_Filters.Values)
                    AllSingleFilters.Add(filter.ID, filter);
            }
            Makers = new SortedList<string, List<string>>();
            for (int i = 0; i < SaveEquip.ValuesList[Values.Maker].Length; i++)
            {
                string[] array = SaveEquip.ValuesList[Values.Maker][i];
                if (Makers.ContainsKey(array[1]) == false)
                    Makers.Add(array[1], new List<string>());
                Makers[array[1]].Add(array[0]);
            }
            MakersFilters = new List<FilterGroup>();
            foreach (KeyValuePair<string, List<string>> pair in Makers)
            {
                FilterGroup group = new FilterGroup(pair.Key, pair.Value, Values.Maker);
                MakersFilters.Add(group);
                AllGroupFilters.Add(group.ID, group);
                foreach (OneFilter filter in group.Cur_Filters.Values)
                    AllSingleFilters.Add(filter.ID, filter);
            }*/
        }
        public uint GetGroupIDFromValue(Values val)
        {
            foreach (KeyValuePair<uint, SortedList<uint, FilterGroup>> pair in List)
            {
                if (pair.Value.Values[0].CurValues == val)
                    return pair.Value.Values[0].Filter_ID;
                else
                    continue;
            }
            throw new Exception("Группа фильтров не найдена");
        }
        /// <summary> Метод обновляет выделенные фильтры со страницы </summary>
        public void UpdateFromPage(ICollection<string> filters_list)
        {
            foreach (KeyValuePair<string,OneFilter> pair in AllSingleFilters)
            {
                if (filters_list.Contains(pair.Key))
                    pair.Value.Selected = true;
                else
                    pair.Value.Selected = false;
            }
            foreach (FilterGroup group in AllGroupFilters.Values)
                group.CheckSelectedAfterUpdate();
        }
        /// <summary> Метод проверяет, существует ли такое расположение</summary>
        public static bool CheckLocation(string val)
        {
            foreach (List<string> list in StartFilters.Locations.Values)
                foreach (string s in list)
                    if (s == val) 
                        return true;
            return false;
        }
        /// <summary> Метод проверяет, существует ли такой тип оборудования</summary>
        public static bool CheckType(string val)
        {
            foreach (List<string> list in StartFilters.Types.Values)
                foreach (string s in list)
                    if (s == val)
                        return true;
            return false;
        }
        /// <summary> Метод проверяет, существует ли такой производитель</summary>
        public static bool CheckMaker(string val)
        {
            foreach (List<string> list in StartFilters.Makers.Values)
                foreach (string s in list)
                    if (s == val)
                        return true;
            return false;
        }
        /*public static Filters GetNewFilters()
        {
            return new Filters();
        }*/
    }
    public class OneFilter
    {
        public string Name { get; private set; } //Наименование фильтра
        public uint ID { get; private set; } //Уникальный ID, по которому фильтр ищется на странице
        public FilterGroup Parent { get; private set; }
        public bool Selected { get; set; } = true; //Метка, что фильтр выбран
        public string StringID { get; private set; }
        public OneFilter(string name, uint id, FilterGroup parent)
        {
            Name = name;
            ID = id;
            Parent = parent;
            StringID = string.Format("OF_{0}_{1}_{2}", Parent.Filter_ID, Parent.ID, ID);
        }
    }
    public class FilterGroup
    {
        public string Name; //Наименование группы фильтров
        public uint ID;//Уникальный ID, по которому фильтр ищется на странице
        public uint Filter_ID; //ID всех фильтров данного типа
        public bool Selected=true; //Метка, что группа фильтров выбрана
        public SortedList<uint, OneFilter> Cur_Filters; //Фильтры, входящие в группу
        public string StringID { get; private set; }
        public Values CurValues { get; private set; }
        public FilterGroup(string name, uint id, uint filter_id, Values val)
        {
            Name = name;
            ID = id;
            Filter_ID = filter_id;
            Cur_Filters = new SortedList<uint, OneFilter>();
            StringID = string.Format("GF_{0}_{1}", Filter_ID, ID);
            CurValues = val;
            /*string idtag = "";
            switch (group)
            {
                case Values.Location: idtag = Filters.LocationWord; ID = Filters.GroupWord+Filters.LocationWord + Name; break;
                case Values.Type: idtag = Filters.TypeWord; ID = Filters.GroupWord+Filters.TypeWord + Name; break;
                case Values.Maker: idtag = Filters.MakerWord; ID = Filters.GroupWord+Filters.MakerWord + Name; break;
            }
            foreach (string filtername in list)
            {
                string id = idtag + filtername;
                Cur_Filters.Add(id, new OneFilter(filtername, id));
            }*/
        }
        public override string ToString()
        {
            return string.Format("{0} {1} элемента", Name, Cur_Filters.Count);
        }
        /// <summary> Метод проверяет, надо ли включать галочку, если все выбраны </summary>
        public void CheckSelectedAfterUpdate()
        {
            Selected = true;
            foreach (OneFilter filter in Cur_Filters.Values)
                if (filter.Selected==false)
                {
                    Selected = false;
                    break;
                }
        }
    }
}

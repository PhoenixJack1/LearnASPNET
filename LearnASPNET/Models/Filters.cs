using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WellEquipment.Models
{
    public class Filters
    {
        public SortedList<string, List<string>> Locations; //Надо убрать
        public static string StartLocation;
        public static OneFilter Start_Location;
        public SortedList<string, List<string>> Types;
        public static string StartType;
        public static OneFilter Start_Type;
        public SortedList<string, List<string>> Makers;
        public static string StartMaker;
        public static OneFilter Start_Maker;
        //public List<FilterGroup> LocationsFilters;
        //public List<FilterGroup> TypesFilters;
        //public List<FilterGroup> MakersFilters;
        
        /// <summary> Все фильтры в одной коллекции. Ключ - текстовый ID </summary>
        public SortedList<string, OneFilter> AllSingleFilters;
        /// <summary> Все группы фильтров в одной коллекции. Ключ - текстовый ID </summary>
        public SortedList<string, FilterGroup> AllGroupFilters;
        /// <summary> Дерево фильтров. Первый ключ - тип фильтра (Локация, тип, производитель), второй ключ - ID группы </summary>
        public SortedList<uint, SortedList<uint, FilterGroup>> List;
        /// <summary> Все фильтры локации </summary>
        public SortedList<uint, FilterGroup> Locations_Filters;
        /// <summary> Все фильтры типа </summary>
        public SortedList<uint, FilterGroup> Types_Filters;
        /// <summary> Все фильтры производителя </summary>
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
           
        }
        /// <summary> Добавляет новый фильтр в массив </summary>
        public void AddNewFilter(OneFilter basefilter)
        {
            FilterGroup group = AllGroupFilters[basefilter.Parent.StringID];
            OneFilter filter = new OneFilter(basefilter.Name, basefilter.ID, group);
            group.Cur_Filters.Add(filter.ID, filter);
            AllSingleFilters.Add(filter.StringID, filter);
            filter.Selected = group.Selected;
        }
        /// <summary> Добавляет новую группу фильтров в массив </summary>
        public void AddNewGroup(FilterGroup basegroup)
        {
            FilterGroup group = new FilterGroup(basegroup.Name, basegroup.ID, basegroup.Filter_ID, basegroup.CurValues);
            List[group.Filter_ID].Add(group.ID, group);
            AllGroupFilters.Add(group.StringID, group);
        }
        /// <summary> Меняет наименование группы фильтров</summary>
        public void ChangeGroupName(FilterGroup basegroup, string name)
        {
            AllGroupFilters[basegroup.StringID].Name = name;
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
            OneFilter result = null;
            //поиск фильтра
            if (SaveEquip.All_Filters.ContainsKey(val))
                result = SaveEquip.All_Filters[val];
            //проверка соответствия фильтра типу данных
            if (result != null && result.Parent.CurValues == Values.Location)
                return true;
            else
                return false;
        }
        /// <summary> Метод проверяет, существует ли такой тип оборудования</summary>
        public static bool CheckType(string val)
        {
            OneFilter result = null;
            //поиск фильтра
            if (SaveEquip.All_Filters.ContainsKey(val))
                result = SaveEquip.All_Filters[val];
            //проверка соответствия фильтра типу данных
            if (result != null && result.Parent.CurValues == Values.Type)
                return true;
            else
                return false;
        }
        /// <summary> Метод проверяет, существует ли такой производитель</summary>
        public static bool CheckMaker(string val)
        {
            OneFilter result = null;
            //поиск фильтра
            if (SaveEquip.All_Filters.ContainsKey(val))
                result = SaveEquip.All_Filters[val];
            //проверка соответствия фильтра типу данных
            if (result != null && result.Parent.CurValues == Values.Maker)
                return true;
            else
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
        public void ChangeName(string newname)
        {
            Name = newname;
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

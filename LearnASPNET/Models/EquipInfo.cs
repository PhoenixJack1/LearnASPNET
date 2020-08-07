using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WellEquipment.Models
{
    public class EquipInfo
    {
        public List<SortedList<Values, string>> Equipments; //Отображаемое в форме оборудование, с учётом фильтров и номера страницы
        public int MaxValues = 0; //Информация об общем количестве оборудования, соответствующем фильтрам (по всем страницам)
        public static int Length = 20; //Максимальное количество отображаемого оборудования на странице
        public int MinPos; public int MaxPos; //Минимальный и максимальный индекс отображаемого оборудования на странице
        public int Page; //Текущая страница
        public EquipInfo(Filters filters,  int page)
        {
            Equipments = new List<SortedList<Values, string>>();
            foreach (Equipment equip in SaveEquip.Equipments.Values)
            {
                if (filters.AllSingleFilters[equip.CurParameters[Values.Type].Filter.StringID].Selected == false) continue;
                if (filters.AllSingleFilters[equip.CurParameters[Values.Location].Filter.StringID].Selected == false) continue;
                if (filters.AllSingleFilters[equip.CurParameters[Values.Maker].Filter.StringID].Selected == false) continue;
                Equipments.Add(equip.GetCurrentElements());
                
            }
            CalcCollectionParams(page);
        }
        /*public EquipInfo(ICollection<string> filters, int page)
        {
            if (filters == null) return;
            Equipments = new List<SortedList<Values, string>>();
            foreach (Equipment equip in SaveEquip.Equipments.Values)
            {
                SortedList<Values, string> CurEquip = equip.GetCurrentElements();
                if (filters.Contains(CurEquip[Values.Location]) && filters.Contains(CurEquip[Values.Type]) && filters.Contains(CurEquip[Values.Maker]))
                    Equipments.Add(CurEquip);
            }
            MaxValues = Equipments.Count;
            if (Equipments.Count==0)
            {
                MaxValues = 0; MinPos = 0; MaxPos = 0; Page = 0; return;
            }
            for (; ; )
            {
                if (page < 0) page = 0;
                MinPos = page * Length;
                if (Equipments.Count <= MinPos)
                    page -= 1;
                else
                    break;
            }
            Page = page;
            MaxPos = MinPos + Length;
            if (Equipments.Count < MaxPos) 
                MaxPos = Equipments.Count;
            List<SortedList<Values, string>> result = new List<SortedList<Values, string>>();
            for (int i = MinPos; i < MaxPos; i++)
                result.Add(Equipments[i]);
            Equipments = result;
            MinPos++; 
        }*/
        /// <summary> Расчитывает количество элементов и количество страниц элементов </summary>
        void CalcCollectionParams(int page)
        {
            MaxValues = Equipments.Count;
            if (Equipments.Count == 0)
            {
                MaxValues = 0; MinPos = 0; MaxPos = 0; Page = 0; return;
            }
            for (; ; )
            {
                if (page < 0) page = 0;
                MinPos = page * Length;
                if (Equipments.Count <= MinPos)
                    page -= 1;
                else
                    break;
            }
            Page = page;
            MaxPos = MinPos + Length;
            if (Equipments.Count < MaxPos)
                MaxPos = Equipments.Count;
            List<SortedList<Values, string>> result = new List<SortedList<Values, string>>();
            for (int i = MinPos; i < MaxPos; i++)
                result.Add(Equipments[i]);
            Equipments = result;
            MinPos++;
        }
        public EquipInfo()
        {
            Equipments = new List<SortedList<Values, string>>();
        }
        public EquipInfo(List<long> idlist, int page)
        {
            Equipments = new List<SortedList<Values, string>>();
            foreach (long id in idlist)
            {
                if (SaveEquip.Equipments.ContainsKey(id))
                    Equipments.Add(SaveEquip.Equipments[id].GetCurrentElements());
            }
            CalcCollectionParams(page);
        }
       /* public static EquipInfo GetInfo(ICollection<string> locations)
        {
            return new EquipInfo(locations);    
        }
        public static EquipInfo GetInfo(List<int> idlist)
        {
            return new EquipInfo(idlist);
        }*/
    }
}

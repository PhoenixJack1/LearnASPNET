using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WellEquipment.Models
{
    public class Parameter
    {
        public DateTime time { get; set; }
        public string value { get; set; }
        public Values Type { get; set; }
        public string TypeText { get; set; }
        public OneFilter Filter { get; set; }
        public Parameter()
        {
        }
        /// <summary>  проверяет, такой же параметр или нет </summary>
        public bool IsFoolEquial(Parameter par)
        {
            if (par == null) return false;
            switch (par.Type)
            {
                case Values.Location: case Values.Type: case Values.Maker:
                    return IsFoolEqual(par.time, par.Filter.StringID, par.Type);
                default:
                    return IsFoolEqual(par.time, par.value, par.Type);
            }
        }
        /// <summary>  проверяет, такой же параметр или нет </summary>
        public bool IsFoolEqual(DateTime time, string value, Values type)
        {
            if (this.time != time) return false;
            if (this.Type != type) return false;
            switch (Type)
            {
                case Values.Location:
                case Values.Maker:
                case Values.Type:
                    OneFilter filter = SaveEquip.All_Filters[value];
                    if (Filter.StringID == filter.StringID) return true; else return false;
                default:
                    if (this.value == value) return true; else return false;
            }
        }
        /// <summary>  проверяет, такое же значение параметра или нет </summary>
        public bool IsValueEqual(Parameter par)
        {
            if (par == null) return false;
            return IsValueEqual(par.Type, par.value);
        }
        /// <summary>  проверяет, такое же значение параметра или нет </summary>
        public bool IsValueEqual(Values type, string value)
        {
            if (this.Type != type) return false;
            switch (Type)
            {
                case Values.Location:
                case Values.Maker:
                case Values.Type:
                    OneFilter filter = SaveEquip.All_Filters[value];
                    if (Filter.StringID == filter.StringID) return true; else return false;
                default:
                    if (this.value == value) return true; else return false;
            }
        }
        /// <summary>  проверяет, такое же значение даты и типа параметра или нет </summary>
        public bool IsTimeEquial(Parameter par)
        {
            if (par == null) return false;
            return IsTimeEquial(par.time, par.Type);
        }
        /// <summary>  проверяет, такое же значение даты и типа параметра или нет </summary>
        public bool IsTimeEquial(DateTime time, Values type)
        {
            if (this.time != time) return false;
            if (this.Type != type) return false;
            return true;
        }
        /// <summary> Отключает сериализацию фильтра </summary>
        /// <returns></returns>
        public bool ShouldSerializeFilter()
        {
            return false;
        }
        public void Fill()
        {
            TypeText = SaveEquip.ValuesNames[Type];
            if (Type == Values.Type || Type == Values.Location || Type == Values.Maker)
            {
                uint[] ids = Get_ID_list_from_name(value);
                if (ids == null)
                    throw new Exception("Ошибка при считывании данных с базы - не распознан ID фильтра");
                if (SaveEquip.Filters_From_File.ContainsKey(ids[0]) == false)
                    throw new Exception("Ошибка при считывании данных с базы - несоответствие первого параметра");
                SortedList<uint, FilterGroup> file = SaveEquip.Filters_From_File[ids[0]];
                if (file.ContainsKey(ids[1]) == false)
                    throw new Exception("Ошибка при считывании данных с базы - несоответствие второго параметра");
                FilterGroup group = file[ids[1]];
                if (group.CurValues != Type)
                    throw new Exception("Ошибка при считывании данных с базы - несоответствие типа фильтра");
                if (group.Cur_Filters.ContainsKey(ids[2]) == false)
                    throw new Exception("Ошибка при считывании данных с базы - несоответствие третьего параметра");
                SetFilter(group.Cur_Filters[ids[2]]);
            }
        }
        public void SetFilter(OneFilter filter)
        {
            Filter = filter;
            SetTextValue();
        }
        public void SetTextValue()
        {
            value = string.Format("{0}_{1}_{2}_{3}", Filter.Parent.Filter_ID, Filter.Parent.ID, Filter.ID, Filter.Name);
        }
        /// <summary> Для загрузки параметра из базы или создания нового параметра</summary>
        public Parameter(DateTime t, Values p, string val)
        {
            time = t;
            value = val;
            Type = p;
            Fill();
        }
        /// <summary> Определяет ID фильтра </summary>
        public uint[] Get_ID_list_from_name(string name)
        {
            uint[] result = new uint[3];
            string[] list = name.Split('_');
            if (list.Length < 3) 
                return null;
            if (list.Length==4 && list[0]=="OF") //Если ID взят с формы, то преобразовывает к формату данных, аналогичных взятых из базы
                list = new string[] { list[1], list[2], list[3] };
            for (int i = 0; i < 3; i++)
                if (UInt32.TryParse(list[i], out result[i]) == false)
                    return null;
            return result;
        }
        public override string ToString()
        {
            return $"Параметр: {TypeText} = {value}, Изменён={time.ToShortDateString()}";
        }
    }
}

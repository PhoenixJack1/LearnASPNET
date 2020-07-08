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
                Filter = group.Cur_Filters[ids[2]];
            }
        }
        public Parameter(DateTime t, Values p, string val)
        {
            time = t;
            value = val;
            Type = p;
            Fill();
        }
        public uint[] Get_ID_list_from_name(string name)
        {
            uint[] result = new uint[3];
            string[] list = name.Split('_');
            if (list.Length < 3) 
                return null;
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

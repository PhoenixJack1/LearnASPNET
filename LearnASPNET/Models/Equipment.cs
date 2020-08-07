#define CHANGE_BASE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WellEquipment.Models
{
    public class Equipment
    {
        public long ID { get; set; }
        public List<Parameter> Parameters;
        static Random rnd = new Random();
        public SortedList<Values, Parameter> CurParameters;
        public Equipment()
        {
            Parameters = new List<Parameter>();
            ID = CreateID();
        }
        public static Equipment CreateNewEquipmentFromHtml(SortedList<string, string> list)
        {
            Equipment equipment = new Equipment();
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.LocationTime]) == false) 
                return null;
            DateTime date;
            if (DateTime.TryParse(list[SaveEquip.HtmlNames[Values.LocationTime]], out date) == false)
                return null;
            equipment.FillStart(date);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Title]) && list[SaveEquip.HtmlNames[Values.Title]] != "")
                equipment.SetParameterFromBegin(Values.Title, date, list[SaveEquip.HtmlNames[Values.Title]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Location]) && list[SaveEquip.HtmlNames[Values.Location]] != "")
                equipment.SetParameterFromBegin(Values.Location, date, list[SaveEquip.HtmlNames[Values.Location]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Type]) && list[SaveEquip.HtmlNames[Values.Type]] != "")
                equipment.SetParameterFromBegin(Values.Type, date, list[SaveEquip.HtmlNames[Values.Type]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Self_Number]) && list[SaveEquip.HtmlNames[Values.Self_Number]] != "")
                equipment.SetParameterFromBegin(Values.Self_Number, date, list[SaveEquip.HtmlNames[Values.Self_Number]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Start_Cost]) && list[SaveEquip.HtmlNames[Values.Start_Cost]] != "")
                equipment.SetParameterFromBegin(Values.Start_Cost, date, list[SaveEquip.HtmlNames[Values.Start_Cost]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Current_Cost]) && list[SaveEquip.HtmlNames[Values.Current_Cost]] != "")
                equipment.SetParameterFromBegin(Values.Current_Cost, date, list[SaveEquip.HtmlNames[Values.Current_Cost]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Maker]) && list[SaveEquip.HtmlNames[Values.Maker]] != "")
                equipment.SetParameterFromBegin(Values.Maker, date, list[SaveEquip.HtmlNames[Values.Maker]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Invetory_Number]) && list[SaveEquip.HtmlNames[Values.Invetory_Number]] != "")
                equipment.SetParameterFromBegin(Values.Invetory_Number, date, list[SaveEquip.HtmlNames[Values.Invetory_Number]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Parameters]) && list[SaveEquip.HtmlNames[Values.Parameters]] != "")
                equipment.SetParameterFromBegin(Values.Parameters, date, list[SaveEquip.HtmlNames[Values.Parameters]]);
            if (list.ContainsKey(SaveEquip.HtmlNames[Values.Info]) && list[SaveEquip.HtmlNames[Values.Info]] != "")
                equipment.SetParameterFromBegin(Values.Info, date, list[SaveEquip.HtmlNames[Values.Info]]);
            equipment.GetCurrentParameters();
            return equipment;
        }
        /// <summary> Используется при создании нового оборудования. Заменяет созданный базовый параметр на полученный из формы </summary>
        public void SetParameterFromBegin(Values type, DateTime date, string val)
        {
            List<Parameter> list = new List<Parameter>();
            foreach(Parameter par in Parameters)
                if (par.Type != type) list.Add(par);
            list.Add(new Parameter(date, type, val));
            Parameters = list;
        }
        public static Equipment LoadEquipmentsFromFile(List<string> source)
        {
            int curid = 0;
            try
            {
                curid = Int32.Parse(source[0]);
            }
            catch
            {
                throw new Exception("Ошибка при чтении №4");
            }
            try
            {
                Equipment equip = new Equipment();
                long ID = Int64.Parse(source[1]);
                equip.ID = ID;
                equip.Parameters = new List<Parameter>();
                for (int i = 2; i < source.Count; i++)
                {
                    Parameter par = JsonConvert.DeserializeObject<Parameter>(source[i]);
                    par.Fill();
                    if (Program.Change_Base)
                    {
                        //ДЛЯ ПЕРЕСОХРАНЕНИЯ БАЗЫ
                        if (par.Type == Values.Type || par.Type == Values.Location || par.Type == Values.Maker)
                        {
                            OneFilter f = SaveEquip.All_Filters[par.value];
                            par.value = string.Format("{0}_{1}_{2}_{3}", f.Parent.Filter_ID, f.Parent.ID, f.ID, f.Name);
                        }
                    }
                    equip.Parameters.Add(par);
                }
                equip.GetCurrentParameters();
                return equip;
            }
            catch
            {
                throw new Exception($"Ошибка при распозновании оборудования {curid} №5");
            }

        }
        /// <summary> Метод удаляет параметр, если возможно </summary>
        public string TryDeleteParamater(int pos)
        {
            Parameter par = Parameters[pos];
            bool hassametype = false;
            List<Parameter> list = new List<Parameter>();
            foreach (Parameter p in Parameters)
            {
                if (p == par) continue;
                list.Add(p);
                if (p.Type == par.Type)  
                    hassametype = true;
            }
            if (hassametype == false) return "Нельзя удалить единственный параметр";
            else Parameters = list;
            GetCurrentParameters();
            return "";
        }
        /// <summary> Метод проверяет возможность добавить парметр и добавляет его </summary>
        public string TryAddParameter(DateTime time, Values type, string val)
        {
            //Проверить, что бы на одну дату и один тип данных не приходилось два параметра
            foreach (Parameter par in Parameters)
                if (par.IsTimeEquial(time, type)==true)
                    return "Для этой даты уже существует запись";
            //Проверить ближайший по дате больший и меньший парметр, что бы не был такой же (замена на такое же значение)
            Parameter EarlyDateParameter = null;
            Parameter LateDateParameter = null;
            foreach (Parameter par in Parameters)
                if (type==par.Type)
                {
                    if (par.time<time)
                    {
                        if (EarlyDateParameter == null) EarlyDateParameter = par;
                        else if (par.time > EarlyDateParameter.time) EarlyDateParameter = par;
                    }
                    else
                    {
                        if (LateDateParameter == null) LateDateParameter = par;
                        else if (par.time < LateDateParameter.time) LateDateParameter = par;
                    }
                }
            if (EarlyDateParameter!=null && EarlyDateParameter.IsValueEqual(type, val) == true)
                return "Значение параметра уже присваивался ранее";
            if (LateDateParameter != null && LateDateParameter.IsValueEqual(type, val) == true)
                return "Значение параметр уже присвоено позже";
            //Добавить параметр
            Parameter par = new Parameter()
        }
        /// <summary> Метод проверяет возможность изменения параметра и меняет его </summary>
        public string TryCorrectParameter(int pos, DateTime date, string val)
        {
            Parameter par = Parameters[pos];
            if (par.time == date && par.value == val) return "Нет изменений";
            if (par.Type == Values.Location && Filters.CheckLocation(val) == false) return "Недопустимое значение";
            if (par.Type == Values.Type && Filters.CheckType(val) == false) return "Недопустимое значение";
            if (par.Type == Values.Maker && Filters.CheckMaker(val) == false) return "Недопустимое значение";
            if (date!=par.time)
            {
                foreach (Parameter p in Parameters)
                {
                    if (p == par) continue;
                    if (p.Type == par.Type && p.time == date) 
                        return "Для этой даты уже существует запись";
                }
            
            }
            par.time = date;
            if (par.Type==Values.Location || par.Type==Values.Type || par.Type==Values.Maker)
            {
                OneFilter filter = SaveEquip.All_Filters[val];
                par.SetFilter(filter);
            }
            else
                par.value = val;
            return "";
        }
        /// <summary> Проверяет, можно ли установить на оборудование данный параметр </summary>
        public bool CheckParameter(Values type, DateTime date, string value)
        {
            foreach (Parameter par in Parameters)
                if (par.Type == type)
                {
                    if (par.time == date && par.value == value)
                        return false;
                }
            return true;
        }
        /// <summary> Меняет наименование фильтра </summary>
        public void ChangeFilter(Values val, string oldfilter, string newfilter)
        {
            foreach (Parameter par in Parameters)
            {
                if (par.Type == val && par.value == oldfilter)
                    par.value = newfilter;
            }
            GetCurrentParameters();
        }
        public Parameter SetParameter(Values type, DateTime date, string value)
        {
            Parameter par = new Parameter(date, type, value);
            Parameters.Add(par);
            GetCurrentParameters();
            return par;
        }
        public string ChangeEquipment(SortedList<string, string> form, string user)
        {
            int count = 0;
            DateTime date;
            if (form.ContainsKey(SaveEquip.HtmlNames[Values.LocationTime]) == false)
                return "Не найден тег даты";
            if (DateTime.TryParse(form[SaveEquip.HtmlNames[Values.LocationTime]], out date) == false)
                return "Ошибка парсинга даты";
            List<Parameter> ToAdd = new List<Parameter>();
            if (CheckParameter(date, Values.Title, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Type, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Location, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Self_Number, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Start_Cost, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Current_Cost, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Maker, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Invetory_Number, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Parameters, form, CurParameters, ToAdd)) count++;
            if (CheckParameter(date, Values.Info, form, CurParameters, ToAdd)) count++;
            if (count == 0) return "Нет изменений";
            foreach (Parameter par in ToAdd)
                Parameters.Add(par);
            Log.AddParameter(user, this, ToAdd);
            GetCurrentParameters();
            return "";
        }
        bool CheckParameter(DateTime date, Values type, SortedList<string, string> form, SortedList<Values, Parameter> cur, List<Parameter> list)
        {
            if (form.ContainsKey(SaveEquip.HtmlNames[type]) && cur[type].value != form[SaveEquip.HtmlNames[type]] && date != cur[type].time)
            {
                list.Add(new Parameter(date, type, form[SaveEquip.HtmlNames[type]]));
                return true;
            }
            return false;
        }
        static long CreateID()
        {
            long result = DateTime.Now.Ticks / 10000000 * 10000000 + rnd.Next(10000000);
            return result;
        }
        public void FillStart(DateTime time)
        {
            Parameters.Add(new Parameter(time, Values.Title, "Нет названия"));
            Parameters.Add(new Parameter(time, Values.Type, Filters.Start_Type.StringID));// SaveEquip.ValuesList[Values.Type][0][0]));
            Parameters.Add(new Parameter(time, Values.Location, Filters.Start_Location.StringID));// SaveEquip.ValuesList[Values.Location][0][0]));
            Parameters.Add(new Parameter(time, Values.Self_Number, "Заводской номер не определён"));
            Parameters.Add(new Parameter(time, Values.Start_Cost, "Начальная цена не определена"));
            Parameters.Add(new Parameter(time, Values.Current_Cost, "Текущая цена не определена"));
            Parameters.Add(new Parameter(time, Values.Maker, Filters.Start_Maker.StringID));// SaveEquip.ValuesList[Values.Maker][0][0]); 
            Parameters.Add(new Parameter(time, Values.Invetory_Number, "Инвентарный номер не определён"));
            Parameters.Add(new Parameter(time, Values.Parameters, "Дополнительные параметры не определены"));
            Parameters.Add(new Parameter(time, Values.Info, "Вновь введено"));


        }
        public SortedList<DateTime, SortedList<Values, string>> GetHistory()
        {
            SortedList<DateTime, SortedList<Values, string>> list = new SortedList<DateTime, SortedList<Values, string>>(); //итоговая коллекция типа <дата, коллекция со строковыми значениями>
            DateTime maxdate = DateTime.MaxValue; //максимальное доступное значение (константа), что бы оставить в итоговой таблице место для текущих значений 
            list.Add(maxdate, new SortedList<Values, string>());
            foreach (Parameter par in CurParameters.Values)
            {
                //Помещение текущих значений в итоговую коллекцию
                if (SaveEquip.FilteredValues.ContainsKey(par.Type))
                    list[maxdate].Add(par.Type, par.Filter.Name);
                else
                    list[maxdate].Add(par.Type, par.value); 
            }
                foreach (Parameter par in Parameters)
            {
                if (list.ContainsKey(par.time) == false)
                    list.Add(par.time, new SortedList<Values, string>()); //если текущей даты в коллекции нет - то добавляем новую коллекцию для строк
                if (SaveEquip.FilteredValues.ContainsKey(par.Type))
                    list[par.time].Add(par.Type, par.Filter.Name);//если параметр из выбираемых - то добавляем его предустановленное значение
                else
                    list[par.time].Add(par.Type, par.value);//добавляем значение параметра
                //list[par.time].Add(par.Type, par.value);//добавляем значение параметра
            }
            return list;
        }
        public string[] GetArrayForSave(int id)
        {
            List<string> list = new List<string>();
            list.Add($"<EQUIPMENT {id}>");
            list.Add(JsonConvert.SerializeObject(ID));
            foreach (Parameter par in Parameters)
            {
                
                list.Add(JsonConvert.SerializeObject(par));
                
            }
            list.Add("</EQUIPMENT>");
            return list.ToArray();
        }
        public void GetCurrentParameters()
        {
            CurParameters = new SortedList<Values, Parameter>();
            foreach (Parameter par in Parameters)
            {
                if (CurParameters.Keys.Contains(par.Type) == false)
                    CurParameters.Add(par.Type, par);
                else if (par.time > CurParameters[par.Type].time)
                    CurParameters[par.Type] = par;
            }
        }
        public SortedList<Values, string> GetCurrentElements()
        {
            SortedList<Values, string> result = new SortedList<Values, string>();
            foreach (Parameter par in CurParameters.Values)
            {
                if (par.Type == Values.Type || par.Type==Values.Location || par.Type==Values.Maker)
                    result.Add(par.Type, par.Filter.Name);
                else
                    result.Add(par.Type, par.value);
            }
                result.Add(Values.ID, ID.ToString());
            int days = (int)((DateTime.Now - CurParameters[Values.Location].time).TotalDays + 1);
            switch (days)
            {
                case 1: result.Add(Values.LocationTime, "1 день"); break;
                case 2:
                case 3:
                case 4: result.Add(Values.LocationTime, days.ToString() + " дня"); break;
                default: result.Add(Values.LocationTime, days.ToString() + " дней"); break;
            }
            return result;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WellEquipment.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System;
using Microsoft.AspNetCore.Authorization;

namespace WellEquipment.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public HomeController()
        {

        }
        [Authorize(Roles ="admin")]
        public IActionResult Index()
        {
            //return Content(User.Identity.Name);
            return View();
        }
        [HttpGet]
        public IActionResult EquipInfo()
        {
            return View(SaveEquip.Accounts[User.Identity.Name].AccountFilters);
            //return View(WellEquipment.Models.Filters.StartFilters);
        }
        /*[HttpPost]
        public IActionResult EquipInfo(string[] vasya, string[] loc_)
        {
            return View(LearnASPNET.Models.EquipInfo.GetInfo());
        }*/
        [HttpPost]
        public IActionResult EquipInfoGetFiltredValue()
        {
            if (Request.ContentType == null)
                return View("Error");
            SaveEquip.Accounts[User.Identity.Name].AccountFilters.UpdateFromPage(Request.Form.Keys);
            return View("EquipmentTable", new WellEquipment.Models.EquipInfo(SaveEquip.Accounts[User.Identity.Name].AccountFilters, 0));
            //ОПРЕДЕЛЕНИЕ ПЕРЕЧНЯ ОБОРУДОВАНИЯ, В ЗАВИСИМОСТИ ОТ ВЫБРАННЫХ ФИЛЬТРОВ
            //if (Request.ContentType != null)
           //     return View("EquipmentTable", new WellEquipment.Models.EquipInfo(Request.Form.Keys, 0));
            //else
            //    return View("EquipmentTable", new WellEquipment.Models.EquipInfo());

        }
        [HttpPost]
        public IActionResult EquipmentInfoGetNextPage()
        {
            int page = 0;
            foreach (string s in Request.Form.Keys)
            {
                Int32.TryParse(s.Substring(4), out page);
                break;
            }
            return View("EquipmentTable", new WellEquipment.Models.EquipInfo(SaveEquip.Accounts[User.Identity.Name].AccountFilters, page+1));
            /*if (Request.ContentType != null)
                return View("EquipmentTable", new WellEquipment.Models.EquipInfo(Request.Form.Keys, page+1));
            else
                return View("EquipmentTable", new WellEquipment.Models.EquipInfo());*/
        }
        [HttpPost]
        public IActionResult EquipmentInfoGetPrevPage()
        {
            int page = 0;
            foreach (string s in Request.Form.Keys)
            {
                Int32.TryParse(s.Substring(4), out page);
                break;
            }
            return View("EquipmentTable", new WellEquipment.Models.EquipInfo(SaveEquip.Accounts[User.Identity.Name].AccountFilters, page - 1));
           /* if (Request.ContentType != null)
                return View("EquipmentTable", new WellEquipment.Models.EquipInfo(Request.Form.Keys, page - 1));
            else
                return View("EquipmentTable", new WellEquipment.Models.EquipInfo());*/
        }
        [HttpGet]
        public IActionResult AddEquipment()
        {
            return View("AddEquipment", WellEquipment.Models.Filters.StartFilters);
        }
        [HttpPost]
        public string[] AddEquipment(bool b)
        {
            SortedList<string, string> form = new SortedList<string, string>();
            foreach (string key in Request.Form.Keys)
            {
                StringValues value = "";
                Request.Form.TryGetValue(key, out value);
                form.Add(key, value.ToString());
            }
            Equipment equip = Equipment.CreateNewEquipmentFromHtml(form);
            if (equip != null)
            {
                SaveEquip.Equipments.Add(equip.ID, equip);
                Log.AddEquipment(SaveEquip.Accounts[User.Identity.Name].Name, equip);
                SaveEquip.SaveEquipments();
                return new string[] { "", equip.ID.ToString() };
            }
            else
            {
                return new string[] { "Ошибка", "" };
            }
        }
        [HttpPost]
        public IActionResult AddEquipmentShowAdded(string idlist)
        {
            if (idlist == null || idlist == "")
                return View("EquipmentTable", new WellEquipment.Models.EquipInfo());
            List<long> list = SplitID(idlist);
            return View("EquipmentTable", new WellEquipment.Models.EquipInfo(list));
        }
        [HttpPost]
        public IActionResult RemoveEquipmentFromHtml(string allid, string idforremove)
        {
            if (idforremove == null || idforremove == "")
                return null;
            List<long> listforremove = SplitID(idforremove);
            List<long> alllist = SplitID(allid);
            foreach (long id in listforremove)
                if (SaveEquip.Equipments.ContainsKey(id))
                {
                    SaveEquip.Equipments.Remove(id);
                    alllist.Remove(id);
                }
            SaveEquip.SaveEquipments();
            return View("EquipmentTable", new WellEquipment.Models.EquipInfo(alllist));
        }
        [HttpGet]
        public IActionResult ChangeEquipment(string id)
        {
            if (id == null || id == "")
                return View("Error");
            long longid = 0;
            if (Int64.TryParse(id, out longid) == false)
                return View("Error");
            if (SaveEquip.Equipments.ContainsKey(longid) == false)
                return View("Error");
            return View("ChangeEquipment", SaveEquip.Equipments[longid].GetCurrentElements());
        }
        [HttpPost]
        public string ChangeEquipment()
        {
            SortedList<string, string> form = new SortedList<string, string>();
            foreach (string key in Request.Form.Keys)
            {
                StringValues value = "";
                Request.Form.TryGetValue(key, out value);
                form.Add(key, value.ToString());
            }
            if (form.ContainsKey(SaveEquip.HtmlNames[Values.ID]) == false)
                return "Нет ID";
            long id = 0;
            if (Int64.TryParse(form[SaveEquip.HtmlNames[Values.ID]], out id) == false)
                return "Ошибка парсинга ID";
            if (SaveEquip.Equipments.ContainsKey(id) == false)
                return "Неправильный ID";
            string saveresult = SaveEquip.Equipments[id].ChangeEquipment(form, SaveEquip.Accounts[User.Identity.Name].Name);
            if (saveresult == "")
                SaveEquip.SaveEquipments();
            return saveresult;
        }
        [HttpGet]
        public IActionResult MoveEquipment(string idlist)
        {
            if (idlist == null || idlist == "")
                return View("Error");
            List<long> listformove = SplitID(idlist);
            foreach (long id in listformove)
                if (SaveEquip.Equipments.ContainsKey(id) == false)
                    return View("Error");
            return View("MoveEquipment", idlist);
        }
        [HttpPost]
        public string MoveEquipment(string idlist, string date, string location)
        {
            if (idlist == null || idlist == "")
                return "Ошибка при парсинге ID";
            if (date == null || date == "")
                return "Ошибка при парсинге даты";
            if (location == null || location == "")
                return "Ошибка при парсинге локации";
            DateTime curdate;
            if (DateTime.TryParse(date, out curdate) == false)
                return "Ошибка парсинга даты";
            List<long> listformove = SplitID(idlist);
            if (listformove.Count == 0)
                return "Ошибка при парсинге ID";
            bool truelocation = false;
            foreach (string[] a in SaveEquip.ValuesList[Values.Location])
            {
                if (location == a[0]) { truelocation = true; break; }
            }
            if (truelocation == false) return "Ошибка при парсинге локации";
            foreach (long id in listformove)
            {
                if (SaveEquip.Equipments.ContainsKey(id) == false)
                    return "Ошибка при парсинге ID";
                if (SaveEquip.Equipments[id].CheckParameter(Values.Location, curdate, location) == false)
                    return "Невозможно изменить расположение для оборудования";
            }
            SortedList<long, Parameter> LogData = new SortedList<long, Parameter>();
            foreach (long id in listformove)
            {
                LogData.Add(id, SaveEquip.Equipments[id].SetParameter(Values.Location, curdate, location));
            }
            Log.MoveEquipment(SaveEquip.Accounts[User.Identity.Name].Name, LogData);
            SaveEquip.SaveEquipments();
            return "";
        }
        [HttpGet]
        public IActionResult CorrectValue(string id)
        {
            if (id==null || id=="")
                return View("Error");
            long longid = 0;
            if (Int64.TryParse(id, out longid) == false)
                return View("Error");
            if (SaveEquip.Equipments.ContainsKey(longid) == false)
                return View("Error");
            ViewData["ID"] = id;
            return View("CorrectValue", SaveEquip.Equipments[longid].Parameters);
        }
        [HttpPost]
        public string CorrectValue(string id, string pos, string date, string val)
        {
            if (id == null || id == "")
                return "Ошибка данных";
            long longid = 0;
            if (Int64.TryParse(id, out longid) == false)
                return "Ошибка данных";
            if (SaveEquip.Equipments.ContainsKey(longid) == false)
                return "Не найдено оборудование";
            if (pos == null || pos == "")
                return "Ошибка данных";
            int intpos = 0;
            if (Int32.TryParse(pos, out intpos)==false)
                return "Ошибка данных";
            if (SaveEquip.Equipments[longid].Parameters.Count<=intpos)
                return "Ошибка данных";
            if (date==null || date=="")
                return "Ошибка данных";
            DateTime ParseDate;
            if (DateTime.TryParse(date, out ParseDate)==false)
                return "Ошибка данных";
            if (val==null || val=="")
                return "Ошибка данных";
            string correctresult = SaveEquip.Equipments[longid].TryCorrectParameter(intpos, ParseDate.Date, val);
            if (correctresult == "")
                SaveEquip.SaveEquipments();
            return correctresult;
        }
        [HttpPost]
        public string DeleteValue(string id, string pos)
        {
            if (id == null || id == "")
                return "Ошибка данных";
            long longid = 0;
            if (Int64.TryParse(id, out longid) == false)
                return "Ошибка данных";
            if (SaveEquip.Equipments.ContainsKey(longid) == false)
                return "Не найдено оборудование";
            if (pos == null || pos == "")
                return "Ошибка данных";
            int intpos = 0;
            if (Int32.TryParse(pos, out intpos) == false)
                return "Ошибка данных";
            if (SaveEquip.Equipments[longid].Parameters.Count <= intpos)
                return "Ошибка данных";
            string deleteresult = SaveEquip.Equipments[longid].TryDeleteParamater(intpos);
            if (deleteresult == "")
                SaveEquip.SaveEquipments();
            return deleteresult;
        }
        [HttpGet]
        public IActionResult HistoryEquipment(string id)
        {
            if (id == null || id == "")
                return View("Error");
            long longid = 0;
            if (Int64.TryParse(id, out longid) == false)
                return View("Error");
            if (SaveEquip.Equipments.ContainsKey(longid) == false)
                return View("Error");
            ViewData["ID"] = id;
            return View("HistoryEquipment", SaveEquip.Equipments[longid].GetHistory());
        }
        [HttpGet]
        public IActionResult FilterAddGroup(string groupname)
        {
            if (groupname == null || groupname == "")
                return View("Error");
            switch (groupname)
            {
                case "well": return View("Filters/AddGroup", new string[] { "Добавление нового месторождения", "Наименование месторождения", "well" });
                case "type": return View("Filters/AddGroup", new string[] { "Добавление новой группы оборудования", "Наименование группы оборудования", "type" });
                case "maker": return View("Filters/AddGroup", new string[] { "Добавление новой группы производителей", "Наименование группы производителей", "maker" });
            }
            return View("Filters/AddField");
        }
        [HttpPost]
        public string FilterAddGroup(string groupname, string value)
        {
            if (groupname == null || groupname == "")
                return "Ошибка данных";
            if (value == null || value == "")
                return "Наименование не может быть пустым";
            switch (groupname)
            {
                case "well":
                    bool checkwell = SaveEquip.CheckGroup(SaveEquip.LocationsFile, value);
                    if (checkwell == false)
                        return "Месторождение уже добавлено";
                    SaveEquip.AddGroup(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.LocationsFile, value);
                    break;
                case "type":
                    bool checktype = SaveEquip.CheckGroup(SaveEquip.TypeFile, value);
                    if (checktype == false)
                        return "Такая группа уже добавлена";
                    SaveEquip.AddGroup(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.TypeFile, value);
                    break;
                case "maker":
                    bool checkmaker = SaveEquip.CheckGroup(SaveEquip.MakerFile, value);
                    if (checkmaker == false)
                        return "Такая группа уже добавлена";
                    SaveEquip.AddGroup(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.MakerFile, value);
                    break;
            }
            return "";
        }
        [HttpGet]
        public IActionResult AddFilter(string groupname)
        {
            if (groupname == null || groupname == "")
                return View("Error");
            switch (groupname)
            {
                case "well": 
                    List<string> fields = SaveEquip.GetFilterGroups(SaveEquip.LocationsFile);
                    fields.Add("Добавление новой скважины");
                    fields.Add("Месторождение");
                    fields.Add("Наименование скважины");
                    fields.Add("well");
                    return View("Filters/AddFilter", fields);
                case "type":
                    List<string> types = SaveEquip.GetFilterGroups(SaveEquip.TypeFile);
                    types.Add("Добавление нового типа оборудования");
                    types.Add("Группа");
                    types.Add("Наименование типа");
                    types.Add("type");
                    return View("Filters/AddFilter", types); 
                case "maker":
                    List<string> makers = SaveEquip.GetFilterGroups(SaveEquip.MakerFile);
                    makers.Add("Добавление нового производителя");
                    makers.Add("Группа");
                    makers.Add("Наименование производителя");
                    makers.Add("maker");
                    return View("Filters/AddFilter", makers); 
                default: return View("Error");
            }
        }
        [HttpPost]
        public string AddFilter(string group, string value, string type)
        {
            if (group == null || group == "")
                return "Ошибка в данных";
            if (type == null || type == "")
                return "Ошибка в данных";
            if (value == null || value == "")
                return "Название не может быть пустым";
            switch (type)
            {
                case "well": 
                    bool checkfields = SaveEquip.CheckFilter(SaveEquip.ValuesList[Values.Location], value);
                    if (checkfields == false)
                        return "Такая скважина уже есть";
                    SaveEquip.AddFilter(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.LocationsFile, group, value);
                    break;
                case "type":
                    bool checktype = SaveEquip.CheckFilter(SaveEquip.ValuesList[Values.Type], value);
                    if (checktype == false)
                        return "Такой тип оборудования уже есть";
                    SaveEquip.AddFilter(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.TypeFile, group, value);
                    break;
                case "maker":
                    bool checkmaker = SaveEquip.CheckFilter(SaveEquip.ValuesList[Values.Maker], value);
                    if (checkmaker == false)
                        return "Такой производитель уже есть";
                    SaveEquip.AddFilter(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.MakerFile, group, value);
                    break;
            }
            return "";
        }
        [HttpGet]
        public IActionResult ChangeGroup(string groupname)
        {
            if (groupname == null || groupname == "")
                return View("Error");
            switch (groupname)
            {
                case "well":
                    List<string> fields = SaveEquip.GetFilterGroups(SaveEquip.LocationsFile);
                    fields.Add("Смена названия месторождения");
                    fields.Add("Текущее название");
                    fields.Add("Новое название");
                    fields.Add("well");
                    return View("Filters/ChangeGroup", fields);
                case "type":
                    List<string> types = SaveEquip.GetFilterGroups(SaveEquip.TypeFile);
                    types.Add("Смена названия группы типа оборудования");
                    types.Add("Текущее название");
                    types.Add("Новое название");
                    types.Add("type");
                    return View("Filters/ChangeGroup", types);
                case "maker":
                    List<string> makers = SaveEquip.GetFilterGroups(SaveEquip.MakerFile);
                    makers.Add("Смена названия группы производителей");
                    makers.Add("Текущее название");
                    makers.Add("Новое название");
                    makers.Add("maker");
                    return View("Filters/ChangeGroup", makers);
                default: return View("Error");
            }
        }
        [HttpPost]
        public string ChangeGroup(string oldgroupname, string newgroupname, string type)
        {
            if (oldgroupname == null || oldgroupname == "")
                return "Ошибка в данных";
            if (type == null || type == "")
                return "Ошибка в данных";
            if (newgroupname == null || newgroupname == "")
                return "Название не может быть пустым";
            switch (type)
            {
                case "well":
                    bool checkwell = SaveEquip.CheckGroup(SaveEquip.LocationsFile, oldgroupname);
                    if (checkwell == true)
                        return "Ошибка данных";
                    SaveEquip.ChangeGroup(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.LocationsFile, oldgroupname, newgroupname);
                    break;
                case "type":
                    bool checktype = SaveEquip.CheckGroup(SaveEquip.TypeFile, oldgroupname);
                    if (checktype == true)
                        return "Ошибка данных";
                    SaveEquip.ChangeGroup(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.TypeFile, oldgroupname, newgroupname);
                    break;
                case "maker":
                    bool checkmaker = SaveEquip.CheckGroup(SaveEquip.MakerFile, oldgroupname);
                    if (checkmaker == true)
                        return "Ошибка данных";
                    SaveEquip.ChangeGroup(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.MakerFile, oldgroupname, newgroupname);
                    break;
            }
            return "";
        }
        [HttpGet]
        public IActionResult ChangeFilter(string type)
        {
            if (type == null || type == "")
                return View("Error");
            switch (type)
            {
                case "well":
                    string[] welllist = new string[] {
                    "Изменить название скважины",
                    "Текущее название скважины",
                    "Новое название скважины",
                    "well" };
                    return View("Filters/ChangeFilter", welllist);
                case "type":
                    string[] typelist = new string[] {
                    "Изменить название оборудования",
                    "Текущее название оборудования",
                    "Новое название оборудования",
                    "type" };
                    return View("Filters/ChangeFilter", typelist);
                case "maker":
                    string[] makerlist = new string[] {
                    "Изменить название производителя",
                    "Текущее название производителя",
                    "Новое название производителя",
                    "maker" };
                    return View("Filters/ChangeFilter", makerlist);
                default:
                    return View("Error");
            }
        }
        [HttpPost]
        public string ChangeFilter(string oldfiltername, string newfiltername, string type)
        {
            if (oldfiltername == null || oldfiltername == "")
                return "Ошибка в данных";
            if (type == null || type == "")
                return "Ошибка в данных";
            if (newfiltername == null || newfiltername == "")
                return "Название не может быть пустым";
            switch (type)
            {
                case "well":
                    bool checkwell = SaveEquip.CheckFilter(SaveEquip.ValuesList[Values.Location], oldfiltername);
                    if (checkwell == true)
                        return "Ошибка данных";
                    SaveEquip.ChangeFilter(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.LocationsFile, oldfiltername, newfiltername);
                    break;
                case "type":
                    bool checktype = SaveEquip.CheckFilter(SaveEquip.ValuesList[Values.Type], oldfiltername);
                    if (checktype == true)
                        return "Ошибка данных";
                    SaveEquip.ChangeFilter(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.TypeFile, oldfiltername, newfiltername);
                    break;
                case "maker":
                    bool checkmaker = SaveEquip.CheckFilter(SaveEquip.ValuesList[Values.Maker], oldfiltername);
                    if (checkmaker == true)
                        return "Ошибка данных";
                    SaveEquip.ChangeFilter(SaveEquip.Accounts[User.Identity.Name].Name, SaveEquip.MakerFile, oldfiltername, newfiltername);
                    break;
            }
            return "";
        }
        List<long> SplitID(string idlist)
        {
            List<long> list = new List<long>();
            string[] ss = idlist.ToString().Split(',');
            foreach (string s in ss)
            {
                long id = 0;
                if (Int64.TryParse(s, out id))
                    list.Add(id);
            }
            return list;
        }
            string CreateListId(List<long> list)
            {
                string result = "";
                for (int i=0;i<list.Count;i++)
                {
                    result += list[i];
                    if (i < list.Count - 1) result += ",";
                }
                return result;
            }
        [AllowAnonymous]
        public string Info()
        {
            return "ООО СП \"Волгодеминойл\" 2020 год";
        }
    }
}

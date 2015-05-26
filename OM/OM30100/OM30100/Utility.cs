using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ext.Net;
using System.Web.Mvc;
using System.Globalization;

namespace OM30100
{
    public static class Utility
    {
        public static int WeeksInYear(DateTime date)
        {
            GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public static Model GenerateModel(this Type type,string modelName,string key)
        {
            Model model = new Model();
  
            var pros = type.GetProperties();
            foreach (var pro in pros)
            {
                model.Fields.Add(new ModelField(pro.Name, ModelFieldType.Auto));
            }
            model.IDProperty = key;
            model.ID = modelName;
            return model;
        }
       
        public static short ToShort(this object s)
        {
            return Convert.ToInt16(s);
        }
       
      
        public static void CloneValue(this object target,FormCollection source, params string[] exclude)
        {
            Type t=target.GetType();
            var lstT = t.GetProperties();
            foreach (var item in source.AllKeys)
	        {
                var proT = lstT.FirstOrDefault(p => p.Name.ToLower() == item.ToLower());
                if (proT != null)
                {
                    if (proT.PropertyType.Name == "Boolean")
                    {
                        proT.SetValue(target, source[item] == null ? false : true, null);
                    }
                    else if (proT.PropertyType.Name == "Int16")
                    {
                        try
                        {
                            proT.SetValue(target, source[item] == null ? 0 : Convert.ToInt16(source[item]), null);
                        }
                        catch (Exception)
                        {

                            proT.SetValue(target, Convert.ToInt16( source[item] == null ? 0 : 1), null);
                        }
                    }
                    else if (proT.PropertyType.Name == "Int32")
                    {
                        try
                        {
                            proT.SetValue(target, source[item] == null ? 0 : Convert.ToInt32(source[item]), null);
                        }
                        catch (Exception)
                        {

                            proT.SetValue(target, source[item] == null ? 0 : 1, null);
                        }
                    }
                    else if (proT.PropertyType.Name == "Double")
                    {
                        
                            proT.SetValue(target, source[item] == null ? 0 : Convert.ToDouble(source[item]), null);
                        
                    }
                    else
                    {
                        proT.SetValue(target, source[item]??string.Empty,null);
                    }
                }

	        }
        }
        public static void CloneValue(this object target, object source, bool excludeSys, params string[] exclude)
        {
            var sys=new string[]{"Crtd_DateTime","Crtd_Prog","Crtd_User","LUpd_DateTime","LUpd_Prog","LUpd_User","tstamp"};
            if (source == null) target = null;
            else
            {
                if (target == null) target = new object();
                Type s = source.GetType();
                Type t = target.GetType();
                var lsts = s.GetProperties();
                var lstt = t.GetProperties();
                foreach (var proS in lsts)
                {
                    if (proS.Name != "EntityState" && proS.Name != "EntityKey" && (!excludeSys || (excludeSys && !sys.Any(p => p.ToLower() == proS.Name.ToLower()))) && !exclude.Any(p => p.ToLower() == proS.Name.ToLower()))
                    {
                        var proT = lstt.FirstOrDefault(p => p.Name == proS.Name);
                        if (proT != null)
                        {
                            proT.SetValue(target, proS.GetValue(source, null), null);
                        }
                    }
                    
                }
            }
        }
    }
}
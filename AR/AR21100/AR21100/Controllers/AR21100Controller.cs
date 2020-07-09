using HQ.eSkyFramework;
using Ext.Net;
using Ext.Net.MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using System.Text;
namespace AR21100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class AR21100Controller : Controller
    {
        private string _screenNbr = "AR21100";
        private string _userName = Current.UserName;
        AR21100Entities _db = Util.CreateObjectContext<AR21100Entities>(false);
        private JsonResult _logMessage;
        public ActionResult Index()
        {

            Util.InitRight(_screenNbr);
            bool chanelType = false;
            var obj = _db.AR21100_pdConfig(Current.CpnyID, Current.UserName, Current.LangID).FirstOrDefault();
            if (obj != null)
            {
                chanelType = obj.ChanelType.Value && obj.ChanelType.HasValue;
            }
            ViewBag.ChannelTypeView = chanelType;

            return View();
        }

        [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetChannel()
        {
            var channel = _db.AR21100_pgLoadChannel(Current.CpnyID,Current.UserName,Current.LangID).ToList();
            return this.Store(channel);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstChannel"]);
                ChangeRecords<AR21100_pgLoadChannel_Result> lstChannel = dataHandler.BatchObjectData<AR21100_pgLoadChannel_Result>();
                foreach (AR21100_pgLoadChannel_Result deleted in lstChannel.Deleted)
                {
                    var del = _db.AR_Channel.Where(p => p.Code == deleted.Code).FirstOrDefault();
                    if (del != null)
                    {
                        _db.AR_Channel.DeleteObject(del);
                    }
                }

                lstChannel.Created.AddRange(lstChannel.Updated);

                bool flagCheck = true;
                string errorcheck = string.Empty;
                string message = string.Empty;

                foreach (AR21100_pgLoadChannel_Result curChannel in lstChannel.Created)
                {
                    if (curChannel.Code.PassNull() == "") continue;
                    var Channel = _db.AR_Channel.Where(p => p.Code.ToLower() == curChannel.Code.ToLower()).FirstOrDefault();
                    var check_Salesperson = _db.AR_Salesperson.Where(p => p.Channel.ToLower() == curChannel.Code.ToLower()).FirstOrDefault();
                    var check_Inventory = _db.IN_Inventory.Where(p => p.Channel.ToLower() == curChannel.Code.ToLower()).FirstOrDefault();
                    var check_Customer = _db.AR_Customer.Where(p => p.Channel.ToLower() == curChannel.Code.ToLower()).FirstOrDefault();
                    var check_User = _db.vs_User.Where(p => p.Channel.ToLower() == curChannel.Code.ToLower()).FirstOrDefault();
                    if (check_Salesperson != null || check_Inventory != null || check_Customer != null || check_User != null)
                    {
                        errorcheck += (curChannel.Code) + ", ";
                        flagCheck = true;
                     //   
                    }
                   
                    if (Channel != null)
                    {
                        if (Channel.tstamp.ToHex() == curChannel.tstamp.ToHex()) // luôn bắt code
                        {
                            Update_AR_Channel(Channel, curChannel, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        Channel = new AR_Channel();
                        Update_AR_Channel(Channel, curChannel, true);
                        _db.AR_Channel.AddObject(Channel);
                    }
                }
                message = errorcheck == "" ? "" : string.Format(Message.GetString("2018082851", null), errorcheck.TrimEnd(','), Util.GetLang("SI24100QuestID"));
                if (string.IsNullOrEmpty(message))
                {
                    _db.SaveChanges();
                }
                else
                {
                    throw new MessageException(MessageType.Message, "2018121755", "", new string[] { errorcheck.TrimEnd(',') });
                }
                Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
               // return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
            return _logMessage;
        }

        private void Update_AR_Channel(AR_Channel t, AR21100_pgLoadChannel_Result s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Type = s.Type;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

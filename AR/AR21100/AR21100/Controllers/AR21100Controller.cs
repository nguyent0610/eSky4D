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

        public ActionResult Index()
        {

            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetChannel()
        {
            return this.Store(_db.AR21100_pgLoadChannel().ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {

                StoreDataHandler dataHandler = new StoreDataHandler(data["lstChannel"]);
                ChangeRecords<AR_Channel> lstChannel = dataHandler.BatchObjectData<AR_Channel>();
                foreach (AR_Channel deleted in lstChannel.Deleted)
                {
                    var del = _db.AR_Channel.Where(p => p.Code == deleted.Code).FirstOrDefault();
                    if (del != null)
                    {
                        _db.AR_Channel.DeleteObject(del);
                    }
                }

                lstChannel.Created.AddRange(lstChannel.Updated);

                foreach (AR_Channel curChannel in lstChannel.Created)
                {
                    if (curChannel.Code.PassNull() == "") continue;

                    var Channel = _db.AR_Channel.Where(p => p.Code.ToLower() == curChannel.Code.ToLower()).FirstOrDefault();

                    if (Channel != null)
                    {
                        if (Channel.tstamp.ToHex() == curChannel.tstamp.ToHex())
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

                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
            }
        }

        private void Update_AR_Channel(AR_Channel t, AR_Channel s, bool isNew)
        {
            if (isNew)
            {
                t.Code = s.Code;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;

            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
    }
}

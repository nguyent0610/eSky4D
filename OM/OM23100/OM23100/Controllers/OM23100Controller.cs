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
using HQ.eSkyFramework.HQControl;
namespace OM23100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23100Controller : Controller
    {
        private string _screenNbr = "OM23100";
        private string _userName = Current.UserName;
        OM23100Entities _db = Util.CreateObjectContext<OM23100Entities>(false);
        public ActionResult Index()
        {
            Column clm;
            NumberColumn nbc;
            var grid = this.GetCmp<GridPanel>("grdOM_FCS");
            DataSource ds = new DataSource();
            var lstIN_ProductClass = _db.OM23100_getIN_ProductClass().ToList();
            
            //Column Sell In
            clm = new Column
            {
                Text = Util.GetLang("OM23100_SellIn"),
                ID = "txt_SellIn"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    //DataIndex = "SellIn_"+lstIN_ProductClass[i].ClassID,
                    Width=50,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column Coverage
            clm = new Column
            {
                Text = Util.GetLang("OM23100_Coverage"),
                ID = "txt_Coverage"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    //DataIndex = "Coverage_"+lstIN_ProductClass[i].ClassID,
                    Width = 50,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column DNA
            clm = new Column
            {
                Text = Util.GetLang("OM23100_DNA"),
                ID = "txt_DNA"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    //DataIndex = "DNA_"+lstIN_ProductClass[i].ClassID,
                    Width = 50,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column Visit
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23100_Visit"),
                ID = "txt_Visit",
                Width = 50,
                DataIndex="Visit",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=2,
                        MinValue=0
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column LPPC
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23100_LPPC")+"(%)",
                ID = "txt_LPPC",
                Width = 50,
                DataIndex = "LPPC",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=2,
                        MinValue=0
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column ForcusedSKU
            clm = new Column
            {
                Text = Util.GetLang("OM23100_ForcusedSKU"),
                ID = "txt_ForcusedSKU"
            };

            for (int i = 0; i < lstIN_ProductClass.Count; i++)
            {
                NumberColumn nbcl = new NumberColumn
                {
                    Text = lstIN_ProductClass[i].Descr,
                    //DataIndex = "ForcusedSKU_"+lstIN_ProductClass[i].ClassID,
                    Width = 50,
                    Editor =
                    {
                        new NumberField
                        {
                            DecimalPrecision=2,
                            MinValue=0
                        }
                    }
                };
                clm.Columns.Add(nbcl);
                grid.AddColumn(clm);
            }

            //Column VisitTime
            nbc = new NumberColumn
            {
                Text = Util.GetLang("OM23100_VisitTime"),
                ID = "txt_VisitTime",
                Width = 50,
                DataIndex = "VisitTime",
                Editor =
                {
                    new NumberField
                    {
                        DecimalPrecision=2,
                        MinValue=0
                    }
                }
            };
            grid.AddColumn(nbc);

            //Column Reject
            CommandColumn commentclm = new CommandColumn
            {
                Width = 100,
                ID = "CommandReject",
                Commands =
                {
                    new GridCommand
                    {
                        Text = Util.GetLang("Reject"), 
                        ToolTip = 
                        {
                            Text = Util.GetLang("Rejectrowchanges"), 
                        },
                        CommandName = "reject",
                        Icon = Ext.Net.Icon.ArrowUndo
                    }
                },
                PrepareToolbar =
                {
                    Handler = "toolbar.items.get(0).setVisible(record.dirty);"
                },
                Listeners =
                {
                    Command =
                    {
                        Handler = "grd_Reject(record);"
                    }
                }
            };
            grid.AddColumn(commentclm);


            Util.InitRight(_screenNbr);
            return View();
        }
        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }
        public ActionResult GetOM_FCS(string BranchID, DateTime FCSDate)
        {
            return this.Store(_db.OM23100_pgLoadGrid(BranchID, FCSDate));
        }
        //[HttpPost]
        //public ActionResult Save(FormCollection data)
        //{
        //    string BranchID = data["cboDist"];
        //    string FCSDate_temp = data["dateFcs"];
        //    DateTime FCSDate = DateTime.Parse(FCSDate_temp);
        //    try
        //    {
        //        StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_FCS"]);
        //        ChangeRecords<OM23100_pgOM_FCS_Result> lstOM_FCS = dataHandler.BatchObjectData<OM23100_pgOM_FCS_Result>();
        //        foreach (OM23100_pgOM_FCS_Result deleted in lstOM_FCS.Deleted)
        //        {
        //            var del = _db.OM_FCS.FirstOrDefault(p => p.SlsperId == deleted.SlsperId && p.BranchID == BranchID && p.FCSDate.Year==FCSDate.Year && p.FCSDate.Month==FCSDate.Month);
        //            if (del != null)
        //            {
        //                _db.OM_FCS.DeleteObject(del);
        //            }
        //        }

        //        lstOM_FCS.Created.AddRange(lstOM_FCS.Updated);

        //        foreach (OM23100_pgOM_FCS_Result curLang in lstOM_FCS.Created)
        //        {
        //            if (curLang.SlsperId.PassNull() == "") continue;

        //            var lang = _db.OM_FCS.FirstOrDefault(p => p.SlsperId.ToLower() == curLang.SlsperId.ToLower() && p.BranchID.ToLower() == BranchID.ToLower() && p.FCSDate.Year == FCSDate.Year && p.FCSDate.Month == FCSDate.Month);

        //            if (lang != null)
        //            {
        //                if (lang.tstamp.ToHex() == curLang.tstamp.ToHex())
        //                {
        //                    Update_AccessDetRights(lang, curLang, false);
        //                }
        //                else
        //                {
        //                    throw new MessageException(MessageType.Message, "19");
        //                }
        //            }
        //            else
        //            {
        //                lang = new OM_FCS();
        //                lang.BranchID = BranchID;
        //                lang.FCSDate=FCSDate;
        //                Update_AccessDetRights(lang, curLang, true);
        //                _db.OM_FCS.AddObject(lang);
        //            }
        //        }

        //        _db.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is MessageException) return (ex as MessageException).ToMessage();
        //        return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
        //    }
        //}
        //private void Update_AccessDetRights(OM_FCS t, OM23100_pgOM_FCS_Result s, bool isNew)
        //{
        //    if (isNew)
        //    {
        //        t.SlsperId = s.SlsperId;
        //        t.Crtd_DateTime = DateTime.Now;
        //        t.Crtd_Prog = _screenNbr;
        //        t.Crtd_User = _userName;
        //    }

        //    t.Coverage = s.Coverage;
        //    t.DNA = s.DNA;
        //    t.Visit = s.Visit;
        //    t.SellIn = s.SellIn;
        //    t.LPPC = s.LPPC;
        //    t.ForcusedSKU = s.ForcusedSKU;
        //    t.VisitTime = s.VisitTime;

        //    t.LUpd_DateTime = DateTime.Now;
        //    t.LUpd_Prog = _screenNbr;
        //    t.LUpd_User = _userName;

        //}
    }
}

using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using HQ.eSkyFramework;
using HQ.eSkySys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;
using System.Data.Metadata.Edm;
using System.Security.Cryptography;
using System.Text;
using PartialViewResult = System.Web.Mvc.PartialViewResult;
using System.IO;
using HQSendMailApprove;
using System.Web;
using Aspose.Cells;
using HQFramework.DAL;
using System.Data;
using HQFramework.Common;
using System.Drawing;
using eBiz4DApp_PJP;
using System.Data.SqlClient;
using System.Globalization;

namespace OM22200.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM22200Controller : Controller
    {
        private string _screenNbr = "OM22200";
        private string _beginStatus = "H";
        private string _noneStatus = "N";
        private string _completeStatus = "C";
        OM22200Entities _db = Util.CreateObjectContext<OM22200Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM22200");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\OM22200");
                }
                return _filePath;
            }
        }

        //
        // GET: /OM22200/
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

        public ActionResult GetSaleRouteMaster(string branchId, string slsperid, string routeID, string pjpID)
        {
            var slsRouteMstr = _db.OM22200_pgSaleRouteMaster(Current.UserName, branchId, pjpID, routeID, slsperid).ToList();
            return this.Store(slsRouteMstr);
        }

        public ActionResult GetPJP(string branchId, string pjpID)
        {
            var pjp = _db.OM_PJP.FirstOrDefault(x => x.BranchID == branchId && x.PJPID == pjpID);
            return this.Store(pjp);
        }

        public ActionResult GetCustomer(string branchID, string pjpID, 
            string slsperID, string routeID,string lstCust)
        {
            var custs = _db.OM22200_pcCustomer(branchID, pjpID, Current.UserName, slsperID, routeID, lstCust).ToList();
            return this.Store(custs);
        }

        [ValidateInput(false)]
        public ActionResult SaveData(FormCollection data)
        {
            try
            {
                var handle = data["cboHandle"];
                var pjpID = data["cboPJPID"];
                var branchID = data["cboBranchID"];

                if (!string.IsNullOrWhiteSpace(pjpID) && !string.IsNullOrWhiteSpace(branchID))
                {
                    var lstPJPHandler = new StoreDataHandler(data["lstPJP"]);
                    var inputPJP = lstPJPHandler.ObjectData<OM_PJP>().FirstOrDefault();

                    if (inputPJP != null)
                    {
                        inputPJP.PJPID = pjpID;
                        inputPJP.BranchID = branchID;

                        #region PJP
                        var pjp = _db.OM_PJP.FirstOrDefault(x => x.BranchID == inputPJP.BranchID && x.PJPID == inputPJP.PJPID);
                        if (pjp != null)
                        {
                            if (!string.IsNullOrWhiteSpace(handle) && handle != _noneStatus)
                            {
                                pjp.StatusHandle = handle;
                                pjp.Status = handle == _completeStatus ? true : false;
                            }
                            updatePJP(ref pjp, inputPJP, false);
                        }
                        else 
                        {
                            updatePJP(ref pjp, inputPJP, true);
                            _db.OM_PJP.AddObject(pjp);
                        }
                        #endregion

                        #region SlsRouteMaster
                        var lstSaleRouteMasterHandler = new StoreDataHandler(data["lstSaleRouteMaster"]);
                        var lstSaleRouteMaster = lstSaleRouteMasterHandler.BatchObjectData<OM22200_pgSaleRouteMaster_Result>();

                        lstSaleRouteMaster.Updated.AddRange(lstSaleRouteMaster.Created);
                        foreach (var slsRoute in lstSaleRouteMaster.Updated)
                        {
                            var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x=>x.BranchID == pjp.BranchID 
                                && x.PJPID == pjp.PJPID && x.CustID == slsRoute.CustID
                                && x.SalesRouteID == slsRoute.SalesRouteID && x.SlsPerID == slsRoute.SlsPerID);
                            if (obj != null)
                            {
                                if (obj.tstamp.ToHex() == slsRoute.tstamp.ToHex())
                                {
                                    updateSlsRoute(ref obj, slsRoute, false);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                            else
                            {
                                updateSlsRoute(ref obj, slsRoute, true);
                                _db.OM_SalesRouteMaster.AddObject(obj);
                            }
                        }

                        foreach (var deleted in lstSaleRouteMaster.Deleted)
                        {
                            var obj = _db.OM_SalesRouteMaster.FirstOrDefault(x => x.BranchID == pjp.BranchID
                                    && x.PJPID == pjp.PJPID && x.CustID == deleted.CustID
                                    && x.SalesRouteID == deleted.SalesRouteID && x.SlsPerID == deleted.SlsPerID);
                            if (obj != null)
                            {
                                if (obj.tstamp.ToHex() == deleted.tstamp.ToHex())
                                {
                                    _db.OM_SalesRouteMaster.DeleteObject(obj);
                                }
                                else
                                {
                                    throw new MessageException(MessageType.Message, "19");
                                }
                            }
                        }

                        #endregion

                        _db.SaveChanges();
                        return Json(new { success = true, msgCode = 201405071 });
                    }
                    else
                    {
                        throw new MessageException(MessageType.Message, "53");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "53");
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }

        public ActionResult DeletePJP(string branchId, string pjpID)
        {
            try
            {
                var pjp = _db.OM_PJP.FirstOrDefault(x => x.BranchID == branchId && x.PJPID == pjpID);
                if (pjp != null)
                {
                    if (pjp.StatusHandle == _beginStatus)
                    {
                        _db.OM_PJP.DeleteObject(pjp);

                        var lstSlsRoute = _db.OM_SalesRouteMaster.Where(x => x.BranchID == branchId && x.PJPID == pjpID).ToList();
                        foreach (var slsRoute in lstSlsRoute)
                        {
                            _db.OM_SalesRouteMaster.DeleteObject(slsRoute);
                        }
                        _db.SaveChanges();
                        return Json(new { success = true, msgcode = 2015032101 });
                    }
                    else {
                        throw new MessageException(MessageType.Message, "20140306");
                    }
                }
                else
                {
                    throw new MessageException(MessageType.Message, "89", "", new string[] { Util.GetLang("PJPID") });
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }
        }


        [HttpPost]
        public ActionResult OM22200Import(FormCollection data)
        {
            string BranchID = data["BranchID"].PassNull();
            string PJP = data["PJPID"].PassNull();
            var tmpCheckRowAdded = "";
            string lstUser = "";
            string lstName = "";
            string lstPass = "";
            var k = 0;
            try
            {
                var date = DateTime.Now.Date;
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("fupImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                string messagestrERouteID = string.Empty;
                string messagestrECustID = string.Empty;
                string messagestrESlsperID = string.Empty;
                string messagestrEBeginDate = string.Empty;
                string messagestrEEndDate = string.Empty;
                string messagestrETBH = string.Empty;
                string messagestrETS = string.Empty;
                string messageDate = string.Empty;
                string messageerror = string.Empty;
                string messageduplicate = string.Empty;
                string message = string.Empty;
                if (fileInfo.Extension == ".xls" || fileInfo.Extension == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    if (workbook.Worksheets.Count > 0)
                    {
                        Worksheet workSheet = workbook.Worksheets[0];

                        string strEBanchID = workSheet.Cells[1, 2].StringValue.PassNull().Trim();//dataArray.GetValue(2, 3).PassNull();// w1.Rows[1].Cells[2).PassNull();
                        string strEPJP = getStrBetweenTags(workSheet.Cells[0, 1].StringValue.PassNull().Trim(), "(", ")");// dataArray.GetValue(2, 3).PassNull();
                        string strERouteID = "";
                        string strECustID = "";
                        string strESlsperID = "";
                        string strEBeginDate = "";
                        string strEEndDate = "";
                        string strETS = "";
                        string strETBH = "";
                        string strESTT = "";
                        DateTime startDate = DateTime.Now;
                        DateTime endDate = DateTime.Now;
                        if (strEPJP.ToUpper().Trim() != PJP.ToUpper().Trim() || BranchID != strEBanchID.ToUpper().Trim())
                        {
                            //return Json(new { success = false, messid = 201401221, error = new string[] { strEPJP, strEBanchID, PJP, BranchID } });
                            throw new MessageException(MessageType.Message, "201401221", "",
                                new string[] { strEPJP, strEBanchID, PJP, BranchID });
                        }
                        var objPJP = _db.OM_PJP.Where(p => p.PJPID == PJP).FirstOrDefault();
                        if (objPJP == null)
                        {
                            objPJP = new OM_PJP();
                            objPJP.PJPID = PJP;
                            objPJP.BranchID = BranchID;
                            objPJP.Descr = "Kế hoạch viếng thăm " + BranchID;
                            objPJP.LUpd_DateTime = DateTime.Today;
                            objPJP.LUpd_Prog = _screenNbr;
                            objPJP.LUpd_User = Current.UserName;
                            objPJP.Status = true;
                            objPJP.StatusHandle = "C";

                            objPJP.Crtd_DateTime = DateTime.Today;
                            objPJP.Crtd_Prog = _screenNbr;
                            objPJP.Crtd_User = Current.UserName;

                            _db.OM_PJP.AddObject(objPJP);
                        }

                        string lstCustomer = "";
                        string strtmpError = "";
                        string id = Guid.NewGuid().ToString();
                        for (int i = 5; i < workSheet.Cells.MaxDataRow; i++)
                        {


                            strESTT = workSheet.Cells[i, 0].StringValue;//dataArray.GetValue(i, 1).PassNull();
                            strERouteID = workSheet.Cells[i, 17].StringValue;//dataArray.GetValue(i, 18).PassNull();
                            strECustID = workSheet.Cells[i, 1].StringValue;//dataArray.GetValue(i, 2).PassNull();
                            strESlsperID = workSheet.Cells[i, 4].StringValue;//dataArray.GetValue(i, 5).PassNull();
                            strEBeginDate = workSheet.Cells[i, 6].StringValue;//dataArray.GetValue(i, 7).PassNull();
                            strEEndDate = workSheet.Cells[i, 7].StringValue;//dataArray.GetValue(i, 8).PassNull();
                            strETS = workSheet.Cells[i, 8].StringValue;//dataArray.GetValue(i, 9).PassNull();
                            strETBH = workSheet.Cells[i, 9].StringValue;//dataArray.GetValue(i, 10).PassNull();

                            if (strECustID == "") continue;
                            else if (strERouteID == ""
                                 || strECustID == ""
                                 || strESlsperID == ""
                                 || strETS == ""
                                || strETBH == ""
                                || strEBeginDate == ""
                                || strEEndDate == "")
                            {
                                if (strERouteID == "")
                                {
                                    messagestrERouteID += (i + 1).ToString() + ",";
                                    continue;
                                }
                                if (strECustID == "")
                                {
                                    messagestrECustID += (i + 1).ToString() + ",";
                                    continue;
                                }

                                if (strESlsperID == "")
                                {
                                    messagestrESlsperID += (i + 1).ToString() + ",";
                                    continue;
                                }
                                if (strETS == "")
                                {
                                    messagestrETS += (i + 1).ToString() + ",";
                                    continue;
                                }
                                if (strETBH == "")
                                {
                                    messagestrETBH += (i + 1).ToString() + ",";
                                    continue;
                                }
                                if (strEBeginDate == "")
                                {
                                    messagestrEBeginDate += (i + 1).ToString() + ",";
                                    continue;
                                }
                                if (strEEndDate == "")
                                {
                                    messagestrEEndDate += (i + 1).ToString() + ",";
                                    continue;
                                }
                            }
                            else
                            {
                                try
                                {
                                    startDate = workSheet.Cells[i, 6].DateTimeValue.ToDateShort();// DateTime.FromOADate(double.Parse(workSheet.Cells[i, 6].StringValue)).Short();
                                    endDate = workSheet.Cells[i, 7].DateTimeValue.ToDateShort(); //DateTime.FromOADate(double.Parse(workSheet.Cells[i, 7].StringValue)).Short();
                                }
                                catch
                                {
                                    messageDate += string.Format("Dòng {0} dữ liệu ngày tháng không hợp lệ<br/>", (i + 1).ToString());
                                    continue;
                                }
                                OM_SalesRouteMasterImport objImport = new OM_SalesRouteMasterImport();
                                bool isNew = false;

                                lstCustomer += strECustID + ";";
                                if (_db.OM_SalesRouteMasterImport.Where(p => p.ID == id
                                                                                    && p.BranchID == BranchID
                                                                                     && p.PJPID == BranchID
                                                                                      && p.SalesRouteID == strERouteID
                                                                                      && p.CustID == strECustID
                                                                                       && p.SlsPerID == strESlsperID).ToList().Count == 0)
                                {
                                    objImport.ID = id;
                                    objImport.BranchID = BranchID;
                                    objImport.PJPID = PJP;
                                    objImport.SalesRouteID = strERouteID;
                                    objImport.CustID = strECustID;
                                    objImport.SlsPerID = strESlsperID;
                                    objImport.tstamp = new byte[1];
                                    objImport.StartDate = startDate;
                                    objImport.EndDate = endDate; ;
                                    objImport.SlsFreq = workSheet.Cells[i, 8].StringValue;//  dataArray.GetValue(i, 9).ToString().Trim().ToUpper();
                                    objImport.SlsFreqType = "R";
                                    objImport.WeekofVisit = workSheet.Cells[i, 9].StringValue;// dataArray.GetValue(i, 10).ToString().Trim().ToUpper();
                                    objImport.Mon = workSheet.Cells[i, 10].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 11) == null ? false : dataArray.GetValue(i, 11).ToString().Trim().ToUpper() == "X" ? true : false;
                                    objImport.Tue = workSheet.Cells[i, 11].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 12) == null ? false : dataArray.GetValue(i, 12).ToString().Trim().ToUpper() == "X" ? true : false;
                                    objImport.Wed = workSheet.Cells[i, 12].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 13) == null ? false : dataArray.GetValue(i, 13).ToString().Trim().ToUpper() == "X" ? true : false;
                                    objImport.Thu = workSheet.Cells[i, 13].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 14) == null ? false : dataArray.GetValue(i, 14).ToString().Trim().ToUpper() == "X" ? true : false;
                                    objImport.Fri = workSheet.Cells[i, 14].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 15) == null ? false : dataArray.GetValue(i, 15).ToString().Trim().ToUpper() == "X" ? true : false;
                                    objImport.Sat = workSheet.Cells[i, 15].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 16) == null ? false : dataArray.GetValue(i, 16).ToString().Trim().ToUpper() == "X" ? true : false;
                                    objImport.Sun = workSheet.Cells[i, 16].StringValue.ToUpper() == "X" ? true : false; ;// dataArray.GetValue(i, 17) == null ? false : dataArray.GetValue(i, 17).ToString().Trim().ToUpper() == "X" ? true : false;
                                    try
                                    {
                                        objImport.VisitSort = workSheet.Cells[i, 19].IntValue;// dataArray.GetValue(i, 20) == null ? 0 : dataArray.GetValue(i, 20).ToString().Trim().ToUpper() == "" ? 0 : int.Parse(dataArray.GetValue(i, 20).ToString().Trim().ToUpper());
                                    }
                                    catch
                                    {
                                        objImport.VisitSort = 0;
                                    }
                                    objImport.LUpd_DateTime = objImport.LUpd_DateTime = DateTime.Now;
                                    objImport.LUpd_Prog = objImport.LUpd_Prog = _screenNbr;
                                    objImport.LUpd_User = objImport.LUpd_User = Current.UserName;
                                    objImport.Crtd_DateTime = objImport.Crtd_DateTime = DateTime.Now;
                                    objImport.Crtd_Prog = objImport.Crtd_Prog = _screenNbr;
                                    objImport.Crtd_User = objImport.Crtd_User = Current.UserName;
                                    if (isValidSelOMSalesRouteMaster(objImport, false))
                                    {
                                        if (workSheet.Cells[i, 20].StringValue != null && workSheet.Cells[i, 20].StringValue == "X")
                                        {
                                            objImport.Del = true;

                                        }
                                        _db.OM_SalesRouteMasterImport.AddObject(objImport);
                                    }
                                    else
                                    {
                                        messageerror += (i + 1).ToString() + ",";
                                        //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu không hợp lệ" + "\r";
                                    }

                                }
                                else messageduplicate += (i + 1).ToString() + ",";  //strtmpError += "   STT: " + strESTT + "Error: Dữ liệu bi trùng" + "\r";
                            }
                        }

                        _db.SaveChanges();
                        Exec(id);
                        message = messagestrECustID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrECustID, workSheet.Cells[3, 1].StringValue);
                        message += messagestrESlsperID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrESlsperID, workSheet.Cells[3, 4].StringValue);
                        message += messagestrETS == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrETS, workSheet.Cells[3, 8].StringValue);
                        message += messagestrETBH == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrETBH, workSheet.Cells[3, 9].StringValue);
                        message += messagestrEBeginDate == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEBeginDate, workSheet.Cells[3, 6].StringValue);
                        message += messagestrEEndDate == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrEEndDate, workSheet.Cells[3, 7].StringValue);
                        message += messageDate == "" ? "" : string.Format("Dòng {0} dữ liệu ngày tháng không hợp lệ<br/>", messageDate.ToString());
                        message += messagestrERouteID == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ thiếu {1}<br/>", messagestrERouteID, workSheet.Cells[3, 13].StringValue);
                        message += messageerror == "" ? "" : string.Format("Dòng {0} dữ liệu không hợp lệ<br/>", messageerror);
                        message += messageduplicate == "" ? "" : string.Format("Dòng {0} dữ liệu bi trùng<br/>", messageduplicate);

                        Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                    }
                    return _logMessage;

                }
                else
                {
                    Util.AppendLog(ref _logMessage, "2014070701", parm: new[] { fileInfo.Extension.Replace(".", "") });
                }
            }
            catch (Exception ex)
            {

                //return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, messid = 9991, type = "error", errorMsg = ex.ToString() });
                }
            }
            return _logMessage;
        }

        private string renameFileName(string oldfileName, string newName, bool isPlus)
        {
            var fileName = string.Empty;

            var fileNameNotExt = Path.GetFileNameWithoutExtension(oldfileName);
            var ext = Path.GetExtension(oldfileName);

            if (isPlus)
            {
                fileName = string.Format("{0}{1}{2}", fileNameNotExt, newName, ext);
            }
            else
            {
                fileName = string.Format("{0}{2}", fileNameNotExt, ext);
            }

            return fileName;
        }


        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                //string branchID = data["BranchID"].PassNull();
                string branchID = data["BranchID"].PassNull();
                string pjp = data["PJPID"].PassNull();
                string branchName = data["BranchName"].PassNull();
                string routeID = data["RouteID"].PassNull();
                string slsperID = data["SlsperID"].PassNull();
                var headerRowIdx = 3;

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet SheetMCP = workbook.Worksheets[0];
                SheetMCP.Name = Util.GetLang("MCP");
                DataAccess dal = Util.Dal();
                Style style = workbook.GetStyleInPool(0);
                StyleFlag flag = new StyleFlag();
                Range range;
                Cell cell;
                #region master data
                ParamCollection pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtCustomer = dal.ExecDataTable("OM22200_peCustomer", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtCustomer, true, 0, 26, false);// du lieu AR_Customer


                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtSales = dal.ExecDataTable("OM22200_peSalesperson", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtSales, true, 0, 52, false);// du lieu Salesperson


                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtRoute = dal.ExecDataTable("OM22200_peRoute", CommandType.StoredProcedure, ref pc);
                SheetMCP.Cells.ImportDataTable(dtRoute, true, 0, 78, false);// du lieu SalesRoute


                SheetMCP.Cells["Z1"].PutValue("W159");
                SheetMCP.Cells["Z2"].PutValue("W2610");
                SheetMCP.Cells["Z3"].PutValue("W3711");
                SheetMCP.Cells["Z4"].PutValue("W4812");
                SheetMCP.Cells["Z5"].PutValue("OW");
                SheetMCP.Cells["Z6"].PutValue("EW");
                SheetMCP.Cells["Z7"].PutValue("NA");

                #endregion

                #region header info
                // Title header
                SetCellValue(SheetMCP.Cells["B1"],
                    string.Format("{0} {1}", Util.GetLang("OM22200EHeader") + "(" + pjp + ")", (string.IsNullOrWhiteSpace(branchID) ? string.Format("({0})", branchID) : string.Empty)),
                    TextAlignmentType.Center, TextAlignmentType.Center, true, 16, true);
                SheetMCP.Cells.Merge(0, 1, 1, 6);

                // Title info
                SetCellValue(SheetMCP.Cells["B2"], Util.GetLang("BranchID"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["B3"], Util.GetLang("BranchName"), TextAlignmentType.Center, TextAlignmentType.Right, true, 10, true);
                SetCellValue(SheetMCP.Cells["C2"], branchID, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);
                SetCellValue(SheetMCP.Cells["C3"], branchName, TextAlignmentType.Center, TextAlignmentType.Left, false, 10, true);

                // Header text columns
                // before of Route column
                var beforeColTexts = new string[] { "N0", "CustID", "CustName", "Address", "SlsperID", "SlsName", "StartDate", "EndDate", "SlsFreq", "WeekofVisit" };
                for (int i = 0; i < beforeColTexts.Length; i++)
                {
                    var colIdx = i;
                    SetCellValue(SheetMCP.Cells[3, colIdx], Util.GetLang(beforeColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }

                //Route column
                var daysOfWeeks = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                SetCellValue(SheetMCP.Cells[headerRowIdx, beforeColTexts.Length], Util.GetLang("Route"), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                SheetMCP.Cells.Merge(headerRowIdx, beforeColTexts.Length, 1, daysOfWeeks.Length);
                for (int i = 0; i < daysOfWeeks.Length; i++)
                {
                    var colIdx = beforeColTexts.Length + i;
                    SetCellValue(SheetMCP.Cells[headerRowIdx + 1, colIdx], Util.GetLang(daysOfWeeks[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);
                }

                // after of Route column
                var afterColTexts = new string[] { "SalesRouteID", "RouteName", "VisitSort", "CustCancel" };
                for (int i = 0; i < afterColTexts.Length; i++)
                {
                    var colIdx = beforeColTexts.Length + daysOfWeeks.Length + i;
                    SetCellValue(SheetMCP.Cells[headerRowIdx, colIdx], Util.GetLang(afterColTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10);



                    SheetMCP.Cells.Merge(headerRowIdx, colIdx, 2, 1);
                }



                var allColumns = new List<string>();
                allColumns.AddRange(beforeColTexts);
                allColumns.AddRange(daysOfWeeks);
                allColumns.AddRange(afterColTexts);

                #endregion

                #region formular



                Validation validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.Date;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.GreaterOrEqual;
                validation.Formula1 = DateTime.Now.ToShortDateString();
                validation.InputTitle = "Chọn Ngày Bắt Đầu(MM/dd/yyyy)";
                validation.InputMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");
                validation.ErrorMessage = "Ngày Bắt Đầu Không Thể Nhỏ Hơn Ngày " + DateTime.Now.ToString("MM/dd/yyyy");


                CellArea area;
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("StartDate");
                area.EndColumn = allColumns.IndexOf("StartDate");
                validation.AddArea(area);

                string formulaDate = "=$" + Getcell(allColumns.IndexOf("StartDate")) + "$6";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.Date;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.GreaterOrEqual;
                validation.Formula1 = formulaDate;
                validation.InputTitle = "Chọn Ngày Kết Thúc(MM/dd/yyyy)";
                validation.InputMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";
                validation.ErrorMessage = "Ngày Kết Thúc Không Thể Nhỏ Hơn Ngày Bắt Đầu ";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("EndDate");
                area.EndColumn = allColumns.IndexOf("EndDate");
                validation.AddArea(area);

                //custid
                string formulaCustomer = "=$AA$2:$AA$" + (dtCustomer.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaCustomer;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Mã Khách Hàng ";
                validation.ErrorMessage = "Mã Khách Hàng này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("CustID");
                area.EndColumn = allColumns.IndexOf("CustID");
                validation.AddArea(area);

                //SALES
                string formulaSales = "=$BA$2:$BA$" + (dtSales.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaSales;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Mã Nhân Viên Bán Hàng";
                validation.ErrorMessage = "Mã Nhân Viên này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("SlsperID");
                area.EndColumn = allColumns.IndexOf("SlsperID");
                validation.AddArea(area);

                //Route
                string formulaRoutes = "=$CA$2:$CA$" + (dtRoute.Rows.Count + 2);
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaRoutes;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Mã Tuyến Đường";
                validation.ErrorMessage = "Mã Tuyến Đường này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("SalesRouteID");
                area.EndColumn = allColumns.IndexOf("SalesRouteID");
                validation.AddArea(area);
                //Requency LIST
                string formulaRequenc = "F1,F2,F4,F8,F12,A";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaRequenc;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Tần Suất Thăm Viếng";
                validation.ErrorMessage = "Mã Tần Suất này không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("SlsFreq");
                area.EndColumn = allColumns.IndexOf("SlsFreq");
                validation.AddArea(area);


                string formula = "=IF(I6=\"F1\",$Z$1:$Z$4,IF(I6=\"F2\",$Z$5:$Z$6,$Z$7:$Z$7))";// + dtOMRoute.Rows.Count + 2;               
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formula;
                validation.InputTitle = "";
                validation.InputMessage = "Chọn Tuần Bán Hàng";
                validation.ErrorMessage = "Mã Tuần Bán Hàng Không tồn tại";

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("WeekofVisit");
                area.EndColumn = allColumns.IndexOf("WeekofVisit");
                validation.AddArea(area);

                string formulaCheck = "X";
                validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaCheck;
                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("Mon");
                area.EndColumn = allColumns.IndexOf("Sun");
                validation.AddArea(area);

                area = new CellArea();
                area.StartRow = 5;
                area.EndRow = dtCustomer.Rows.Count + 5;
                area.StartColumn = allColumns.IndexOf("CustCancel");
                area.EndColumn = allColumns.IndexOf("CustCancel");
                validation.AddArea(area);


                string formulaCustName = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,2,0)),\"\",VLOOKUP({0},AA:AC,2,0))", "B6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("CustName")) + "6"].SetSharedFormula(formulaCustName, (dtCustomer.Rows.Count + 6), 1);


                string formulaCustAddr = string.Format("=IF(ISERROR(VLOOKUP({0},AA:AC,3,0)),\"\",VLOOKUP({0},AA:AC,3,0))", "B6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("Address")) + "6"].SetSharedFormula(formulaCustAddr, (dtCustomer.Rows.Count + 6), 1);


                string formulaSalesName = string.Format("=IF(ISERROR(VLOOKUP({0},BA:BC,2,0)),\"\",VLOOKUP({0},BA:BC,2,0))", "E6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("SlsName")) + "6"].SetSharedFormula(formulaSalesName, (dtCustomer.Rows.Count + 6), 1);


                string formulaRoute = string.Format("=IF(ISERROR(VLOOKUP({0},CA:CC,2,0)),\"\",VLOOKUP({0},CA:CC,2,0))", "R6");
                SheetMCP.Cells[Getcell(allColumns.IndexOf("RouteName")) + "6"].SetSharedFormula(formulaRoute, (dtCustomer.Rows.Count + 6), 1);


                string formulaSTT = "=IFERROR( IF(B6<>\"\",A5+1 & \"\",\"\"),1)";
                SheetMCP.Cells["A6"].SetSharedFormula(formulaSTT, (dtCustomer.Rows.Count + 6), 1);



                #endregion
                #region export data
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@PJPID", DbType.String, clsCommon.GetValueDBNull(branchID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@RouteID", DbType.String, clsCommon.GetValueDBNull(routeID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@SlsperID", DbType.String, clsCommon.GetValueDBNull(slsperID), ParameterDirection.Input, 30));

                DataTable dtDataExport = dal.ExecDataTable("OM22200_peExportData", CommandType.StoredProcedure, ref pc);
                //SheetMCP.Cells.ImportDataTable(dtDataExport, false, "B6");// du lieu data export


                for (int i = 0; i < dtDataExport.Rows.Count; i++)
                {
                    for (int j = 0; j < allColumns.Count; j++)
                    {
                        if (allColumns[j] == "N0" || allColumns[j] == "CustName" || allColumns[j] == "SlsName" || allColumns[j] == "Address" || allColumns[j] == "RouteName")
                        {
                            //SheetMCP.Cells[5 + i, j].PutValue(i + 1);
                        }
                        else if (dtDataExport.Columns.Contains(allColumns[j]))
                        {
                            SheetMCP.Cells[5 + i, j].PutValue(dtDataExport.Rows[i][allColumns[j]]);
                        }
                    }
                }
                #endregion
                #region Fomat cell

                style = SheetMCP.Cells[allColumns.IndexOf("StartDate")].GetStyle();
                style.Custom = "MM/dd/yyyy";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Left;

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("StartDate")) + "5", Getcell(allColumns.IndexOf("StartDate")) + dtCustomer.Rows.Count + 5);
                range.SetStyle(style);

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("EndDate")) + "5", Getcell(allColumns.IndexOf("EndDate")) + dtCustomer.Rows.Count + 5);
                range.SetStyle(style);


                style = SheetMCP.Cells[allColumns.IndexOf("VisitSort")].GetStyle();
                style.Custom = "#,##0";
                style.Font.Color = Color.Black;
                style.HorizontalAlignment = TextAlignmentType.Right;

                range = SheetMCP.Cells.CreateRange(Getcell(allColumns.IndexOf("VisitSort")) + "5", Getcell(allColumns.IndexOf("VisitSort")) + dtCustomer.Rows.Count + 5);
                range.SetStyle(style);


                style = SheetMCP.Cells["Z1"].GetStyle();
                style.Font.Color = Color.Transparent;
                flag.FontColor = true;
                flag.NumberFormat = true;
                flag.Locked = true;

                range = SheetMCP.Cells.CreateRange("Z1", "ZZ" + (dtCustomer.Rows.Count + 1));
                range.ApplyStyle(style, flag);


                #endregion
                SheetMCP.AutoFitColumns();

                SheetMCP.Cells.Columns[allColumns.IndexOf("CustID")].Width = 30;
                SheetMCP.Cells.Columns[allColumns.IndexOf("CustName")].Width = 30;
                SheetMCP.Cells.Columns[allColumns.IndexOf("SlsName")].Width = 30;
                SheetMCP.Cells.Columns[allColumns.IndexOf("Address")].Width = 30;

                //SheetPOSuggest.Protect(ProtectionType.Objects);
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = Util.GetLang("OM22200") + ".xlsx" };

            }
            catch (Exception ex)
            {
                if (ex is MessageException)
                {
                    return (ex as MessageException).ToMessage();
                }
                else
                {
                    return Json(new { success = false, type = "error", errorMsg = ex.ToString() });
                }
            }

        }

        public ActionResult Download(string filePath, string fileName)
        {
            var dlFileName = string.Format("{0}.xlsx", fileName);

            return File(filePath, "application/xls", dlFileName);
        }

        #region other

        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle = false)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            if (isTitle)
                style.Font.Color = Color.Blue;
            c.SetStyle(style);
        }

        public string Exec(string id)
        {
            try
            {


                DateTime Todate = DateTime.Now;
                DateTime Fromdate = DateTime.Now;
                var type = "import";
                var _daapp = Util.Dal(false);



                DateTime date = DateTime.Now;

                string ID = Guid.NewGuid().ToString();
                clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(_daapp);
                clsOM_SalesRouteMaster objOM_SalesRouteMaster = new clsOM_SalesRouteMaster(_daapp);
                clsSQL objSql = new clsSQL(_daapp);
                #region insert Bulk BANG TAM
                string strOM_Dettmp = "select * from OM_SalesRouteDetTmp where 'A'='B'";
                System.Data.DataTable dtOm_SalesRouteDet = new System.Data.DataTable() { TableName = "OM_SalesRouteDetTmp" };
                dtOm_SalesRouteDet = objSql.ExcuteSQLProcText(strOM_Dettmp);
                dtOm_SalesRouteDet.TableName = "OM_SalesRouteDetTmp";

                #endregion


                try
                {


                    clsAR_Customer objCust = new clsAR_Customer(_daapp);
                    clsAR_NewCustomerInfor objNew = new clsAR_NewCustomerInfor(_daapp);
                    clsAR_SalesRouteMaster objRoute = new clsAR_SalesRouteMaster(_daapp);
                    clsOM_SalesRouteMaster objOMRoute = new clsOM_SalesRouteMaster(_daapp);
                    clsOM_SalesRouteMasterImport objImport = new clsOM_SalesRouteMasterImport(_daapp);
                    clsOM_SalesRouteDet objDet = new clsOM_SalesRouteDet(_daapp);

                    System.Data.DataTable dt = objSql.GetListNewOM_SalesRouteMaster(id);//, "%", "%", "%", "%", "%");

                    foreach (DataRow r in dt.Rows)
                    {

                        Fromdate = Convert.ToDateTime(r["StartDate"], CultureInfo.InvariantCulture);
                        Todate = Convert.ToDateTime(r["EndDate"], CultureInfo.InvariantCulture);
                        Int32 iWeekStart = Utility.WeeksInYear(Fromdate);
                        Int32 iWeekEnd = Utility.WeeksInYear(Todate);

                        clsOM_SalesRouteMaster objmaster = new clsOM_SalesRouteMaster();
                        objmaster.Reset();
                        objmaster.BranchID = r["BranchID"].ToString();
                        objmaster.CustID = r["CustID"].ToString();
                        objmaster.Fri = r["Fri"].ToString() == "1" ? true : false;
                        objmaster.Mon = r["Mon"].ToString() == "1" ? true : false;
                        objmaster.PJPID = r["PJPID"].ToString();
                        objmaster.SalesRouteID = r["SalesRouteID"].ToString();
                        objmaster.Sat = r["Sat"].ToString() == "1" ? true : false;
                        objmaster.SlsFreq = r["SlsFreq"].ToString();
                        objmaster.SlsFreqType = r["SlsFreqType"].ToString();
                        objmaster.SlsPerID = r["SlsPerID"].ToString();
                        objmaster.Sun = r["Sun"].ToString() == "1" ? true : false;
                        objmaster.Thu = r["Thu"].ToString() == "1" ? true : false;
                        objmaster.Tue = r["Tue"].ToString() == "1" ? true : false;
                        objmaster.VisitSort = r["VisitSort"].ToString() != "" ? int.Parse(r["VisitSort"].ToString()) : 0;
                        objmaster.Wed = r["Wed"].ToString() == "1" ? true : false;
                        objmaster.WeekofVisit = r["WeekofVisit"].ToString();
                        objmaster.Crtd_User = Current.UserName;
                        objmaster.Crtd_Prog = _screenNbr;
                        if (Convert.ToBoolean(r["Del"]))
                        {
                            //objOMRoute.Delete(r["PJPID"].ToString(), r["SalesRouteID"].ToString(), r["CustID"].ToString(), r["SlsPerID"].ToString(), r["BranchID"].ToString());
                            //objSql.OM_DeleteSalesRouteDetByDate(Fromdate, Todate, r["SalesRouteID"].ToString(), r["CustID"].ToString(), r["BranchID"].ToString(), r["BranchID"].ToString(), r["SlsPerID"].ToString());
                            //_daapp.CommitTrans();
                        }
                        else
                        {

                            //objSql.OM_DeleteSalesRouteDetByDate(Fromdate, Todate, r["SalesRouteID"].ToString(), r["CustID"].ToString(), r["BranchID"].ToString());
                            //_daapp.CommitTrans();

                            var lstobjdetail = CreateItemNotCommit(_daapp, objmaster, Fromdate, Todate, iWeekStart, iWeekEnd);

                            if (lstobjdetail.Count > 0)
                            {
                                foreach (var objdetail in lstobjdetail)
                                {

                                    DataRow dtRow = dtOm_SalesRouteDet.NewRow();
                                    dtRow["ID"] = ID;
                                    dtRow["BranchID"] = objdetail.BranchID;
                                    dtRow["SalesRouteID"] = objdetail.SalesRouteID;
                                    dtRow["CustID"] = objdetail.CustID;
                                    dtRow["SlsPerID"] = objdetail.SlsPerID;
                                    dtRow["PJPID"] = objdetail.PJPID;
                                    dtRow["SlsFreq"] = objdetail.SlsFreq;
                                    dtRow["SlsFreqType"] = objdetail.SlsFreqType;
                                    dtRow["WeekofVisit"] = objdetail.WeekofVisit;
                                    dtRow["VisitSort"] = objdetail.VisitSort;
                                    dtRow["Crtd_Datetime"] = objdetail.Crtd_Datetime;
                                    dtRow["Crtd_Prog"] = objdetail.Crtd_Prog;
                                    dtRow["Crtd_User"] = objdetail.Crtd_User;
                                    dtRow["LUpd_Datetime"] = objdetail.LUpd_Datetime;
                                    dtRow["LUpd_Prog"] = objdetail.Crtd_Prog;
                                    dtRow["LUpd_User"] = objdetail.Crtd_User;
                                    dtRow["VisitDate"] = objdetail.VisitDate; ;
                                    dtRow["DayofWeek"] = objdetail.DayofWeek;
                                    dtRow["WeekNbr"] = objdetail.WeekNbr;
                                    dtOm_SalesRouteDet.Rows.Add(dtRow);

                                }
                            }
                            //lognet.Info("lstobjdetail:" + lstobjdetail.Count);

                        }
                    }
                    //CreateItem(p);

                    if (dtOm_SalesRouteDet.Rows.Count > 0)
                    {
                        using (SqlConnection dbConnection = new SqlConnection(_daapp.ConnectionString))
                        {
                            dbConnection.Open();
                            using (SqlBulkCopy s1 = new SqlBulkCopy(dbConnection))
                            {

                                s1.DestinationTableName = dtOm_SalesRouteDet.TableName;
                                foreach (var column in dtOm_SalesRouteDet.Columns)
                                    s1.ColumnMappings.Add(column.ToString(), column.ToString());
                                s1.WriteToServer(dtOm_SalesRouteDet, DataRowState.Added);
                                objSql.OM22200_CoppyMCP(ID, DateTime.Now, DateTime.Now, "IMPOM22200");
                            }
                        }
                    }



                }
                catch (Exception ex)
                {

                    throw ex;
                }



                return string.Empty;


            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void CreateItemNotCommit(clsOM_SalesRouteMaster objSaleMaster, DataAccess _daapp, bool IsMonth)//(clsOM_SalesRouteMaster objSaleMaster)
        {
            DateTime Fromdate;
            DateTime Todate;
            var row = objSaleMaster;
            Int32 iWeekStart = default(Int32);
            Int32 iWeekEnd = default(Int32);
            Fromdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1 > 12 ? 11 : DateTime.Now.Month, 1);
            Todate = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1 > 12 ? 12 : DateTime.Now.Month + 1, 1);
            System.DateTime dMon = default(System.DateTime);
            System.DateTime dTue = default(System.DateTime);
            System.DateTime dWed = default(System.DateTime);
            System.DateTime dThu = default(System.DateTime);
            System.DateTime dFri = default(System.DateTime);
            System.DateTime dSat = default(System.DateTime);
            System.DateTime dSun = default(System.DateTime);
            List<clsOM_SalesRouteDet> lstOM_SalesRouteDet = new List<clsOM_SalesRouteDet>();
            clsSQL objSql = new clsSQL(_daapp);
            objSql.OM_DeleteSalesRouteDetByDate(Fromdate, Todate, objSaleMaster.SalesRouteID, objSaleMaster.CustID, objSaleMaster.BranchID, objSaleMaster.PJPID, objSaleMaster.SlsPerID);
            if (row.SlsFreqType == "R")
            {

                //Cal the visting date, week number and Day of week

                iWeekStart = Utility.WeeksInYear(Fromdate);
                iWeekEnd = Utility.WeeksInYear(Todate);
                for (Int32 i = iWeekStart; i <= iWeekEnd; i++)
                {
                    clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(_daapp);
                    objOM_SalesRouteDet.BranchID = row.BranchID;
                    objOM_SalesRouteDet.SalesRouteID = row.SalesRouteID;
                    objOM_SalesRouteDet.CustID = row.CustID;
                    objOM_SalesRouteDet.SlsPerID = row.SlsPerID;
                    objOM_SalesRouteDet.PJPID = row.PJPID;
                    objOM_SalesRouteDet.SlsFreq = row.SlsFreq;
                    objOM_SalesRouteDet.SlsFreqType = row.SlsFreqType;
                    objOM_SalesRouteDet.WeekofVisit = row.WeekofVisit;
                    objOM_SalesRouteDet.VisitSort = row.VisitSort;
                    objOM_SalesRouteDet.Crtd_Datetime = DateTime.Now;
                    objOM_SalesRouteDet.Crtd_Prog = _screenNbr;
                    objOM_SalesRouteDet.Crtd_User = Current.UserName;
                    objOM_SalesRouteDet.LUpd_Datetime = DateTime.Now;
                    objOM_SalesRouteDet.LUpd_Prog = _screenNbr;
                    objOM_SalesRouteDet.LUpd_User = Current.UserName;
                    dMon = GetDateFromDayofWeek(Fromdate.Year, i, "Monday");
                    dTue = GetDateFromDayofWeek(Fromdate.Year, i, "Tuesday");
                    dWed = GetDateFromDayofWeek(Fromdate.Year, i, "Wednesday");
                    dThu = GetDateFromDayofWeek(Fromdate.Year, i, "Thursday");
                    dFri = GetDateFromDayofWeek(Fromdate.Year, i, "Friday");
                    dSat = GetDateFromDayofWeek(Fromdate.Year, i, "Saturday");
                    dSun = GetDateFromDayofWeek(Fromdate.Year, i, "Sunday");
                    if (row.SlsFreq == "F1")
                    {
                        if ((row.WeekofVisit == "W159" && (i % 4) == 1) || (row.WeekofVisit == "W2610" && (i % 4) == 2) || (row.WeekofVisit == "W3711" && (i % 4) == 3) || (row.WeekofVisit == "W4812" && (i % 4) == 0))
                        {
                            if (row.Mon && dMon <= Todate && dMon >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dMon;
                                objOM_SalesRouteDet1.DayofWeek = "Mon";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Tue && dTue <= Todate && dTue >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dTue;
                                objOM_SalesRouteDet1.DayofWeek = "Tue";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Wed && dWed <= Todate && dWed >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dWed;
                                objOM_SalesRouteDet1.DayofWeek = "Wed";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Thu && dThu <= Todate && dThu >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dThu;
                                objOM_SalesRouteDet1.DayofWeek = "Thu";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Fri && dFri <= Todate && dFri >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dFri;
                                objOM_SalesRouteDet1.DayofWeek = "Fri";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Sat && dSat <= Todate && dSat >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSat;
                                objOM_SalesRouteDet1.DayofWeek = "Sat";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Sun && dSun <= Todate && dSun >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSun;
                                objOM_SalesRouteDet1.DayofWeek = "Sun";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                        }
                    }
                    else if (row.SlsFreq == "F2")
                    {
                        if ((row.WeekofVisit == "OW" && (i % 2) != 0) || (row.WeekofVisit == "EW" && (i % 2) == 0))
                        {
                            if (row.Mon && dMon <= Todate && dMon >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dMon;
                                objOM_SalesRouteDet1.DayofWeek = "Mon";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Tue && dTue <= Todate && dTue >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dTue;
                                objOM_SalesRouteDet1.DayofWeek = "Tue";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Wed && dWed <= Todate && dWed >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dWed;
                                objOM_SalesRouteDet1.DayofWeek = "Wed";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Thu && dThu <= Todate && dThu >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dThu;
                                objOM_SalesRouteDet1.DayofWeek = "Thu";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Fri && dFri <= Todate && dFri >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dFri;
                                objOM_SalesRouteDet1.DayofWeek = "Fri";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Sat && dSat <= Todate && dSat >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSat;
                                objOM_SalesRouteDet1.DayofWeek = "Sat";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                            if (row.Sun && dSun <= Todate && dSun >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = row.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = row.CustID;
                                objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = row.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSun;
                                objOM_SalesRouteDet1.DayofWeek = "Sun";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                objOM_SalesRouteDet1.Add();
                            }
                        }
                    }
                    else if (row.SlsFreq == "F4" || row.SlsFreq == "F8" || row.SlsFreq == "F12" || row.SlsFreq == "A")
                    {
                        if (row.Mon && dMon <= Todate && dMon >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dMon;
                            objOM_SalesRouteDet1.DayofWeek = "Mon";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                            objOM_SalesRouteDet1.Add();
                        }
                        if (row.Tue && dTue <= Todate && dTue >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dTue;
                            objOM_SalesRouteDet1.DayofWeek = "Tue";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                            objOM_SalesRouteDet1.Add();
                        }
                        if (row.Wed && dWed <= Todate && dWed >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dWed;
                            objOM_SalesRouteDet1.DayofWeek = "Wed";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            objOM_SalesRouteDet1.Add();
                        }
                        if (row.Thu && dThu <= Todate && dThu >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dThu;
                            objOM_SalesRouteDet1.DayofWeek = "Thu";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            objOM_SalesRouteDet1.Add();
                        }
                        if (row.Fri && dFri <= Todate && dFri >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dFri;
                            objOM_SalesRouteDet1.DayofWeek = "Fri";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            objOM_SalesRouteDet1.Add();
                        }
                        if (row.Sat && dSat <= Todate && dSat >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dSat;
                            objOM_SalesRouteDet1.DayofWeek = "Sat";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            objOM_SalesRouteDet1.Add();
                        }
                        if (row.Sun && dSun <= Todate && dSun >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = row.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = row.CustID;
                            objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = row.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dSun;
                            objOM_SalesRouteDet1.DayofWeek = "Sun";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            objOM_SalesRouteDet1.Add();
                        }
                    }
                    //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                }
            }
            else
            {
                iWeekStart = Utility.WeeksInYear(Fromdate);
                iWeekEnd = Utility.WeeksInYear(Todate);
                for (Int32 i = iWeekStart; i <= iWeekEnd; i++)
                {
                    clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(_daapp);
                    objOM_SalesRouteDet.BranchID = row.BranchID;
                    objOM_SalesRouteDet.SalesRouteID = row.SalesRouteID;
                    objOM_SalesRouteDet.CustID = row.CustID;
                    objOM_SalesRouteDet.SlsPerID = row.SlsPerID;
                    objOM_SalesRouteDet.PJPID = row.PJPID;
                    objOM_SalesRouteDet.SlsFreq = row.SlsFreq;
                    objOM_SalesRouteDet.SlsFreqType = row.SlsFreqType;
                    objOM_SalesRouteDet.WeekofVisit = row.WeekofVisit;
                    objOM_SalesRouteDet.VisitSort = row.VisitSort;
                    objOM_SalesRouteDet.Crtd_Datetime = DateTime.Now;
                    objOM_SalesRouteDet.Crtd_Prog = _screenNbr;
                    objOM_SalesRouteDet.Crtd_User = Current.UserName;
                    objOM_SalesRouteDet.LUpd_Datetime = DateTime.Now;
                    objOM_SalesRouteDet.LUpd_Prog = _screenNbr;
                    objOM_SalesRouteDet.LUpd_User = Current.UserName;
                    dMon = GetDateFromDayofWeek(Fromdate.Year, i, "Monday");
                    dTue = GetDateFromDayofWeek(Fromdate.Year, i, "Tuesday");
                    dWed = GetDateFromDayofWeek(Fromdate.Year, i, "Wednesday");
                    dThu = GetDateFromDayofWeek(Fromdate.Year, i, "Thursday");
                    dFri = GetDateFromDayofWeek(Fromdate.Year, i, "Friday");
                    dSat = GetDateFromDayofWeek(Fromdate.Year, i, "Saturday");
                    dSun = GetDateFromDayofWeek(Fromdate.Year, i, "Sunday");
                    if (row.Mon && dMon <= Todate && dMon >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dMon;
                        objOM_SalesRouteDet1.DayofWeek = "Mon";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                        objOM_SalesRouteDet1.Add();
                    }
                    if (row.Tue && dTue <= Todate && dTue >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dTue;
                        objOM_SalesRouteDet1.DayofWeek = "Tue";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                        objOM_SalesRouteDet1.Add();
                    }
                    if (row.Wed && dWed <= Todate && dWed >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dWed;
                        objOM_SalesRouteDet1.DayofWeek = "Wed";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        objOM_SalesRouteDet1.Add();
                    }
                    if (row.Thu && dThu <= Todate && dThu >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dThu;
                        objOM_SalesRouteDet1.DayofWeek = "Thu";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        objOM_SalesRouteDet1.Add();
                    }
                    if (row.Fri && dFri <= Todate && dFri >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dFri;
                        objOM_SalesRouteDet1.DayofWeek = "Fri";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        objOM_SalesRouteDet1.Add();
                    }
                    if (row.Sat && dSat <= Todate && dSat >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dSat;
                        objOM_SalesRouteDet1.DayofWeek = "Sat";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        objOM_SalesRouteDet1.Add();
                    }
                    if (row.Sun && dSun <= Todate && dSun >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = row.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = row.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = row.CustID;
                        objOM_SalesRouteDet1.SlsPerID = row.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = row.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = row.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = row.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = row.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = row.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dSun;
                        objOM_SalesRouteDet1.DayofWeek = "Sun";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        objOM_SalesRouteDet1.Add();
                    }
                }

            }


            //_daapp.CommitTrans();
        }
        
        public List<clsOM_SalesRouteDet> CreateItemNotCommit(DataAccess _daapp, clsOM_SalesRouteMaster objSaleMaster, DateTime Fromdate, DateTime Todate, Int32 iWeekStart, Int32 iWeekEnd)//(clsOM_SalesRouteMaster objSaleMaster)
        {
            string prog = objSaleMaster.Crtd_Prog;
            string user = objSaleMaster.Crtd_User;


            System.DateTime dMon = default(System.DateTime);
            System.DateTime dTue = default(System.DateTime);
            System.DateTime dWed = default(System.DateTime);
            System.DateTime dThu = default(System.DateTime);
            System.DateTime dFri = default(System.DateTime);
            System.DateTime dSat = default(System.DateTime);
            System.DateTime dSun = default(System.DateTime);
            List<clsOM_SalesRouteDet> lstOM_SalesRouteDet = new List<clsOM_SalesRouteDet>();

            if (objSaleMaster.SlsFreqType == "R")
            {

                //Cal the visting date, week number and Day of week


                for (Int32 i = iWeekStart; i <= iWeekEnd; i++)
                {
                    //clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(_daapp);
                    //objOM_SalesRouteDet.BranchID = objSaleMaster.BranchID;
                    //objOM_SalesRouteDet.SalesRouteID = objSaleMaster.SalesRouteID;
                    //objOM_SalesRouteDet.CustID = objSaleMaster.CustID;
                    //objOM_SalesRouteDet.SlsPerID = objSaleMaster.SlsPerID;
                    //objOM_SalesRouteDet.PJPID = objSaleMaster.PJPID;
                    //objOM_SalesRouteDet.SlsFreq = objSaleMaster.SlsFreq;
                    //objOM_SalesRouteDet.SlsFreqType = objSaleMaster.SlsFreqType;
                    //objOM_SalesRouteDet.WeekofVisit = objSaleMaster.WeekofVisit;
                    //objOM_SalesRouteDet.VisitSort = objSaleMaster.VisitSort;
                    //objOM_SalesRouteDet.Crtd_Datetime = DateTime.Now;
                    //objOM_SalesRouteDet.Crtd_Prog = _screenNbr;
                    //objOM_SalesRouteDet.Crtd_User = Current.UserName;
                    //objOM_SalesRouteDet.LUpd_Datetime = DateTime.Now;
                    //objOM_SalesRouteDet.LUpd_Prog = user;
                    //objOM_SalesRouteDet.LUpd_User = Current.UserName;
                    dMon = GetDateFromDayofWeek(Fromdate.Year, i, "Monday");
                    dTue = GetDateFromDayofWeek(Fromdate.Year, i, "Tuesday");
                    dWed = GetDateFromDayofWeek(Fromdate.Year, i, "Wednesday");
                    dThu = GetDateFromDayofWeek(Fromdate.Year, i, "Thursday");
                    dFri = GetDateFromDayofWeek(Fromdate.Year, i, "Friday");
                    dSat = GetDateFromDayofWeek(Fromdate.Year, i, "Saturday");
                    dSun = GetDateFromDayofWeek(Fromdate.Year, i, "Sunday");
                    if (objSaleMaster.SlsFreq == "F1")
                    {
                        if ((objSaleMaster.WeekofVisit == "W159" && (i % 4) == 1) || (objSaleMaster.WeekofVisit == "W2610" && (i % 4) == 2) || (objSaleMaster.WeekofVisit == "W3711" && (i % 4) == 3) || (objSaleMaster.WeekofVisit == "W4812" && (i % 4) == 0))
                        {
                            if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dMon;
                                objOM_SalesRouteDet1.DayofWeek = "Mon";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dTue;
                                objOM_SalesRouteDet1.DayofWeek = "Tue";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);


                            }
                            if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dWed;
                                objOM_SalesRouteDet1.DayofWeek = "Wed";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dThu;
                                objOM_SalesRouteDet1.DayofWeek = "Thu";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dFri;
                                objOM_SalesRouteDet1.DayofWeek = "Fri";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSat;
                                objOM_SalesRouteDet1.DayofWeek = "Sat";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                            {

                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSun;
                                objOM_SalesRouteDet1.DayofWeek = "Sun";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                        }
                    }
                    else if (objSaleMaster.SlsFreq == "F2")
                    {
                        if ((objSaleMaster.WeekofVisit == "OW" && (i % 2) != 0) || (objSaleMaster.WeekofVisit == "EW" && (i % 2) == 0))
                        {
                            if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dMon;
                                objOM_SalesRouteDet1.DayofWeek = "Mon";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dTue;
                                objOM_SalesRouteDet1.DayofWeek = "Tue";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dWed;
                                objOM_SalesRouteDet1.DayofWeek = "Wed";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dThu;
                                objOM_SalesRouteDet1.DayofWeek = "Thu";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dFri;
                                objOM_SalesRouteDet1.DayofWeek = "Fri";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSat;
                                objOM_SalesRouteDet1.DayofWeek = "Sat";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                            if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                            {
                                var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                                objOM_SalesRouteDet1.Reset();
                                objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                                objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                                objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                                objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                                objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                                objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                                objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                                objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                                objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                                objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                                objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                                objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                                objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                                objOM_SalesRouteDet1.VisitDate = dSun;
                                objOM_SalesRouteDet1.DayofWeek = "Sun";
                                objOM_SalesRouteDet1.WeekNbr = i;
                                //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                                lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            }
                        }
                    }
                    else if (objSaleMaster.SlsFreq == "F4" || objSaleMaster.SlsFreq == "F8" || objSaleMaster.SlsFreq == "F12" || objSaleMaster.SlsFreq == "A")
                    {

                        if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dMon;
                            objOM_SalesRouteDet1.DayofWeek = "Mon";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dTue;
                            objOM_SalesRouteDet1.DayofWeek = "Tue";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dWed;
                            objOM_SalesRouteDet1.DayofWeek = "Wed";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dThu;
                            objOM_SalesRouteDet1.DayofWeek = "Thu";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dFri;
                            objOM_SalesRouteDet1.DayofWeek = "Fri";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dSat;
                            objOM_SalesRouteDet1.DayofWeek = "Sat";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                        if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                        {
                            var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                            objOM_SalesRouteDet1.Reset();
                            objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                            objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                            objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                            objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                            objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                            objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                            objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                            objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                            objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                            objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                            objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                            objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                            objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                            objOM_SalesRouteDet1.VisitDate = dSun;
                            objOM_SalesRouteDet1.DayofWeek = "Sun";
                            objOM_SalesRouteDet1.WeekNbr = i;
                            //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                            lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        }
                    }
                    //lstOM_SalesRouteDet = lstOM_SalesRouteDet.ToList();
                }
            }
            else
            {

                for (Int32 i = iWeekStart; i <= iWeekEnd; i++)
                {
                    clsOM_SalesRouteDet objOM_SalesRouteDet = new clsOM_SalesRouteDet(_daapp);
                    objOM_SalesRouteDet.BranchID = objSaleMaster.BranchID;
                    objOM_SalesRouteDet.SalesRouteID = objSaleMaster.SalesRouteID;
                    objOM_SalesRouteDet.CustID = objSaleMaster.CustID;
                    objOM_SalesRouteDet.SlsPerID = objSaleMaster.SlsPerID;
                    objOM_SalesRouteDet.PJPID = objSaleMaster.PJPID;
                    objOM_SalesRouteDet.SlsFreq = objSaleMaster.SlsFreq;
                    objOM_SalesRouteDet.SlsFreqType = objSaleMaster.SlsFreqType;
                    objOM_SalesRouteDet.WeekofVisit = objSaleMaster.WeekofVisit;
                    objOM_SalesRouteDet.VisitSort = objSaleMaster.VisitSort;
                    objOM_SalesRouteDet.Crtd_Datetime = DateTime.Now;
                    objOM_SalesRouteDet.Crtd_Prog = _screenNbr;
                    objOM_SalesRouteDet.Crtd_User = Current.UserName;
                    objOM_SalesRouteDet.LUpd_Datetime = DateTime.Now;
                    objOM_SalesRouteDet.LUpd_Prog = user;
                    objOM_SalesRouteDet.LUpd_User = Current.UserName;
                    dMon = GetDateFromDayofWeek(Fromdate.Year, i, "Monday");
                    dTue = GetDateFromDayofWeek(Fromdate.Year, i, "Tuesday");
                    dWed = GetDateFromDayofWeek(Fromdate.Year, i, "Wednesday");
                    dThu = GetDateFromDayofWeek(Fromdate.Year, i, "Thursday");
                    dFri = GetDateFromDayofWeek(Fromdate.Year, i, "Friday");
                    dSat = GetDateFromDayofWeek(Fromdate.Year, i, "Saturday");
                    dSun = GetDateFromDayofWeek(Fromdate.Year, i, "Sunday");
                    if (objSaleMaster.Mon && dMon <= Todate && dMon >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dMon;
                        objOM_SalesRouteDet1.DayofWeek = "Mon";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                    if (objSaleMaster.Tue && dTue <= Todate && dTue >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dTue;
                        objOM_SalesRouteDet1.DayofWeek = "Tue";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);

                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                    if (objSaleMaster.Wed && dWed <= Todate && dWed >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dWed;
                        objOM_SalesRouteDet1.DayofWeek = "Wed";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                    if (objSaleMaster.Thu && dThu <= Todate && dThu >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dThu;
                        objOM_SalesRouteDet1.DayofWeek = "Thu";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                    if (objSaleMaster.Fri && dFri <= Todate && dFri >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dFri;
                        objOM_SalesRouteDet1.DayofWeek = "Fri";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                    if (objSaleMaster.Sat && dSat <= Todate && dSat >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dSat;
                        objOM_SalesRouteDet1.DayofWeek = "Sat";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                    if (objSaleMaster.Sun && dSun <= Todate && dSun >= Fromdate)
                    {
                        var objOM_SalesRouteDet1 = new clsOM_SalesRouteDet(_daapp);
                        objOM_SalesRouteDet1.Reset();
                        objOM_SalesRouteDet1.BranchID = objSaleMaster.BranchID;
                        objOM_SalesRouteDet1.SalesRouteID = objSaleMaster.SalesRouteID;
                        objOM_SalesRouteDet1.CustID = objSaleMaster.CustID;
                        objOM_SalesRouteDet1.SlsPerID = objSaleMaster.SlsPerID;
                        objOM_SalesRouteDet1.PJPID = objSaleMaster.PJPID;
                        objOM_SalesRouteDet1.SlsFreq = objSaleMaster.SlsFreq;
                        objOM_SalesRouteDet1.SlsFreqType = objSaleMaster.SlsFreqType;
                        objOM_SalesRouteDet1.WeekofVisit = objSaleMaster.WeekofVisit;
                        objOM_SalesRouteDet1.VisitSort = objSaleMaster.VisitSort;
                        objOM_SalesRouteDet1.Crtd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.Crtd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.Crtd_User = Current.UserName;
                        objOM_SalesRouteDet1.LUpd_Datetime = DateTime.Now;
                        objOM_SalesRouteDet1.LUpd_Prog = _screenNbr;
                        objOM_SalesRouteDet1.LUpd_User = Current.UserName;

                        objOM_SalesRouteDet1.VisitDate = dSun;
                        objOM_SalesRouteDet1.DayofWeek = "Sun";
                        objOM_SalesRouteDet1.WeekNbr = i;
                        //lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                        lstOM_SalesRouteDet.Add(objOM_SalesRouteDet1);
                    }
                }

            }
            //lognet.Info("lstOM_SalesRouteDet COUNT:" + lstOM_SalesRouteDet.Count);
            return lstOM_SalesRouteDet;
        }

        private bool isValidSelOMSalesRouteMaster(OM_SalesRouteMasterImport OM, bool Message)
        {
            int iVisit = 0;
            try
            {
                if (OM.Mon == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Sat == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Sun == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Fri == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Thu == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Tue == true)
                {
                    iVisit = iVisit + 1;
                }
                if (OM.Wed == true)
                {
                    iVisit = iVisit + 1;
                }

                switch (OM.SlsFreq)
                {
                    case "F1":
                    case "F2":
                    case "F4":
                        if (iVisit != 1)
                        {
                            //if (_complete)
                            //    HQMessageBox.Show(53, hqSys.LangID, null, HQmesscomplete);
                            //_complete = false;
                            return false;
                        }
                        break;
                    case "F8":
                        if (iVisit != 2)
                        {

                            return false;
                        }
                        break;
                    case "F12":
                        if (iVisit != 3)
                        {

                            return false;
                        }
                        break;
                    case "A":
                        if (iVisit == 0)
                        {

                            return false;
                        }
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {

                throw ex;
                return false;
            }
        }
        
        public DateTime GetDateFromDayofWeek(int year, int week, string strDayofWeek)
        {
            System.DateTime firstOfYear = new System.DateTime(year, 1, 1);
            DayOfWeek dayOfweek = default(DayOfWeek);
            try
            {
                switch (strDayofWeek)
                {
                    case "Monday":
                        dayOfweek = DayOfWeek.Monday;
                        break;
                    case "Tuesday":
                        dayOfweek = DayOfWeek.Tuesday;
                        break;
                    case "Wednesday":
                        dayOfweek = DayOfWeek.Wednesday;
                        break;
                    case "Thursday":
                        dayOfweek = DayOfWeek.Thursday;
                        break;
                    case "Friday":
                        dayOfweek = DayOfWeek.Friday;
                        break;
                    case "Saturday":
                        dayOfweek = DayOfWeek.Saturday;
                        break;
                    case "Sunday":
                        dayOfweek = DayOfWeek.Sunday;
                        break;
                }
                return firstOfYear.AddDays((week - 1) * 7 + dayOfweek - firstOfYear.DayOfWeek);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private string Getcell(int column)
        {
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 > 1)
            {
                cell += ABC.Substring(column / 26, 1);
                column += column - 26;
            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            return cell;
        }

        private void updateSlsRoute(ref OM_SalesRouteMaster obj, OM22200_pgSaleRouteMaster_Result slsRoute, bool isNew)
        {
            if (isNew)
            {
                obj = new OM_SalesRouteMaster();

                obj.CustID = slsRoute.CustID;
                obj.BranchID = slsRoute.BranchID;
                obj.PJPID = slsRoute.PJPID;
                obj.SalesRouteID = slsRoute.SalesRouteID;
                obj.SlsPerID = slsRoute.SlsPerID;

                obj.Crtd_DateTime = DateTime.Now;
                obj.Crtd_Prog = _screenNbr;
                obj.Crtd_User = Current.UserName;
            }
            obj.SlsFreq = slsRoute.SlsFreq;
            obj.SlsFreqType = "R";
            obj.WeekofVisit = slsRoute.WeekofVisit;
            obj.Sun = slsRoute.Sun;
            obj.Mon = slsRoute.Mon;
            obj.Tue = slsRoute.Tue;
            obj.Wed = slsRoute.Wed;
            obj.Thu = slsRoute.Thu;
            obj.Fri = slsRoute.Fri;
            obj.Sat = slsRoute.Sat;
            obj.VisitSort = slsRoute.VisitSort;

            obj.LUpd_DateTime = DateTime.Now;
            obj.LUpd_Prog = _screenNbr;
            obj.LUpd_User = Current.UserName;
        }

        private void updatePJP(ref OM_PJP updated, OM_PJP input, bool isNew)
        {
            if (isNew)
            {
                updated = new OM_PJP();
                updated.PJPID = input.PJPID;
                updated.BranchID = input.BranchID;
                updated.Status = false;
                updated.StatusHandle = _beginStatus;

                updated.Crtd_DateTime = DateTime.Now;
                updated.Crtd_Prog = _screenNbr;
                updated.Crtd_User = Current.UserName;
            }
            updated.Descr = input.Descr;

            updated.LUpd_DateTime = DateTime.Now;
            updated.LUpd_Prog = _screenNbr;
            updated.LUpd_User = Current.UserName;
        }

        private string getStrBetweenTags(string value,
                                       string startTag,
                                       string endTag)
        {
            if (value.Contains(startTag) && value.Contains(endTag))
            {
                int index = value.IndexOf(startTag) + startTag.Length;
                return value.Substring(index, value.IndexOf(endTag) - index);
            }
            else
                return string.Empty;
        }

        #endregion
    }
}

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
using Aspose.Cells;
using HQFramework.DAL;
using HQFramework.Common;
using System.Drawing;
using HQ.eSkySys;
using Ionic.Zip;

namespace OM23000.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class OM23000Controller : Controller
    {
        private string _screenNbr = "OM23000";
        private string _userName = Current.UserName;
        OM23000Entities _db = Util.CreateObjectContext<OM23000Entities>(false);
        eSkySysEntities _sys = Util.CreateObjectContext<eSkySysEntities>(true);
        private JsonResult _logMessage;

        private string _filePath;
        internal string FilePath
        {
            get
            {
                var config = _sys.SYS_Configurations.FirstOrDefault(x => x.Code == "UploadOM23000");
                if (config != null && !string.IsNullOrWhiteSpace(config.TextVal))
                {
                    _filePath = config.TextVal;
                    //_filePath = Server.MapPath("~\\Images\\OM23000");
                }
                else
                {
                    _filePath = Server.MapPath("~\\Images\\OM23000");
                }
                if (!Directory.Exists(_filePath))
                {
                    throw new MessageException(MessageType.Message, "2016111510", "", null, "", _filePath);
                    //Directory.CreateDirectory(_filePath);
                }
                return _filePath;
            }
        }

        public ActionResult Index()
        {
            if (!Directory.Exists(@"\\192.168.130.4\DevProjects\eSky4D"))
            {
                throw new MessageException(MessageType.Message, "2016111510", "", null, "", "");
                //Directory.CreateDirectory(_filePath);
            }
            Util.InitRight(_screenNbr);
            return View();
        }

        //[OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetOM_Advertise(string CpnyID, string UserID)
        {
            return this.Store(_db.OM23000_pgOM_Advertise(CpnyID, UserID).ToList());
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                StoreDataHandler dataHandler = new StoreDataHandler(data["lstOM_Advertise"]);
                var lstOM_Advertise = dataHandler.ObjectData<OM23000_pgOM_Advertise_Result>() == null ? new List<OM23000_pgOM_Advertise_Result>() : dataHandler.ObjectData<OM23000_pgOM_Advertise_Result>().Where(p => p.ClassID != "" && p.AdverID != "").ToList();


                var lstOld_OM_Advertise = _db.OM_Advertise.ToList();

                foreach (var objold in lstOld_OM_Advertise)
                {
                    if (lstOM_Advertise.Where(p => p.ClassID == objold.ClassID
                                                && p.AdverID == objold.AdverID).FirstOrDefault() == null)
                    {
                        if (objold.Video != "")
                        {
                            var oldPath = string.Format("{0}\\{1}", FilePath, objold.Video);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                                DeleteFileInZip(objold.Video, FilePath);
                            }
                        }
                        if (objold.Profile != "")
                        {
                            var oldPath = string.Format("{0}\\{1}", FilePath, objold.Profile);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                                DeleteFileInZip(objold.Profile, FilePath);
                            }
                        }
                        _db.OM_Advertise.DeleteObject(objold);
                    }
                }

                foreach (var item in lstOM_Advertise)
                {
                    if (item.ClassID.PassNull() == "" || item.AdverID.PassNull() == "") continue;

                    var lang = _db.OM_Advertise.Where(p => p.ClassID == item.ClassID && p.AdverID == item.AdverID).FirstOrDefault();

                    if (lang != null)
                    {
                        if (lang.tstamp.ToHex() == item.tstamp.ToHex())
                        {
                            Update_Language(lang, item, false);
                        }
                        else
                        {
                            throw new MessageException(MessageType.Message, "19");
                        }
                    }
                    else
                    {
                        lang = new OM_Advertise();
                        lang.ResetET();
                        Update_Language(lang, item, true);
                        _db.OM_Advertise.AddObject(lang);
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

        private void Update_Language(OM_Advertise t, OM23000_pgOM_Advertise_Result s, bool isNew)
        {
            if (isNew)
            {
                t.ClassID = s.ClassID;
                t.AdverID = s.AdverID;

                t.Crtd_DateTime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.Descr = s.Descr;
            t.Video = s.Video;
            t.Profile = s.Profile;
            t.Status = s.Status;

            t.LUpd_DateTime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        public ActionResult UploadImage(string fileOldName, string fileName, string ClassID, string AdverID, string flagPosition)
        {
            try
            {
                string tstamp = "";
                string newFileName = "";
                var files = Request.Files;
                if (files.Count > 0)
                {
                    // Xoa file cu di
                    var oldPath = string.Format("{0}\\{1}", FilePath, fileOldName);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                        DeleteFileInZip(fileOldName, FilePath);
                    }
                    if (!System.IO.Directory.Exists(FilePath))
                    {
                        throw new MessageException(MessageType.Message, "2016111510", "", null, "", FilePath);
                        //Directory.CreateDirectory(FilePath);
                    }
                    // Upload file moi
                    if(flagPosition == "1")
                        newFileName = string.Format("{0}{1}", "ADV" + "_" + ClassID + "_" + AdverID, Path.GetExtension(files[0].FileName));
                    else if (flagPosition == "2")
                        newFileName = string.Format("{0}{1}", "ADI" + "_" + ClassID + "_" + AdverID, Path.GetExtension(files[0].FileName));
                    
                    files[0].SaveAs(string.Format("{0}\\{1}", FilePath, newFileName));
                    ZipFiles(FilePath, newFileName);
                    var objAdv = _db.OM_Advertise.Where(p => p.ClassID == ClassID && p.AdverID == AdverID).FirstOrDefault();

                    
                    if (objAdv != null)
                    {
                        if (flagPosition == "1")
                            objAdv.Video = newFileName;
                        else if (flagPosition == "2")
                            objAdv.Profile = newFileName;
                        objAdv.LUpd_DateTime = DateTime.Now;
                        objAdv.LUpd_Prog = _screenNbr;
                        objAdv.LUpd_User = _userName;
                        _db.SaveChanges();

                        tstamp = Convert.ToBase64String(objAdv.tstamp);
                    }

                }
                return Json(new { success = true, Exist = newFileName, newTstamp = tstamp }, JsonRequestBehavior.AllowGet);

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

        public void DeleteFileInZip(string fileName, string path)
        {
            try
            {
                string zipFolder = path;
                path += "\\";

                if (System.IO.File.Exists(zipFolder + "/ABC.zip"))
                {
                    using (ZipFile zip = ZipFile.Read(zipFolder + "/ABC.zip"))
                    {
                        string filePath = path + fileName;
                        Delete(filePath);
                        string zipFile = fileName;
                        if (zip.ContainsEntry(zipFile))
                        {
                            zip.RemoveEntry(zipFile);
                            zip.Save();
                        }
                    }
                }
                else
                {
                    string filePath = path + fileName;
                    Delete(filePath);
                }
            }
            catch
            {
            }
        }

        private void Delete(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        public void ZipFiles(string serverPath, string fileName)
        {
            try
            {
                string[] fileNames = fileName.Split(';');
                for (int i = 0; i < fileNames.Count(); i++)
                {
                    fileNames[i] = serverPath + "\\" + fileNames[i];
                }
                string zipFilePath = serverPath + "\\ABC.zip";
                FileInfo file = new FileInfo(zipFilePath);
                if (file.Exists)
                {
                    using (ZipFile zip = ZipFile.Read(zipFilePath))
                    {
                        zip.AddFiles(fileNames, "");
                        zip.Save();
                    }
                }
                else
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.UseUnicodeAsNecessary = true;  // utf-8
                        zip.AddFiles(fileNames, "");
                        zip.Save(zipFilePath);
                    }
                }
            }
            catch
            { }
        }

    }
}

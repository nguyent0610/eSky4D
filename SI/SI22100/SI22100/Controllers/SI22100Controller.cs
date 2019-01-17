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
using HQ.eSkyFramework.HQControl;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SI22100.Controllers
{
    [DirectController]
    [CustomAuthorize]
    [CheckSessionOut]
    public class SI22100Controller : Controller
    {
        private string _screenNbr = "SI22100";
        private readonly string _userName = Current.UserName;
        SI22100Entities _db = Util.CreateObjectContext<SI22100Entities>(false);
        private JsonResult _logMessage;

        public ActionResult Index()
        {
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Util.InitRight(_screenNbr);
            return View();
        }

    //    [OutputCache(Duration = 1000000, VaryByParam = "lang")]
        public PartialViewResult Body(string lang)
        {
            return PartialView();
        }

        public ActionResult GetQuestionList(string questListID)
        {
            var item = _db.SI_QuestList.FirstOrDefault(p => p.QuestListID == questListID);
            return this.Store(item);
        }

        public ActionResult GetQuestion(string questListID)
        {
            var list = _db.SI22100_pgQuestion(questListID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(list);
        }

        public ActionResult GetAnswer(string questListID)
        {
            var list = _db.SI22100_pgAnswer(questListID, Current.CpnyID, Current.UserName, Current.LangID).ToList();
            return this.Store(list);
        }
        public ActionResult GetQuestionID(string QuestListID, DateTime ? FromDate, DateTime ? ToDate)
        {
            var LoadGrid = _db.SI22100_pgLoadGrid(Current.UserName, Current.CpnyID, Current.LangID, QuestListID, FromDate, ToDate).ToList();
            return this.Store(LoadGrid);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                string questListID = data["cboQuestListID"];
                string handle = data["cboHandle"];
                string questListDescr = data["txtDescr"];
                string type = data["cboType"];
                bool isNew = false;

                StoreDataHandler questionList = new StoreDataHandler(data["questionList"]);
                var inputHeader = questionList.ObjectData<SI_QuestList>().FirstOrDefault();

                #region Header

                var objQuestList = _db.SI_QuestList.FirstOrDefault(p => p.QuestListID.ToUpper() == questListID.ToUpper());
                if (objQuestList == null)
                {
                    isNew = true;
                    objQuestList = new SI_QuestList();
                    objQuestList.ResetET();
                    objQuestList.QuestListID = questListID;

                    objQuestList.Crtd_Datetime = DateTime.Now;
                    objQuestList.Crtd_Prog = _screenNbr;
                    objQuestList.Crtd_User = _userName;
                }

                if (isNew == false)
                {
                    if (inputHeader != null && objQuestList.tstamp.ToHex() != inputHeader.tstamp.ToHex())
                    {
                        throw new MessageException(MessageType.Message, "19");
                    }
                }

                objQuestList.QuestTListDescr = questListDescr;
                objQuestList.Type = type;
                if (string.IsNullOrEmpty(handle) && string.IsNullOrEmpty(objQuestList.Status))
                {
                    objQuestList.Status = "H";
                }
                else if (!string.IsNullOrEmpty(handle))
                {
                        objQuestList.Status = handle.ToUpper();
                }
                objQuestList.LUpd_Datetime = DateTime.Now;
                objQuestList.LUpd_Prog = _screenNbr;
                objQuestList.LUpd_User = _userName;

                if (isNew)
                {
                    _db.SI_QuestList.AddObject(objQuestList);
                }



                #endregion

                StoreDataHandler dataQuestion = new StoreDataHandler(data["listQuestion"]);
                ChangeRecords<SI22100_pgQuestion_Result> listQuestion = dataQuestion.BatchObjectData<SI22100_pgQuestion_Result>();

                #region Question

                foreach (var deleted in listQuestion.Deleted)
                {
                    var del =
                        _db.SI_Question.FirstOrDefault(
                            p =>
                                p.QuestListID.ToUpper() == deleted.QuestListID.ToUpper() &&
                                p.QuestID == deleted.QuestID);
                    if (del != null)
                    {
                        _db.SI_Question.DeleteObject(del);
                    }
                }

                listQuestion.Created.AddRange(listQuestion.Updated);
                foreach (var item in listQuestion.Created)
                {
                    if (item.QuestID.PassNull() == "") continue;
                    //if (item.Type == "M" || curLang.Type == "O")
                    //{
                    //    if (lstAnswer.Where(p => p.LineRef.PassNull() != "" && p.Number == item.Number).Count() == 0)
                    //    {
                    //        throw new MessageException(MessageType.Message, "2018080951", "", new string[] { });
                    //    }
                    //}
                    var lang = _db.SI_Question.FirstOrDefault( p => p.QuestListID.ToUpper() == questListID.ToUpper() && p.QuestID == item.QuestID);

                    if (lang != null)
                    {
                        UpdateQuestion(lang, item, questListID, false);
                    }
                    else
                    {
                        lang = new SI_Question();
                        lang.ResetET();
                        UpdateQuestion(lang, item, questListID, true);
                        _db.SI_Question.AddObject(lang);
                    }

                }

                #endregion

                #region Detail

                StoreDataHandler dataAnswer = new StoreDataHandler(data["listAnswer"]);
                List<SI22100_pgAnswer_Result> listAnswer = dataAnswer.ObjectData<SI22100_pgAnswer_Result>();

                StoreDataHandler dataAnswerDel = new StoreDataHandler(data["lstAnswerDel"]);
                List<SI22100_pgAnswer_Result> listAnswerDel = dataAnswerDel.ObjectData<SI22100_pgAnswer_Result>();


                foreach (var item in listAnswerDel)
                {
                    var del =
                        _db.SI_Answer.FirstOrDefault(
                            p =>
                                p.QuestListID == item.QuestListID && p.QuestID == item.QuestID &&
                                p.AnswerID == item.AnswerID);
                    if (del != null)
                    {
                        _db.SI_Answer.DeleteObject(del);
                    }
                }

                foreach (var item in listAnswer)
                {
                    if (string.IsNullOrEmpty(item.AnswerID) || string.IsNullOrEmpty(item.QuestListID) || string.IsNullOrEmpty(item.QuestID)) continue;

                    var lang = _db.SI_Answer.FirstOrDefault( p => p.QuestListID == item.QuestListID && p.QuestID == item.QuestID && p.AnswerID == item.AnswerID);

                    if (lang != null)
                    {
                        UpdateAnswer(lang, item, questListID, false);
                    }
                    else
                    {
                        lang = new SI_Answer();
                        lang.ResetET();
                        UpdateAnswer(lang, item, questListID, true);
                        _db.SI_Answer.AddObject(lang);
                    }
                }

                #endregion

                _db.SaveChanges();
                return Json(new {success = true, QuestionList = questListID});
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new {success = false, type = "error", errorMsg = ex.ToString()});
            }

        }

        private void UpdateQuestion(SI_Question t, SI22100_pgQuestion_Result s, string questListID, bool isNew)
        {
            if (isNew)
            {
                t.QuestListID = questListID;
                t.QuestID = s.QuestID;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.QuestDescr = s.QuestDescr;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }

        private void UpdateAnswer(SI_Answer t, SI22100_pgAnswer_Result s, string questListID, bool isNew)
        {
            if (isNew)
            {
                t.QuestListID = questListID;
                t.QuestID = s.QuestID;
                t.AnswerID = s.AnswerID;
                t.Crtd_Datetime = DateTime.Now;
                t.Crtd_Prog = _screenNbr;
                t.Crtd_User = _userName;
            }
            t.AnswerDescr = s.AnswerDescr;
            t.Correct = s.Correct;
            t.LUpd_Datetime = DateTime.Now;
            t.LUpd_Prog = _screenNbr;
            t.LUpd_User = _userName;
        }
        [HttpPost]
        public ActionResult Delete(FormCollection data,string questListID)
        {
            try
            {
                var objQuestList = _db.SI_QuestList.FirstOrDefault(p => p.QuestListID.ToUpper() == questListID.ToUpper());
                if (objQuestList != null)
                {
                    _db.SI_QuestList.DeleteObject(objQuestList);
                }
                var listQuestion = _db.SI_Question.Where(p => p.QuestListID.ToUpper() == questListID.ToUpper()).ToList();
                foreach (var item in listQuestion)
                {
                    if (item != null)
                    {
                        _db.SI_Question.DeleteObject(item);
                    }
                }

                var listAnswer = _db.SI_Answer.Where( p => p.QuestListID.ToUpper() == questListID.ToUpper()).ToList();
                foreach (var itemAnswer in listAnswer)
                {
                    if (itemAnswer != null)
                    {
                        _db.SI_Answer.DeleteObject(itemAnswer);
                    }
                }

                _db.SaveChanges();
                return Json(new {success = true});
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new {success = false, type = "error", errorMsg = ex.ToString()});
            }
        }

        [HttpPost]

        #region [IMPORT]
        public ActionResult Import()
        {
            try
            {
                var regexItem = new Regex("^[a-zA-Z0-9-/_]*$");
                Dictionary<string, List<string>> dicQuestion = new Dictionary<string, List<string>>();
                Dictionary<string, string> dicQuestionListType = new Dictionary<string, string>();
                Dictionary<string, int> dicQuestionList = new Dictionary<string, int>();
                var colTexts = HeaderExcel();
                FileUploadField fileUploadField = X.GetCmp<FileUploadField>("btnImport");
                HttpPostedFile file = fileUploadField.PostedFile;
                FileInfo fileInfo = new FileInfo(file.FileName);
                var dataRowIdx = 2;
                if (fileInfo.Extension.ToLower() == ".xls" || fileInfo.Extension.ToLower() == ".xlsx")
                {
                    Workbook workbook = new Workbook(fileUploadField.PostedFile.InputStream);
                    Worksheet workSheet = workbook.Worksheets[0];
                    if (workSheet.Cells.MaxDataRow < 1)
                    {
                        throw new MessageException(MessageType.Message, "2018081251");
                    }
                    List<string> lstQuestListID = new List<string>();

                    List<CheckTypeImport> lstCheckTypeImport = new List<CheckTypeImport>();

                    //List<string> lstQuestID = new List<string>();
                    string message = string.Empty;
                    string errorQuestListIDNull = string.Empty;
                    string errorQuestListID = string.Empty;
                    string errorTypeNull = string.Empty;
                    string errorType = string.Empty;
                    string errorCheckType = string.Empty;
                    string errorQuestListDescrNull = string.Empty;
                    string errorQuestIDNull = string.Empty;

                    string errorQuestID = string.Empty;

                    string errorAnswer = string.Empty;
                    string errorAnswer1 = string.Empty;
                    string errorAnswerCorrect = string.Empty; // không có đáp án đúng
                    string errorAnswerCorrect1 = string.Empty;  // nhiều hơn 1 đáp án đúng

                    string errorStatus = string.Empty;
                    string errorChangeType = string.Empty;

                    string errorQuestDescrNull = string.Empty;
                    string errorAnswerDescrNull = string.Empty;
                    string errorCorrect = string.Empty;
                    string errorDuplicateID = string.Empty;

                    bool flagCheck = true;
                    int index = 1;
                    for (int i = dataRowIdx; i <= workSheet.Cells.MaxDataRow; i++)
                    {
                        #region Get data from excel

                        string questListID = workSheet.Cells[i, colTexts.IndexOf("SI22100QuestionGr")].StringValue.Trim();
                        string type = workSheet.Cells[i, colTexts.IndexOf("SI22100Type")].StringValue.Trim();
                        string questListDescr = workSheet.Cells[i, colTexts.IndexOf("QuestListDescr")].StringValue.Trim();
                        string questID = workSheet.Cells[i, colTexts.IndexOf("SI22100QuestID")].StringValue.Trim();
                        double id = 0;
                        try
                        {
                            if (!string.IsNullOrEmpty(questID))
                            {
                                id = questID.ToDouble();
                            }
                        }
                        catch
                        {
                            errorQuestID += (i + 1) + ", ";
                            flagCheck = false;
                        }

                        string quesDescr = workSheet.Cells[i, colTexts.IndexOf("QuestionDescr")].StringValue.Trim();
                        string answerDescr = workSheet.Cells[i, colTexts.IndexOf("AnswerDescr")].StringValue.Trim();
                        string correct = workSheet.Cells[i, colTexts.IndexOf("Correct")].StringValue.Trim();
                        if (string.IsNullOrEmpty(questListID))
                        {
                            errorQuestListIDNull += (i + 1) + ", ";
                            flagCheck = false;
                        }

                        if (regexItem.IsMatch(questListID) == false)
                        {
                            errorQuestListID += (i + 1) + ", ";
                            flagCheck = false;
                        }

                        if (string.IsNullOrEmpty(type))
                        {
                            errorTypeNull += (i + 1) + ", ";
                            flagCheck = false;
                        }
                        else
                        {

                            if (!dicQuestionListType.ContainsKey(questListID))
                            {
                                dicQuestionListType.Add(questListID, type);
                            }
                            else
                            {
                                if (dicQuestionListType[questListID] != type)
                                {
                                    errorCheckType += (i + 1) + ", ";
                                    flagCheck = false;
                                }
                            }

                        }

                        if (!string.IsNullOrEmpty(type) && (GetCodeFromExcel(type) != "Y" && GetCodeFromExcel(type) != "T" && GetCodeFromExcel(type) != "M" && GetCodeFromExcel(type) != "O"))
                        {
                            errorType += (i + 1) + ", ";
                            flagCheck = false;
                        }

                        if (string.IsNullOrEmpty(questListDescr))
                        {
                            errorQuestListDescrNull += (i + 1) + ", ";
                            flagCheck = false;
                        }
                        else
                        {
                            if (questListDescr.Length > 500)
                            {
                                questListDescr = questListDescr.Substring(0, 500);
                            }
                        }

                        //if (string.IsNullOrEmpty(questID))
                        //{
                        //    errorQuestIDNull += (i + 1) + ", ";
                        //    flagCheck = false;
                        //}

                        if (string.IsNullOrEmpty(quesDescr))
                        {
                            errorQuestDescrNull += (i + 1) + ", ";
                            flagCheck = false;
                        }
                        else
                        {
                            if (quesDescr.Length > 500)
                            {
                                quesDescr = quesDescr.Substring(0, 500);
                            }
                        }

                        if (string.IsNullOrEmpty(answerDescr) && GetCodeFromExcel(type) != "T")
                        {
                            errorAnswerDescrNull += (i + 1) + ", ";
                            flagCheck = false;
                        }
                        else
                        {
                            if (answerDescr.Length > 500)
                            {
                                answerDescr = answerDescr.Substring(0, 500);
                            }
                        }

                        if (!string.IsNullOrEmpty(correct) && correct.ToUpper() != "X")
                        {
                            errorCorrect += (i + 1) + ", ";
                            flagCheck = false;
                        }
                        var objQuestListcheck = _db.SI_QuestList.FirstOrDefault(x => x.QuestListID.ToUpper() == questListID.ToUpper());
                        if (objQuestListcheck != null)
                        {
                            errorDuplicateID += (i + 1) + ", ";
                            flagCheck = false;
                        }

                        if (!string.IsNullOrEmpty(questListID) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(quesDescr))
                        {
                            var key = questListID + "," + GetCodeFromExcel(type) + "," + quesDescr;
                            var objCheck = lstCheckTypeImport.FirstOrDefault(x => x.Key == key);
                            if (objCheck == null)
                            {
                                var newObj = new CheckTypeImport();
                                newObj.Key = key;
                                newObj.Qty = 1;
                                newObj.Type = GetCodeFromExcel(type);
                                newObj.QtyCorrect = 0;
                                if (correct == "X")
                                {
                                    newObj.QtyCorrect++;
                                }
                                lstCheckTypeImport.Add(newObj);
                            }
                            else
                            {
                                objCheck.Qty++;
                                if (correct == "X")
                                {
                                    objCheck.QtyCorrect++;
                                }
                            }

                        }
                        

                        #endregion
                        if (flagCheck)
                        {
                            if (!string.IsNullOrEmpty(questListID))
                            {
                                string tempQuest = questListID + "," + quesDescr;



                                if (lstQuestListID.All(x => x != questListID)) // Lưu SI_QuestList
                                {
                                    index = 1; //set lại i = 1 khi chuyển mã bộ câu hỏi mới
                                    lstQuestListID.Add(questListID);
                                    var objQuestList = _db.SI_QuestList.FirstOrDefault(x => x.QuestListID.ToUpper() == questListID.ToUpper());

                                    if (objQuestList == null)
                                    {
                                        objQuestList = new SI_QuestList
                                        {
                                            QuestListID = questListID,
                                            Type = GetCodeFromExcel(type),
                                            Status = "H",
                                            QuestTListDescr = questListDescr,
                                            Crtd_Datetime = DateTime.Now,
                                            Crtd_User = Current.UserName,
                                            Crtd_Prog = _screenNbr,
                                            LUpd_User = Current.UserName,
                                            LUpd_Datetime = DateTime.Now,
                                            LUpd_Prog = _screenNbr,
                                            tstamp = new byte[1]
                                        };

                                        _db.SI_QuestList.AddObject(objQuestList);
                                    }
                                    else
                                    {
                                        if (objQuestList.Status != "H")
                                        {
                                            errorStatus = questListID;
                                            break;

                                        }
                                        else if (objQuestList.Type != GetCodeFromExcel(type))
                                        {
                                            errorChangeType = questListID;
                                            break;
                                        }


                                        // objQuestList.Type = type;
                                        objQuestList.QuestTListDescr = questListDescr;

                                        objQuestList.LUpd_User = Current.UserName;
                                        objQuestList.LUpd_Datetime = DateTime.Now;
                                        objQuestList.LUpd_Prog = _screenNbr;
                                        objQuestList.tstamp = new byte[1];
                                    }


                                }

                                if (!dicQuestion.ContainsKey(tempQuest))
                                {
                                    dicQuestion.Add(tempQuest, new List<string>());

                                    var objQuest = _db.SI_Question.FirstOrDefault(x => x.QuestListID == questListID && x.QuestID == questID);
                                    if (objQuest == null)
                                    {
                                        if (!dicQuestionList.ContainsKey(tempQuest)) // Stt tự tăng cho ID QUESTION
                                        {
                                            dicQuestionList.Add(tempQuest, index);
                                            index += 1;
                                        }
                                        var idQuest = _db.SI_Question.Where(x => x.QuestListID == questListID).Max(x => x.QuestID).ToInt();
                                        var dicQuest = dicQuestionList[tempQuest] + idQuest;

                                        questID = dicQuest.ToString();
                                        //idQuestAnswer = questID;
                                        objQuest = new SI_Question
                                        {
                                            QuestListID = questListID,
                                            QuestID = dicQuest.ToString(),
                                            QuestDescr = quesDescr,
                                            Crtd_Datetime = DateTime.Now,
                                            Crtd_User = Current.UserName,
                                            Crtd_Prog = _screenNbr,
                                            LUpd_User = Current.UserName,
                                            LUpd_Datetime = DateTime.Now,
                                            LUpd_Prog = _screenNbr,
                                            tstamp = new byte[1]
                                        };
                                        _db.SI_Question.AddObject(objQuest);
                                    }
                                    else
                                    {
                                        //idQuestAnswer = questID;
                                        objQuest.QuestDescr = quesDescr;
                                        objQuest.LUpd_User = Current.UserName;
                                        objQuest.LUpd_Datetime = DateTime.Now;
                                        objQuest.LUpd_Prog = _screenNbr;
                                        objQuest.tstamp = new byte[1];
                                    }

                                    dicQuestion[tempQuest].Add(questListID + "," + questID + "," + answerDescr + "," + correct);
                                }
                                else
                                {
                                    var objQuest = _db.SI_Question.FirstOrDefault(x => x.QuestListID == questListID && x.QuestID == questID);

                                    if (objQuest == null)
                                    {
                                        var idQuest = _db.SI_Question.Where(x => x.QuestListID == questListID).Max(x => x.QuestID).ToInt();
                                        var dicQuest = dicQuestionList[tempQuest] + idQuest;
                                        questID = dicQuest.ToString();
                                    }

                                    dicQuestion[tempQuest].Add(questListID + "," + questID + "," + answerDescr + "," + correct);
                                }


                                var lstAnswer = _db.SI_Answer.Where(x => x.QuestListID == questListID && x.QuestID == questID).ToList();

                                foreach (var item in lstAnswer)
                                {
                                    _db.SI_Answer.DeleteObject(item);
                                }


                            }
                        }

                    }

                    if (flagCheck)
                    {


                        foreach (var item in lstCheckTypeImport)
                        {
                            var keyArr = item.Key.Split(',');
                            if ((item.Type == "Y" || item.Type == "M" || item.Type == "O") && item.Qty < 2)
                            {
                                errorAnswer = keyArr[0];
                                flagCheck = false;
                                break;
                            }

                            if (item.Type == "Y" && item.Qty > 2)
                            {
                                errorAnswer1 = keyArr[0];
                                flagCheck = false;
                                break;
                            }

                            if ((item.Type == "Y" || item.Type == "M" || item.Type == "O") && item.QtyCorrect == 0)
                            {
                                errorAnswerCorrect = keyArr[0];
                                flagCheck = false;
                                break;
                            }

                            if ((item.Type == "Y" || item.Type == "O") && item.QtyCorrect > 1)
                            {
                                errorAnswerCorrect1 = keyArr[0];
                                flagCheck = false;
                                break;
                            }
                        }
                    }

                    if (flagCheck) // lưu SI_Answer
                    {
                        foreach (var item in dicQuestion)
                        {
                            foreach (var dic in dicQuestion[item.Key].Select((value, i) => new { i, value }))
                            {

                                var keyAnswer = dic.value.Split(',');
                                if (!string.IsNullOrEmpty(keyAnswer[2]))
                                {
                                    var objAswer = new SI_Answer
                                    {
                                        QuestListID = keyAnswer[0],
                                        QuestID = keyAnswer[1],
                                        AnswerID = dic.i.ToString(),
                                        AnswerDescr = keyAnswer[2],
                                        Correct = keyAnswer[3] == "X",
                                        Crtd_Datetime = DateTime.Now,
                                        Crtd_User = Current.UserName,
                                        Crtd_Prog = _screenNbr,
                                        LUpd_User = Current.UserName,
                                        LUpd_Datetime = DateTime.Now,
                                        LUpd_Prog = _screenNbr,
                                        tstamp = new byte[1]
                                    };
                                    _db.SI_Answer.AddObject(objAswer);
                                }


                            }
                        }
                    }
                    message = errorQuestListIDNull == "" ? "" : string.Format(Message.GetString("2018082851", null), errorQuestListIDNull.TrimEnd(','), Util.GetLang("SI22100QuestionGr"));
                    message += errorTypeNull == "" ? "" : string.Format(Message.GetString("2018082851", null), errorTypeNull.TrimEnd(','), Util.GetLang("SI22100Type"));
                    message += errorQuestListDescrNull == "" ? "" : string.Format(Message.GetString("2018082851", null), errorQuestListDescrNull.TrimEnd(','), Util.GetLang("QuestListDescr"));
                    message += errorQuestIDNull == "" ? "" : string.Format(Message.GetString("2018082851", null), errorQuestIDNull.TrimEnd(','), Util.GetLang("SI22100QuestID"));
                    message += errorQuestDescrNull == "" ? "" : string.Format(Message.GetString("2018082851", null), errorQuestDescrNull.TrimEnd(','), Util.GetLang("QuestionDescr"));
                    message += errorAnswerDescrNull == "" ? "" : string.Format(Message.GetString("2018082851", null), errorAnswerDescrNull.TrimEnd(','), Util.GetLang("AnswerDescr"));


                    message += errorCorrect == "" ? "" : string.Format(Message.GetString("2018082051", null), errorCorrect.TrimEnd(','), Util.GetLang("Correct"));
                    message += errorQuestListID == "" ? "" : string.Format(Message.GetString("2018082051", null), errorQuestListID.TrimEnd(','), Util.GetLang("SI22100QuestionGr"));
                    message += errorType == "" ? "" : string.Format(Message.GetString("2018082051", null), errorType.TrimEnd(','), Util.GetLang("SI22100Type"));
                    message += errorQuestID == "" ? "" : string.Format(Message.GetString("2019011451", null), errorQuestID.TrimEnd(','), Util.GetLang("SI22100QuestID"));
                    message += errorDuplicateID == "" ? "" : string.Format(Message.GetString("2019011452", null), errorDuplicateID.TrimEnd(','), Util.GetLang("SI22100QuestID"));

                    message += errorCheckType == "" ? "" : string.Format(Message.GetString("2018082852", null), errorCheckType.TrimEnd(','), Util.GetLang("SI22100Type"));
                    message += errorStatus == "" ? "" : string.Format(Message.GetString("2018083052", null), errorStatus);
                    message += errorChangeType == "" ? "" : string.Format(Message.GetString("2018083053", null), errorChangeType);


                    message += errorAnswer == "" ? "" : string.Format(Message.GetString("2018090853", null), errorAnswer);
                    message += errorAnswer1 == "" ? "" : string.Format(Message.GetString("2018091151", null), errorAnswer1);
                    message += errorAnswerCorrect == "" ? "" : string.Format(Message.GetString("2018090851", null), errorAnswerCorrect);
                    message += errorAnswerCorrect1 == "" ? "" : string.Format(Message.GetString("2018090852", null), errorAnswerCorrect1);
                    

                    if (string.IsNullOrEmpty(message))
                    {
                        _db.SaveChanges();
                    }
                    Util.AppendLog(ref _logMessage, "20121418", "", data: new { message });
                }
                else
                {
                    throw new MessageException(MessageType.Message, "148");
                }
            }
            catch (Exception ex)
            {
                if (ex is MessageException) return (ex as MessageException).ToMessage();
                return Json(new { success = false, messid = 9991, errorMsg = ex.ToString(), type = "error", fn = "", parm = "" });
            }
            return _logMessage;
        }

        #endregion

        #region -Export-
        [HttpPost]
        public ActionResult Export(FormCollection data)
        {
            try
            {
                string questListID = data["cboQuestListID"];
                var lstQuestion = _db.SI22100_peQuestion(Current.CpnyID,Current.UserName,Current.LangID,questListID).ToList();

                Stream stream = new MemoryStream();
                Workbook workbook = new Workbook();
                Worksheet sheetData = workbook.Worksheets[0];
                sheetData.Name = Util.GetLang("SI22100NameSheet"); ;
                workbook.Worksheets.Add();
                Worksheet masterData = workbook.Worksheets[1];
                masterData.Name = "MasterData";
                sheetData.AutoFitColumns();
                DataAccess dal = Util.Dal();
                var colTexts = HeaderExcel();
                sheetData.Cells.Merge(0,0, 1, 3);
                SetCellValueGridHeader(sheetData.Cells["A1"], "Bộ Câu Hỏi", TextAlignmentType.Center, TextAlignmentType.Center);
                sheetData.Cells.Merge(0,3, 1, 2);
                SetCellValueGridHeader(sheetData.Cells["D1"], "Câu Hỏi", TextAlignmentType.Center, TextAlignmentType.Center);
                sheetData.Cells.Merge(0,5, 1, 3);
                SetCellValueGridHeader(sheetData.Cells["F1"], "Đáp Án", TextAlignmentType.Center, TextAlignmentType.Center);


                for (int i = 0; i < colTexts.Count; i++)
                {
                    SetCellValue(sheetData.Cells[1, i], Util.GetLang(colTexts[i]), TextAlignmentType.Center, TextAlignmentType.Center, true, 10, true, false, false);
                    sheetData.Cells.SetColumnWidth(i, 25);

                }
                int commentIndex = sheetData.Comments.Add("D2");
                Comment comment = sheetData.Comments[commentIndex];
                comment.Note = "Mã Số Câu Hỏi chỉ nhập kiểu số";


             //   Cell cell;
               // int iRow = 3;
                //foreach (var item in lstQuestion)
                //{
                //    cell = sheetData.Cells["A" + iRow];
                //    cell.PutValue(item.QuestListID.ToString());

                //    cell = sheetData.Cells["B" + iRow];
                //    cell.PutValue(item.Type);

                //    cell = sheetData.Cells["C" + iRow];
                //    cell.PutValue(item.QuestTListDescr);

                //    cell = sheetData.Cells["D" + iRow];
                //    cell.PutValue(item.QuestID);

                //    cell = sheetData.Cells["E" + iRow];
                //    cell.PutValue(item.QuestDescr);

                //    cell = sheetData.Cells["F" + iRow];
                //    cell.PutValue(item.AnswerDescr);

                //    cell = sheetData.Cells["G" + iRow];
                //    cell.PutValue(item.Correct == true ? "X" : "");


                //    iRow++;
                //}


                ParamCollection pc = new ParamCollection();
                pc = new ParamCollection();
                pc.Add(new ParamStruct("@CpnyID", DbType.String, clsCommon.GetValueDBNull(Current.CpnyID), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@UserName", DbType.String, clsCommon.GetValueDBNull(Current.UserName), ParameterDirection.Input, 30));
                pc.Add(new ParamStruct("@LangID", DbType.Int16, clsCommon.GetValueDBNull(Current.LangID), ParameterDirection.Input, 30));

                DataTable dtType = dal.ExecDataTable("SI22100_pcTypeExcel", CommandType.StoredProcedure, ref pc);
                masterData.Cells.ImportDataTable(dtType, true, 0, 1, false);

                string formulaType = "=MasterData!$B$2:$B$" + (dtType.Rows.Count + 2);
                Validation validation = GetValidation(ref sheetData, formulaType, "Chọn loại trả lời", "Loại trả lời này không tồn tại");
                validation.AddArea(GetCellArea(2, dtType.Rows.Count + 100, colTexts.IndexOf("SI22100Type")));


               // Validation validationDelete = sheetData.Validations[sheetData.Validations.Add()];
                string formulaDelete = "X";
                validation = sheetData.Validations[sheetData.Validations.Add()];
                validation.IgnoreBlank = true;
                validation.Type = Aspose.Cells.ValidationType.List;
                validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
                validation.Operator = OperatorType.Between;
                validation.Formula1 = formulaDelete;

                CellArea position = new CellArea();
                position.StartRow = 2;
                position.EndRow = 1000;
                position.StartColumn = colTexts.IndexOf("Correct");
                position.EndColumn = colTexts.IndexOf("Correct");
                validation.AddArea(position);
                StyleFlag flag = new StyleFlag();
                flag.NumberFormat = true;
                //Set the formating on the as text formating 
                Range range;
                var allColumns = new List<string>();
                allColumns.AddRange(colTexts);
                var style = workbook.GetStyleInPool(0);


                style.Number = 49;

                range = sheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("SI22100QuestionGr")) + 3, Getcell(allColumns.IndexOf("SI22100QuestionGr")) + 1000);
                range.SetStyle(style);

                range = sheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("QuestListDescr")) + 3, Getcell(allColumns.IndexOf("QuestListDescr")) + 1000);
                range.SetStyle(style);

                range = sheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("SI22100QuestID")) + 3, Getcell(allColumns.IndexOf("SI22100QuestID")) + 1000);
                range.SetStyle(style);

                range = sheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("QuestionDescr")) + 3, Getcell(allColumns.IndexOf("QuestionDescr")) + 1000);
                range.SetStyle(style);

                range = sheetData.Cells.CreateRange(Getcell(allColumns.IndexOf("AnswerDescr")) + 3, Getcell(allColumns.IndexOf("AnswerDescr")) + 1000);
                range.SetStyle(style);
            

                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Flush();
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.ms-excel") { FileDownloadName = "SI22100_Template_Exp.xlsx" };
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

        private string Getcell(int column) // Hàm bị sai khi lấy vị trí column AA
        {
            if (column == 0)
            {
                return "A";
            }
            bool flag = false;
            string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string cell = "";
            while (column / 26 >= 1)
            {
                cell += ABC.Substring((column / 26) - 1, 1);
                column = column - 26;
                flag = true;

            }
            if (column % 26 != 0)
            {
                cell += ABC.Substring(column % 26, 1);
            }
            else
            {
                if (column % 26 == 0 && flag)
                {
                    cell += ABC.Substring(0, 1);
                }
            }

            return cell;
        }

        private string GetCodeFromExcel(string codeDescr)
        {
            int index = codeDescr.IndexOf("-");
            if (index > 0)
            {
                return codeDescr.Substring(0, index).Trim();
            }
            return codeDescr.Trim();
        }


        private List<string> HeaderExcel()
        {
            return new List<string>() { "SI22100QuestionGr", "SI22100Type", "QuestListDescr", "SI22100QuestID","QuestionDescr","AnswerDescr","Correct" };
        }

        #endregion

        #region 

        public string SetLineRefAnswer(int num)
        {
            var lineRef = num.ToString();
            var count = num.ToString();
            for (int i = 0; i < 5 - count.Length; i++)
            {
                lineRef = "0" + lineRef;
            }
                return lineRef;
        }

        private Validation GetValidation(ref Worksheet SheetMCP, string formular1, string inputMess, string errMess)
        {
            var validation = SheetMCP.Validations[SheetMCP.Validations.Add()];
            validation.IgnoreBlank = true;
            validation.Type = Aspose.Cells.ValidationType.List;
            validation.AlertStyle = Aspose.Cells.ValidationAlertType.Stop;
            validation.Operator = OperatorType.Between;
            validation.Formula1 = formular1;
            validation.InputTitle = "";
            validation.InputMessage = inputMess;
            validation.ErrorMessage = errMess;
            return validation;
        }
        private CellArea GetCellArea(int startRow, int endRow, int columnIndex, int endColumnIndex = -1)
        {
            var area = new CellArea();
            area.StartRow = startRow;
            area.EndRow = endRow;
            area.StartColumn = columnIndex;
            area.EndColumn = endColumnIndex == -1 ? columnIndex : endColumnIndex;
            return area;
        }
        private void SetCellValue(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH, bool isBold, int size, bool isTitle, bool isBackground, bool isWrapsTex)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Font.IsBold = isBold;
            style.Font.Size = size;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            style.Number = 49;
            if (isTitle)
            {
                style.Font.Color = Color.Red;
            }
            if (isBackground)
            {
                style.Font.Color = Color.Red;
                style.Pattern = BackgroundType.Solid;
                style.ForegroundColor = Color.Yellow;
            }
            if (isWrapsTex)
            {
                style.IsTextWrapped = true;
            }
            c.SetStyle(style);
        }
        private void SetCellValueGridHeader(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.BackgroundColor = Color.Yellow;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 20;
            style.Font.Color = Color.Red;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }
        private void SetCellValueGrid(Cell c, string lang, TextAlignmentType alignV, TextAlignmentType alignH)
        {
            c.PutValue(" " + lang);
            var style = c.GetStyle();
            style.Number = 49;
            style.Font.Name = "Times New Roman";
            style.Font.IsBold = true;
            style.Font.Size = 11;
            style.Font.Color = Color.Black;
            style.HorizontalAlignment = alignH;
            style.VerticalAlignment = alignV;
            c.SetStyle(style);
        }

        #endregion

        public class CheckTypeImport
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public int Qty { get; set; }
            public int? QtyCorrect { get; set; }


        }

    }
    
}

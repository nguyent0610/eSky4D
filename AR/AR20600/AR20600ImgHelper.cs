using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AR20600
{
    public class AR20600ImgHelper
    {
        #region TinhHV Image
        public static byte[] AR20600GetImage(string fileName, string pPath, bool isConfig)
        {
            string path = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            DirectoryInfo di;
            try
            {
                if (isConfig)
                {
                    path = pPath;
                    di = new DirectoryInfo(path);

                }
                else
                {
                    path = pathserver + "Images\\AR20600";
                    di = new DirectoryInfo(path);


                }
            }
            catch
            {
                path = pathserver + "Images\\AR20600";
                di = new DirectoryInfo(path);
            }
            if (!Directory.Exists(di.FullName))
            {
                Directory.CreateDirectory(di.FullName);
            }
            FileInfo file = di.GetFiles().Where(p => p.Name.ToUpper() == fileName.ToUpper()).FirstOrDefault();
            fileName = di.FullName + "\\" + fileName;
            if (File.Exists(fileName))
            {
                //if (file != null)
                //{
                //    if (File.Exists(file.FullName))
                //    {
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fileStream);
                byte[] imageBytes = reader.ReadBytes((int)fileStream.Length);
                reader.Close();
                return imageBytes;

                //    }
                //}
            }
            return null;
        }


        public static void AR20600UploadImage(string fileName, byte[] fileContext, string Path, bool isConfig)
        {
            string uploadDir = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            try
            {
                if (isConfig) uploadDir = Path;
                else
                {
                    uploadDir = pathserver + "Images\\AR20600";
                    //uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
                }
            }
            catch
            {
                uploadDir = pathserver + "Images\\AR20600";
                // uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
            }
            DirectoryInfo di = new DirectoryInfo(uploadDir);
            if (!Directory.Exists(di.FullName))
            {
                Directory.CreateDirectory(di.FullName);
            }
            string fullFileName = string.Format("{0}\\{1}", uploadDir, fileName);

            File.WriteAllBytes(fullFileName, fileContext);
        }

        public static bool DeleteFile(string fileName, string Path, bool isConfig)
        {
            var isSuccess = false;

            string uploadDir = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            try
            {
                if (isConfig) uploadDir = Path;
                else
                {
                    uploadDir = pathserver + "Images\\AR20600";
                    //uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
                }
            }
            catch
            {
                uploadDir = pathserver + "Images\\AR20600";
                // uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
            }
            string fullFileName = string.Format("{0}\\{1}", uploadDir, fileName);

            try
            {
                if (File.Exists(fullFileName))
                {
                    File.Delete(fullFileName);
                    isSuccess = true;
                }
            }
            catch { }

            return isSuccess;
        }


        public static void AR20600UploadMedia(string fileName, byte[] Context, string Path, bool isConfig)
        {
            string uploadDir = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            try
            {
                if (isConfig) uploadDir = Path.Substring(0, Path.Length - 14);
                else
                {
                    uploadDir = pathserver + "Media";
                    //uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
                }
            }
            catch
            {
                uploadDir = pathserver + "Media";
                // uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
            }
            var uploadDirLast = uploadDir + "Media";
            DirectoryInfo di = new DirectoryInfo(uploadDirLast);
            if (!Directory.Exists(di.FullName))
            {
                Directory.CreateDirectory(di.FullName);
            }
            string fullFileName = string.Format("{0}\\{1}", uploadDirLast, fileName);

            File.WriteAllBytes(fullFileName, Context);
            //File.WriteAllBytes(fileName, Context);

        }


        public static bool DeleteMedia(string fileName, string Path, bool isConfig)
        {
            var isSuccess = false;

            string uploadDir = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            try
            {
                if (isConfig) uploadDir = Path.Substring(0, Path.Length - 14);
                else
                {
                    uploadDir = pathserver + "Media";
                    //uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
                }
            }
            catch
            {
                uploadDir = pathserver + "Media";
                // uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
            }
            var uploadDirLast = uploadDir + "Media";
            string fullFileName = string.Format("{0}\\{1}", uploadDirLast, fileName);

            try
            {
                if (File.Exists(fullFileName))
                {
                    File.Delete(fullFileName);
                    
                    isSuccess = true;
                }
            }
            catch { }

            return isSuccess;
        }

        public static bool AR20600CopyMedia(string fileName, string tmpOldFileName, string Path, bool isConfig)
        {
            var isSuccess = false;

            string uploadDir = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            try
            {
                if (isConfig) uploadDir = Path.Substring(0, Path.Length - 14);
                else
                {
                    uploadDir = pathserver + "Media";
                    //uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
                }
            }
            catch
            {
                uploadDir = pathserver + "Media";
                // uploadDir = HttpContext.Current.Server.MapPath(uploadDir);
            }
            var uploadDirLast = uploadDir + "Media";
            string fullFileName = string.Format("{0}\\{1}", uploadDirLast, fileName);
            string oldfullFileName = string.Format("{0}\\{1}", uploadDirLast, tmpOldFileName);
            try
            {
                if (!File.Exists(fullFileName))
                {
                    File.Copy(oldfullFileName, fullFileName);

                    isSuccess = true;
                }
            }
            catch { }

            return isSuccess;

        }


        #endregion
    }
}
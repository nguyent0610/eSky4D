using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace SA00300
{
    public class SA00300ImgHelper
    {
        public static byte[] SA00300GetImage(string fileName, string pPath, bool isConfig)
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
                    path = pathserver + "Images\\SA00300";
                    di = new DirectoryInfo(path);
                }
            }
            catch
            {
                path = pathserver + "Images\\SA00300";
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

        public static void SA00300UploadImage(string fileName, byte[] fileContext, string Path, bool isConfig)
        {
            string uploadDir = "";
            string pathserver = HttpContext.Current.Server.MapPath("");
            pathserver = pathserver.Substring(0, pathserver.Length - 7);
            try
            {
                if (isConfig) uploadDir = Path;
                else
                {
                    uploadDir = pathserver + "Images\\SA00300";
                }
            }
            catch
            {
                uploadDir = pathserver + "Images\\SA00300";
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
                    uploadDir = pathserver + "Images\\SA00300";
                }
            }
            catch
            {
                uploadDir = pathserver + "Images\\SA00300";
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
    }
}
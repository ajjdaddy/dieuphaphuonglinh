using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiem_HL
{
    class Util
    {
        //full path of a file 
        //full path of a file 
        static string _strErrLogPath = System.Configuration.ConfigurationManager.AppSettings.Get("ErrLogPath");
        static string _strErrLogFile = System.Configuration.ConfigurationManager.AppSettings.Get("ErrLogFile");
        static string _strErrLogFilePath = "";

        public static string GetAppRelativePath(string strEndPath)
        {
            return Path.Combine(Environment.CurrentDirectory, strEndPath);
        }
        static public bool IsDirExist(string strDirPath)
        {
            if (!Directory.Exists(strDirPath))
                return false;

            return true;
        }

        /// <summary>
        /// if Directory (FolderPath) NOT EXIST, then create directory
        /// </summary>
        public static void DirCheck(string strDirPath)
        {
            if (!Directory.Exists(strDirPath))
                Directory.CreateDirectory(strDirPath);
        }

        public static string[] GetSubDir(string strDirPath)
        {
            if (Directory.Exists(strDirPath))
            {
                string[] aStrDefaultPath = { strDirPath };

                string[] aStrSubDirFound = Directory.GetDirectories(strDirPath);

                if (aStrSubDirFound.Length > 0)
                    return aStrSubDirFound;
                else
                    return aStrDefaultPath;
            }
            else
                return null;
        }
        public static bool IsFileNameExist(string strDirPath, string strFileName)
        {
            string strFileNamePath = System.IO.Path.Combine(strDirPath, strFileName);

            if (File.Exists(strFileNamePath))
                return true;

            return false;
        }


        /// <summary>
        /// if FileName NOT EXIST, then create filename
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public static void FileNameCheck(string strDirPath, string strFileName)
        {
            string strFileNamePath = System.IO.Path.Combine(strDirPath, strFileName);

            if (!File.Exists(strFileNamePath))
                File.Create(strFileNamePath);
        }

        /// <summary>
        /// return ArrayList of all FileNames found in the directory
        /// </summary>
        /// <param name="strDirPath"></param>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public static ArrayList GetFileNameList(string strDirPath)
        {
            ArrayList aLstImgFileName = new ArrayList();

            DirCheck(strDirPath);

            //FileInfo[] files = dir.GetFiles().OrderBy(p=>p.CreationTime).ToArray();

            //string[] arrStrScanImgFileNameList = Directory.GetFiles(strDirPath).OrderBy(p=>p.).ToArray();

            string[] arrStrScanImgFileNameList = Directory.GetFiles(strDirPath).OrderBy(d => d).ToArray();

            if (arrStrScanImgFileNameList.Length > 0)
            {
                foreach (string strImgFileName in arrStrScanImgFileNameList)
                {
                    aLstImgFileName.Add(strImgFileName.Replace(strDirPath, ""));
                }
            }
            else
                aLstImgFileName.Add("File Not Found.");

            return aLstImgFileName;
        }

        /// <summary>
        /// return ArrayList of all FileNames found by (strImgFileNameSearch) in the directory
        /// </summary>
        /// <param name="strImgFilePath"></param>
        /// <param name="strImgFileNameSearch"></param>
        /// <returns></returns>
        public static ArrayList SearchFileName(string strImgFilePath, string strImgFileNameSearch)
        {
            ArrayList aLstImgFileName = new ArrayList();

            if (IsDirExist(strImgFilePath))
            {
                string[] arrStrScanImgFileNameList = Directory.GetFiles(strImgFilePath, strImgFileNameSearch);
                if (arrStrScanImgFileNameList.Length > 0)
                {
                    foreach (string strImgFileName in arrStrScanImgFileNameList)
                        aLstImgFileName.Add(strImgFileName.TrimStart('\\').Replace(strImgFilePath, ""));
                }
                else
                    aLstImgFileName.Add("File Not Found.");
            }
            else
                aLstImgFileName.Add("File Path Not Found.");

            return aLstImgFileName;
        }

        /// <summary>
        /// Remove these char. from a Filename: [(),+] and also remove "Loc ..." 
        /// </summary>
        /// <param name="strOrigFilename"></param>
        /// <returns></returns>
        public static string RenameFile(string strOrigFilename)
        {
            string strNewFilename = "";

            try
            {
                string strLoc = "+";

                int ixBeginLoc = GetLocationIndex(strOrigFilename);
                int ixEndLoc = strOrigFilename.LastIndexOf(".");

                if (ixBeginLoc > 0)
                    strLoc = strOrigFilename.Substring(ixBeginLoc, (ixEndLoc - ixBeginLoc));

                strNewFilename = strOrigFilename.Replace(strLoc, "").Replace("(", "").Replace(")", "").Replace(",", "").Replace("+", "").Replace("(", "").Replace(")", "");
            }
            catch (Exception ex)
            {
                string strErr = ex.Message + " | " + ex.InnerException.ToString();
                ErrLog(strErr);
            }

            return strNewFilename;  // + "\r\n";
        }
        public static int GetLocationIndex(string strImgFileName)
        {
            int ixLoc = strImgFileName.LastIndexOf(",Loc ");

            if (ixLoc == -1)
            {
                ixLoc = strImgFileName.LastIndexOf(", Loc ");

                if (ixLoc == -1)
                {
                    ixLoc = strImgFileName.LastIndexOf(", Loc ");

                    if (ixLoc == -1)
                    {
                        ixLoc = strImgFileName.LastIndexOf("] Loc ");
                    }
                }
            }

            return ixLoc;
        }
        public static void MoveFile(string strSourcePath, string strDestPath, string strFileName)
        {
            DirCheck(strDestPath);

            string strSourceFileName = System.IO.Path.Combine(strSourcePath, strFileName);
            string strDestFileName = System.IO.Path.Combine(strDestPath, strFileName);

            if (IsDirExist(strSourcePath))
            {
                if (IsFileNameExist(strSourcePath, strFileName))
                {
                    try
                    {
                        if (!System.IO.File.Exists(strDestFileName))
                            File.Move(strSourceFileName, strDestFileName);
                        else
                        {
                            File.Delete(strSourceFileName); //Destination FileName Found - Just delete the file
                            //string strErrMsg = string.Format("Move File Error - Destination FileName Found:\r\n{0}", strDestFileName);
                            //ErrLog(strErrMsg);
                            //throw new Exception(strErrMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        string strErrMsg = ex.ToString();
                        ErrLog(strErrMsg);
                        throw new Exception(strErrMsg);
                    }
                }
                else
                {
                    string strErrMsg = string.Format("Move File Error - Source FileName Not Found:\r\n{0}", strSourceFileName);
                    ErrLog(strErrMsg);
                    throw new Exception(strErrMsg);
                }
            }
            else
            {
                string strErrMsg = string.Format("Move File Error - Source Path Not Found:\r\n{0}", strSourcePath);
                ErrLog(strErrMsg);
                throw new Exception(strErrMsg);
            }

        }
        public static void CopyAndRenameFile(string strSourcePath, string strDestPath, string strSrcFileName, string strDestFileName)
        {
            DirCheck(strDestPath);

            string strSourceFileName = System.IO.Path.Combine(strSourcePath, strSrcFileName);
            string strDestinFileName = System.IO.Path.Combine(strDestPath, strDestFileName);

            if (IsDirExist(strSourcePath))
            {
                if (IsFileNameExist(strSourcePath, strSrcFileName))
                {
                    try
                    {
                        File.Copy(strSourceFileName, strDestinFileName, true);
                    }
                    catch (Exception ex)
                    {
                        string strErrMsg = ex.ToString();
                        ErrLog(strErrMsg);
                        throw new Exception(strErrMsg);
                    }
                }
                else
                {
                    string strErrMsg = string.Format("Move File Error - Source FileName Not Found:\r\n{0}", strSourceFileName);
                    ErrLog(strErrMsg);
                    throw new Exception(strErrMsg);
                }
            }
            else
            {
                string strErrMsg = string.Format("Move File Error - Source Path Not Found:\r\n{0}", strSourcePath);
                ErrLog(strErrMsg);
                throw new Exception(strErrMsg);
            }

        }

        public static void FileSavAs(string strSourcePath, string strDestPath, string strSrcFileName, string strDestFileName)
        {
            DirCheck(strDestPath);

            string strSourceFileName = System.IO.Path.Combine(strSourcePath, strSrcFileName);
            string strDestinFileName = System.IO.Path.Combine(strDestPath, strDestFileName);

            if (IsDirExist(strSourcePath))
            {
                if (IsFileNameExist(strSourcePath, strSrcFileName))
                {
                    try
                    {
                        File.Copy(strSourceFileName, strDestinFileName, true);
                        File.Delete(strSourceFileName);
                    }
                    catch (Exception ex)
                    {
                        string strErrMsg = ex.ToString();
                        ErrLog(strErrMsg);
                        throw new Exception(strErrMsg);
                    }
                }
                else
                {
                    string strErrMsg = string.Format("File SavAs Error - Source FileName Not Found:\r\n{0}", strSourceFileName);
                    ErrLog(strErrMsg);
                    throw new Exception(strErrMsg);
                }
            }
            else
            {
                string strErrMsg = string.Format("File SavAs Error - Source Path Not Found:\r\n{0}", strSourcePath);
                ErrLog(strErrMsg);
                throw new Exception(strErrMsg);
            }

        }

        public static void FileSavAsAndMove(string strSourcePath, string strDestPath, string strSrcFileName, string strDestFileName)
        {
            DirCheck(strDestPath);

            string strSourceFileName = System.IO.Path.Combine(strSourcePath, strSrcFileName);
            string strDestinFileNameMove = System.IO.Path.Combine(strDestPath, strDestFileName);

            if (IsDirExist(strSourcePath))
            {
                if (IsFileNameExist(strSourcePath, strSrcFileName))
                {
                    try
                    {
                        if (!System.IO.File.Exists(strDestFileName))
                            File.Move(strSourceFileName, strDestinFileNameMove);
                        else
                        {
                            string strErrMsg = string.Format("Move File Error - Destination FileName Found:\r\n{0}", strDestinFileNameMove);
                            ErrLog(strErrMsg);
                            throw new Exception(strErrMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        string strErrMsg = ex.ToString() + "\r\n\r\nSource FileName: " + strSrcFileName + "\r\n\r\nDestination FileName: " + strDestFileName;
                        ErrLog(strErrMsg);
                        throw new Exception(strErrMsg);
                    }
                }
                else
                {
                    string strErrMsg = string.Format("Move File Error - Source FileName Not Found:\r\n{0}", strSrcFileName);
                    ErrLog(strErrMsg);
                }
            }
            else
            {
                string strErrMsg = string.Format("Move File Error - Source Path Not Found:\r\n{0}", strSourcePath);
                ErrLog(strErrMsg);
            }

        }

        public static string SubFolerFileCopyToNewLoc(string strSourcePath, string strDestPath)
        {
            string strErrMsg = "Done.";

            DirCheck(strDestPath);

            string[] aStrFolderScr = GetSubDir(strSourcePath);

            if (aStrFolderScr != null)
            {
                foreach (string strScrFolderPath in aStrFolderScr)
                {
                    ArrayList aLstScrFilename = GetFileNameList(strScrFolderPath);

                    if (aLstScrFilename.Count > 0)
                    {
                        foreach (string strSrcFileName in aLstScrFilename)
                        {
                            string strSourcePathFileName = System.IO.Path.Combine(strScrFolderPath, strSrcFileName.TrimStart('\\'));
                            string strDestinPathFileName = System.IO.Path.Combine(strDestPath, strSrcFileName.TrimStart('\\'));

                            if (IsFileNameExist(strScrFolderPath, strSrcFileName.TrimStart('\\')))
                                File.Copy(strSourcePathFileName, strDestinPathFileName, true);
                            else
                            {
                                strErrMsg = string.Format("SubFolerFileCopyToNewLoc() Error - Source FileName Not Found:\r\n{0}", strSourcePathFileName);
                                ErrLog(strErrMsg);
                                throw new Exception(strErrMsg);
                            }
                        }
                    }
                    else
                    {
                        strErrMsg = string.Format("SubFolerFileCopyToNewLoc() Error - Sub File Not Found:\r\n{0}", strScrFolderPath);
                        ErrLog(strErrMsg);
                        throw new Exception(strErrMsg);
                    }
                }
            }
            else
            {
                strErrMsg = string.Format("SubFolerFileCopyToNewLoc() Error - Sub Folder Not Found: {0}", strSourcePath);
                ErrLog(strErrMsg);
                throw new Exception(strErrMsg);
            }

            return strErrMsg;
        }

        public static ArrayList RemoveDS_StoreFile(string strScrFolderPath, ArrayList aLstrFilename)
        {
            string strDeleteFileName = "";
            int ix = 0;

            foreach (string strCurFileName in aLstrFilename)
            {
                if (strCurFileName.TrimStart('\\') == ".DS_Store")
                {
                    strDeleteFileName = System.IO.Path.Combine(strScrFolderPath, strCurFileName.TrimStart('\\'));

                    File.Delete(@strDeleteFileName);

                    aLstrFilename.RemoveAt(ix);
                    break;
                }
            }

            return aLstrFilename;
        }
        public static string RemoveDupFiles(string strSourcePath)
        {
            string strErrMsg = "Done Remove Duplicate.";

            string[] aStrFolderScr = GetSubDir(strSourcePath);

            if (aStrFolderScr != null)
            {
                string strFistFileNameInFolerPath = ""; //C:\DP_Project\DP_HL_Wall_Layout\Tram
                string strNextFileNameInFolerPath = "";
                string strDeleteFileName = "";
                string strFirstExt = "";

                //string strExt = "";

                int ixFileCount = 0;

                foreach (string strScrFolderPath in aStrFolderScr)
                {
                    ixFileCount = 0;
                    ArrayList aLstScrFilename = GetFileNameList(strScrFolderPath);

                    if (aLstScrFilename.Count > 0)
                    {
                        aLstScrFilename = RemoveDS_StoreFile(strScrFolderPath, aLstScrFilename);

                        string strFirstFileName = aLstScrFilename[ixFileCount].ToString().TrimStart('\\');

                        try
                        {
                            //if (strFirstFileName == ".DS_Store")
                            //    strFistFileNameInFolerPath = strFirstFileName;
                            //{}

                            strFirstExt = strFirstFileName.Substring(strFirstFileName.LastIndexOf("."));
                            strFistFileNameInFolerPath = strFirstFileName.Substring(0, strFirstFileName.LastIndexOf("."));

                            foreach (string strCurFileNameInList in aLstScrFilename)
                            {
                                if (ixFileCount > 0)
                                {
                                    string strNextExt = strCurFileNameInList.Substring(strCurFileNameInList.LastIndexOf("."));
                                    string strNextFileName = strCurFileNameInList.TrimStart('\\');

                                    strNextFileNameInFolerPath = strNextFileName.Substring(0, strNextFileName.LastIndexOf("."));

                                    //if (strFistFileNameInFolerPath == ".DS_Store")
                                    //{
                                    //    strDeleteFileName = System.IO.Path.Combine(strScrFolderPath, strFistFileNameInFolerPath);

                                    //    File.Delete(@strDeleteFileName);

                                    //    strFistFileNameInFolerPath = strNextFileNameInFolerPath;
                                    //    strFirstExt = strNextExt;
                                    //}
                                    //else if (strNextFileNameInFolerPath == ".DS_Store")
                                    //{
                                    //    strDeleteFileName = System.IO.Path.Combine(strScrFolderPath, strNextFileNameInFolerPath);

                                    //    File.Delete(@strDeleteFileName);
                                    //}else 

                                    if (strFistFileNameInFolerPath.Contains(strNextFileNameInFolerPath))
                                    {
                                        strDeleteFileName = System.IO.Path.Combine(strScrFolderPath, strFistFileNameInFolerPath + strFirstExt);

                                        if (File.Exists(@strDeleteFileName))
                                        {
                                            File.Delete(@strDeleteFileName);
                                        }
                                        //System.IO.Directory.Delete(strDeleteFileName);

                                        //File.Delete(strDeleteFileName);
                                    }
                                    else if (strNextFileNameInFolerPath.Contains(strFistFileNameInFolerPath))
                                    {
                                        strDeleteFileName = System.IO.Path.Combine(strScrFolderPath, strNextFileNameInFolerPath + strNextExt);

                                        File.Delete(strDeleteFileName);

                                        strFistFileNameInFolerPath = strNextFileNameInFolerPath;
                                        strFirstExt = strNextExt;
                                    }
                                    else
                                    {
                                        strFistFileNameInFolerPath = strNextFileNameInFolerPath;
                                        strFirstExt = strNextExt;
                                    }
                                }
                                ixFileCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            string strExcpt = ex.ToString();
                        }
                    }
                    else
                    {
                        strErrMsg = string.Format("SubFolerFileCopyToNewLoc() Error - Sub File Not Found: {0}", strScrFolderPath);
                        ErrLog(strErrMsg);
                    }
                }
            }
            else
            {
                strErrMsg = string.Format("SubFolerFileCopyToNewLoc() Error - Sub Folder Not Found: {0}", strSourcePath);
                ErrLog(strErrMsg);
            }

            return strErrMsg;
        }

        static public void WriteToFile(string strFilePath, string strFileName, string strMessage)
        {
            DirCheck(strFilePath);
            FileNameCheck(strFilePath, strFileName);
            string strWriteFilePath = System.IO.Path.Combine(strFilePath, strFileName);

            using (FileStream fs = new FileStream(strWriteFilePath, FileMode.Append, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(DateTime.Now + " " + strMessage);
                sw.Flush();
                fs.Close();
            }
        }

        private static void WriteErrMsg(string strErrMsg)
        {
            using (FileStream fs = new FileStream(_strErrLogFilePath, FileMode.Append, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(DateTime.Now + " " + strErrMsg);
                sw.Flush();
                fs.Close();
            }

            //////Read and Write at the same time
            ////https://stackoverflow.com/questions/33633344/read-and-write-to-a-file-in-the-same-stream
            ////string filePath = "test.txt";
            ////FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            ////StreamReader sr = new StreamReader(fs);
            ////StreamWriter sw = new StreamWriter(fs);
            ////newString = sr.ReadToEnd() + "somethingNew";
            ////sw.Write(newString);
            ////sw.Flush(); //HERE
            ////fs.Close();
        }
        public static void ErrLog(string strErrMsg)
        {
            DirCheck(_strErrLogPath);
            FileNameCheck(_strErrLogPath, _strErrLogFile);
            _strErrLogFilePath = System.IO.Path.Combine(_strErrLogPath, _strErrLogFile);

            WriteErrMsg(strErrMsg);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Kiem_HL
{
    public partial class Form1 : Form
    {
        //https://www.youtube.com/watch?v=uONQaT-nwls
        public string _ImgFolderPath = "";
        public string _ImgFolderDonePath = "";
        public string _ImgFolderArchivePath = "";
        public string _InfoSubject = "";
        public string _InfoMessage = "";
        public string _AnnivSubject = "";
        public string _AnnivMessage = "";
        public string _Twilio_Acct_SID = "";
        public string _Twilio_Auth_Token = "";
        public string _STOP_Message = "";

        static int _ixSelectLength = 0;             //mousedown event (hold index selected length) on txtImgFilename Click
        static int _iClickCount = 0;                //keep track of number clicks on txtImgFilename

        const string _strVietAlpha_A = "aàảãáạăằẳẵắặâầẩẫấậ";         //[0]
        const string _strVietAlpha_D = "dđ";                         //[1]
        const string _strVietAlpha_E = "eèẻẽéẹêềểễếệ";               //[2]
        const string _strVietAlpha_I = "iìỉĩíị";                     //[3]
        const string _strVietAlpha_O = "oòỏõóọôồổỗốộơờởỡớợ";         //[4]
        const string _strVietAlpha_U = "uùủũúụưừửữứự";               //[5]
        const string _strVietAlpha_Y = "yỳỷỹýỵ";                     //[6]

        enum Days { Sun, Mon, Tue, Wed, Thu, Fri, Sat };

        public Form1()
        {
            InitializeComponent();
        }
        #region Run ONCE
        // RUN THIS FUNCTION ONLY ONCE TO: change the DB from the old format [Ho,Dem,Ten] to the new format [HoTen] AND extract filenumber from filename
        // trying to parse Filenumber from the filename and Convert_VN_To_Eng for Fullname
        // 1. Open/Copy from RemovableDisk:		D:\CDN\LinhTu\DB\linhtu.mdb
        // 2. Select table: tblHuongLinh and Export data to Excel file 	C:\CDN\CDN_HL\CDN_HL\DB\tblHL.xlsx
        // 3. Open tblHL.xlsx file and ADD these columns: HoTen, Fullname, FileNumber, InsertDate, UpdateDate, Note
        // 4. In tblHL.xlsx file, combines [Ho,Dem,Ten] to [HoTen] uses this format: [=D2&" "&E2&" "&F2]
        // 5. Delete [HL_ID, GC_ID, LienHeVoi_GC, Ho, Dem, Ten, Tho, GioiTinh, SinhTai, MatGio, MatTai, NhapLiem, ChonThieu, int_ViTriHinh] columns in tblHL.xlsx file
        // 6. Rename [int_ViTriCot] to [ViTriCot]
        // 7. Must rename the excel tab [tblHuongLinh] to [tblHL] and save the file
        // 8. Open Access >> File >> New DB(Empty) >> import Data from excel file >> tblHL.xlsx and the TABLE as tblHL, and then SAVE the DB AS >> DN_HL.accdb
        // 9. In this application MUST: add these columns to the gridview: Fullname,FileNumber,InsertDate,UpdateDate
        // 10. And RUN THIS APPLICATION ONLY ONCE!!
        // 11. DO NOT USE THIS APPLICATION EVER AGAIN.
        private void ParseFullnameAndSinhTu()
        {
            if (datasGridView.Rows.Count > 1)
            {
                try
                {
                    foreach (DataRow dr in hL_DBDataSet.HL_Tbl.Rows)
                    {
                        //#1. Split Fullname into Fullname and FullPhapDanh
                        string strTempFullnamePhapDanh = dr["Fullname"].ToString().Replace(" PD ", "|");
                        string[] astrFullnamePhapDanh = strTempFullnamePhapDanh.Split('|');
                        if (astrFullnamePhapDanh.Count() == 2)
                        {
                            dr["Fullname"] = astrFullnamePhapDanh[0];
                            dr["FullPhapDanh"] = astrFullnamePhapDanh[1];
                        }

                        //#2. Split MatNgay_DL into Sinh_DL and MatNgay_DL
                        string strTempSinhTu = dr["MatNgay_DL"].ToString();
                        string[] astrSinhTu = strTempSinhTu.Split('-');
                        if (astrSinhTu.Count() == 2)
                        {
                            dr["SinhNgay_DL"] = astrSinhTu[0];
                            dr["MatNgay_DL"] = astrSinhTu[1];
                        }
                    }
                    hLTblBindingSource.EndEdit();
                    hL_TblTableAdapter.Update(this.hL_DBDataSet.HL_Tbl);  //Update the HL_DB
                    hL_DBDataSet.HL_Tbl.AcceptChanges();
                }
                catch (Exception ex)
                {
                    string strError = ex.ToString();
                    strError += ex.InnerException;
                }
            }

        }

        #endregion Run ONCE

        private void Form1_Load(object sender, EventArgs e)
        {
            cksPhapDanh.Checked = false;
            lblsErrorMsg.Text = "";

            //Get AppSettings Values from System.Configuration
            _ImgFolderPath = System.Configuration.ConfigurationManager.AppSettings.Get("ImgFolderPath");
            _ImgFolderDonePath = System.Configuration.ConfigurationManager.AppSettings.Get("ImgFolderDonePath");
            _ImgFolderArchivePath = ConfigurationManager.AppSettings.Get("ImgFolderArchivePath");

            //Get Text Subject and Messages
            _InfoSubject = System.Configuration.ConfigurationManager.AppSettings.Get("InfoSubject");
            _InfoMessage = System.Configuration.ConfigurationManager.AppSettings.Get("InfoMessage");
            _AnnivSubject = System.Configuration.ConfigurationManager.AppSettings.Get("AnnivSubjectVN");
            _AnnivMessage = System.Configuration.ConfigurationManager.AppSettings.Get("AnnivMessageVN");
            _STOP_Message = System.Configuration.ConfigurationManager.AppSettings.Get("StopMessage");

            //Twilio Account SIS and Auth Token
            _Twilio_Acct_SID = System.Configuration.ConfigurationManager.AppSettings.Get("TWILIO_ACCOUNT_SID");
            _Twilio_Auth_Token = System.Configuration.ConfigurationManager.AppSettings.Get("TWILIO_AUTH_TOKEN");

            Location = new Point(20, 20);   //Starts the Form at this location

            tabSearch.Select();             //Active the tab control and select the tabSearch

            RefreshSearchTab();

            ///--------------------------------------------------------------------------------------
            ///- 
            ///- ////////Run this function only ONCE when change the DB format from the old to the new
            ///- //////ParseFilenumberAndConvert_VN_TO_ENG();
            ///- 
            ///--------------------------------------------------------------------------------------

        }
        /// <summary>
        /// 1. ClearAllSearchTextBoxFields() - 2. Fill the hL_DBDataSet.HL_Tbl - 3. Bind the datasGridView - Populate Ten, ViTriHinh, etc... - 4. Display HL Image
        /// </summary>
        private void RefreshSearchTab()
        {
            ClearAllSearchTextBoxFields();

            ClearAllSearchTabFieldsBackGroundColor();

            hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = "";
            hL_DBDataSet.HL_Tbl.DefaultView.Sort = "";
            hL_DBDataSet.HL_Tbl.AcceptChanges();

            this.hL_TblTableAdapter.Fill(this.hL_DBDataSet.HL_Tbl);

            hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;

            datasGridView.DataSource = hLTblBindingSource;
            datasGridView.Focus();
            DisplayImage();

            txtsSearch.Focus();
        }
        /// <summary>
        /// Set lblsErrorMsg.Text = "";	txtsSearch.Text = ""; txtsFNumbSearch.Text = ""; cksPhapDanh.Checked = false; txtsFilename.Text = "";
        /// </summary>
        private void ClearAllSearchTextBoxFields()
        {
            lblsErrorMsg.Text = "";
            txtsSearch.Text = "";
            txtsSearch.ForeColor = System.Drawing.Color.Black;       //Text 
            txtsSearch.BackColor = System.Drawing.Color.White;       //Background
            txtsFNumbSearch.Text = "";
            cksPhapDanh.Checked = false;
        }
        private void ClearAllSearchTabFieldsBackGroundColor()
        {
            txtsViTriHinh.ForeColor = System.Drawing.Color.Red;          //Text 
            txtsViTriHinh.BackColor = System.Drawing.Color.Yellow;       //Background

            txtsViTriCot.ForeColor = System.Drawing.Color.Blue;
            txtsViTriCot.BackColor = System.Drawing.Color.Yellow;

            txtsHoTen.ForeColor = System.Drawing.Color.Black;
            txtsHoTen.BackColor = System.Drawing.Color.White;

            txtsPhapDanh.ForeColor = System.Drawing.Color.Black;
            txtsPhapDanh.BackColor = System.Drawing.Color.White;

            txtsSinh.ForeColor = System.Drawing.Color.Black;
            txtsSinh.BackColor = System.Drawing.Color.White;

            txtsTu.ForeColor = System.Drawing.Color.Black;
            txtsTu.BackColor = System.Drawing.Color.White;

            txtsTuAL.ForeColor = System.Drawing.Color.Black;
            txtsTuAL.BackColor = System.Drawing.Color.White;

            txtsFileNumber.ForeColor = System.Drawing.Color.Black;
            txtsFileNumber.BackColor = System.Drawing.Color.White;

            txtsFilename.ForeColor = System.Drawing.Color.Black;
            txtsFilename.BackColor = System.Drawing.Color.White;

            txtsNote.ForeColor = System.Drawing.Color.Black;
            txtsNote.BackColor = System.Drawing.Color.White;
        }
        private void SetAllSearchTabFieldsBackGroundColor()
        {
            if (lblsOrigViTriHinh.Text.Trim() != txtsViTriHinh.Text.Trim())
                txtsViTriHinh.BackColor = System.Drawing.Color.Gainsboro;        //Background

            if (lblsOrigViTriCot.Text.Trim() != txtsViTriCot.Text.Trim())
                txtsViTriCot.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigHoTen.Text.Trim() != txtsHoTen.Text.Trim())
                txtsHoTen.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigPhapDanh.Text.Trim() != txtsPhapDanh.Text.Trim())
                txtsPhapDanh.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigSinh.Text.Trim() != txtsSinh.Text.Trim())
                txtsSinh.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigTu.Text.Trim() != txtsTu.Text.Trim())
                txtsTu.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigTuAl.Text.Trim() != txtsTuAL.Text.Trim())
                txtsTuAL.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigFilename.Text.Trim() != txtsFilename.Text.Trim())
                txtsFilename.BackColor = System.Drawing.Color.Gainsboro;

            if (lblsOrigFileNumber.Text.Trim() != txtsFileNumber.Text.Trim())
                txtsFileNumber.BackColor = System.Drawing.Color.Gainsboro;
        }


        /******************************************** SEARCH TAB SECTION **********************************************/
        #region - Search Tab Section

        private void DisplayImage()
        {
            if (lblsOrigFilename.Text.Trim().Length > 0)
            {
                string strImgFileNameDonePath = _ImgFolderDonePath + lblsOrigFilename.Text;

                if (File.Exists(strImgFileNameDonePath))
                {
                    Bitmap bitImageFileOrig = new Bitmap(strImgFileNameDonePath);
                    Bitmap bitImageFileCopy = new Bitmap((Image)bitImageFileOrig);

                    picsBoxHL.SizeMode = PictureBoxSizeMode.StretchImage;    //in order to have any image "resize" to fit a picturebox, you must set this  
                    picsBoxHL.Width = 440;   // 300;   // 580;
                    picsBoxHL.Height = 450;  // 260;   // 500;
                    picsBoxHL.Image = (Image)bitImageFileCopy;
                    picsBoxHL.Tag = lblsOrigFilename.Text;

                    bitImageFileOrig.Dispose(); //release the Original image file to allow this file to be deleted in this program
                    //bitImageFileCopy.Dispose();   //DO NOT >> SET bitImageFileCopy.Dispose(); << CAUSES THE APPLICATION STOP RUNNING!!
                }
                else
                {
                    picsBoxHL.Image = null;  //Hinh Not found.
                    lblsErrorMsg.Text = "Hinh Not Found.";
                }
            }
            else
            {
                picsBoxHL.Image = null;
                lblsErrorMsg.Text = "Hinh file name is empty.";
            }
        }


        /// <summary>
        /// When HL Name or PhapDanh has changed, must change ImageFileName as well to reflex the HL Name and or PhapDanh in image filename
        /// ex:(from: 1 Ngo Dung Pd Nhat Doan 1919-2002.jpg  >>>  1 Ngô Dung Pd Nhật Đoan 1919-2002.jpg 
        /// </summary>
        /// <returns></returns>
        private string ReFormatImageFileName()
        {
            string strNewFileName = string.Format("{0}.jpg", txtsFileNumber.Text);   //54.2.jpg

            if (txtsHoTen.Text.Trim().Length > 0 && txtsPhapDanh.Text.Trim().Length > 0 && txtsSinh.Text.Trim().Length > 0)
                strNewFileName = string.Format("{0} {1} Pd {2} {3}.jpg", txtsFileNumber.Text, txtsHoTen.Text.Trim(), txtsPhapDanh.Text.Trim(), txtsSinh.Text.Trim());
            else if (txtsHoTen.Text.Trim().Length > 0 && txtsPhapDanh.Text.Trim().Length > 0)
                strNewFileName = string.Format("{0} {1} Pd {2}.jpg", txtsFileNumber.Text, txtsHoTen.Text.Trim(), txtsPhapDanh.Text.Trim());
            else if (txtsHoTen.Text.Trim().Length > 0 && txtsSinh.Text.Trim().Length > 0)
                strNewFileName = string.Format("{0} {1} {2}.jpg", txtsFileNumber.Text, txtsHoTen.Text.Trim(), txtsSinh.Text.Trim());
            else if (txtsHoTen.Text.Trim().Length > 0)
                strNewFileName = string.Format("{0} {1}.jpg", txtsFileNumber.Text, txtsHoTen.Text.Trim());

            return strNewFileName;
        }

        /// <summary>
        /// Remove the Vietnamese accent from the Name and Phap Danh.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static String Convert_VN_To_Eng(String str)
        {

            str = str.Replace("à", "a").Replace("á", "a").Replace("ạ", "a").Replace("ả", "a").Replace("ã", "a").Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ậ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ặ", "a").Replace("ẳ", "a").Replace("ẵ", "a");
            str = str.Replace("è", "e").Replace("é", "e").Replace("ẹ", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ệ", "e").Replace("ể", "e").Replace("ễ", "e");
            str = str.Replace("ì", "i").Replace("í", "i").Replace("ị", "i").Replace("ỉ", "i").Replace("ĩ", "i");
            str = str.Replace("ò", "o").Replace("ó", "o").Replace("ọ", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ộ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ợ", "o").Replace("ở", "o").Replace("ỡ", "o");
            str = str.Replace("ù", "u").Replace("ú", "u").Replace("ụ", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ự", "u").Replace("ử", "u").Replace("ữ", "u");
            str = str.Replace("ỳ", "y").Replace("ý", "y").Replace("ỵ", "y").Replace("ỷ", "y").Replace("ỹ", "y");
            str = str.Replace("đ", "d");

            str = str.Replace("À", "A").Replace("Á", "A").Replace("Ạ", "A").Replace("Ả", "A").Replace("Ã", "A").Replace("Â", "A").Replace("Ầ", "A").Replace("Ấ", "A").Replace("Ậ", "A").Replace("Ẩ", "A").Replace("Ẫ", "A").Replace("Ă", "A").Replace("Ằ", "A").Replace("Ắ", "A").Replace("Ặ", "A").Replace("Ẳ", "A").Replace("Ẵ", "A");
            str = str.Replace("È", "E").Replace("É", "E").Replace("Ẹ", "E").Replace("Ẻ", "E").Replace("Ẽ", "E").Replace("Ê", "E").Replace("Ề", "E").Replace("Ế", "E").Replace("Ệ", "E").Replace("Ể", "E").Replace("Ễ", "E");
            str = str.Replace("Ì", "I").Replace("Í", "I").Replace("Ị", "I").Replace("Ỉ", "I").Replace("Ĩ", "I");
            str = str.Replace("Ò", "O").Replace("Ó", "O").Replace("Ọ", "O").Replace("Ỏ", "O").Replace("Õ", "O").Replace("Ô", "O").Replace("Ồ", "O").Replace("Ố", "O").Replace("Ộ", "O").Replace("Ổ", "O").Replace("Ỗ", "O").Replace("Ơ", "O").Replace("Ờ", "O").Replace("Ớ", "O").Replace("Ợ", "O").Replace("Ở", "O").Replace("Ỡ", "O");
            str = str.Replace("Ù", "U").Replace("Ú", "U").Replace("Ụ", "U").Replace("Ủ", "U").Replace("Ũ", "U").Replace("Ư", "U").Replace("Ừ", "U").Replace("Ứ", "U").Replace("Ự", "U").Replace("Ử", "U").Replace("Ữ", "U");
            str = str.Replace("Ỳ", "Y").Replace("Ý", "Y").Replace("Ỵ", "Y").Replace("Ỷ", "Y").Replace("Ỹ", "Y");
            str = str.Replace("Đ", "D");

            //remove special char
            str = str.Replace("̣", "");
            str = str.Replace("̃", "");

            return str;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveEdit();
        }
        private void dataGridView_Click(object sender, EventArgs e)
        {
            ClearAllSearchTextBoxFields();
            ClearAllSearchTabFieldsBackGroundColor();
            DisplayImage();
        }
        private void dataGridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            DisplayImage();
        }
        /// <summary>
        /// Delete HL record if user press on "Delete" button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (datasGridView.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show("Do you want to delete this HL", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        
                        datasGridView.Rows.RemoveAt(datasGridView.SelectedRows[0].Index);

                        hLTblBindingSource.EndEdit();
                        hL_TblTableAdapter.Update(hL_DBDataSet.HL_Tbl);  //Update the HL_DB: C:\DP_Project\Kiem_HL\Kiem_HL\HL_DBDataSet.xsd(63):

                        hL_TblTableAdapter.Fill(hL_DBDataSet.HL_Tbl);
                        datasGridView.Focus();
                        DisplayImage();
                        txtsSearch.Text = "Deleted";
                    }
                    else
                    {
                        txtsSearch.Text = "NOT deleted";
                    }
                }
                else
                {
                    MessageBox.Show("Please select the entire row '>' to delete HL.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                DisplayImage();
            }
        }

        private void dataGridView_KeyUp(object sender, KeyEventArgs e)
        {
            DisplayImage();
        }

        private void dataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DisplayImage();
        }

        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            DisplayImage();
        }

        /// <summary>
        /// Do Search when user hit "Enter" key in Search textBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)   //Enter key press
            {
                Search();
            }
        }
        private void txtSearch_MouseEnter(object sender, EventArgs e)
        {
            txtsSearch.Text = "";
            lblsErrorMsg.Text = "";
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Home) //(e.KeyCode == Keys.Back)
            {
                txtsSearch.Text = "";
                txtsFNumbSearch.Text = "";
                lblsErrorMsg.Text = "";
            }
        }

        private void btnsSearch_Click(object sender, EventArgs e)
        {
            Search();
        }
        
        /// <summary>
        /// clear Search HL picBoxHL
        /// </summary>
        private void ClearSearchPicBox()
        {
            //release Picturebox's image in order for the File.Move() to work!
            if (picsBoxHL.Image != null)
            {
                picsBoxHL.Image.Dispose();
                picsBoxHL.Image = null;
            }
        }
        private void SaveEdit()
        {
            try
            {   //1 Ngô Dung Pd Nhật Đoan 1919-2002.jpg
                lblsErrorMsg.Text = "";
                string strOrigFileName = txtsFilename.Text.Trim();

                txtsFilename.Text = ReFormatImageFileName();         //set new image FileName
                string strEnglishHoTen = Convert_VN_To_Eng(txtsHoTen.Text.Trim());
                string strEnglishPhapDanh = Convert_VN_To_Eng(txtsPhapDanh.Text.Trim());

                if (strEnglishHoTen.Length > 0)
                    lblsFullname.Text = strEnglishHoTen;

                if (strEnglishPhapDanh.Length > 0)
                    lblsFullPhapDanh.Text = strEnglishPhapDanh;

                txtsDtUpdate.Text = DateTime.Now.ToString("G");

                //RENAME the imagefilename in imgFolderDonePath before save image filename in HL_DB
                if (lblsOrigFilename.Text != txtsFilename.Text.Trim())
                {
                    ClearSearchPicBox();
                    Util.FileSavAs(_ImgFolderDonePath, _ImgFolderDonePath, strOrigFileName, txtsFilename.Text.Trim());
                    DisplayImage();
                }

                hLTblBindingSource.EndEdit();
                hL_TblTableAdapter.Update(this.hL_DBDataSet.HL_Tbl);  //Update the HL_DB

                if (!txtsViTriHinh.Text.Equals(lblsOrigViTriHinh.Text))
                {
                    txtsViTriHinh.ForeColor = System.Drawing.Color.Yellow;        //Text
                    txtsViTriHinh.BackColor = System.Drawing.Color.Purple;        //Background
                    lblsOrigViTriHinh.Text = txtsViTriHinh.Text;
                }
                lblsErrorMsg.Text = "Saved!!";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblsErrorMsg.Text = ex.Message;
                hLTblBindingSource.ResetBindings(false);
            }
        }

        private bool SearchWordAsIs(string strSearch)
        {
            bool bDataFound = false;
            string strRowFilter = string.Empty;

            strSearch = strSearch.Trim().Replace("%')", "");              //"Hunz"
            char[] aSearch = strSearch.ToCharArray();                     //('H','u','n','z')

            for (int y = aSearch.Length - 1; y > 0; --y)
            {
                if (!bDataFound)
                {
                    strSearch = "";
                    for (int z = 0; z < y; z++)
                        strSearch += aSearch[z];                            //"Hun"

                    strRowFilter = "(Fullname like '%" + strSearch + "%')";      //"(Fullname like '%Hun%')"

                    if (strRowFilter.Length > 0 && !bDataFound)
                    {
                        hL_DBDataSet.HL_Tbl.CaseSensitive = false;  //search (upper/lower) cases
                        hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                        hL_DBDataSet.HL_Tbl.DefaultView.Sort = "HoTen ASC";
                        if (hL_DBDataSet.HL_Tbl.DefaultView.Count > 0)
                        {
                            //"txtSearch" == (%Hun%) and data found in ["Fullname"], Bind Data Source and return results
                            hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;
                            datasGridView.DataSource = hLTblBindingSource;
                            bDataFound = true;
                            break;
                        }
                    }
                }
                else
                    break;
            }
            return bDataFound;
        }
        private bool SearchStringAsIs(string[] astrSearch)
        {
            bool bDataFound = false;

            for (int i = astrSearch.Length - 1; i >= 0; --i)
            {
                if (!bDataFound)
                {
                    string strNewSearch = "";
                    string strRowFilter = "";
                    string strSearch = " " + astrSearch[i] + "%')";                     //"Hung%')"

                    for (int x = 0; x < i; x++)
                        strNewSearch += " " + astrSearch[x];                            //" Diep The"

                    strRowFilter = "(Fullname like '" + strNewSearch.TrimStart() + strSearch;      //"(Fullname like 'Diep The Hung%')"

                    if (strRowFilter.Length > 0 && !bDataFound)
                    {
                        hL_DBDataSet.HL_Tbl.CaseSensitive = false;  //search (upper/lower) cases
                        hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                        hL_DBDataSet.HL_Tbl.DefaultView.Sort = "HoTen ASC";
                        if (hL_DBDataSet.HL_Tbl.DefaultView.Count > 0)
                        {
                            //"txtSearch" == (Diep The Hung%) and data found in ["Fullname"], Bind Data Source and return results
                            hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;
                            datasGridView.DataSource = hLTblBindingSource;
                            bDataFound = true;
                            break;
                        }
                        else
                        {
                            strSearch = strSearch.Trim().Replace("%')", "");              //"Hunz"
                            char[] aSearch = strSearch.ToCharArray();                     //('H','u','n','z')

                            for (int y = aSearch.Length - 1; y > 0; --y)
                            {
                                if (!bDataFound)
                                {
                                    strSearch = "";
                                    for (int z = 0; z < y; z++)
                                        strSearch += aSearch[z];                            //"Hun"

                                    strRowFilter = "(Fullname like '" + strNewSearch.TrimStart() + " " + strSearch + "%')";      //"(Fullname like 'Diep The Hun%')"

                                    if (strRowFilter.Length > 0 && !bDataFound)
                                    {
                                        hL_DBDataSet.HL_Tbl.CaseSensitive = false;  //search (upper/lower) cases
                                        hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                                        hL_DBDataSet.HL_Tbl.DefaultView.Sort = "HoTen ASC";
                                        if (hL_DBDataSet.HL_Tbl.DefaultView.Count > 0)
                                        {
                                            //"txtSearch" == (Diep The Hun%) and data found in ["Fullname"], Bind Data Source and return results
                                            hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;
                                            datasGridView.DataSource = hLTblBindingSource;
                                            bDataFound = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                    break;
                            }
                        }
                    }

                }
                else
                    break;
            }

            return bDataFound;
        }
        private void Search()
        {
            try
            {
                lblsErrorMsg.Text = "";
                string strRowFilter = "FileName like '%'";

                if (string.IsNullOrEmpty(txtsSearch.Text.Trim()) && string.IsNullOrEmpty(txtsFNumbSearch.Text.Trim()))
                {
                    //DataTable dtHL = hL_DataSet.HL_Tbl;
                    ////dtHL.DefaultView.RowFilter = "";        //Remove all Filter
                    //hLTblBindingSource.DataSource = hL_DataSet.HL_Tbl.DefaultView;    //dtHL.DefaultView;

                    //Search By No Filter
                    //===================================================================================================================
                    hL_DBDataSet.HL_Tbl.CaseSensitive = false;
                    hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                    hL_DBDataSet.HL_Tbl.DefaultView.Sort = "HoTen ASC";
                    hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;

                    datasGridView.DataSource = hLTblBindingSource;
                    datasGridView.Focus();
                }
                else
                {
                    //strRowFilter = "FileName like '%" + txtSearch.Text.Trim() + "%'";
                    //strRowFilter = "(FileName like '%" + txtSearch.Text.Trim() + "%') OR (Fullname like '%" + txtSearch.Text.Trim() + "%')";

                    if (!string.IsNullOrEmpty(txtsFNumbSearch.Text.Trim()))
                    {
                        //===================================================================================================================
                        //Search By: txtFileNumbSearch      
                        //===================================================================================================================
                        //1. Search by ["FileNumber"]          - FileNumber like '1917%'

                        //strRowFilter = "(FileNumber like '%" + txtFileNumbSearch.Text.Trim() + "%')";     //08/03/2021 Changed search logic
                        strRowFilter = "(FileNumber like '" + txtsFNumbSearch.Text.Trim() + "%')";

                        if (cksPhapDanh.Checked)
                            strRowFilter = "(FileNumber = '" + txtsFNumbSearch.Text.Trim() + "')";

                        hL_DBDataSet.HL_Tbl.CaseSensitive = false;
                        hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                        hL_DBDataSet.HL_Tbl.DefaultView.Sort = "FileNumber ASC";
                        hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;

                        datasGridView.DataSource = hLTblBindingSource;
                        datasGridView.Focus();
                    }
                    else
                    {
                        //08/03/2021 Changed search logic
                        //strRowFilter = "(FileNumber like '%" + txtSearch.Text.Trim() + "%') OR (Fullname like '%" + txtSearch.Text.Trim() + "%')";
                        
                        //===================================================================================================================
                        //Search By: txtSearch
                        //===================================================================================================================
                        //1. Search by ["FileNumber"]          - FileNumber like 'Diep The Hung%'
                        strRowFilter = "(FileNumber like '" + txtsSearch.Text.Trim() + "%')";

                        if (cksPhapDanh.Checked)
                            strRowFilter = "(FileNumber = '" + txtsSearch.Text.Trim() + "')";

                        hL_DBDataSet.HL_Tbl.CaseSensitive = false;  //search (upper/lower) cases
                        hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                        hL_DBDataSet.HL_Tbl.DefaultView.Sort = "FileNumber ASC";
                        if (hL_DBDataSet.HL_Tbl.DefaultView.Count > 0)
                        {
                            //"txtSearch" == (Diep The Hung) and data found in ["FileNumber"], Bind Data Source and return results
                            hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;
                            datasGridView.DataSource = hLTblBindingSource;
                        }
                        else
                        {
                            //===================================================================================================================
                            //2. Search by ["Fullname"]          
                            string[] astrSearch = txtsSearch.Text.Trim().Replace("  "," ").Split(' ');

                            if (astrSearch.Length == 1)
                            {
                                //===================================================================================================================
                                //A. Search by ["Fullname"]          - Search value has only One Word   - Fullname like '%Hung%'
                                strRowFilter = "(Fullname like '%" + txtsSearch.Text.Trim().Replace("  ", " ") + "%')";
                                hL_DBDataSet.HL_Tbl.CaseSensitive = false;  //search (upper/lower) cases
                                hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;
                                hL_DBDataSet.HL_Tbl.DefaultView.Sort = "HoTen ASC";
                                if (hL_DBDataSet.HL_Tbl.DefaultView.Count > 0)
                                {
                                    //"txtSearch" == (Diep The Hung%) and data found in ["Fullname"], Bind Data Source and return results
                                    hLTblBindingSource.DataSource = hL_DBDataSet.HL_Tbl;
                                    datasGridView.DataSource = hLTblBindingSource;
                                }
                                else
                                {
                                    bool bDataFound = SearchWordAsIs(txtsSearch.Text.Trim().Replace("  ", " "));
                                }
                            }
                            else
                            {
                                //Ex: Hung The Diep
                                bool bDataFound = SearchStringAsIs(astrSearch);

                                if (!bDataFound)
                                {
                                    int i = astrSearch.Length;

                                    string[] astrNewSearch = new string[i];

                                    int iy = 0;
                                    for (int ix = astrSearch.Length - 1; ix >= 0; --ix)
                                    {
                                        astrNewSearch[iy] = astrSearch[ix];
                                        iy++;
                                    }

                                    //Ex: Diep The Hung
                                    bDataFound = SearchStringAsIs(astrNewSearch);
                                }
                            }
                        }
                    }

                }

                DisplayImage();     //last call for DisplayImage() Search
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                hLTblBindingSource.ResetBindings(false);
            }
        }

        /// <summary>
        /// Save record when user hit "Enter" key in Location textBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void txtLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Location Entered and Save()
            if (e.KeyChar == (char)13)   //Enter key press
            {
                SaveEdit();
            }
        }

        private void txtFileNumbSearch_MouseClick(object sender, MouseEventArgs e)
        {
            txtsFNumbSearch.Text = "";
            txtsSearch.Text = "";
        }

        private void txtFileNumbSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)   //Enter key press
            {
                Search();
            }
        }

        private void txtSearch_MouseClick(object sender, MouseEventArgs e)
        {
            txtsFNumbSearch.Text = "";
            txtsSearch.Text = "";
            lblsErrorMsg.Text = "";
        }

        private void txtFileNumbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Home) //if (e.KeyCode == Keys.Back)
            {
                txtsSearch.Text = "";
                txtsFNumbSearch.Text = "";
                lblsErrorMsg.Text = "";
            }
        }


        private void ckExactMatch_Click(object sender, EventArgs e)
        {
            Search();
        }
        private void txtLocation_MouseClick(object sender, MouseEventArgs e)
        {
            if (txtsViTriHinh.Text.Trim().Length > 0)
            {
                txtsViTriHinh.ForeColor = System.Drawing.Color.Blue;
                lblsOrigViTriHinh.Text = txtsViTriHinh.Text.Trim();
            }
            else
                lblsOrigViTriHinh.Text = "";
            lblsErrorMsg.Text = "";
        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {
            txtsViTriHinh.ForeColor = System.Drawing.Color.Blue;
            lblsErrorMsg.Text = "";
        }

        #endregion - Search Tab Section

        /******************************************** TAB CONTROLS SECTION **********************************************/
        /// <summary>
        /// TABS Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)         //Search Tab
            {
                txtsSearch.Text = "";
                txtsFNumbSearch.Text = "";
                //Search();

                this.hL_TblTableAdapter.Fill(this.hL_DBDataSet.HL_Tbl);
                datasGridView.Focus();
                DisplayImage();
            }
            else if (tabControl1.SelectedIndex == 1)    //Insert Tab
            {
                BindingHLImgListbox();
            }
        }

        /// <summary>
        /// When TabPage.Selected change, then change the Tab Header Font to Bold and Red
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //http://vbcity.com/blogs/xtab/archive/2014/09/14/windows-forms-how-to-bold-the-header-of-a-selected-tab.aspx
            //The Windows Form
            //Start by dragging a TabControl onto the Windows Form.
            //Then in the "Properties" pane for the TabControl find the "DrawMode" property and change it to "OwnerDrawFixed:"

            //http://vbcity.com/blogs/xtab/archive/2014/09/16/tabcontrol-how-to-change-color-and-size-of-the-selected-tab.aspx

            //Identify which TabPage is currently selected
            TabPage tabPageSelected = tabControl1.TabPages[e.Index];

            //Get the area of the header of this TabPage
            Rectangle headerRect = tabControl1.GetTabRect(e.Index);

            //Create two Brushes to paint the Text
            SolidBrush blackTextBrush = new SolidBrush(Color.Black);
            SolidBrush redTextBrush = new SolidBrush(Color.Red);

            //Set the Alignment of the Text
            StringFormat strFmt = new StringFormat();
            strFmt.Alignment = StringAlignment.Center;
            strFmt.LineAlignment = StringAlignment.Center;

            //Paint the Text using the appropriate Bold and Color setting
            if (Convert.ToBoolean(e.State) && Convert.ToBoolean(DrawItemState.Selected))
            {
                Font boldFont = new Font(tabControl1.Font.Name, tabControl1.Font.Size, FontStyle.Bold);
                e.Graphics.DrawString(tabPageSelected.Text, boldFont, redTextBrush, headerRect, strFmt);
            }
            else
                e.Graphics.DrawString(tabPageSelected.Text, e.Font, blackTextBrush, headerRect, strFmt);

            //Job is done: dispose of the brushes
            blackTextBrush.Dispose();
            redTextBrush.Dispose();

        }
        
        /******************************************** INSERT TAB SECTION *********************************************/
        #region - Insert Tab Section

        private void BindingHLImgListbox()
        {

            try
            {
                if (lstiBoxHLImg.Items.Count > 0)
                    lstiBoxHLImg.Items.Clear();

                //#1. get the new HL images list, then rename the files if necessary.
                ArrayList aLstSourceFilename = Util.SearchFileName(_ImgFolderPath, "*.jpg");

                foreach (string strOrigFilename in aLstSourceFilename)
                {
                    string strNewFileName = Util.RenameFile(strOrigFilename);

                    if (!strNewFileName.Equals(strOrigFilename))
                        Util.FileSavAs(_ImgFolderPath, _ImgFolderPath, strOrigFilename, strNewFileName);
                }

                //#2. get the list of HL images again after checking filename
                ArrayList aLstHLImg = Util.SearchFileName(_ImgFolderPath, "*.jpg");

                //DO NOT use lstBoxHLImg.DataSource = aLstHLImg;
                //because you can't use lstBoxHLImg.Items.RemoveAt(ixSelected);
                foreach (string strHL_Img in aLstHLImg)
                {
                    lstiBoxHLImg.Items.Add(strHL_Img);          //MUST USE: lstBoxHLImg.Items.Add(strHL_Img) IN ORDER FOR: lstBoxHLImg.Items.RemoveAt(ixSelected);    TO WORK!!
                }

                if (aLstHLImg.Count > 0)
                    lstiBoxHLImg.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                string strErr = ex.ToString();
            }

        }


        /******************************************************************************************************************/
        private void DisplayImageInsert()
        {
            lbliErrorMsg.Text = "";

            if (txtiFilename.Text.Trim().Length > 0)
            {
                //string strImgFileNameDonePath = _ImgFolderDonePath + txtOrigImgFileName.Text;
                string strImgFileNamePath = _ImgFolderPath + txtiFilename.Text;

                if (File.Exists(strImgFileNamePath))
                {
                    Bitmap bitImageFile = new Bitmap(strImgFileNamePath);

                    if (piciBoxHL == null)
                    {
                        piciBoxHL = new PictureBox();
                        piciBoxHL.Location = new Point(17, 230);
                    }
                    piciBoxHL.SizeMode = PictureBoxSizeMode.StretchImage;    //in order to have any image "resize" to fit a picturebox, you must set PictureBoxSizeMode.StretchImage 
                    piciBoxHL.Width = 440;   // 300;   // 580;
                    piciBoxHL.Height = 450;  // 260;   // 500;
                    piciBoxHL.Image = (Image)bitImageFile;
                    bitImageFile = null;
                }
                else
                {
                    piciBoxHL.Image = null;  //Hinh Not found.
                    lbliErrorMsg.Text = "Hinh Not Found.";
                }
            }
            else
            {
                piciBoxHL.Image = null;
            }
        }

        private void DisplayDupImage(string strDupImgFilename)
        {
            lbliErrorMsg.Text = "";

            if (strDupImgFilename != "")
            {
                txtiDupImgFilename.Text = strDupImgFilename;
                string strDupImgFileNamePath = _ImgFolderDonePath + strDupImgFilename;

                if (File.Exists(strDupImgFileNamePath))
                {
                    Bitmap bitDupImageFile = new Bitmap(strDupImgFileNamePath);

                    piciBoxHLDup.SizeMode = PictureBoxSizeMode.StretchImage;    //in order to have any image "resize" to fit a picturebox, you must set PictureBoxSizeMode.StretchImage 
                    piciBoxHLDup.Width = 182;
                    piciBoxHLDup.Height = 185;
                    piciBoxHLDup.Image = (Image)bitDupImageFile;
                    bitDupImageFile = null;
                }
                else
                {
                    piciBoxHLDup.Image = null;  //Hinh Not found.
                    lbliErrorMsg.Text = "Hinh Not Found.";
                }
            }
            else
            {
                piciBoxHLDup.Image = null;
            }
        }

        private string GetDupHL(string strNewImgFilename, string strNewHL_Fullname)
        {
            string strDupHL = "";
            string strRowFilter = "";   //"(FileName like '%" + strNewImgFilename + "%') OR (Fullname like '%" + (strNewHL_Fullname != "Sen" ? strNewHL_Fullname: strNewImgFilename) + "%')";

            if (strNewHL_Fullname.ToUpper() == "SEN" || strNewHL_Fullname.ToUpper() == "A DI DA PHAT" || strNewHL_Fullname.ToUpper() == "BAI VI CHU HOA" || strNewHL_Fullname.ToUpper() == "VO DANH")
                strRowFilter = "(FileName like '%" + strNewImgFilename + "%') OR (Fullname like '%" + strNewImgFilename + "%')";
            else
                strRowFilter = "(FileName like '%" + strNewImgFilename + "%')";  // OR (Fullname like '%" + strNewHL_Fullname + "%')";

            hL_DBDataSet.HL_Tbl.CaseSensitive = false;  //search (upper/lower) cases

            hL_DBDataSet.HL_Tbl.DefaultView.RowFilter = strRowFilter;

            DataView dataView = hL_DBDataSet.HL_Tbl.DefaultView;

            if (dataView.Count > 0)
            {
                strDupHL = dataView[0]["FileName"].ToString();
            }

            return strDupHL;
        }
        
        /// <summary>
        /// clear Insert HL picBox
        /// </summary>
        private void ClearInsertPicBox()
        {
            //release Picturebox's image in order for the File.Move() to work!
            if (piciBoxHL.Image != null)
            {
                piciBoxHL.Image.Dispose();
                piciBoxHL.Image = null;
            }
        }
        private void ClearDupPicBox()
        {
            //release Picturebox's Duplicate image in order for the File.Move() to work!
            if (piciBoxHLDup.Image != null)
            {
                txtiDupImgFilename.Text = "";
                piciBoxHLDup.Image.Dispose();
                piciBoxHLDup.Image = null;
            }
        }

        private void txtImgFilename_MouseDown(object sender, MouseEventArgs e)
        {
            if (_iClickCount <= 3)
            {
                //MouseDown     - occurs when a mouse button is pressed
                //MouseClick    - occurs when a mouse is pressed and released

                _ixSelectLength = txtiFilenameParsing.SelectionStart;

                string strFullFileName = txtiFilenameParsing.Text;
                string strPhapDanh = "";

                switch (_iClickCount)
                {
                    case 0: //Filenumber
                        txtiFileNumber.Text = strFullFileName.Substring(0, _ixSelectLength).Trim();
                        txtiFilenameParsing.Text = strFullFileName.Substring(_ixSelectLength).Trim();
                        if (txtiFilenameParsing.Text == "")
                            txtiHoTen.Text = "Sen";  //54.3.jpg (Sen) must set txtHTen.Text = "Sen"
                        break;
                    case 1: //HoTen
                        txtiHoTen.Text = strFullFileName.Substring(0, _ixSelectLength).Trim();
                        txtiFilenameParsing.Text = strFullFileName.Substring(_ixSelectLength).Trim();
                        break;
                    case 2: //Phap Danh or Sinh Tu
                        strPhapDanh = strFullFileName.Substring(0, _ixSelectLength).Trim();
                        if (strPhapDanh.ToUpper().IndexOf("PD ") == 0)
                            txtiPhapDanh.Text = strPhapDanh.Substring(3);
                        else
                            txtiSinh.Text = strPhapDanh;
                        txtiFilenameParsing.Text = strFullFileName.Substring(_ixSelectLength).Trim();
                        break;
                    case 3: //Phap Danh or Sinh Tu
                        string strTemp = strFullFileName.Substring(0, _ixSelectLength).Trim();
                        if (strTemp.ToUpper().IndexOf("PD ") == 0)
                            txtiPhapDanh.Text = strTemp.Substring(3);
                        else
                        {
                            if (txtiSinh.Text != "")
                                txtiSinh.Text = txtiSinh.Text + " " + strTemp;
                            else
                                txtiSinh.Text = strTemp;
                        }
                        txtiFilenameParsing.Text = strFullFileName.Substring(_ixSelectLength).Trim();
                        break;
                }

                _iClickCount++;
            }
        }
        
        private void ClearDupHLPicBox()
        {
            //release Picturebox's Duplicate image in order for the File.Move() to work!
            if (piciBoxHLDup.Image != null)
            {
                txtiDupImgFilename.Text = "";
                piciBoxHLDup.Image.Dispose();
                piciBoxHLDup.Image = null;
            }
        }
        private void lstBoxHLImg_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _ixSelectLength = 0;    //reset when SelectedIndexChanged
                _iClickCount = 0;       //reset when SelectedIndexChanged
                ClearInsertPicBox();

                txtiViTriHinh.Text = "";
                txtiFileNumber.Text = "";
                txtiHoTen.Text = "";
                txtiPhapDanh.Text = "";
                txtiSinh.Text = "";
                lbliFullname.Text = "";
                btniSave.Visible = true;

                if (lstiBoxHLImg.SelectedIndex >= 0)
                {
                    txtiFilename.Text = lstiBoxHLImg.SelectedItem.ToString();
                    txtiFilenameParsing.Text = lstiBoxHLImg.SelectedItem.ToString().Replace(".jpg", "");
                    txtiFilenameParsing.Enabled = true;

                    DisplayImageInsert();            //first call for DisplayImageInsert()

                    if (txtiFilenameParsing.Text == "9999z EndOfFile")
                    {
                        txtiFilename.Text = "";
                        txtiFilenameParsing.Text = "No HL image found!";
                        txtiFilenameParsing.Enabled = false;
                        btniSave.Visible = false;
                    }
                    //////else
                    //////{
                    //////    txtImgFilename.Text = "No HL image found!";
                    //////    txtImgFilename.Enabled = false;
                    //////    btnImgSave.Visible = false;
                    //////}
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                lbliErrorMsg.Text = ex.ToString();
            }

        }
        private void Display_HLName_Image()
        {
            //Not use

            //dataGridView.ClearSelection();
            //dataGridView.CurrentCell = dataGridView.Rows[0].Cells[0];
            //dataGridView.CurrentCell.Selected = true;

            txtiFilename.Text = lstiBoxHLImg.SelectedItem.ToString();
            txtiFilenameParsing.Text = lstiBoxHLImg.SelectedItem.ToString().Replace(".jpg", "");

            DisplayImageInsert();

        }

        /// <summary>
        /// When txtHTen.Text = ""; must set txtHTen.Text = "Sen"
        /// txtFNumb.Text + txtHTen.Text + txtPDanh.Text + txtSiTu.Text. 
        /// When HL Name or PhapDanh changed, must change ImageFileName as well to reflex the HL Name and or PhapDanh 
        /// ex:(from: 1 Ngo Dung Pd Nhat Doan 1919-2002.jpg  >>>  1 Ngô Dung Pd Nhật Đoan 1919-2002.jpg 
        /// </summary>
        /// <returns></returns>
        private string ReFormatInsertImageFileName()
        {
            string strNewImgFileName = string.Format("{0}.jpg", txtiFileNumber.Text);   //54.2.jpg

            if (txtiHoTen.Text.Trim().Length > 0 && txtiPhapDanh.Text.Trim().Length > 0 && txtiSinh.Text.Trim().Length > 0)
                strNewImgFileName = string.Format("{0} {1} Pd {2} {3}.jpg", txtiFileNumber.Text, txtiHoTen.Text.Trim(), txtiPhapDanh.Text.Trim(), txtiSinh.Text.Trim());
            else if (txtiHoTen.Text.Trim().Length > 0 && txtiPhapDanh.Text.Trim().Length > 0)
                strNewImgFileName = string.Format("{0} {1} Pd {2}.jpg", txtiFileNumber.Text, txtiHoTen.Text.Trim(), txtiPhapDanh.Text.Trim());
            else if (txtiHoTen.Text.Trim().Length > 0 && txtiSinh.Text.Trim().Length > 0)
                strNewImgFileName = string.Format("{0} {1} {2}.jpg", txtiFileNumber.Text, txtiHoTen.Text.Trim(), txtiSinh.Text.Trim());
            else if (txtiHoTen.Text.Trim().Length > 0)
                strNewImgFileName = string.Format("{0} {1}.jpg", txtiFileNumber.Text, txtiHoTen.Text.Trim());
            else if (txtiHoTen.Text.Trim().Length <= 0)
            {
                txtiHoTen.Text = "Sen";
                strNewImgFileName = string.Format("{0} {1}.jpg", txtiFileNumber.Text, txtiHoTen.Text.Trim());
            }
            return strNewImgFileName;
        }
        private void btnImgSave_Click(object sender, EventArgs e)
        {
            if (txtiHoTen.Text.Trim() != "")
            {
                try
                {
                    string strOrigImgFileName = txtiFilename.Text.Trim();
                    string strNewImgFileName = ReFormatInsertImageFileName();               //set new image FileName

                    string strEnglishHoTen = Convert_VN_To_Eng(txtiHoTen.Text.Trim());        //must remove Vietnamese accent from HoTen
                    string strEnglishPhapDanh = Convert_VN_To_Eng(txtiPhapDanh.Text.Trim());    //must remove Vietnamese accent from PhapDanh

                    //Format HL's Fullname for searching purpose only
                    if (strEnglishHoTen.Length > 0 && strEnglishPhapDanh.Length > 0)
                        lbliFullname.Text = strEnglishHoTen + " PD " + strEnglishPhapDanh;
                    else if (strEnglishHoTen.Length > 0)
                        lbliFullname.Text = strEnglishHoTen;
                    else if (strEnglishPhapDanh.Length > 0)
                        lbliFullname.Text = "PD " + strEnglishPhapDanh;

                    
                    txtiFilename.Text = strNewImgFileName;            //rename the image filename

                    string strDupHLImgage = GetDupHL(txtiFilename.Text, lbliFullname.Text);

                    if (strDupHLImgage == "")
                    {
                        //Insert New HL into DB
                        hL_TblTableAdapter.Insert(txtiHoTen.Text, txtiPhapDanh.Text, txtiViTriHinh.Text, lbliFullname.Text, txtiFileNumber.Text.Trim(',').TrimEnd('-').TrimEnd('/').TrimEnd('.'), 
                            txtiFilename.Text, DateTime.Now, null, txtiPhapDanh.Text.Trim(), txtiTuAL.Text.Trim().Trim(',').TrimEnd('-').TrimEnd('/').TrimEnd('.'), 
                            txtiSinh.Text.Trim(',').TrimEnd('-').TrimEnd('/').TrimEnd('.'), txtiTu.Text.Trim().Trim(',').TrimEnd('-').TrimEnd('/').TrimEnd('.'), 
                            txtiViTriCot.Text.Trim(), txtiNote.Text.Trim(), null, null, null, null, null);

                        hLTblBindingSource.EndEdit();
                        hL_TblTableAdapter.Update(this.hL_DBDataSet.HL_Tbl);  //Update the HL_DB table

                        //release Image filename from HL ListBox in order for the File.Move() to work!
                        lstiBoxHLImg.Items.RemoveAt(lstiBoxHLImg.SelectedIndex);

                        //release Picturebox's image in order for the File.Move() to work!
                        ClearInsertPicBox();

                        //Rename the imagefilename in imgFolderPath and then MOVE it to imgFolderDonePath after save HL data and image filename in HL_DB
                        if (!strOrigImgFileName.Equals(strNewImgFileName))
                            Util.FileSavAsAndMove(_ImgFolderPath, _ImgFolderDonePath, strOrigImgFileName, strNewImgFileName);
                        else
                            Util.MoveFile(_ImgFolderPath, _ImgFolderDonePath, strNewImgFileName);

                        if (lstiBoxHLImg.Items.Count >= 1)
                            lstiBoxHLImg.SelectedIndex = 0;     //refresh HL image and move to the next HL data
                        else
                            lstiBoxHLImg.SelectedIndex = -1;
                    }
                    else
                    {
                        DisplayDupImage(strDupHLImgage);

                        if (MessageBox.Show("Duplicate Huong Linh Found.\r\n\r\n" + strDupHLImgage + "\r\n\r\nDo you want to Archive this HL?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            //release Image filename from HL ListBox in order for the File.Move() to work!
                            lstiBoxHLImg.Items.RemoveAt(lstiBoxHLImg.SelectedIndex);

                            //release Picturebox's image in order for the File.Move() to work!
                            ClearInsertPicBox();

                            //release Picturebox's Duplicate image in order for the File.Move() to work!
                            ClearDupPicBox();

                            if (strOrigImgFileName != strNewImgFileName)
                                Util.FileSavAsAndMove(_ImgFolderPath, _ImgFolderArchivePath, strOrigImgFileName, strNewImgFileName);
                            else
                                Util.MoveFile(_ImgFolderPath, _ImgFolderArchivePath, strNewImgFileName);

                            if (lstiBoxHLImg.Items.Count >= 1)
                                lstiBoxHLImg.SelectedIndex = 0;     //refresh HL image and move to the next HL data
                            else
                                lstiBoxHLImg.SelectedIndex = -1;
                        }
                        else
                        {
                            //clear Duplicate image
                            ClearDupPicBox();
                        }
                    }
                }
                catch (System.Data.OleDb.OleDbException ex) //(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    hLTblBindingSource.CancelEdit();            //roll back
                    hLTblBindingSource.ResetBindings(false);
                }
            }
            else
            {
                lbliErrorMsg.Text = "Please select Filenumber, HoTen, ... before click SAVE!";
            }
        }

        #endregion - Insert Tab Section

        /******************************************** Send Text Messages with TWILIO *********************************************/
        #region Send Text message via Twilio API
        private void btnsTextLoc_Click(object sender, EventArgs e)
        {
            SendText_SMS();

            //SendTextAndImage_MMS();
        }

        private void SendText_SMS()
        {
            try
            {
                TwilioClient.Init(_Twilio_Acct_SID, _Twilio_Auth_Token);

                string strBodyMessage = txtsHoTen.Text + " - Hình trên tường - " + txtsViTriHinh.Text;

                // Send the message
                var message = MessageResource.Create(
                    body: strBodyMessage,
                    from: new Twilio.Types.PhoneNumber("+19785848089"),
                    to: new Twilio.Types.PhoneNumber("+16266171436")
                );

                //must update ckTextOptOut = 0; and SAVE();
                ckTextOptOut.Checked = false;

                MessageBox.Show("Text Sent");   //MessageBox.Show(message.Sid);     //Console.WriteLine(message.Sid);
            }
            catch (Exception ex)
            {
                //when user responde "STOP" Twilio will automatically handles STOP, HELP, START
                //if user responded "STOP", the message will raise an exception with below message.
                if (ex.Message.Equals("Attempt to send to unsubscribed recipient"))
                {
                    //must update ckTextOptOut = 1; and SAVE();
                    ckTextOptOut.Checked = true;
                    MessageBox.Show("User opt-out of TEXT MESSAGING service!");
                }
            }
        }

        private void SendTextAndImage_MMS()
        {

            TwilioClient.Init(_Twilio_Acct_SID, _Twilio_Auth_Token);

            //1. Send an MMS message : requires Media Image Url
            var mediaUrl = new[] {
                new Uri("https://demo.twilio.com/owl.png")          // The size limit for message media is 5MB.
            }.ToList();

            string strBodyMessage = txtsHoTen.Text + " - Hình trên tường - " + txtsViTriHinh.Text;

            // Send the message
            var message = MessageResource.Create(
                body: strBodyMessage,
                from: new Twilio.Types.PhoneNumber("+19785848089"),
                mediaUrl: mediaUrl,                                 //2. Send an MMS message : Specify Media Image Url
                to: new Twilio.Types.PhoneNumber("+16266171436")
            );

            MessageBox.Show("Text Sent");   //MessageBox.Show(message.Sid);     //Console.WriteLine(message.Sid);
        }
        //private void SendTextResponse_TwiML()
        //{
        //    ///Get this info from Twilio Phone Numbers - Active numbers
        //    ///Voice:       Webhook(POST):  https://demo.twilio.com/welcome/voice/
        //    ///Messaging:   Webhook(POST):  https://demo.twilio.com/welcome/sms/reply/

        //    TwilioClient.Init(_Twilio_Acct_SID, _Twilio_Auth_Token);

        //    string strBodyMessage = txtsHoTen.Text + " Hình trên tường bảng: " + txtsViTriHinh.Text;

        //    // Send the message
        //    var message = MessageResource.Create(
        //        body: strBodyMessage,
        //        from: new Twilio.Types.PhoneNumber("+19785848089"),
        //        to: new Twilio.Types.PhoneNumber("+16266171436")
        //    );

        //    MessageBox.Show("Text Sent");   //MessageBox.Show(message.Sid);     //Console.WriteLine(message.Sid);
        //}

        #endregion Send Text message via Twilio API

        /******************************************** Send Text Messages with Gmail *********************************************/
        //private void btnsTextInfo_Click(object sender, EventArgs e)
        //{
        //    //Send Text Message with Gmail
        //    try
        //    {
        //        ///Carrier destinations
        //        ///ATT: Compose a new email and use the recipient's 10-digit wireless phone number, followed by @txt.att.net. For example, 5551234567@txt.att.net.
        //        ///Verizon: Similarly, ##@vtext.com
        //        ///Sprint: ##@messaging.sprintpcs.com
        //        ///TMobile: ##@tmomail.net
        //        ///Virgin Mobile: ##@vmobl.com
        //        ///Nextel: ##@messaging.nextel.com
        //        ///Boost: ##@myboostmobile.com
        //        ///Alltel: ##@message.alltel.com
        //        ///EE: ##@mms.ee.co.uk (might support send without reply-to)
        //        ///

        //        string strPhoneNumber = "6266171436";
        //        MailMessage mail = new MailMessage();
        //        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

        //        mail.SubjectEncoding = System.Text.Encoding.UTF32;
        //        mail.BodyEncoding = System.Text.Encoding.UTF32;

        //        mail.From = new MailAddress("leanhdao5@gmail.com");

        //        /// *******************************************************************************
        //        /// *
        //        /// *  DO NOT TRY TO SEND TO ALL CARRIERS BECAUSE IT WILL HAVE 
        //        /// *  550 5.1.1 <6266171436@txt.att.net> recipient cannot be reached (#550)
        //        /// *
        //        /// *******************************************************************************
        //        /// mail.To.Add(new MailAddress(strPhoneNumber + "@txt.att.net"));
        //        /// mail.To.Add(new MailAddress(strPhoneNumber + "@vtext.com"));
        //        /// mail.To.Add(new MailAddress(strPhoneNumber + "@messaging.sprintpcs.com"));
        //        /// *******************************************************************************

        //        mail.To.Add("leanhdao5@gmail.com");
        //        mail.To.Add(new MailAddress(strPhoneNumber + "@tmomail.net"));

        //        //mail.To.Add(new MailAddress("6264549803@tmomail.net"));   //Anh Binh
        //        //mail.To.Add(new MailAddress("3109054361@vtext.com"));     // SC Chon Nhu

        //        mail.Subject = _AnnivSubject.Replace("[HoTen]",txtsHoTen.Text).Replace("[Tu]",txtsTu.Text);     //"Giỗ HL [HoTen] ngày [Tu]"

        //        mail.Body = _AnnivMessage + " - UTF32 - Send: " + DateTime.Now.ToString("G");  //"Xin gọi cho Chùa Diệu Pháp biết để chuẩn bị cơm cúng nếu quý vị đến. Chân thành cảm ơn. (626)614-0566"

        //        SmtpServer.Port = 587;
        //        SmtpServer.UseDefaultCredentials = false;
        //        SmtpServer.Credentials = new System.Net.NetworkCredential("leanhdao5@gmail.com", "osyguosauajfphbb");
        //        SmtpServer.EnableSsl = true;        //How To Generate App password: login to Google Account >> "Security" >> enable Two-step Verification
        //                                            //  - then "App passwords" >> a. Select app:    Mail
        //        SmtpServer.Send(mail);              //                            b. Select device: Windows Computer
        //        MessageBox.Show("mail Sent");       //                            c. Click on:      Generate >> and get the 16 character password and paste here.
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        /******************************************** NOTES SECTION *********************************************/
        #region - Notes Section

        //DateTime.Now.ToString("G") : 08/17/2000 16:32:32
        // d :08/17/2000
        // D :Thursday, August 17, 2000
        // f :Thursday, August 17, 2000 16:32
        // F :Thursday, August 17, 2000 16:32:32
        // g :08/17/2000 16:32
        // G :08/17/2000 16:32:32
        // m :August 17
        // r :Thu, 17 Aug 2000 23:32:32 GMT
        // s :2000-08-17T16:32:32
        // t :16:32
        // T :16:32:32
        // u :2000-08-17 23:32:32Z
        // U :Thursday, August 17, 2000 23:32:32
        // y :August, 2000
        // dddd, MMMM dd yyyy :Thursday, August 17 2000
        // ddd, MMM d "'"yy :Thu, Aug 17 '00
        // dddd, MMMM dd :Thursday, August 17
        // M/yy :8/00
        // dd-MM-yy :17-08-00

        /* To set Tab Order of controls on FORM */
        // On the FORM Design, click the VIEW menu >> select Tab Order
        // then Click the control
        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-set-the-tab-order-on-windows-forms?view=netframeworkdesktop-4.8

        /* Add a TabControl to a FORM */
        // On the Design tab, in the Controls group, click the TabControl tool.
        // This activates the tab-order selection mode on the form.
        // Click the controls sequentially to establish the tab order you want.
        // https://support.microsoft.com/en-us/office/create-a-tabbed-form-6869dee9-3ab7-4f3d-8e65-3a84183c9815#bm4

        /* Add/Remove a TabControl.TabPage Control to a FORM */
        // Select TabControl on a FORM
        // Right click and select Add Tab or Remove Tab

        /* Reorder TabPages on a FORM */
        // 1. Right-click a tab, or right-click the blank area at the top of the tab control.
        // 2. Click Page Order.
        // 3. In the Page Order dialog box, select the page that you want to move.
        // 4. Click Move Up or Move Down to place the page in the order you want.
        // 5. Repeat steps 3 and 4 for any other pages that you want to move.

        /* Move existing Form's controls to a tab page */
        // Drag the Form's control on to the TabPage for it to bind to the TabPage 

        /* tabcontrol resize with form */
        //Use the Anchor property. Anchor the tab control to all 4 edges of the form.

        #endregion - Notes Sections
    }

}

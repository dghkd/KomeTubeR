using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using CsvHelper;
using CsvHelper.Expressions;
using CsvHelper.Configuration;

using KomeTubeR.Kernel;
using KomeTubeR.Kernel.YtLiveChatDataModel;
using System.Globalization;

namespace KomeTubeR.ViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        #region Private Member

        private String _videoUrl;
        private bool _isStopped;
        private bool _isEnableStop;
        private int _totalCommentCount;
        private int _totalAuthorCount;

        /// <summary>
        /// Key:Author ID, Value:Author Name
        /// </summary>
        private Dictionary<String, String> _authorTable;

        private String _statusText;
        private String _errText;

        private CommentLoader _cmtLoader;
        private object _lockCommentColleObj = new object();
        private ObservableCollection<CommentVM> _commentColle;

        #endregion Private Member

        #region Constructor

        public MainWindowVM()
        {
            _cmtLoader = new CommentLoader();
            _commentColle = new ObservableCollection<CommentVM>();
            this.CommentColle = CollectionViewSource.GetDefaultView(_commentColle);
            BindingOperations.EnableCollectionSynchronization(_commentColle, _lockCommentColleObj);

            _authorTable = new Dictionary<string, string>();
            _isStopped = true;
            _videoUrl = "";

            _cmtLoader.OnStatusChanged += On_CommetLoader_StatusChanged;
            _cmtLoader.OnError += On_CommentLoader_Error;
            _cmtLoader.OnCommentsReceive += On_CommentLoader_CommentsReceive;

            this.CmdStart = new RelayCommand(Start, CanStart);
            this.CmdStop = new RelayCommand(Stop, CanStop);
            this.CmdExportComment = new RelayCommand(ExportComment);

            this.StatusText = "已停止";
        }

        #endregion Constructor

        #region Public Member

        /// <summary>
        /// 取得或設定影片網址
        /// </summary>
        public String VideoUrl
        {
            get { return _videoUrl; }
            set { Set(ref _videoUrl, value); }
        }

        /// <summary>
        /// 取得是否已停止擷取留言狀態
        /// </summary>
        public bool IsStopped
        {
            get { return _isStopped; }
            private set { Set(ref _isStopped, value); }
        }

        /// <summary>
        /// 取得或設定是否允許暫停
        /// </summary>
        public bool IsEnableStop
        {
            get { return _isEnableStop; }
            set { Set(ref _isEnableStop, value); }
        }

        /// <summary>
        /// 取得總留言數量
        /// </summary>
        public int TotalCommentCount
        {
            get { return _totalCommentCount; }
            private set { Set(ref _totalCommentCount, value); }
        }

        /// <summary>
        /// 取得總留言人數
        /// </summary>
        public int TotalAuthorCount
        {
            get { return _totalAuthorCount; }
            private set { Set(ref _totalAuthorCount, value); }
        }

        /// <summary>
        /// 目前處理狀態
        /// </summary>
        public String StatusText
        {
            get { return _statusText; }
            set { Set(ref _statusText, value); }
        }

        /// <summary>
        /// 發生錯誤時的錯誤訊息
        /// </summary>
        public String ErrorText
        {
            get { return _errText; }
            set { Set(ref _errText, value); }
        }

        /// <summary>
        /// 留言集合
        /// </summary>
        public ICollectionView CommentColle
        {
            get;
            private set;
        }

        /// <summary>
        /// 關閉視窗
        /// </summary>
        public Action CloseWindow { get; set; }

        #endregion Public Member

        #region Command

        public RelayCommand CmdStart { get; set; }
        public RelayCommand CmdStop { get; set; }
        public RelayCommand CmdExportComment { get; set; }

        private bool CanStart()
        {
            if (this.VideoUrl == ""
                || this.VideoUrl == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CanStop()
        {
            return this.IsEnableStop;
        }

        #endregion Command

        #region Public Method

        /// <summary>
        /// 由命令列參數取得影片網址後自動執行讀取留言並匯出
        /// </summary>
        public void AutoStart()
        {
            this.VideoUrl = App.AppStartupParameter.Url;
            this.CmdStart.Execute(null);
            //等待讀取留言完畢後匯出留言
            //請參考On_CommetLoader_StatusChanged方法中的case CommentLoaderStatus.Completed
        }

        /// <summary>
        /// 開始取得留言
        /// </summary>
        public void Start()
        {
            _commentColle.Clear();
            _authorTable.Clear();
            this.ErrorText = null;
            _cmtLoader.Start(this.VideoUrl);
        }

        /// <summary>
        /// 停止取得留言
        /// </summary>
        public void Stop()
        {
            _cmtLoader.Stop();
        }

        /// <summary>
        /// 以CSV格式匯出留言
        /// </summary>
        public void ExportComment()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV |*.csv",
                Title = "匯出留言",
                DefaultExt = ".csv"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }

            ExportComment(dlg.FileName);
        }

        /// <summary>
        /// 以CSV格式匯出留言
        /// </summary>
        /// <param name="filename">儲存CSV檔案路徑</param>
        public void ExportComment(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                using (CsvWriter cw = new CsvWriter(sw, CultureInfo.InvariantCulture))
                {
                    cw.Configuration.HasHeaderRecord = false;
                    cw.Configuration.SanitizeForInjection = true;

                    //寫入內容
                    List<CommentVM> commentLs = _commentColle.ToList();
                    foreach (CommentVM vm in commentLs)
                    {
                        CommentExportData data = new CommentExportData()
                        {
                            VideoOffsetTime = vm.VideoOffsetTime,
                            AuthorName = vm.AuthorName,
                            AuthorBadges = vm.AuthorBadges,
                            Message = vm.Message,
                            PaidMsg = vm.PaidMessage,
                            AuthorID = vm.AuthorID
                        };
                        cw.WriteRecord<CommentExportData>(data);
                        cw.NextRecord();
                    }
                }
            }
        }

        #endregion Public Method

        #region Event Handle

        /// <summary>
        /// CommentLoader收到新留言處理函式
        /// </summary>
        /// <param name="sender">CommentLoader</param>
        /// <param name="lsComments">新留言</param>
        private void On_CommentLoader_CommentsReceive(CommentLoader sender, List<CommentData> lsComments)
        {
            if (lsComments != null)
            {
                foreach (CommentData cmt in lsComments)
                {
                    //將留言加入留言集合中
                    CommentVM vm = new CommentVM(cmt);
                    _commentColle.Add(vm);
                    this.TotalCommentCount = _commentColle.Count;

                    //紀錄留言者ID供人數統計
                    if (!_authorTable.ContainsKey(vm.AuthorID))
                    {
                        _authorTable.Add(vm.AuthorID, vm.AuthorName);
                        this.TotalAuthorCount = _authorTable.Keys.Count;
                    }
                }
            }
        }

        /// <summary>
        /// CommentLoader讀取留言發生錯誤處理函式
        /// </summary>
        /// <param name="sender">CommentLoader</param>
        /// <param name="errCode">錯誤碼</param>
        /// <param name="obj">附加錯誤資訊</param>
        private void On_CommentLoader_Error(CommentLoader sender, CommentLoaderErrorCode errCode, object obj)
        {
            String errStr = "";

            switch (errCode)
            {
                case CommentLoaderErrorCode.CanNotGetLiveChatUrl:
                    errStr = String.Format("無法取得聊天室位址。請檢查輸入的網址:{0}", Convert.ToString(obj));
                    break;

                case CommentLoaderErrorCode.CanNotGetLiveChatHtml:
                    errStr = String.Format("無法取得聊天室內容。 {0}", Convert.ToString(obj));
                    break;

                case CommentLoaderErrorCode.CanNotParseLiveChatHtml:
                    errStr = String.Format("無法解析聊天室HTML。 {0}", Convert.ToString(obj));
                    break;

                case CommentLoaderErrorCode.GetCommentsError:
                    errStr = String.Format("取得留言時發生錯誤。 {0}", Convert.ToString(obj));
                    break;

                default:
                    break;
            }

            this.ErrorText = errStr;
        }

        /// <summary>
        /// CommentLoader處理狀態改變
        /// </summary>
        /// <param name="sender">CommentLoader</param>
        /// <param name="status">改變後的狀態</param>
        private void On_CommetLoader_StatusChanged(CommentLoader sender, CommentLoaderStatus status)
        {
            switch (status)
            {
                case CommentLoaderStatus.Null:
                    this.StatusText = "已停止";
                    break;

                case CommentLoaderStatus.Started:
                    this.StatusText = "開始";
                    this.IsStopped = false;
                    this.IsEnableStop = false;
                    break;

                case CommentLoaderStatus.GetLiveChatHtml:
                    this.StatusText = "讀取聊天室";
                    break;

                case CommentLoaderStatus.ParseLiveChatHtml:
                    this.StatusText = "解析聊天室內容";
                    break;

                case CommentLoaderStatus.GetComments:
                    this.StatusText = "取得留言";
                    this.IsEnableStop = true;
                    break;

                case CommentLoaderStatus.StopRequested:
                    this.StatusText = "停止中";
                    break;

                case CommentLoaderStatus.Completed:
                    this.StatusText = "已停止";
                    this.IsStopped = true;
                    this.IsEnableStop = false;
                    if (App.AppStartupParameter.IsParsed
                        && App.AppStartupParameter.OutputFilePath != ""
                        && App.AppStartupParameter.OutputFilePath != null)
                    {
                        ExportComment(App.AppStartupParameter.OutputFilePath);
                        if (this.CloseWindow != null
                            && (App.AppStartupParameter.IsHide || App.AppStartupParameter.IsClose))
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CloseWindow();
                            });
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion Event Handle
    }
}
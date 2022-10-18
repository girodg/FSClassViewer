using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualBasic.FileIO;
using Prism.Commands;
using RosterTreeViewer;
using Syroot.Windows.IO;

namespace FSClassViewer
{
    public class FailureValue
    {
        public Failures FailureEnum { get; set; } = Failures.None;
        public string FailureString { get; set; }
    }

    public class FSViewModel : ViewModelBase
    {
        #region Fields
        private FileSystemWatcher _fileWatcher;
        private string _filePath = string.Empty;
        private string _className;
        private string _month;
        private string _course;
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();
        private ObservableCollection<Student> _failingStudents = new ObservableCollection<Student>();
        private Student _selectedStudent;
        private bool _gradesLoaded = false;
        private MainWindow _mainWin;
        private float _failRate = 0;
        private RecentClass _recentClass = null;
        private string _contactMsg = "Please let me know a good time so we can have a Discord chat about this.\n\n";
        private readonly string _originalContact = "Please let me know a good time so we can have a Discord chat about this.\n\n";
        private string _contactList = "jhinders@fullsail.edu;rleis@fullsail.com;jfecko@fullsail.com";
        #endregion

        #region Properties

        public string ClassName
        {
            get { return _className; }
            set
            {
                _className = value;
                OnPropertyChanged();
            }
        }

        public string Month
        {
            get { return _month; }
            set
            {
                _month = value;
                OnPropertyChanged();
            }
        }


        public string Course
        {
            get { return _course; }
            set
            {
                _course = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Student> Students
        {
            get { return _students; }
            set
            {
                _students = value;
                OnPropertyChanged();
            }
        }

        public Student SelectedStudent
        {
            get { return _selectedStudent; }
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
                if (_selectedStudent != null)
                {

                }
            }
        }


        public ObservableCollection<Student> FailingStudents
        {
            get { return _failingStudents; }
            set
            {
                _failingStudents = value;
                OnPropertyChanged();
            }
        }



        public bool GradesLoaded
        {
            get { return _gradesLoaded; }
            set
            {
                _gradesLoaded = value;
                OnPropertyChanged();
            }
        }


        public float FailRate
        {
            get { return _failRate; }
            set
            {
                _failRate = value;
                OnPropertyChanged();
            }
        }


        public List<FailureValue> ListOfFailures { get; set; }

        public ObservableCollection<RecentClass> Recents
        {
            get
            {
                return App.RecentClasses;
            }
        }

        public List<YearData> Years { get; set; } = new List<YearData>();

        private ClassData _selectedClass;
        public ClassData SelectedClass
        {
            get
            {
                return _selectedClass;
            }
            set
            {
                if (value != _selectedClass)
                {
                    _selectedClass = value;
                    _filePath = _selectedClass.Rosters[0];
                    OnLoadClass();
                    OnPropertyChanged();
                    UpdateCommands();
                }
            }
        }


        public string ContactMessage
        {
            get { return _contactMsg; }
            set
            {
                _contactMsg = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    App.ContactMessage = value;
                }
            }
        }

        public string ContactList
        {
            get { return _contactList; }
            set
            {
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _contactList = value;
                    App.ContactList = value;
                }
            }
        }



        #endregion

        #region Commands
        public AppCommand<object> MakeIRCommand { get; set; }
        public AppCommand<object> CreateFeedbackCommand { get; set; }
        public AppCommand<object> SetRosterRootCommand { get; set; }
        public AppCommand<object> RefreshClassCommand { get; set; }
        public AppCommand<object> ResetMessageCommand { get; set; }
        #endregion

        public FSViewModel(MainWindow mainWin)
        {
            InitFailList();

            if (!string.IsNullOrWhiteSpace(App.ContactMessage))
                ContactMessage = App.ContactMessage;

            MakeIRCommand = new AppCommand<object>(MakeFinalIR, CanRefreshClassCommand);
            CreateFeedbackCommand = new AppCommand<object>(GenerateFiles, CanRefreshClassCommand);
            SetRosterRootCommand = new AppCommand<object>(SetRosterRoot);
            RefreshClassCommand = new AppCommand<object>(RefreshClass, CanRefreshClassCommand);
            ResetMessageCommand = new AppCommand<object>(ResetContactMessage);

            CreateRosterTree();

            _mainWin = mainWin;
        }


        #region Command Methods

        private void UpdateCommands()
        {
            MakeIRCommand.RaiseCanExecuteChanged();
            CreateFeedbackCommand.RaiseCanExecuteChanged();
            RefreshClassCommand.RaiseCanExecuteChanged();
        }

        private bool CanRefreshClassCommand(object arg)
        {
            return _selectedClass != null;
        }
        private void ResetContactMessage(object obj)
        {
            ContactMessage = _originalContact;
        }
        #endregion

        #region Roster Loading

        private void CreateRosterTree()
        {
            var rosters = LoadRosterData();
            BuildTree(rosters);
        }

        private List<string> LoadRosterData()
        {
            string dwnLoads = (string.IsNullOrWhiteSpace(App.RosterRootPath)) ? KnownFolders.Downloads.Path : App.RosterRootPath;

            var rosters = Directory.EnumerateFiles(dwnLoads, "*_Roster.csv").ToList();//.OrderByDescending(c => c).ToList();
            rosters.Sort(new RosterComparer());
            return rosters;
        }

        public void SetRosterRoot(object param = null)
        {
            string dwnLoads = (string.IsNullOrWhiteSpace(App.RosterRootPath)) ? KnownFolders.Downloads.Path : App.RosterRootPath;

            if (GetDirectory(dwnLoads, out string newRoot))
            {
                App.RosterRootPath = newRoot;
                CreateRosterTree();
            }
        }

        private void BuildTree(List<string> rosters)
        {
            string currentYear = string.Empty;
            string currentMonth = string.Empty;
            string currentClass = string.Empty;

            foreach (var roster in rosters)
            {
                Debug.WriteLine(roster);
                //int start = roster.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
                string year = GetYear(roster);// roster.Substring(start+1, 4);
                if (year.CompareTo(currentYear) != 0)
                {
                    //
                    //start a new year
                    //
                    ClassData cd = new ClassData();
                    cd.Rosters.Add(roster);
                    //int classStart = start + 8;
                    //int classSize = roster.LastIndexOf('-') - classStart;
                    currentClass = GetClass(roster);// roster.Substring(classStart, classSize);
                    cd.ClassName = currentClass;

                    MonthData md = new MonthData();
                    md.Classes.Add(cd);
                    currentMonth = GetMonth(roster);// roster.Substring(start+5, 2);
                    md.Month = GetMonthName(currentMonth);

                    YearData yd = new YearData();
                    yd.Year = year;
                    yd.Months.Add(md);

                    Years.Add(yd);
                    currentYear = year;
                }
                else
                {
                    //add to current year
                    YearData yd = Years[Years.Count - 1];


                    string nextMonth = GetMonth(roster);// roster.Substring(start + 5, 2);
                    //int classStart = start + 8;
                    //int classSize = roster.LastIndexOf('-') - classStart;
                    string nextClass = GetClass(roster);//roster.Substring(classStart, classSize);

                    if (currentMonth.CompareTo(nextMonth) == 0)
                    {
                        //add to current month
                        MonthData md = yd.Months[yd.Months.Count - 1];

                        if (currentClass.CompareTo(nextClass) == 0)
                        {
                            //add roster to current class
                            ClassData cd = md.Classes[md.Classes.Count - 1];
                            cd.Rosters.Add(roster);
                        }
                        else
                        {
                            //add a new class to the 
                            ClassData cd = new ClassData();
                            cd.Rosters.Add(roster);
                            cd.ClassName = nextClass;
                            md.Classes.Add(cd);
                            currentClass = nextClass;
                        }
                    }
                    else
                    {
                        //start a new month
                        currentMonth = nextMonth;
                        currentClass = nextClass;
                        ClassData cd = new ClassData();
                        cd.ClassName = nextClass;
                        cd.Rosters.Add(roster);

                        MonthData md = new MonthData();
                        md.Month = GetMonthName(currentMonth);
                        md.Classes.Add(cd);

                        yd.Months.Add(md);
                    }
                }
            }
        }

        private string GetMonthName(string currentMonth)
        {
            return new DateTime(DateTime.Now.Year, int.Parse(currentMonth), 1).ToString("MMMM", CultureInfo.InvariantCulture);
        }

        private string GetMonth(string roster)
        {
            int start = roster.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
            return roster.Substring(start + 5, 2);
        }

        private string GetClass(string roster)
        {
            int start = roster.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
            int classStart = start + 8;
            int classSize = roster.LastIndexOf('-') - classStart;
            return roster.Substring(classStart, classSize);
        }

        private string GetYear(string roster)
        {
            int start = roster.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
            return roster.Substring(start + 1, 4);
        }
        #endregion

        #region Loading Methods
        internal void LoadGrades(bool showMessage = true)
        {
            string gradeFile = _filePath.Replace("Roster", "Gradebook");
            if (!File.Exists(gradeFile))
            {
                System.Windows.Forms.MessageBox.Show("Cannot find the gradebook. Please download from FSO.");
            }
            else
            {
                // READ THE FILE
                string fileContent;
                using (StreamReader reader = new StreamReader(gradeFile))
                {
                    fileContent = reader.ReadToEnd();
                }

                // READ THE STUDENTS 
                string[] lines = fileContent.Split('\n');
                int index = 0;

                List<Activity> activities = new List<Activity>();

                char[] trims = new char[] { '\"' };
                foreach (var line in lines)
                {
                    //the activity name row
                    //
                    if (index == 0)
                    {
                        //string[] names = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        using (var parser = new TextFieldParser(new StringReader(line)))
                        {
                            parser.HasFieldsEnclosedInQuotes = true;
                            parser.TrimWhiteSpace = true;
                            parser.Delimiters = new string[] { ",", ":" };
                            string[] names = parser.ReadFields().Where(x => !string.IsNullOrEmpty(x)).ToArray();// line.Split(',');
                            foreach (var name in names)
                            {
                                if (name != "Activity")
                                    activities.Add(new Activity() { Name = name.Trim(trims) });
                            }
                        }

                        index++;
                    }
                    //the activity weight row
                    //
                    else if (index == 1)
                    {
                        string[] weights = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < weights.Length; i++)
                        {
                            if (i > 0)
                            {
                                if (float.TryParse(weights[i], out float wt))
                                {
                                    activities[i - 1].Weight = wt;
                                }
                            }
                        }
                        index++;
                    }
                    else //the student grades
                    {
                        string[] vals = line.Split(',');
                        List<string> professionalismNames = new List<string>() { "0.3 Professionalism", "0.7 Professionalism", "0.9 Professionalism" };
                        foreach (var student in Students)
                        {
                            //find the student in the List of students
                            string valID = vals[0].Trim('\"').Trim('\'');
                            Console.WriteLine(valID);
                            Console.WriteLine(student.ID);
                            if (student.ID.Equals(valID))
                            {
                                student.Clear();
                                List<Activity> profActivities = new List<Activity>();
                                Activity professionalism = null;
                                for (int i = 2; i < vals.Length; i++) //skip the ID and name fields
                                {
                                    if (i == vals.Length - 1)
                                    {
                                        student.IsActive = vals[i].Equals("ACTIVE");
                                    }
                                    else if ((i - 2) < activities.Count && activities[i - 2].Weight > 0) //if(i == vals.Length - 2)
                                    {
                                        Activity cAct = activities[i - 2];
                                        Activity ac = new Activity() { Name = cAct.Name, Weight = cAct.Weight };
                                        ac.IsGraded = vals[i] != "-" && vals[i] != "C";
                                        ac.Grade = vals[i];
                                        student.Grades.Add(ac);
                                        ac.PropertyChanged += student.WhatIf_PropertyChanged;
                                        //if (professionalismNames.Contains(ac.Name))//specific to PG2
                                        //    professionalism = ac;
                                    }
                                    //else if ((i - 2) < activities.Count && IsProfessionalismActivity(activities[i - 2]))
                                    //{
                                    //    Activity cAct = activities[i - 2];
                                    //    Activity ac = new Activity() { Name = cAct.Name, Weight = cAct.Weight };
                                    //    ac.IsGraded = vals[i] != "-" && vals[i] != "C";
                                    //    ac.Grade = vals[i];
                                    //    profActivities.Add(ac);
                                    //}
                                }
                                //if (professionalism != null)
                                //{
                                //    //sum up the grades of the following: 
                                //    // Profile pic (1%), GitHub (2%), Discord (1%), Attendance (2%), Plan (1%), Plan (1%), Plan (1%), Plan (1%)\
                                //    float profSum = 0F;
                                //    foreach (var grade in profActivities)
                                //    {
                                //        if (grade.Name.Contains("Profile Picture") ||
                                //            grade.Name.Contains("Discord") ||
                                //            grade.Name.Contains("1 Plan"))
                                //        {
                                //            if (float.TryParse(grade.Grade, out float gd))
                                //            {
                                //                float thisGrade = gd * 1;
                                //                profSum += thisGrade;
                                //            }
                                //        }
                                //        else if (grade.Name.Contains("GitHub") ||
                                //                 grade.Name.Contains("Attendance"))
                                //        {
                                //            if (float.TryParse(grade.Grade, out float gd))
                                //            {
                                //                float thisGrade = gd * 2;
                                //                profSum += thisGrade;
                                //            }
                                //        }
                                //    }
                                //    if (professionalism != null)
                                //        professionalism.Grade = (profSum * 0.1F).ToString();//10%
                                //}
                                student.CalculateGrades();
                                break;
                            }
                        }
                    }
                }
                CalculateFailRate();
                GradesLoaded = true;
                if (showMessage)
                    System.Windows.Forms.MessageBox.Show("The grades have been loaded.", "Load Grades");
            }
        }

        private bool IsProfessionalismActivity(Activity activity)
        {
            return (activity.Name.Contains("Profile Picture") ||
                    activity.Name.Contains("Discord") ||
                    activity.Name.Contains("1 Plan") ||
                    activity.Name.Contains("GitHub") ||
                    activity.Name.Contains("Attendance"));
        }

        internal void LoadClass()
        {
            //var fileContent = string.Empty;
            ////var filePath = string.Empty;
            bool fileSelected = false;

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                //string initDir = GetInitialDirectory();
                openFileDialog.InitialDirectory = GetInitialDirectory(); ;// @"c:\";
                openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                //openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    _filePath = openFileDialog.FileName;
                    //SetupWatcher(Path.GetDirectoryName(_filePath));

                    //App.LastFilePath = Path.GetDirectoryName(_filePath);
                    //_recentClass = GetRecentClass(_filePath);
                    //UpdateRecents(App.RecentClasses, _recentClass);
                    //App.RecentClasses.Insert(0, _recentClass);
                    //GradesLoaded = false;

                    ////Read the contents of the file into a stream
                    ////var fileStream = openFileDialog.OpenFile();

                    ////using (StreamReader reader = new StreamReader(fileStream))
                    ////{
                    ////    fileContent = reader.ReadToEnd();
                    ////}
                    fileSelected = true;

                }
            }

            if (fileSelected)
            {
                OnLoadClass();
                //SetupWatcher(Path.GetDirectoryName(_filePath));

                //App.LastFilePath = Path.GetDirectoryName(_filePath);
                //_recentClass = GetRecentClass(_filePath);
                //UpdateRecents(App.RecentClasses, _recentClass);
                //App.RecentClasses.Insert(0, _recentClass);

                //GradesLoaded = false;

                //string fName = Path.GetFileName(_filePath);
                //string[] fParts = fName.Split('_');

                //Month = fParts[0];
                //ClassName = fParts[1];
                //Course = LookupCourse(fParts[1]);

                //ReadClassInfo();

                //_fileWatcher.EnableRaisingEvents = true;
            }
        }

        public void RefreshClass(object param = null)
        {
            ReadClassInfo();
        }

        private void OnLoadClass()
        {
            //SetupWatcher(Path.GetDirectoryName(_filePath));

            App.LastFilePath = Path.GetDirectoryName(_filePath);
            _recentClass = GetRecentClass(_filePath);
            UpdateRecents(App.RecentClasses, _recentClass);
            App.RecentClasses.Insert(0, _recentClass);
            if (App.RecentClasses.Count > 24)
            {
                while (App.RecentClasses.Count > 24)
                    App.RecentClasses.RemoveAt(App.RecentClasses.Count - 1);
            }

            GradesLoaded = false;

            string fName = Path.GetFileName(_filePath);
            string[] fParts = fName.Split('_');

            Month = fParts[0].Replace("C", "");
            ClassName = fParts[1].Replace("-L", "").Replace("-O", "");
            Course = LookupCourse(fParts[1]).Shortcut;

            ReadClassInfo();

            //_fileWatcher.EnableRaisingEvents = true;
        }

        internal void LoadRecent(RecentClass recentClass)
        {
            //is recentClass different than current class?
            //if so, load class
            Debug.WriteLine(recentClass.FileName);

            if (recentClass != _recentClass)
            {
                _filePath = recentClass.FilePath;
                OnLoadClass();
            }
        }

        private void UpdateRecents(ObservableCollection<RecentClass> recentClasses, RecentClass recentClass)
        {
            for (int i = recentClasses.Count - 1; i >= 0; i--)
            {
                if (recentClasses[i].FileName.Equals(recentClass.FileName))
                {
                    recentClasses.RemoveAt(i);
                    break;
                }
            }
        }

        private RecentClass GetRecentClass(string filePath)
        {
            //C:\Grading\Courses\COP2334-O\2020\202010\C202010_COP2334-O_01_Roster.csv
            //2010\COP2334-O
            string fName = Path.GetFileName(_filePath);
            string[] fParts = fName.Split('_');

            string month = fParts[0].Substring(3);
            string course = fParts[1].Substring(0, 9);

            RecentClass rc = new RecentClass() { FileName = $"{month} {course}", FilePath = filePath };
            return rc;
        }

        private void ReadClassInfo()
        {
            if (_selectedClass != null && _selectedClass.Rosters.Count > 0)
            {
                Students.Clear();
                //load each roster file for the class
                foreach (var roster in _selectedClass.Rosters)
                {
                    _filePath = roster;
                    LoadStudents();
                    string gradeFile = _filePath.Replace("Roster", "Gradebook");
                    if (File.Exists(gradeFile))
                    {
                        LoadGrades(false);
                    }
                }
                //default it to the first roster file in the list
                _filePath = _selectedClass.Rosters[0];
            }
        }

        #region File System Watcher
        private void SetupWatcher(string directory)
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Path = directory;
            }
            else
            {
                _fileWatcher = new FileSystemWatcher(directory);
                //_fileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                //_fileWatcher.Changed += FileWatcher_Changed;
                //_fileWatcher.Created += FileWatcher_Changed;
                //_fileWatcher.Renamed += _fileWatcher_Renamed;
                _fileWatcher.Changed += FileSystemWatcher_Changed;
                _fileWatcher.Created += FileSystemWatcher_Created;
                _fileWatcher.Deleted += FileSystemWatcher_Deleted;
                _fileWatcher.Renamed += FileSystemWatcher_Renamed;
            }
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine($"A new file has been renamed from {e.OldName} to {e.Name}\n{e.FullPath}");
            //string file = Path.GetFileName(e.FullPath);
            //if (Path.GetExtension(file).Equals(".csv") && file.Contains(Course))
            //{
            //    if (file.Contains("Gradebook"))
            //    {
            //        //reload grades
            //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            LoadGrades(false);
            //        });
            //    }
            //    else if (file.Contains("Roster"))
            //    {
            //        //reload class roster and grades
            //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            ReadClassInfo();
            //        });
            //    }
            //}
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"A new file has been deleted - {e.Name}");
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"A new file has been changed - {e.Name}");
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"A new file has been created - {e.Name}");

            string file = Path.GetFileName(e.FullPath);
            if (Path.GetExtension(file).Equals(".csv") && file.Contains(ClassName))
            {
                if (file.Contains("Gradebook"))
                {
                    Debug.WriteLine($"Reloading Grades");
                    //reload grades
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadGrades(false);
                    });
                }
                else if (file.Contains("Roster"))
                {
                    Debug.WriteLine($"Reloading Class");
                    //reload class roster and grades
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ReadClassInfo();
                    });
                }
            }
        }
        #endregion

        private string GetInitialDirectory()
        {
            if (App.LastFilePath != string.Empty)
                return App.LastFilePath;

            return @"c:\";
        }

        private void LoadStudents()
        {
            // READ THE FILE
            string fileContent;
            using (StreamReader reader = new StreamReader(_filePath))
            {
                fileContent = reader.ReadToEnd();
            }

            //SECTION
            string[] fileParts = _filePath.Split('_');
            string section = fileParts[2];//sample name: C202203_COP2334-O_01_Roster

            // READ THE STUDENTS 
            string[] lines = fileContent.Split('\n');
            int index = 0;
            //Students.Clear();
            //List<string> students = new List<string>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    using (var parser = new TextFieldParser(new StringReader(line)))
                    {
                        parser.HasFieldsEnclosedInQuotes = true;
                        parser.Delimiters = new string[] { "," };
                        if (index == 0) //skip the header row
                        {
                            index++;
                            continue;
                        }
                        string[] vals = parser.ReadFields();// line.Split(',');
                        char[] trims = new char[] { '\"' };
                        char[] trims2 = new char[] { '\'' };
                        if (vals[0].Length > 4) //then it's a student record
                        {
                            string id = vals[0].Trim(trims).Trim(trims2);
                            string name = $"{vals[1].Trim(trims)} {vals[2].Trim(trims)}";
                            Student nextStudent = new Student()
                            {
                                ID = id,
                                Name = name,
                                FirstName = vals[1].Trim(trims),
                                LastName = vals[2].Trim(trims),
                                Degree = vals[12].Trim(trims),
                                PrimaryEmail = vals[4].Trim(trims),
                                PersonalEmail = vals[5].Trim(trims),
                                BestTime = vals[8],
                                LastAccess = vals[9].Trim(trims2),
                                IsOnline = _filePath.Contains("-O"),
                                Section = section
                            };
                            nextStudent.AddPhones(vals[6].Trim(trims));
                            nextStudent.AddPhones(vals[7].Trim(trims));
                            if (nextStudent.Degree.Contains(','))
                            {
                                nextStudent.Degree = nextStudent.Degree.Substring(0, nextStudent.Degree.IndexOf(','));
                            }
                            nextStudent.Degree += $"{(nextStudent.IsOnline ? "-O" : "-L")}";
                            Students.Add(nextStudent);
                        }
                    }
                }
            }
        }

        #endregion

        #region Create Files

        internal void GenerateFiles(object param = null)
        {
            //var fileContent = string.Empty;
            //var filePath = string.Empty;
            //bool fileSelected = false;

            //using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            //{
            //    openFileDialog.InitialDirectory = @"c:\";
            //    openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            //    openFileDialog.FilterIndex = 1;
            //    openFileDialog.RestoreDirectory = true;

            //    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        //Get the path of specified file
            //        filePath = openFileDialog.FileName;

            //        //Read the contents of the file into a stream
            //        var fileStream = openFileDialog.OpenFile();

            //        using (StreamReader reader = new StreamReader(fileStream))
            //        {
            //            fileContent = reader.ReadToEnd();
            //        }
            //        fileSelected = true;
            //    }
            //}

            string gradingRoot = (string.IsNullOrWhiteSpace(App.GradingRootPath)) ? Path.GetDirectoryName(_filePath) : App.GradingRootPath;
            if (GetDirectory(gradingRoot, out string filePath))
            {
                //
                // CREATE THE IR FILE
                //
                //CreateIRFile(filePath);

                //
                // CREATE THE FEEDBACK FILES FOR EACH LAB
                //
                try
                {

                    //-------------------------------------------------------------------------------------------
                    // NOTE: filePath should be the root directory where you store the course feedback files.
                    //      Course is separated by campus and online.
                    //      Expected directory hierarchy: root\year\course\month
                    //          EX: C:\Grading\Courses\2020\COP2334\202012
                    //      All sections (campus + online) combined in same feedback file
                    //

                    string gradingPath = Path.Combine(filePath, Month.Substring(0, 4), ClassName, Month);
                    if (Directory.Exists(gradingPath))
                    {
                        App.GradingRootPath = filePath;
                        foreach (var roster in _selectedClass.Rosters)
                        {

                            DirectoryInfo dirs = new DirectoryInfo(gradingPath);// Path.GetDirectoryName(filePath));
                            DirectoryInfo[] subDirs = dirs.GetDirectories();
                            foreach (var dir in subDirs)
                            {
                                if (dir.FullName.EndsWith("Professionalism"))
                                {
                                    GenerateProfessionalismCalculators(dir.FullName);
                                }
                                else
                                {
                                    string subPath = Path.Combine(dir.FullName, "Feedback_Lab.docx");
                                    CreateFeedbackFile(subPath);
                                }
                            }
                        }
                        //for some reason, using File.Copy to copy the file causes a corruption? the copied file cannot be opened but the original can. :-(
                        //maybe the original file is still locked then copy somehow fails without throwing exceptions?
                        //CopyToFolders(_filePath); 
                        System.Windows.Forms.MessageBox.Show("The initial files have been generated.", "Initial Files");

                    }
                    else
                    {

                        System.Windows.Forms.MessageBox.Show($"The grading path does not exists: {gradingPath}.", "Initial Files");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"There was an error generating the initial files: {ex.Message}", "ERROR: Initial Files");
                }
            }

        }

        private void GenerateProfessionalismCalculators(string fullPath)
        {
            string TemplatePath = Path.Combine(fullPath,@"ProfessionalismCalculator.xlsx");
            if (File.Exists(TemplatePath))
            {
                var orderedStudents = Students.OrderBy(st => st.LastName);
                foreach (var student in orderedStudents)
                {
                    string newFileName = $"ProfessionalismCalculator_{student.LastName}{student.FirstName}.xlsx";
                    string FinalPath = Path.Combine(fullPath, newFileName);
                    File.Copy(TemplatePath, FinalPath, true);
                }
            }
        }

        private bool GetDirectory(string currentDirectory, out string selectedDirectory)
        {
            selectedDirectory = string.Empty;
            using (System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                openFileDialog.SelectedPath = currentDirectory;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    selectedDirectory = openFileDialog.SelectedPath;
                }
                else
                    return false;
            }
            return true;
        }

        private void CalculateFailRate()
        {
            //float currentlyFailing = 0;
            //foreach (var student in Students)
            //{
            //    if (student.CurrentGrade < Student.FailThreshold && student.IsActive)
            //    {
            //        currentlyFailing++;
            //    }
            //}
            InitFailures(true);
            FailRate = (float)FailingStudents.Count / Students.Count * 100.0F;
        }

        private void InitFailList()
        {
            ListOfFailures = new List<FailureValue>();
            ListOfFailures.Add(new FailureValue() { FailureEnum = Failures.None, FailureString = "None" });
            ListOfFailures.Add(new FailureValue() { FailureEnum = Failures.First, FailureString = "First" });
            ListOfFailures.Add(new FailureValue() { FailureEnum = Failures.Other, FailureString = "Multiple" });
            //ListOfFailures.Add(new FailureValue() { FailureEnum = Failures.Second, FailureString = "Second" });
            //ListOfFailures.Add(new FailureValue() { FailureEnum = Failures.Third, FailureString = "Third" });
        }

        #region Feedback File
        private void CreateFeedbackFile(string filePath)
        {
            string TemplatePath = @"Feedback_Lab.docx";// @"C:\Grading\Courses\Feedback_Lab.docx";
            string FinalPath = filePath;// Path.Combine(Path.GetDirectoryName(filePath), "Feedback_Lab.docx");
            //if (File.Exists(FinalPath))
            //    File.Delete(FinalPath);
            File.Copy(TemplatePath, FinalPath, true);
            var orderedStudents = Students.OrderBy(st => st.LastName);
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(FinalPath, true))
            {
                // Add a main document part. 
                //MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                // Create the document structure and add some text.
                //mainPart.Document = new Document();
                //Body body = mainPart.Document.AppendChild(new Body());
                Body body = wordDocument.MainDocumentPart.Document.Body;
                foreach (var student in orderedStudents)
                {
                    //string name = student.Split(',')[0];
                    AddStudent(body, student.Name);
                    AddProblemsTips(body);
                }
            }
            //CopyToFolders(FinalPath);
        }

        private void CopyToFolders(string finalPath)
        {
            DirectoryInfo dirs = new DirectoryInfo(Path.GetDirectoryName(finalPath));
            DirectoryInfo[] subDirs = dirs.GetDirectories();
            FileInfo fi = new FileInfo(finalPath);
            foreach (var dir in subDirs)
            {
                string subPath = Path.Combine(dir.FullName, "Feedback_Lab.docx");
                if (!File.Exists(subPath))
                    fi.CopyTo(subPath);
                //File.Copy(finalPath, subPath);
            }
        }
        #endregion

        #region Word Doc

        private void AddProblemsTips(Body body)
        {
            AddSection(body, "Feedback on GitHub");
            AddSection(body, "Problems");
            AddSection(body, "Tips");
        }

        private static void AddSection(Body body, string sectionText)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            Run run = para.AppendChild(new Run());
            RunProperties runProps = new RunProperties();
            Bold bold = new Bold();
            runProps.Append(bold);
            run.AppendChild(new RunProperties(runProps));
            run.AppendChild(new Text(sectionText));
        }

        private static void AddStudent(Body body, string name)
        {
            Paragraph para = body.AppendChild(new Paragraph());

            Run run = para.AppendChild(new Run());
            run.AppendChild(new Text(name));
            para.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" });
        }
        #endregion

        #region IR FIle

        /// <summary>
        /// Generate an IR file based on the current grading
        /// </summary>
        internal void MakeFinalIR(object param = null)
        {
            if (GradesLoaded)
            {
                InitFailures();
                FailureWindow fw = new FailureWindow();
                fw.DataContext = this;
                fw.ShowView(_mainWin);
                if (fw.IsOk)
                {
                    string fName = Path.GetFileName(_filePath);
                    string[] fParts = fName.Split('_');


                    ClassInformation info = LookupCourse(fParts[1]);
                    //string courseName = LookupCourse(fParts[1]);
                    //string outFile = Path.Combine(Path.GetDirectoryName(_filePath), "finalIR.txt");
                    string outFile = Path.Combine(Path.GetDirectoryName(_filePath), $"finalIR_{Month}_{info.Shortcut}.txt");

                    //string classInfo = $"{fParts[1]},{courseName}";
                    using (StreamWriter sw = new StreamWriter(outFile))
                    {
                        //DUMP IR list
                        //foreach (var student in FailingStudents)
                        //{
                        //    //if (student.ActualGrade < Student.FailThreshold)
                        //    {
                        //        string failTimes = (student.FailureCount == Failures.Second) ? ", SECOND FAILURE" : (student.FailureCount == Failures.Third) ? ", THIRD FAILURE" : "";
                        //        sw.WriteLine($"{classInfo},{student.Name},{student.ID}{failTimes}");
                        //    }
                        //}
                        WriteFailureList(FailingStudents, sw, info);
                        sw.WriteLine("\n\n-----------BUBBLES-----------\n\n");
                        WriteCourseDirectorAwards(Students, sw, info);

                        //sw.WriteLine("\n\n-----------Failure emails-----------");

                        //DUMP N-strike emails
                        //foreach (var student in FailingStudents)
                        //{
                        //    if (student.FailureCount == Failures.Other)
                        //    {
                        //        Write2StrikeEmail(sw, student, info);
                        //    }
                        //    //if (student.FailureCount == Failures.Second)
                        //    //{
                        //    //    Write2StrikeEmail(sw, student);
                        //    //}
                        //    //else if (student.FailureCount == Failures.Third)
                        //    //{
                        //    //    //use same messaging for multi-failures
                        //    //    Write2StrikeEmail(sw, student);
                        //    //    //Write3StrikeEmail(sw, student);
                        //    //}
                        //}
                    }

                    System.Windows.Forms.MessageBox.Show("Final IR file has been saved.", "Final IR");
                    System.Diagnostics.Process.Start("notepad.exe", outFile);
                }
            }
        }

        private void WriteCourseDirectorAwards(ObservableCollection<Student> students, StreamWriter sw, ClassInformation info)
        {
            var campusInfo = info.ClassCode;
            var onlineInfo = campusInfo.Replace("-L", "-O");

            var campusCDA = students.Where(x => !x.IsOnline && x.IsActive == true && x.CurrentGrade >= 90).OrderByDescending(x => x.CurrentGrade).OrderBy(x => x.Section).ToList();
            var onlineCDA = students.Where(x => x.IsOnline && x.IsActive == true && x.CurrentGrade >= 90).OrderByDescending(x => x.CurrentGrade).OrderBy(x => x.Section).ToList();
            sw.WriteLine("--------------Potential Course Director Awards--------------");
            WriteCDA(sw, campusCDA, "CAMPUS", campusInfo);
            WriteCDA(sw, onlineCDA, "ONLINE", onlineInfo);

        }

        private void WriteCDA(StreamWriter sw, List<Student> students, string courseLocation, string courseInfo)
        {
            sw.WriteLine();
            sw.WriteLine(courseLocation);
            if (students.Count > 0)
            {
                string section = "";
                //sw.WriteLine($"{courseInfo}\n{Month} Sec00");
                foreach (var student in students)
                {
                    if (!student.Section.Equals(section))
                    {
                        sw.WriteLine($"\n{courseInfo}\n{Month} Sec{student.Section}");
                        section = student.Section;
                    }
                    sw.WriteLine($"{student.CurrentGrade,6:N2} {student.Name} {student.ID}");
                }
            }
        }

        internal void WriteFailureList(IEnumerable<Student> s, StreamWriter sw, ClassInformation info)
        {
            var campusInfo = info.ClassCode;
            var onlineInfo = campusInfo.Replace("-L", "-O");

            var campusFirst = FailingStudents.Where(x => !x.IsOnline && x.FailureCount == Failures.First).ToList();
            var onlineFirst = FailingStudents.Where(x => x.IsOnline && x.FailureCount == Failures.First).ToList();

            var campusMultiple = FailingStudents.Where(x => !x.IsOnline && x.FailureCount == Failures.Other).ToList();
            var onlineMultiple = FailingStudents.Where(x => x.IsOnline && x.FailureCount == Failures.Other).ToList();

            sw.WriteLine($"\n{Month} - {ClassName} {Course}\n");
            sw.WriteLine("--------------Immediate Reschedule--------------");

            sw.WriteLine("Campus");
            if (campusFirst.Count > 0)
            {
                foreach (var student in campusFirst)
                {
                    sw.WriteLine($"\t{campusInfo},{student.Name},{student.ID}");
                }
            }
            else
                Console.WriteLine("\t(none)");

            sw.WriteLine("\nOnline");
            if (onlineFirst.Count > 0)
            {
                foreach (var student in onlineFirst)
                {
                    sw.WriteLine($"\t{onlineInfo},{student.Name},{student.ID}");
                }
            }
            else
                Console.WriteLine("\t(none)");

            //sw.WriteLine("\n--------------Multiple Failures (Please pull schedules)--------------");
            //foreach (var student in campusMultiple)
            //{
            //    sw.WriteLine($"\t{campusInfo},{student.Name},{student.ID}, MULTIPLE FAILURES");
            //}
            //foreach (var student in onlineMultiple)
            //{
            //    sw.WriteLine($"\t{onlineInfo},{student.Name},{student.ID}, MULTIPLE FAILURES");
            //}


        }

        //private void Write3StrikeEmail(StreamWriter sw, Student student)
        //{
        //    //dump emails
        //    //dump subject line
        //    //dump email body
        //    sw.WriteLine($"\n\n{student.PrimaryEmail.Replace(',', ';')};{student.PersonalEmail.Replace(',', ';')}");
        //    sw.WriteLine($"RE: PG2 - COP2334 Third Failure - {student.Name} #{student.ID}");
        //    sw.WriteLine($"Hi {student.FirstName}.\n\nYou have failed PG2 - COP2334 for a third time.\n" +
        //        "Normally we would recommend you change programs if you fail a class three times.\n\n" +
        //        "If you are struggling with the content, I would suggest that you go back to PG1 and audit the class. Maybe the extra time with the basics can help you get a better understanding to go into PG2.\n\n" +
        //        "Thanks,\nGarrett");
        //}

        private void Write2StrikeEmail(StreamWriter sw, Student student, ClassInformation info)
        {
            //dump emails
            //dump subject line
            //dump email body
            //sw.WriteLine($"\n\n{student.PrimaryEmail.Replace(',', ';')};{student.PersonalEmail.Replace(',', ';')}");
            //sw.WriteLine($"RE: PG2 - COP2334 Second Failure - {student.Name} #{student.ID}");
            //sw.WriteLine($"Hi {student.FirstName}.\n\nYou have failed PG2 - COP2334 for a second time.\n" +
            //    "Students who fail beginning programming courses tend to not complete the Game Development / Software Development degree and we would like you to succeed in your time here at Full Sail.\n\n" +
            //    "It is important that we talk, so we can determine what class to place you in for next month.\n" +
            //    "You will not be enrolled into a class until we evaluate your situation.\n\n" +
            //    "Please let me know a good time and phone number to contact you so we can chat about this.\n\n" +
            //    "In general, unless there are extenuating circumstances for you failing the course (one or more of the times ), we recommend you consider another degree program.\n\n" +
            //    "Thanks,\nGarrett");

            //-------------------------------
            // Fecko's message
            //
            //string contactInfo = "Please select a time slot from my calendar so we can chat about this.\n" +
            //                     "https://calendly.com/john_fecko/phone-call \n\n";
            //string contactInfo = "Please let me know a good time so we can have a Discord chat about this.\n\n";
            string classInfo = $"{info.Shortcut} - {info.ClassCode}";// "PG2 - COP2334";//{info.Shortcut} - {info.ClassCode}
            string classType = $"{info.ClassType}";// "programming";//{info.ClassType}
            sw.WriteLine($"\n\n{student.PrimaryEmail.Replace(',', ';')};{student.PersonalEmail.Replace(',', ';')}");
            sw.WriteLine(ContactList);
            sw.WriteLine($"RE: {classInfo} Failure - {student.Name} #{student.ID}");
            sw.WriteLine($"Hey {student.FirstName},\n\nYou have failed {classInfo} again.\n\n" +
                $"Students who fail beginning {classType} courses tend to not complete the Computer Science / Game Development degree and we would like you to succeed in your time here at Full Sail.\n\n" +
                "It is important that we talk, so we can determine what class to place you in for next month.\n" +
                "You will NOT be enrolled into a class until we evaluate your situation.\n\n" +
                ContactMessage +
                "In general, unless there are extenuating circumstances for you failing the course (one or more of the times ), we recommend you consider another degree program.\n\n" +
                "");
        }

        private void InitFailures(bool useCurrentGrade = false)
        {
            FailingStudents.Clear();
            foreach (var student in Students)
            {
                float grade = (useCurrentGrade) ? student.CurrentGrade : student.WorstGrade;
                if (grade < Student.FailThreshold && student.IsActive)
                {
                    FailingStudents.Add(student);
                    if (student.FailureCount == Failures.None) student.FailureCount = Failures.First;
                }
            }
        }

        private void CreateIRFile(string filePath)
        {
            string fName = Path.GetFileName(filePath);
            string[] fParts = fName.Split('_');


            string outFile = Path.Combine(Path.GetDirectoryName(filePath), "tempIR.txt");
            string courseName = LookupCourse(fParts[1]).Shortcut;
            string classInfo = $"{fParts[1]},{courseName}";
            using (StreamWriter sw = new StreamWriter(outFile))
            {
                foreach (var student in Students)
                {
                    sw.WriteLine($"{classInfo},{student.Name},{student.ID}");
                }
            }
        }

        private ClassInformation LookupCourse(string course)
        {
            ClassInformation courseInfo = new ClassInformation();
            courseInfo.ClassCode = course;
            courseInfo.ClassType = "programming";
            //string courseInfo = string.Empty;
            if (course.Contains("COP2334"))
            {
                courseInfo.Shortcut = "PG2";
            }
            else if (course.Contains("COP1000"))
            {
                courseInfo.Shortcut = "PG1";
            }
            else if (course.Contains("SDV3111"))
            {
                courseInfo.Shortcut = "SPR";
            }
            else if (course.Contains("GDD002"))
            {
                courseInfo.Shortcut = "PCAL";
                courseInfo.ClassType = "math";
            }
            else if (course.Contains("GDD003"))
            {
                courseInfo.Shortcut = "CAL";
                courseInfo.ClassType = "math";
            }
            //add other course as needed

            //if (course.Contains("-L"))
            //    courseInfo += "-L";
            //else
            //    courseInfo += "-O";

            return courseInfo;
        }
        #endregion

        #endregion
    }
}

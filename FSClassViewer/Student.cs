using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSClassViewer
{
	public enum Failures
	{
		None,
		First=1,
		Other
	}
    public class Student : ModelBase
    {
		private string _id;
		private string _name;
		private float _bestGrade = 100;
		private float _worstGrade = 100;
		private float _whatIfGrade = 100;
		private bool _isActive = true;
		private ObservableCollection<Activity> _grades = new ObservableCollection<Activity>();
		private string _degree;
		private bool _isOnline = false;
		private Failures _failureCount = Failures.None;
		private string _primaryEmail;
		private string _personalEmail;

		private float _currentGrade;

		//private ObservableCollection<string> _phones = new ObservableCollection<string>();
		private string _phones = string.Empty;
		private string _bestTime = string.Empty;
		private string _lastAccess = string.Empty;

		private string _section = string.Empty;

		public static readonly float FailThreshold = 59.5F;

		public string LastAccess
		{
			get { return _lastAccess; }
			set { _lastAccess = value;
				OnPropertyChanged();
			}
		}


		public string BestTime
		{
			get { return _bestTime; }
			set { _bestTime = value;
				OnPropertyChanged();
			}
		}


		public string Phones
		{
			get { return _phones; }
			set { _phones = value;
				OnPropertyChanged();
			}
		}


		public Failures FailureCount
		{
			get { return _failureCount; }
			set { _failureCount = value;
				OnPropertyChanged();
			}
		}


		public string PrimaryEmail
		{
			get { return _primaryEmail; }
			set { _primaryEmail = value;
				OnPropertyChanged();
			}
		}

		public string PersonalEmail
		{
			get { return _personalEmail; }
			set { _personalEmail = value;
				OnPropertyChanged();
			}
		}



		public string ID
		{
			get { return _id; }
			set { _id = value;
				OnPropertyChanged();
			}
		}


		public string Name
		{
			get { return _name; }
			set { _name = value;
				OnPropertyChanged();
			}
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }

		public float CurrentGrade
		{
			get { return _currentGrade; }
			set { _currentGrade = value;
				OnPropertyChanged();
			}
		}


		public float BestGrade
		{
			get { return _bestGrade; }
			set { _bestGrade = value;
				OnPropertyChanged();
			}
		}


		public float WorstGrade
		{
			get { return _worstGrade; }
			set { _worstGrade = value;
				OnPropertyChanged();
			}
		}

		public float WhatIfGrade
		{
			get { return _whatIfGrade; }
			set { _whatIfGrade = value;
				OnPropertyChanged();
			}
		}



		public bool IsActive
		{
			get { return _isActive; }
			set { _isActive = value;
				OnPropertyChanged();
			}
		}

		public string Degree
		{
			get { return _degree; }
			set { _degree = value;
				OnPropertyChanged();
			}
		}


        public bool IsOnline
        {
            get { return _isOnline; }
            set { _isOnline = value;
				OnPropertyChanged();
			}
		}


		public string Section
		{
			get { return _section; }
			set
			{
				_section = value;
				OnPropertyChanged();
			}
		}



		public ObservableCollection<Activity> Grades
		{
			get { return _grades; }
			set { _grades = value;
				OnPropertyChanged();
			}
		}

		internal void CalculateGrades()
		{
			//loop over grades and calculate the current grade and the min grade if all "-" are 0
			float bestSum = 0;
			float currentWt = 0;
			float currentSum = 0;
			float worstSum = 0;
			foreach (var grade in Grades)
			{
				if(grade.Weight > 0)
				{
					if(grade.Grade != "-" && grade.Grade != "C")
					{
						currentWt += grade.Weight;
						if (float.TryParse(grade.Grade, out float gd))
						{
							float thisGrade = gd * grade.Weight;
							currentSum += thisGrade;
							bestSum += thisGrade;
							worstSum += thisGrade;
						}
					}
					else
					{
						bestSum += (100 * grade.Weight);
						worstSum += 0;
					}
				}
			}

			CurrentGrade = currentSum / currentWt;
			BestGrade = bestSum / 100.0F; //cWt;//IS THIS CORRECT?
			WorstGrade = worstSum / 100.0F;
			WhatIfGrade = WorstGrade;
		}


		#region Event Handlers
		public void WhatIf_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("WhatIfGrade"))
			{
				//recalculate the WhatIf grade for the student
				float aSum = 0;
				foreach (var grade in Grades)
				{
					if (grade.Weight > 0)
					{
						if (!string.IsNullOrWhiteSpace(grade.WhatIfGrade) && float.TryParse(grade.WhatIfGrade, out float wGd))
						{
							aSum += (wGd * grade.Weight);
						}
						else if (float.TryParse(grade.Grade, out float gd))
						{
							aSum += (gd * grade.Weight);
						}
						else
						{
							aSum += 0;
						}
					}
				}

				WhatIfGrade = aSum / 100.0F;
			}
		}

		internal void Clear()
		{
			//disconnect any listeners
			if(Grades.Count > 0)
			{
				foreach (var grade in Grades)
				{
					grade.PropertyChanged -= WhatIf_PropertyChanged;
				}
			}
			Grades.Clear();
		}

		internal void AddPhones(string phoneString)
		{
			//if not empty AND not N/A
			if(!string.IsNullOrWhiteSpace(phoneString) && !phoneString.Equals("N/A",StringComparison.InvariantCultureIgnoreCase))
			{
				Phones += phoneString;
				//string[] phones = phoneString.Split(',');
				//foreach (var phone in phones)
				//{
				//	Phones.Add(phone);
				//}
			}
		}
		#endregion
	}
}

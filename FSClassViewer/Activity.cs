using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSClassViewer
{
    public class Activity : ModelBase
    {
		#region Fields

		private string _name;
		private float _weight;
		private string _grade;
		private bool _isGraded = false;
		private string _whatIfGrade = string.Empty;
		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}


		public float Weight
		{
			get { return _weight; }
			set { _weight = value; }
		}


		public string Grade
		{
			get { return _grade; }
			set { _grade = value; }
		}


		public bool IsGraded
		{
			get { return _isGraded; }
			set { _isGraded = value; }
		}

		public string WhatIfGrade
		{
			get { return _whatIfGrade; }
			set
			{
				_whatIfGrade = value;
				OnPropertyChanged();
			}
		}
		#endregion

	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TPCourse.Source.Table;
using TPCourse.Source.Types;
using TPCourse.Table;

namespace TPCourse.Source
{
	public class MainModel
	{
		private MainForm _form;
		private MainView _view;
		public List<TableData> TableDatas { get; set; }

		public MainModel(MainForm form)
		{
			_form = form;
			_view = form.View;
			TableDatas = new List<TableData>();
		}

		public void Set(ProjectData data)
		{
			// Model
			TableDatas = data.TableDatas;

			// View
			foreach(TableData tableData in TableDatas)
			{
				_view.AddTableButton(tableData.Descriptor.Name);
			}
		}

		public void AddNewTable()
		{
			var dialog = new AddTableDialog();
			int tablesCount = TableDatas.Count;
			dialog.Init(this, _view, GetRandomTableName());

			dialog.ShowDialog();
			var result = dialog.Result;

			if(result.Success)
			{
				var tableForm = new TableForm();
				tableForm.Init(
					result.Value,
					_form.TableForm_FormClosed,
					_form.TableForm_ModelChanged);
				tableForm.MdiParent = _form;

				TableDatas.Add(TableData.Get(tableForm));

				_view.AddTableButton(result.Value.Name);

				_form.IsFileSaved = false;

				tableForm.Show();
			}
		}

		private string GetRandomTableName()
		{
			string tableName = "";

			do
			{
				tableName = $"Таблица [{Guid.NewGuid().ToString().Substring(0, 8)}]";
			}
			while (TableDatas.Any(x => x.Descriptor.Name == tableName));

			return tableName;
		}

		/*
			Создание и открытие окна таблицы, TableData которой уже есть в MainModel
			*/
		public void AddExistingTable(TableData tableData)
		{
			TableForm tableForm = new TableForm();
			tableForm.Init(
				tableData,
				_form.TableForm_FormClosed,
				_form.TableForm_ModelChanged);
			tableForm.MdiParent = _form;

			tableForm.Show();
		}

		public void UpdateTableData(TableData tableData)
		{
			var index = TableDatas.FindIndex(x => x.Descriptor.Name == tableData.Descriptor.Name);
			TableDatas[index] = tableData;
		}

		public ProjectData Update()
		{
			foreach (TableForm form in _form.MdiChildren)
			{
				UpdateTableData(TableData.Get(form));
			}

			return new ProjectData(TableDatas);
		}

		public void RenameTable(string oldName, string newName)
		{
			TableDatas.ToList()
				.Where(x => x.Descriptor.Name == oldName)
				.ElementAt(0).Descriptor.Name = newName;

			_form.MdiChildren.ToList()
				.Where(x => x.Text == oldName)
				.ElementAt(0).Text = newName;

			_form.FLPanel_Tables.Controls.Cast<TableButton>()
				.Where(x => x.Text == oldName)
				.ElementAt(0).Text = newName;
		}

		public void DeleteTable(string tableName)
		{
			// from form (mdi childs)
			_form.MdiChildren.ToList()
				.Where(x => x.Text == tableName)
				.ElementAt(0)
				.Close();

			// from tableButtons panel
			int index = _form.FLPanel_Tables.Controls.Cast<TableButton>().ToList().FindIndex(x => x.Text == tableName);
			_form.FLPanel_Tables.Controls.RemoveAt(index);

			// from model
			index = TableDatas.FindIndex(x => x.Descriptor.Name == tableName);
			TableDatas.RemoveAt(index);

			_form.IsFileSaved = false;
		}
	}
}

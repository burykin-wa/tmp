using Telerik.Web.UI;
using System;
using System.Collections;
using System.Web;
using Telerik.Web.UI.PersistenceFramework;
using System.Linq;
using System.Collections.Generic;
using TelerikWebFormsDemos.DataAccess.Services.Telerik;
using TelerikWebFormsDemos.DataAccess.Dto.Telerik;

namespace Telerik.GridExamplesCSharp.Overview
{
	public partial class DefaultCS : System.Web.UI.Page
	{
        private CarService CarSvc
        {
            get
            {
                return new CarService();
            }
        }

        protected string NormalizeValue(object inputValue)
		{
			return inputValue.ToString().Replace(" ", "");
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			RadPersistenceManager1.StorageProvider = new SessionStorageProvider(RadPersistenceManager1.StorageProviderKey);

			GridFilterMenu filterMenu = RadGrid1.FilterMenu;

			int currentItemIndex = 0;
			while (currentItemIndex < filterMenu.Items.Count)
			{
				RadMenuItem item = filterMenu.Items[currentItemIndex];
				if (item.Text.Contains("Empty") || item.Text.Contains("Null"))
				{
					filterMenu.Items.Remove(item);
				}
				else currentItemIndex++;
			}
		}

		protected string GetCarImageUrl(object container)
		{
			Hashtable values = new Hashtable();
			(container as GridNestedViewItem).ParentItem.ExtractValues(values);
			return String.Format("~/Grid/Examples/Overview/Images/Cars/{0}_{1}.png", NormalizeValue(values["BrandName"]), NormalizeValue(values["Model"]));
		}

		protected void RadGrid1_ItemCommand(object sender, GridCommandEventArgs e)
		{
			if (e.CommandName == "RowClick" || e.CommandName == "ExpandCollapse")
			{
				bool lastState = e.Item.Expanded;

				if (e.CommandName == "ExpandCollapse")
				{
					lastState = !lastState;
				}

				CollapseAllRows();
				e.Item.Expanded = !lastState;
			}
			if (e.CommandName == "UpdateCount")
			{
				UpdateCarRentCounter(e.CommandArgument.ToString());
			}
		}

		private void CollapseAllRows()
		{
			foreach (GridItem item in RadGrid1.MasterTableView.Items)
			{
				item.Expanded = false;
			}
		}

		protected void BrandNameCombo_DataBound(object sender, EventArgs e)
		{
			RadComboBox combo = sender as RadComboBox;
			foreach (RadComboBoxItem item in combo.Items)
			{
				item.ImageUrl = String.Format("~/Grid/Examples/Overview/Images/SmallLogos/{0}.png", NormalizeValue(item.Text));
			}
		}

		protected void SaveSettingsButton_Click(object sender, EventArgs e)
		{
			RadPersistenceManager1.SaveState();
		}

		protected void LoadSettingsButton_Click(object sender, EventArgs e)
		{
			RadPersistenceManager1.LoadState();
			RadGrid1.Rebind();
		}

		private void UpdateCarRentCounter(string carID)
		{
			int carIdValue;
			if (!Int32.TryParse(carID, out carIdValue))
			{
				return;
			}

            CarDto targetCar = CarSvc.GetCarById(carIdValue);

            if (targetCar != null)
            {
                targetCar.RentedCount++;
            }

            CarSvc.UpdateCar(targetCar);

            RadGrid1.Rebind();
		}

		protected void RadGrid1_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
		{
            RadGrid1.DataSource = CarSvc.GetCars();
        }

        protected void BrandNameCombo_DataBinding(object sender, EventArgs e)
        {
            List<CarBrand> brandsList = CarSvc.GetCars().Select(car => car.BrandName).Distinct().OrderBy(name => name).Select(name => new CarBrand()
            {
                BrandName = name
            }).ToList();

            (sender as RadComboBox).DataSource = brandsList;
        }
    }

    class CarBrand
    {
        public string BrandName { get; set; }
    }

    public class SessionStorageProvider : IStateStorageProvider
	{
		private System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;
		string storageKey;

		public SessionStorageProvider(string key)
		{
			storageKey = key;
		}

		public void SaveStateToStorage(string key, string serializedState)
		{
			session[storageKey] = serializedState;
		}

		public string LoadStateFromStorage(string key)
		{
			if (session[key] == null)
			{
				return String.Empty;
			}
			return session[key].ToString();
		}
	}
}

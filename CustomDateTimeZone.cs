using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Collections;
using System.Web.UI.WebControls;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using ListItem = Sitecore.Web.UI.HtmlControls.ListItem;

namespace Sitecore.DateTimeZoneField
{
    public class CustomDateTimeZone : Input, IContentField
    {
        private DateTimePicker _picker;
        private Combobox _timeZone;

        public string ItemID
        {
            get
            {
                return this.GetViewStateString("ItemID");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.SetViewStateString("ItemID", value);
            }
        }

        public string RealValue
        {
            get
            {
                return this.GetViewStateString("RealValue");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.SetViewStateString("RealValue", value);
            }
        }

        public bool IsModified
        {
            get
            {
                return System.Convert.ToBoolean(this.ServerProperties["IsModified"]);
            }
            protected set
            {
                this.ServerProperties["IsModified"] = value ? 1 : 0;
            }
        }

        public CustomDateTimeZone()
        {
            this.Class = "scContentControl";
            this.Change = "#";
            this.Activation = true;
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull((object)message, "message");
            base.HandleMessage(message);
            if (message["id"] != this.ID)
                return;
            switch (message.Name)
            {
                case "contentdate:today":
                    this.Today();
                    break;
                case "contentdate:clear":
                    this.ClearField();
                    break;
            }
        }

        public string GetValue()
        {
            if (_picker == null || !IsModified) 
                return RealValue;
            // Ajust for time zone
            if (string.IsNullOrWhiteSpace(_picker.Value))
                return _picker.Value;
            var selectedDate = DateUtil.ParseDateTime(_picker.Value, System.DateTime.MinValue);
            if (selectedDate == System.DateTime.MinValue)
                return _picker.Value;

            var id = _timeZone.SelectedItem.Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(id);
                selectedDate = TimeZoneInfo.ConvertTimeToUtc(selectedDate, timezone);
            }
            return DateUtil.ToIsoDate(selectedDate);
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            this.RealValue = value;
            SetPickerValue(value);
        }

        protected void SetPickerValue(string realValue)
        {
            if (_picker == null)
                return;

            var id = _timeZone.SelectedItem.Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var selectedDate = DateUtil.ParseDateTime(realValue, System.DateTime.MinValue);
                if (selectedDate != System.DateTime.MinValue)
                {
                    var timezone = TimeZoneInfo.FindSystemTimeZoneById(id);
                    selectedDate = TimeZoneInfo.ConvertTimeFromUtc(selectedDate, timezone);
                    realValue = DateUtil.ToIsoDate(selectedDate);
                }
            }

            _picker.Value = realValue;
        }

        protected override Item GetItem()
        {
            return Client.ContentDatabase.GetItem(this.ItemID);
        }

        protected override bool LoadPostData(string value)
        {
            if (!base.LoadPostData(value))
                return false;
            _picker.Value = value ?? string.Empty;
            return true;
        }

        protected override void OnInit(EventArgs e)
        {
            _picker = new DateTimePicker();
            _picker.ID = this.ID + "_picker";
            this.Controls.Add(_picker);
            if (!string.IsNullOrEmpty(this.RealValue))
                _picker.Value = this.RealValue;
            _picker.Changed += (EventHandler)((param0, param1) => this.SetModified());
            _picker.ShowTime = true;
            _picker.Disabled = this.Disabled;

            _timeZone = new Combobox();
            _timeZone.ID = this.ID + "_timezone";
            this.Controls.Add(_timeZone);
            _picker.Changed += (EventHandler)((param0, param1) => this.SetModified());
            _timeZone.Disabled = this.Disabled;

            var profile = Sitecore.Context.User.Profile;
            var profileTimeZone = profile.GetCustomProperty("TimeZone");
            foreach (var zoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                var li = new ListItem();
                li.ID = zoneInfo.Id;
                li.Header = zoneInfo.DisplayName;
                li.Value = zoneInfo.Id;
                li.Selected = zoneInfo.Id == profileTimeZone;
                _timeZone.Controls.Add(li);
            }

            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.ServerProperties["Value"] = this.ServerProperties["Value"];
            this.ServerProperties["RealValue"] = this.ServerProperties["RealValue"];
        }

        protected override void SetModified()
        {
            base.SetModified();
            this.IsModified = true;
            if (!this.TrackModified)
                return;
            Sitecore.Context.ClientPage.Modified = true;
        }

        protected virtual string GetCurrentDate()
        {
            return DateUtil.ToIsoDate(System.DateTime.Today);
        }

        private void ClearField()
        {
            this.SetRealValue(string.Empty);
        }

        protected void SetRealValue(string realvalue)
        {
            if (realvalue != this.RealValue)
                this.SetModified();
            this.RealValue = realvalue;
            SetPickerValue(realvalue);
        }

        private void Today()
        {
            this.SetRealValue(this.GetCurrentDate());
        }
    }
}
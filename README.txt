DateTime with TimeZone field for Sitecore

See this blog post on how it works:
http://mikael.com/2014/07/sitecore-date-time-picker-with-time-zone/

* Compile the control and add it to your solution. 
* Add the control to your controlSources section by adding the DateTimeZone.config patch file
* Create a new field in Sitecore Core database, by duplicating the Datetime in 
core:/sitecore/system/Field types/Simple Types/Datetime. Give the copy a suitable name, such as DatetimeZone and chnage the Control field to "dateTimeZone:CustomDateTimeZone".


﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RApID_Project_WPF.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int MaxSonarBeams {
            get {
                return ((int)(this["MaxSonarBeams"]));
            }
            set {
                this["MaxSonarBeams"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\"\"")]
        public string SPPortName {
            get {
                return ((string)(this["SPPortName"]));
            }
            set {
                this["SPPortName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("9600")]
        public int SPBaudRate {
            get {
                return ((int)(this["SPBaudRate"]));
            }
            set {
                this["SPBaudRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Even")]
        public global::System.IO.Ports.Parity SPParity {
            get {
                return ((global::System.IO.Ports.Parity)(this["SPParity"]));
            }
            set {
                this["SPParity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7")]
        public int SPDataBit {
            get {
                return ((int)(this["SPDataBit"]));
            }
            set {
                this["SPDataBit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("One")]
        public global::System.IO.Ports.StopBits SPStopBit {
            get {
                return ((global::System.IO.Ports.StopBits)(this["SPStopBit"]));
            }
            set {
                this["SPStopBit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\joi\\eu\\Public\\EE Process Test\\Logs\\RApID\\")]
        public string LogWriteLocation {
            get {
                return ((string)(this["LogWriteLocation"]));
            }
            set {
                this["LogWriteLocation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\eufs04\\EU_B3_Repair label printer")]
        public string PrinterToUse {
            get {
                return ((string)(this["PrinterToUse"]));
            }
            set {
                this["PrinterToUse"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int PrinterXOffset {
            get {
                return ((int)(this["PrinterXOffset"]));
            }
            set {
                this["PrinterXOffset"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int PrinterYOffset {
            get {
                return ((int)(this["PrinterYOffset"]));
            }
            set {
                this["PrinterYOffset"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool PrinterInitSetup {
            get {
                return ((bool)(this["PrinterInitSetup"]));
            }
            set {
                this["PrinterInitSetup"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AccessDatabase {
            get {
                return ((bool)(this["AccessDatabase"]));
            }
            set {
                this["AccessDatabase"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://eudb01/Reports/Pages/Folder.aspx?ItemPath=%2fRApID+(Repair+Database)&ViewM" +
            "ode=List")]
        public string DefaultReportManagerLink {
            get {
                return ((string)(this["DefaultReportManagerLink"]));
            }
            set {
                this["DefaultReportManagerLink"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=eudb01;Initial Catalog=Repair;Persist Security Info=True;User ID=inho" +
            "use;Password=Password1")]
        public string RepairConnectionString {
            get {
                return ((string)(this["RepairConnectionString"]));
            }
        }
    }
}

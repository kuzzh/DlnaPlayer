﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DlnaPlayerApp.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DlnaPlayerApp.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!DOCTYPE html&gt;
        ///&lt;html lang=&quot;en&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;meta charset=&quot;UTF-8&quot; /&gt;
        ///    &lt;meta name=&quot;viewport&quot; content=&quot;width=device-width, initial-scale=1.0&quot; /&gt;
        ///    &lt;title&gt;DLNA Player Web&lt;/title&gt;
        ///    &lt;script src=&quot;https://cdn.jsdelivr.net/npm/vue@2&quot;&gt;&lt;/script&gt;
        ///    &lt;style&gt;
        ///        body {
        ///            font-family: &apos;Microsoft YaHei&apos;, Arial, sans-serif;
        ///            background-color: #f4f4f4;
        ///            margin: 0;
        ///            padding: 0;
        ///        }
        ///
        ///        .container {
        ///            width: 95%;
        ///            margin: [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string control {
            get {
                return ResourceManager.GetString("control", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #user  nobody;
        ///worker_processes  1;
        ///
        ///#error_log  logs/error.log;
        ///#error_log  logs/error.log  notice;
        ///#error_log  logs/error.log  info;
        ///
        ///#pid        logs/nginx.pid;
        ///
        ///
        ///events {
        ///    worker_connections  1024;
        ///}
        ///
        ///
        ///http {
        ///    include       mime.types;
        ///    default_type  application/octet-stream;
        ///
        ///    #log_format  main  &apos;$remote_addr - $remote_user [$time_local] &quot;$request&quot; &apos;
        ///    #                  &apos;$status $body_bytes_sent &quot;$http_referer&quot; &apos;
        ///    #                  &apos;&quot;$http_user_agent&quot; &quot;$http_x_fo [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NginxConf {
            get {
                return ResourceManager.GetString("NginxConf", resourceCulture);
            }
        }
    }
}

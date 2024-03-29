﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExhaustiveMatching.Analyzer.Enums {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ExhaustiveMatching.Analyzer.Enums.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to A switch on an enum type marked as exhaustive by throwing InvalidEnumArgumentException is not exhustive. It omits one or more enum values from the cases..
        /// </summary>
        internal static string EM0001Description {
            get {
                return ResourceManager.GetString("EM0001Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enum value not handled by switch &apos;{0}&apos;.
        /// </summary>
        internal static string EM0001Message {
            get {
                return ResourceManager.GetString("EM0001Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Switch on Enum Not Exhaustive.
        /// </summary>
        internal static string EM0001Title {
            get {
                return ResourceManager.GetString("EM0001Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A switch on a nullable enum type marked as exhaustive by throwing InvalidEnumArgumentException is not exhustive. It omits null from the cases..
        /// </summary>
        internal static string EM0002Description {
            get {
                return ResourceManager.GetString("EM0002Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;null&apos; value not handled by switch.
        /// </summary>
        internal static string EM0002Message {
            get {
                return ResourceManager.GetString("EM0002Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Switch on Nullable Enum Not Exhaustive.
        /// </summary>
        internal static string EM0002Title {
            get {
                return ResourceManager.GetString("EM0002Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This case patterns are not supported in exhaustive switchs on enum types..
        /// </summary>
        internal static string EM0101Description {
            get {
                return ResourceManager.GetString("EM0101Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Case pattern not supported in exhaustive switch on enum type &apos;{0}&apos;.
        /// </summary>
        internal static string EM0101Message {
            get {
                return ResourceManager.GetString("EM0101Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Case Pattern Not Supported.
        /// </summary>
        internal static string EM0101Title {
            get {
                return ResourceManager.GetString("EM0101Title", resourceCulture);
            }
        }
    }
}

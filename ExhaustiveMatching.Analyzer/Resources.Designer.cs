﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExhaustiveMatching.Analyzer {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ExhaustiveMatching.Analyzer.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to An switch statement on an enum type marked as exhustive by throwing InvalidEnumArgumentException or ExhaustiveMatchFailedException is not exhustive. It omits one or more enum values from the cases..
        /// </summary>
        internal static string EM0001Description {
            get {
                return ResourceManager.GetString("EM0001Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enum value not handled by switch: {0}.
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
        ///   Looks up a localized string similar to An switch statement on a closed type marked as exhaustive by throwing ExhaustiveMatchFailedException is not exhaustive. It will fail to match one or more possible types..
        /// </summary>
        internal static string EM0002Description {
            get {
                return ResourceManager.GetString("EM0002Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subtype not handled by switch: {0}.
        /// </summary>
        internal static string EM0002Message {
            get {
                return ResourceManager.GetString("EM0002Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Switch on Closed Type Not Exhaustive.
        /// </summary>
        internal static string EM0002Title {
            get {
                return ResourceManager.GetString("EM0002Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A subtype of a type marked with the &quot;Closed&quot; attribute must be a case of the closed type..
        /// </summary>
        internal static string EM0011Description {
            get {
                return ResourceManager.GetString("EM0011Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is not a case of its closed supertype: {1}.
        /// </summary>
        internal static string EM0011Message {
            get {
                return ResourceManager.GetString("EM0011Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subtype of a Closed Type Must Be a Case.
        /// </summary>
        internal static string EM0011Title {
            get {
                return ResourceManager.GetString("EM0011Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A closed type&apos;s case must be a direct subtype of the closed type. That is, it must inherit from the class or implement the interface..
        /// </summary>
        internal static string EM0012Description {
            get {
                return ResourceManager.GetString("EM0012Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Closed type case is not a direct subtype: {0}.
        /// </summary>
        internal static string EM0012Message {
            get {
                return ResourceManager.GetString("EM0012Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Closed Type Case Must Be a Direct Subtype.
        /// </summary>
        internal static string EM0012Title {
            get {
                return ResourceManager.GetString("EM0012Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A closed type&apos;s case must be a direct subtype of the closed type. That is, it must inherit from the class or implement the interface..
        /// </summary>
        internal static string EM0013Description {
            get {
                return ResourceManager.GetString("EM0013Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Closed type case is not a subtype: {0}.
        /// </summary>
        internal static string EM0013Message {
            get {
                return ResourceManager.GetString("EM0013Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Closed Type Case Must Be a Subtype.
        /// </summary>
        internal static string EM0013Title {
            get {
                return ResourceManager.GetString("EM0013Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exhustive switch statement does not support case clauses with &quot;when&quot; guards.
        /// </summary>
        internal static string EM0100Description {
            get {
                return ResourceManager.GetString("EM0100Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When guard is not supported in an exhaustive switch.
        /// </summary>
        internal static string EM0100Message {
            get {
                return ResourceManager.GetString("EM0100Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When Guards Not Supported.
        /// </summary>
        internal static string EM0100Title {
            get {
                return ResourceManager.GetString("EM0100Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This type of case clause is not supported in exhaustive switch statements. Only &quot;case Type name:&quot; and &quot;case null:&quot; are supported..
        /// </summary>
        internal static string EM0101Description {
            get {
                return ResourceManager.GetString("EM0101Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Case clause type not supported in exhaustive switch: {0}.
        /// </summary>
        internal static string EM0101Message {
            get {
                return ResourceManager.GetString("EM0101Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Case Clause Type Not Supported.
        /// </summary>
        internal static string EM0101Title {
            get {
                return ResourceManager.GetString("EM0101Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exhuastive switch on an open type is not supported. The expression being switched on must be an enum or closed..
        /// </summary>
        internal static string EM0102Description {
            get {
                return ResourceManager.GetString("EM0102Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exhaustive switch must be on enum or closed type, was on: {0}.
        /// </summary>
        internal static string EM0102Message {
            get {
                return ResourceManager.GetString("EM0102Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open Type Not Supported.
        /// </summary>
        internal static string EM0102Title {
            get {
                return ResourceManager.GetString("EM0102Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Case clauses must match on types that are case types inheriting from the type being matched on.
        /// </summary>
        internal static string EM0103Description {
            get {
                return ResourceManager.GetString("EM0103Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type is not a case type inheriting from type being matched: {0}.
        /// </summary>
        internal static string EM0103Message {
            get {
                return ResourceManager.GetString("EM0103Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Match Must Be On Case Type.
        /// </summary>
        internal static string EM0103Title {
            get {
                return ResourceManager.GetString("EM0103Title", resourceCulture);
            }
        }
    }
}

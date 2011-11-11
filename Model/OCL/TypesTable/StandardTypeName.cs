 

using System;
using System.Collections.Generic;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.TypesTable{
   partial class Library{
     public class StandardTypeName {
	 			
	 	private string _Integer = "Integer";
		/// <summary>
		/// Name of type Integer from OCL Standard library in curent library;
		/// </summary>
        public string Integer {
             get {
                    return _Integer;
            }
             set {
                 _Integer = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Real = "Real";
		/// <summary>
		/// Name of type Real from OCL Standard library in curent library;
		/// </summary>
        public string Real {
             get {
                    return _Real;
            }
             set {
                 _Real = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _UnlimitedNatural = "UnlimitedNatural";
		/// <summary>
		/// Name of type UnlimitedNatural from OCL Standard library in curent library;
		/// </summary>
        public string UnlimitedNatural {
             get {
                    return _UnlimitedNatural;
            }
             set {
                 _UnlimitedNatural = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _String = "String";
		/// <summary>
		/// Name of type String from OCL Standard library in curent library;
		/// </summary>
        public string String {
             get {
                    return _String;
            }
             set {
                 _String = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Boolean = "Boolean";
		/// <summary>
		/// Name of type Boolean from OCL Standard library in curent library;
		/// </summary>
        public string Boolean {
             get {
                    return _Boolean;
            }
             set {
                 _Boolean = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Invalid = "Invalid";
		/// <summary>
		/// Name of type Invalid from OCL Standard library in curent library;
		/// </summary>
        public string Invalid {
             get {
                    return _Invalid;
            }
             set {
                 _Invalid = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Any = "Any";
		/// <summary>
		/// Name of type Any from OCL Standard library in curent library;
		/// </summary>
        public string Any {
             get {
                    return _Any;
            }
             set {
                 _Any = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Message = "Message";
		/// <summary>
		/// Name of type Message from OCL Standard library in curent library;
		/// </summary>
        public string Message {
             get {
                    return _Message;
            }
             set {
                 _Message = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Void = "Void";
		/// <summary>
		/// Name of type Void from OCL Standard library in curent library;
		/// </summary>
        public string Void {
             get {
                    return _Void;
            }
             set {
                 _Void = value;
				 isNameChange = true;
             }
        }
		
				
	 	private string _Type = "Type";
		/// <summary>
		/// Name of type Type from OCL Standard library in curent library;
		/// </summary>
        public string Type {
             get {
                    return _Type;
            }
             set {
                 _Type = value;
				 isNameChange = true;
             }
        }
		
				
		bool isNameChange = true;
		
		HashSet<string> usedName;
		
		public bool IsNameUsed(string name){
			if(isNameChange){
				usedName = new HashSet<string>(new string[] {
												_Integer,_Real,_UnlimitedNatural,_String,_Boolean,_Invalid,_Any,_Message,_Void,_Type				});
				isNameChange = false;
			}
			return usedName.Contains(name);
		}
		
	 }
	 
	 	 public Classifier Integer {
            get {
                return RootNamespace.NestedClassifier[TypeName.Integer];
            }
        }
		 public Classifier Real {
            get {
                return RootNamespace.NestedClassifier[TypeName.Real];
            }
        }
		 public Classifier UnlimitedNatural {
            get {
                return RootNamespace.NestedClassifier[TypeName.UnlimitedNatural];
            }
        }
		 public Classifier String {
            get {
                return RootNamespace.NestedClassifier[TypeName.String];
            }
        }
		 public Classifier Boolean {
            get {
                return RootNamespace.NestedClassifier[TypeName.Boolean];
            }
        }
		 public Classifier Invalid {
            get {
                return RootNamespace.NestedClassifier[TypeName.Invalid];
            }
        }
		 public Classifier Any {
            get {
                return RootNamespace.NestedClassifier[TypeName.Any];
            }
        }
		 public Classifier Message {
            get {
                return RootNamespace.NestedClassifier[TypeName.Message];
            }
        }
		 public Classifier Void {
            get {
                return RootNamespace.NestedClassifier[TypeName.Void];
            }
        }
		 public Classifier Type {
            get {
                return RootNamespace.NestedClassifier[TypeName.Type];
            }
        }
	  }
}
 

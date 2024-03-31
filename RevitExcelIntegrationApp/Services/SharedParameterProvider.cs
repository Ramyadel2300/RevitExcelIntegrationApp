using Autodesk.Revit.DB;
using PdfSharp.Pdf;
using System;
using System.IO;

namespace RevitExcelIntegrationApp.Services
{
    internal class SharedParameterProvider
    {

        //shared parameters File procedures

        /* Shared Parameter Structure
         *  - Shared Parameter:Shared parameters are definitions of parameters that can be shared to different families or projects.  
         *  - In Revit APi:presnted Using DefinitionFile Class
         *  - Each shared paramter contains groups, this Groupes is identified in Revit APi Using DefinitionGroup Class
         *  - Each Group has Parameters, each parameter is repesented in Revit APi using Definition class  
         *  - DefinitionFile 
         *      --> DefinitionGroup
         *          --> Definition
         */

        /// <summary>
        /// 1- Get Shared parameter File from a given path 
        /// Definition File:The DefinitionFile object represents a shared parameters file on disk.
        /// </summary>
        /// <param name="app">this parameter is used to Open the shared parameter file </param>
        /// <param name="path">the path od the shared parameter text file</param>
        /// <returns></returns>
        public static DefinitionFile GetOrCreateSharedParamsFile(Autodesk.Revit.ApplicationServices.Application app, string path)
        {
            try
            {
                if (path != app.SharedParametersFilename || !File.Exists(path))
                {
                    new StreamWriter(path).Close();
                    app.SharedParametersFilename = path;
                }
                return app.OpenSharedParameterFile();
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 2- getting the each group from the shared parameter file
        /// any Parameter File/Definition File Contains Groups
        /// </summary>
        /// <param name="definitionFile">the shared parameter after is being opened</param>
        /// <param name="groupName">the group name you need to get from the groups exist in the definition file </param>
        /// <returns></returns>
        public static DefinitionGroup GetOrCreateSharedParamsGroup(DefinitionFile definitionFile, string groupName)
        {
            try
            {
                DefinitionGroup Group = definitionFile.Groups.get_Item(groupName);//getting Group name 
                                                                                  //if there is no group with this name, create a new group with this name 
                if (Group == null)
                {
                    Group = definitionFile.Groups.Create(groupName);
                }
                return Group;//return this group
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// this method is made to retrieve the paramter from the shared paremter file or create a new one if there is not parameter with this name 
        /// </summary>
        /// <param name="defGrp">the definition group that has all the groups that contains all the parameters </param>
        /// <param name="parType">this parameter is used when there is no paramter whith this name, then we need to create a new paramter</param>
        /// <param name="parName">this paramter is used to get the paramter from the Definition group</param>
        /// <param name="visible">set this new pramarater to be cisible or not</param>
        /// <returns></returns>
        public static Definition GetOrCreateSharedParamDefinition(DefinitionGroup defGrp, ForgeTypeId parTypeId, string parName, bool visible)
        {
            try
            {
                //getting the proposed parameter
                Definition Parameter = defGrp.Definitions.get_Item(parName);
                //if there is no Parameter with this name, create a new Parameter with this name with the specific type gieb in the method parameter
                if (Parameter == null)
                {               
                   //ExternalDefinitionCreationOptions:An option class used for creating a new shared parameter definition, including options such as name, type, visibility, Guid description and modifiable flag.
                   ExternalDefinitionCreationOptions NewlyCreatedParameter = new ExternalDefinitionCreationOptions(parName, parTypeId);
                    NewlyCreatedParameter.Visible = visible;
                    NewlyCreatedParameter.UserModifiable = true;
                    NewlyCreatedParameter.HideWhenNoValue = false;
                    NewlyCreatedParameter.Description = "The Price Parameter which is used when Revit-Excel Application is Executed";
                    defGrp.Definitions.Create(NewlyCreatedParameter);//return the newly created paramter
                    Parameter = defGrp.Definitions.get_Item(parName);
                    Parameter.GetGroupTypeId();
                }
                return Parameter;//return the existed parameter
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// here we are binding a given value to be set as a Revit Shared Paramter this process is called Binding 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="myGroup"></param>
        /// <param name="paraName"></param>
        /// <param name="category"></param>
        /// <param name="paraType"></param>
        /// <param name="isParentGroupIFC"></param>
        /// <returns></returns>
        public static bool SetBinding(Document document, DefinitionGroup myGroup, string paraName, BuiltInCategory category, ForgeTypeId parTypeId, bool visible)
        {
            Definition GetorCreateSharedParamDefinition = GetOrCreateSharedParamDefinition(myGroup, parTypeId, paraName, visible);//our method to retrieve the defintion
                                                                                                                                        //if there is no parameter with the given data 
            if (GetorCreateSharedParamDefinition == null)//some how Revit needs to execut this method once again to add the parameter
            {
                return false;
            }
            //then we will insert 
            Category ElementCategoty = document.Settings.Categories.get_Item(category);//get the element category in which we will set this Value into it  
                                                                                       //the next lines is ued to set the new Paramter value
            CategorySet CategorySet = document.Application.Create.NewCategorySet();
            var isCategoryInserted = CategorySet.Insert(ElementCategoty);
            InstanceBinding val = document.Application.Create.NewInstanceBinding(CategorySet);
            var isBinded = document.ParameterBindings.Insert(GetorCreateSharedParamDefinition, (Binding)(object)val);
            return isBinded;
        }
        //this method is consisdered to be the gate or the main method to triger the loading of the shared paramter file 
        public static void AddSharedParameters(Document document, BuiltInCategory builtCat, string paraGroupName, string path, string ParaName, bool visible=true)
        {
            var app = document.Application;
            DefinitionFile sharedParamsFile = GetOrCreateSharedParamsFile(app, path);
            DefinitionGroup definitionGroup = null;
            if (sharedParamsFile != null)
            {
                definitionGroup = GetOrCreateSharedParamsGroup(sharedParamsFile, paraGroupName);
            }
            if (definitionGroup == null)
            {
                return;
            }
            bool flag = false;

            //i think the following Lines Invistigate if this Defintion/Paramter allready exsits in the element

            //DefinitionBindingMapIterator:An iterator to a parameter definition within parameters binding map.
            DefinitionBindingMapIterator definitionBindingMapIterator = document.ParameterBindings.ForwardIterator();
            while (definitionBindingMapIterator.MoveNext())
            {
                Definition definition = definitionBindingMapIterator.Key;//Key:Retrieves the definition that is the current focus of the iterator.
                CategorySetIterator definitionCategoriesIterator = ((ElementBinding)definitionBindingMapIterator.Current).Categories.ForwardIterator();
                while (definitionCategoriesIterator.MoveNext())
                {
                    object current = definitionCategoriesIterator.Current;
                    Category definitionCategory = current as Category;
                    if (definitionCategory != null && definitionCategory.Id.Value == (int)builtCat)
                    {
                        if (definition.Name == ParaName)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            //if not set the paramter 
            if (!flag)
            {
                bool isBinded = SetBinding(document, definitionGroup, ParaName, builtCat, SpecTypeId.Number,visible);
            }
        }
    }
}

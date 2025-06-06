﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyRevitCommands
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Sheets : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get Family Symbol
            

            try
            {
                using (Transaction trans = new Transaction(doc, "Create Sheet"))
                {
                    trans.Start();

                    //Create Sheet
                    

                    trans.Commit();
                }
                
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            
        }
    }
}

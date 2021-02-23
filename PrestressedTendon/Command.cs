using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace PrestressedTendon
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document revitDoc = commandData.Application.ActiveUIDocument.Document;  //取得文档           
            Application revitApp = commandData.Application.Application;             //取得应用程序            
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;           //取得当前活动文档        

            UIApplication uiApp = commandData.Application;

            //载入族轮廓并激活
            string file1 = @"C:\Users\zyx\Desktop\2RevitArcBridge\Test\Test\xxxx.rfa";//轮廓族的文件路径来这里输入一下
            string file2 = @"C:\Users\zyx\Desktop\2RevitArcBridge\Test\Test\aaaa.rfa";//轮廓族的文件路径来这里输入一下

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            IList<Reference> modelLines = null;

            //新建一个窗口
            Window1 window1 = new Window1();

            if (window1.ShowDialog() == true)
            {
                //窗口打开并停留，只有点击按键之后，窗口关闭并返回true
            }


            //按键会改变window的属性，通过对属性的循环判断来实现对按键的监测
            while (!window1.Done)
            {
                //选择平曲线
                if (window1.FlatCurveSelected)
                {

                    //因为要对原有模型线进行一个删除是对文件进行一个删除，故要创建一个事件
                    using (Transaction transaction = new Transaction(uiDoc.Document))
                    {
                        transaction.Start("选择平曲线");

                        //选择平曲线
                        Selection sel = uiDoc.Selection;
                        modelLines = sel.PickObjects(ObjectType.Element, "选一组模型线");

                        //2、重置window1.FlatCurve

                        window1.FlatCurveSelected = false;


                        transaction.Commit();
                    }

                }

                if (window1.ShowDialog() == true)
                {
                    //窗口打开并停留，只有点击按键之后，窗口关闭并返回true

                }


            }



            List<FamilySymbol> fList1 = new List<FamilySymbol>();
            List<FamilySymbol> fList2 = new List<FamilySymbol>();
            //对每条模型线进行放样拉伸
            foreach (Reference reference in modelLines)
            {
                Element elem = revitDoc.GetElement(reference);
                CurveElement curveElement = elem as CurveElement;

                //在项目中创建公制常规模型
                FamilySymbol familySymbol1 = createSweepFamilySymbol(commandData, curveElement, file1, true);
                FamilySymbol familySymbol2 = createSweepFamilySymbol(commandData, curveElement, file2, false);

                fList1.Add(familySymbol1);
                fList2.Add(familySymbol2);
            }



            //族实例激活并放到指定位置；剪切
            for (int i = 0; i < fList1.Count(); i++)
            {
                FamilyInstance familyInstance2;
                string tName = i + "族实例放样";
                using (Transaction transaction = new Transaction(revitDoc))
                {
                    transaction.Start(tName);

                    //在项目中激活族并放置到位
                    fList1[i].Activate();
                    fList2[i].Activate();

                    FamilyInstance familyInstance1 = revitDoc.Create.NewFamilyInstance(XYZ.Zero, fList1[i], Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    familyInstance2 = revitDoc.Create.NewFamilyInstance(XYZ.Zero, fList2[i], Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    transaction.Commit();
                }

                tName = i + "剪切";
                using (Transaction transaction = new Transaction(revitDoc))
                {
                    transaction.Start(tName);

                    Selection selend = uiDoc.Selection;
                    Reference refend = selend.PickObject(ObjectType.Element, "选择族实例");
                    Element elemet1 = revitDoc.GetElement(refend);
                    Element element2 = revitDoc.GetElement(familyInstance2.Id);

                    InstanceVoidCutUtils.AddInstanceVoidCut(revitDoc, elemet1, element2);//空心剪切

                    transaction.Commit();
                }




            }


            return Result.Succeeded;

        }








        private FamilySymbol createSweepFamilySymbol(ExternalCommandData commandData, CurveElement curveElement, string filePath, bool v)
        {
            Document revitDoc = commandData.Application.ActiveUIDocument.Document;  //取得文档           
            Application revitApp = commandData.Application.Application;             //取得应用程序            
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;           //取得当前活动文档        

            //创建一个族文档
            Document familyDoc = revitDoc.Application.NewFamilyDocument(@"C:\ProgramData\Autodesk\RVT 2020\Family Templates\Chinese\公制常规模型.rft");

            FamilySymbol profileFamilySymbol;

            //在族文件中载入轮廓族
            using (Transaction transaction = new Transaction(familyDoc))
            {
                transaction.Start("载入族");

                string ProfilePath = filePath;//轮廓族的路径来这里输入一下
                bool loadSuccess = familyDoc.LoadFamily(ProfilePath, out Family family);//在族文件中载入轮廓族
                string familyName = family.Name;
                //获取族文件的族类型
                if (loadSuccess)
                {
                    //假如成功导入
                    //得到族模板
                    ElementId elementId;
                    ISet<ElementId> symbols = family.GetFamilySymbolIds();
                    elementId = symbols.First();
                    profileFamilySymbol = familyDoc.GetElement(elementId) as FamilySymbol;

                }
                else
                {
                    //假如已经导入,则通过名字找到这个族
                    FilteredElementCollector collector = new FilteredElementCollector(familyDoc);
                    collector.OfClass(typeof(Family));//过滤得到文档中所有的族
                    IList<Element> families = collector.ToElements();
                    profileFamilySymbol = null;
                    foreach (Element e in families)
                    {

                        Family f = e as Family;
                        //通过名字进行筛选
                        if (f.Name == familyName)
                        {
                            profileFamilySymbol = familyDoc.GetElement(f.GetFamilySymbolIds().First()) as FamilySymbol;
                            break;
                        }
                    }

                }

                transaction.Commit();
            }

            //在族文件中拉伸放样，并进行一些参数设置
            using (Transaction transaction = new Transaction(familyDoc))
            {
                transaction.Start("内建模型");

                SweepProfile sweepProfile = familyDoc.Application.Create.NewFamilySymbolProfile(profileFamilySymbol);    //从轮廓族里面获取到轮廓

                //在族平面内画一条线，该线的位置与在项目文件中的位置完全一致
                Plane plane = curveElement.SketchPlane.GetPlane();
                SketchPlane sketchPlane = SketchPlane.Create(familyDoc, plane);
                ModelCurve modelCurve = familyDoc.FamilyCreate.NewModelCurve(curveElement.GeometryCurve, sketchPlane);
                //将线设置为放样路径
                ReferenceArray path = new ReferenceArray();
                path.Append(modelCurve.GeometryCurve.Reference);
                //创建放样
                Sweep sweep1 = familyDoc.FamilyCreate.NewSweep(v, path, sweepProfile, 0, ProfilePlaneLocation.Start);


                //设置该族文件中的空心放样可以去切割别的实例，为空心放样做准备
                Parameter p = familyDoc.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_ALLOW_CUT_WITH_VOIDS);
                p.Set(1);

                transaction.Commit();
            }

            //获取族类型
            Family loadFamily = familyDoc.LoadFamily(revitDoc);            //在项目中载入这个族
            FamilySymbol familySymbol = revitDoc.GetElement(loadFamily.GetFamilySymbolIds().First()) as FamilySymbol;//获取到组类型

            return familySymbol;

        }




    }
}

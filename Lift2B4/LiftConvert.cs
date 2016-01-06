﻿// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2015, SIL International. All Rights Reserved.
// <copyright from='2015' to='2015' company='SIL International'>
//		Copyright (c) 2015, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: LiftConvert.cs
// Responsibility: Greg Trihus
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using ICSharpCode.SharpZipLib.Zip;

namespace Lift2B4
{
    public class LiftConvert
    {
        private static string _fileName;
        private static string _listFullName;
        private static readonly XmlDocument LiftDoc = new XmlDocument();
        private static readonly XmlDocument ListDoc = new XmlDocument();
        private static readonly XslCompiledTransform Lift2B4X = new XslCompiledTransform();
        private static string _unitName;
        private static int _unit;
        private static int _lesson;
        private static string _langFolder;
        private const string BykiSchema = "byki_schema.xsd";
        private static string _folder;

        public LiftConvert(string fileName)
        {
            _fileName = fileName;
            var xsltSettings = new XsltSettings() { EnableDocumentFunction = true };
// ReSharper disable AssignNullToNotNullAttribute
            Lift2B4X.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "Lift2B4.Lift2B4x.xsl")), xsltSettings, null);
// ReSharper restore AssignNullToNotNullAttribute
            LiftDoc.Load(fileName);
            _unit = 0;
            _lesson = 0;
        }

        private readonly ArrayList _units = new ArrayList();
        public ArrayList Units()
        {
            var unitNodes = LiftDoc.SelectNodes(@"//field[@type='Semantic Field']//text[following::text = . and not(preceding::text = normalize-space(.))]");
            Debug.Assert(unitNodes != null, "unitNodes != null");
            foreach (XmlNode unitNode in unitNodes)
            {
                var unitText = unitNode.InnerText.Trim();
                if (!unitText.Contains(" "))
                {
                    continue;
                }
                unitText = unitText.Substring(0, unitText.IndexOf(" ", StringComparison.Ordinal));
                if (!_units.Contains(unitText))
                {
                    _units.Add(unitText);
                }
            }
            return _units;
        }

        private readonly ArrayList _categories = new ArrayList();
        public ArrayList Categories(string unit)
        {
            var xpath = "//field[@type='Semantic Field']//text[following::text = . and not(preceding::text = normalize-space(.))]";
            if (unit != null)
            {
                xpath = string.Format(@"//field[@type='Semantic Field']//text[starts-with(normalize-space(.), '{0}') and following::text = . and not(preceding::text = normalize-space(.))]", unit);
            }
            var catNodes = LiftDoc.SelectNodes(xpath);
            Debug.Assert(catNodes != null, "catNodes != null");
            foreach (XmlNode catNode in catNodes)
            {
                var catText = catNode.InnerText.Trim();
                // Skip categories included in another unit
                if (unit == null)
                {
                    if (catText.Contains(" "))
                    {
                        var i = catText.IndexOf(" ", StringComparison.Ordinal);
                        if (_units.Contains(catText.Substring(0, i)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (_units.Contains(catText))
                        {
                            continue;
                        }
                    }
                }
                if (!_categories.Contains(catText))
                {
                    _categories.Insert(0,catText);
                }
            }
            return _categories;
        }

        public string Slide1Lang()
        {
            var lang = LiftDoc.SelectSingleNode("//entry[1]//definition[1]//form[1]/@lang");
            Debug.Assert(lang != null, "lang1 != null");
            return lang.Value != "en"? lang.Value: "eng";
        }

        public string Slide2Lang()
        {
            var lang = LiftDoc.SelectSingleNode("//entry[1]//citation[1]/form[1]/@lang");
            Debug.Assert(lang != null, "lang2 != null");
            return lang.Value;
        }

        public string LangFolder()
        {
            return string.Format(@"{0}xx_{1}us", Slide2Lang().ToUpper(), Slide1Lang().ToUpper());
        }

        public void SetUnit(string unit)
        {
            _unitName = unit;
            _unit += 1;
            _lesson = 0;
            _categories.RemoveRange(0, _categories.Count);
        }

        public void Convert(string cat, string langFolder, string dateStamp)
        {
            _lesson += 1;
            var listSpec = string.Format(@"{0}{1:000}_{2:000}", langFolder, _unit, _lesson);
            _folder = Path.Combine(langFolder, listSpec);
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            _listFullName = Path.Combine(_folder, "list.xml");
            var sw = new StreamWriter(_listFullName, false, new UTF8Encoding(true));

            var xsltArgs = new XsltArgumentList();
            xsltArgs.AddParam("UnitTitle", "", _unitName);
            xsltArgs.AddParam("Category","",cat);
            xsltArgs.AddParam("Unit","", string.Format("{0:00}", _unit));
            xsltArgs.AddParam("Lesson", "", string.Format("{0:00}", _lesson));
            xsltArgs.AddParam("Date", "", dateStamp);
            xsltArgs.AddParam("uuid", "", Guid.NewGuid().ToString());
            xsltArgs.AddParam("WritingSystemsFolder","",string.Format("{0}/WritingSystems/", Path.GetDirectoryName(_fileName)));

            Lift2B4X.Transform(_fileName, xsltArgs, sw);
            sw.Close();
        }

        //public void CopySchema()
        //{
        //    const bool overwrite = true;
        //    File.Copy(BykiSchema, Path.Combine(_folder, BykiSchema), overwrite);
        //}

        public void CopyAudio()
        {
            ListDoc.Load(_listFullName);
            var nsmgr = new XmlNamespaceManager(LiftDoc.NameTable);
            nsmgr.AddNamespace("b4x", "http://www.transparent.com/xml/BykiList/v1-transitional");
            var audioNodes = ListDoc.SelectNodes("//b4x:side1_sound/@url | //b4x:side2_sound/@url", nsmgr);
            var srcFolder = Path.Combine(Path.GetDirectoryName(_fileName), "audio");
            if (audioNodes != null)
            {
                var dstFolder = Path.Combine(_folder, "sounds");
                if (!Directory.Exists(dstFolder))
                {
                    Directory.CreateDirectory(dstFolder);
                }
                foreach (XmlAttribute audioNode in audioNodes)
                {
                    var audioName = Path.GetFileName(audioNode.InnerText);
                    var audioNameWoExt = Path.GetFileNameWithoutExtension(audioName);
                    var pcmTemp = Path.Combine(Path.GetTempPath(),Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".wav");
                    var p1 = new Process();
                    p1.StartInfo.FileName = "lame.exe";
                    p1.StartInfo.Arguments = string.Format("--decode {0} {1}", Path.Combine(srcFolder, audioName), pcmTemp);
                    p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p1.Start();
                    p1.WaitForExit();
                    var p2 = new Process();
                    p2.StartInfo.FileName = "oggenc2.exe";
                    p2.StartInfo.Arguments = string.Format("--output={0} {1}", Path.Combine(dstFolder, audioNameWoExt + ".ogg"), pcmTemp);
                    p2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p2.Start();
                    p2.WaitForExit();
                    File.Delete(pcmTemp);
                    audioNode.InnerText = audioNode.InnerText.Replace(".mp3", ".ogg");
                }
            }
            ListDoc.Save(_listFullName);
            ListDoc.RemoveAll();
        }

        public void Package()
        {
            var folder = Path.GetDirectoryName(_listFullName);
            var z = new FastZip();
            const bool recurse = true;
            z.CreateZip(folder + ".b4x", folder, recurse, null);
            Directory.Delete(folder,recurse);
        }
    }
}
